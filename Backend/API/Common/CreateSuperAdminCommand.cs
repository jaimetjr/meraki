using API.Features.Users;
using Domain.Enums;
using Infrastructure.Data;

namespace API.Common;

/// <summary>
/// <c>dotnet run --project API -- --create-superadmin</c>: asks for an e-mail, a name and a password, then
/// creates that SuperAdmin, or resets its password if the e-mail exists. Run it from your own machine with
/// ConnectionStrings__DefaultConnection pointing at the production database, so the credentials are never
/// stored in the host's settings. It exits before the startup seeding, so no demo content is ever added.
/// </summary>
public static class CreateSuperAdminCommand
{
    public const string Flag = "--create-superadmin";

    public static async Task<int> RunAsync(IServiceProvider services, TextReader input, TextWriter output, Func<string> readSecret)
    {
        output.Write("E-mail: ");
        var email = input.ReadLine()?.Trim() ?? "";
        output.Write("Nome: ");
        var firstName = input.ReadLine()?.Trim() ?? "";
        output.Write($"Senha (mínimo {CreateUserValidator.PasswordMinLength} caracteres): ");
        var password = readSecret();
        output.Write("Repita a senha: ");
        if (readSecret() != password)
        {
            output.WriteLine("As senhas não conferem. Nada foi alterado.");
            return 1;
        }

        var validation = new CreateUserValidator().Validate(
            new CreateUserDto { Email = email, FirstName = firstName, Password = password, Role = UserRole.SuperAdmin });
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors) output.WriteLine(error.ErrorMessage);
            output.WriteLine("Nada foi alterado.");
            return 1;
        }

        using var scope = services.CreateScope();
        var created = await Seeder.SetSuperAdminAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>(), email, firstName, password);
        output.WriteLine(created
            ? $"SuperAdmin {email} criado. Já pode entrar em /admin/login."
            : $"Senha de {email} redefinida; a conta agora é um SuperAdmin ativo.");
        return 0;
    }

    /// <summary>Reads a password without echoing it (falls back to a plain line when input is redirected).</summary>
    public static string ReadHidden()
    {
        if (Console.IsInputRedirected) return Console.ReadLine() ?? "";
        var password = new System.Text.StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) break;
            if (key.Key == ConsoleKey.Backspace) { if (password.Length > 0) password.Length--; }
            else if (!char.IsControl(key.KeyChar)) password.Append(key.KeyChar);
        }
        Console.WriteLine();
        return password.ToString();
    }
}
