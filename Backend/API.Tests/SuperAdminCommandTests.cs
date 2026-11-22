using API.Common;
using API.Tests.Infrastructure;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace API.Tests;

/// <summary>
/// `--create-superadmin` creates the first admin in production without putting credentials in the host's
/// settings, and resets a forgotten password when run again with the same e-mail.
/// </summary>
public class SuperAdminCommandTests(ApiFactory api)
{
    private async Task<(int ExitCode, string Output)> RunAsync(params string[] answers)
    {
        var input = new StringReader(string.Join("\n", answers) + "\n");
        var output = new StringWriter();
        var exitCode = await CreateSuperAdminCommand.RunAsync(api.Services, input, output, () => input.ReadLine() ?? "");
        return (exitCode, output.ToString());
    }

    private static string NewEmail() => $"owner-{Guid.NewGuid():N}@test.local";

    [Fact]
    public async Task Creates_an_active_superadmin_who_can_log_in()
    {
        var email = NewEmail();

        var (exitCode, _) = await RunAsync(email, "Jaime", "Owner-Passw0rd!", "Owner-Passw0rd!");

        Assert.Equal(0, exitCode);
        Assert.False(string.IsNullOrWhiteSpace(await api.LoginAsync(email, "Owner-Passw0rd!")));
        await using var db = api.CreateDbContext();
        var user = await db.Users.SingleAsync(u => u.Email == email);
        Assert.Equal(UserRole.SuperAdmin, user.Role);
        Assert.Equal("Jaime", user.FirstName);
    }

    [Fact]
    public async Task Run_again_with_the_same_email_resets_the_password_and_restores_access()
    {
        var (_, email) = await api.CreateUserAsync(UserRole.Admin, active: false);

        var (exitCode, output) = await RunAsync(email, "Jaime", "Brand-New-Passw0rd!", "Brand-New-Passw0rd!");

        Assert.Equal(0, exitCode);
        Assert.Contains("redefinida", output);
        Assert.False(string.IsNullOrWhiteSpace(await api.LoginAsync(email, "Brand-New-Passw0rd!")));
        await using var db = api.CreateDbContext();
        Assert.Equal(UserRole.SuperAdmin, (await db.Users.SingleAsync(u => u.Email == email)).Role);
    }

    [Fact]
    public async Task A_short_password_is_refused_and_nothing_is_saved()
    {
        var email = NewEmail();

        var (exitCode, output) = await RunAsync(email, "Jaime", "short", "short");

        Assert.NotEqual(0, exitCode);
        Assert.Contains("'Senha'", output);
        await using var db = api.CreateDbContext();
        Assert.False(await db.Users.AnyAsync(u => u.Email == email));
    }

    [Fact]
    public async Task Passwords_that_do_not_match_are_refused_and_nothing_is_saved()
    {
        var email = NewEmail();

        var (exitCode, output) = await RunAsync(email, "Jaime", "Owner-Passw0rd!", "Other-Passw0rd!");

        Assert.NotEqual(0, exitCode);
        Assert.Contains("não conferem", output);
        await using var db = api.CreateDbContext();
        Assert.False(await db.Users.AnyAsync(u => u.Email == email));
    }
}
