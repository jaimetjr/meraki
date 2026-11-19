using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Features.Contact;

[Route("api/contact")]
[ApiController]
public class ContactController(ContactService service) : ControllerBase
{
    public const string RateLimitPolicy = "contact";

    /// <summary>Public: the website's contact form. Rate-limited per client IP.</summary>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicy)]
    public async Task<IActionResult> Submit(ContactRequestDto dto)
    {
        await service.SubmitAsync(dto, HttpContext.Connection.RemoteIpAddress?.ToString());
        return Ok(new ContactResponseDto
        {
            Success = true,
            Message = "Mensagem enviada com sucesso! Entraremos em contato em breve.",
        });
    }

    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetAll() => Ok(await service.GetAllAsync());

    [HttpPatch("{id}/read")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> MarkRead(Guid id, MarkReadDto dto) =>
        await service.MarkReadAsync(id, dto.IsRead) ? NoContent() : NotFound();

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id) =>
        await service.DeleteAsync(id) ? NoContent() : NotFound();
}
