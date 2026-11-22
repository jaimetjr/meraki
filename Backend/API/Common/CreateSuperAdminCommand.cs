using API.Features.Users;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (await DatabaseProblemAsync(db) is { } problem)
        {
            output.WriteLine(problem);
            output.WriteLine("Nada foi alterado.");
            return 1;
        }

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

        var created = await Seeder.SetSuperAdminAsync(db, email, firstName, password);
        output.WriteLine(created
            ? $"SuperAdmin {email} criado. Já pode entrar em /admin/login."
            : $"Senha de {email} redefinida; a conta agora é um SuperAdmin ativo.");
        return 0;
    }

    /// <summary>
    /// Checked before asking for anything, so a wrong address doesn't cost you typing the password. Says which
    /// server it tried and why it failed (never the password), with the fix for the usual Railway mistakes.
    /// </summary>
    private static async Task<string?> DatabaseProblemAsync(AppDbContext db)
    {
        if (AddressMistake(db.Database.GetConnectionString() ?? "", out var where) is { } mistake) return mistake;
        try
        {
            await db.Database.OpenConnectionAsync();
            await db.Database.CloseConnectionAsync();
            if ((await db.Database.GetPendingMigrationsAsync()).Any())
                return "O banco ainda não tem as tabelas. Faça o deploy primeiro e espere o log \"Database migrations applied.\".";
            return null;
        }
        catch (Exception exception)
        {
            while (exception.InnerException is { } inner) exception = inner;
            return $"Não foi possível conectar ao banco em {where}: {exception.Message}\n{UsePublicUrl}";
        }
    }

    private static string? AddressMistake(string connectionString, out string where)
    {
        where = "";
        if (connectionString.Contains("${{"))
            return "Você copiou o modelo da variável (com ${{...}}), não o valor. No Railway: Postgres → Variables → " +
                   "DATABASE_PUBLIC_URL → ícone de copiar ao lado do valor.";

        NpgsqlConnectionStringBuilder address;
        try { address = new NpgsqlConnectionStringBuilder(connectionString); }
        catch (ArgumentException) { return Incomplete; }
        if (string.IsNullOrWhiteSpace(address.Host)) return Incomplete;
        if (address.Host.EndsWith(".railway.internal", StringComparison.OrdinalIgnoreCase))
            return $"{address.Host} é o endereço interno do Railway (DATABASE_URL), que só funciona lá dentro. {UsePublicUrl}";

        where = $"{address.Host}:{address.Port}/{address.Database}";
        return null;
    }

    private const string UsePublicUrl = "Use o DATABASE_PUBLIC_URL do Postgres no Railway (endereço ….proxy.rlwy.net:porta).";

    private const string Incomplete =
        "O endereço do banco está incompleto ou inválido. Se o DATABASE_PUBLIC_URL do Postgres estiver vazio ou sem " +
        "servidor, ative o acesso público em Postgres → Settings → Networking → TCP Proxy e copie o valor de novo.";

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
