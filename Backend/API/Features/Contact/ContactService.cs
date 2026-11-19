using Domain.Entities;
using Domain.Exceptions;
using Infrastructure.Captcha;
using Infrastructure.Data;
using Infrastructure.Email;
using Microsoft.EntityFrameworkCore;

namespace API.Features.Contact;

/// <summary>
/// Contact form: CAPTCHA first, then the message is stored (so nothing is lost), then the clinic is
/// notified by e-mail on a best-effort basis.
/// </summary>
public class ContactService(
    AppDbContext db,
    ICaptchaVerifier captcha,
    IEmailSender emailSender,
    IConfiguration configuration,
    ILogger<ContactService> logger)
{
    public async Task SubmitAsync(ContactRequestDto dto, string? remoteIp)
    {
        if (!await captcha.VerifyAsync(dto.CaptchaToken, remoteIp))
            throw new DomainException("CAPTCHA verification failed. Please try again.");

        var message = new ContactMessage(dto.Name.Trim(), dto.Email.Trim(), dto.Phone.Trim(), dto.Service.Trim(),
            dto.Message?.Trim(), consentGivenAt: DateTime.UtcNow);
        db.ContactMessages.Add(message);
        await db.SaveChangesAsync();

        await NotifyClinicAsync(message);
    }

    public async Task<List<ContactMessageDto>> GetAllAsync() =>
        (await db.ContactMessages.AsNoTracking().OrderByDescending(m => m.CreatedAt).ToListAsync()).ConvertAll(m => m.ToDto());

    public async Task<bool> MarkReadAsync(Guid id, bool isRead)
    {
        var message = await db.ContactMessages.FirstOrDefaultAsync(m => m.Id == id);
        if (message == null) return false;

        message.MarkRead(isRead);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>Permanently deletes the message (personal data is not kept once the admin is done with it).</summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var message = await db.ContactMessages.FirstOrDefaultAsync(m => m.Id == id);
        if (message == null) return false;

        db.ContactMessages.Remove(message);
        await db.SaveChangesAsync();
        return true;
    }

    private async Task NotifyClinicAsync(ContactMessage message)
    {
        var to = configuration["Contact:NotifyEmail"];
        if (string.IsNullOrWhiteSpace(to))
        {
            logger.LogWarning("Contact:NotifyEmail is not configured; message {Id} was stored without an e-mail notification.", message.Id);
            return;
        }

        try
        {
            await emailSender.SendAsync(new EmailMessage(to, $"Novo contato pelo site: {message.Name}", NotificationBody(message), ReplyTo: message.Email));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not send the e-mail for contact message {Id}; it is still in the admin inbox.", message.Id);
        }
    }

    private static string NotificationBody(ContactMessage message) => $"""
        Nova mensagem recebida pelo formulário de contato do site.

        Nome: {message.Name}
        E-mail: {message.Email}
        Telefone: {message.Phone}
        Serviço de interesse: {message.ServiceName}
        Recebida em: {message.CreatedAt:dd/MM/yyyy HH:mm} (UTC)

        Mensagem:
        {message.Message ?? "(sem mensagem)"}

        Responda a este e-mail para falar diretamente com a pessoa.
        A mensagem também está no painel administrativo, em Mensagens.
        """;
}
