using System.Net;
using System.Net.Http.Json;
using API.Tests.Infrastructure;
using Domain.Enums;

namespace API.Tests;

/// <summary>Deactivation is reversible and visible; DELETE deactivates; nobody can lock themselves out.</summary>
public class UserLifecycleTests(ApiFactory api)
{
    [Fact]
    public async Task Deactivated_user_stays_listed_and_can_be_reactivated()
    {
        var client = await api.ClientAsAsync(UserRole.SuperAdmin);
        var (id, email) = await api.CreateUserAsync(UserRole.Admin);

        var deactivate = await client.PutAsJsonAsync($"/api/users/{id}", new { email, firstName = "Ana", isActive = false });

        Assert.Equal(HttpStatusCode.NoContent, deactivate.StatusCode);
        var listed = (await client.GetJson("/api/users")).EnumerateArray().Single(u => u.Id() == id);
        Assert.False(listed.GetProperty("isActive").GetBoolean());
        var blocked = await api.CreateClient().PostAsJsonAsync("/api/auth/login", new { email, password = ApiFactory.Password });
        Assert.Equal(HttpStatusCode.Unauthorized, blocked.StatusCode);

        var reactivate = await client.PutAsJsonAsync($"/api/users/{id}", new { email, firstName = "Ana", isActive = true });

        Assert.Equal(HttpStatusCode.NoContent, reactivate.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(await api.LoginAsync(email, ApiFactory.Password)));
    }

    [Fact]
    public async Task Delete_deactivates_the_user_instead_of_removing_it()
    {
        var client = await api.ClientAsAsync(UserRole.SuperAdmin);
        var (id, _) = await api.CreateUserAsync(UserRole.Admin);

        var response = await client.DeleteAsync($"/api/users/{id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var user = await client.GetJson($"/api/users/{id}");
        Assert.False(user.GetProperty("isActive").GetBoolean());
    }

    [Fact]
    public async Task A_user_cannot_deactivate_themselves()
    {
        var (id, email) = await api.CreateUserAsync(UserRole.SuperAdmin);
        var self = api.ClientWithToken(await api.LoginAsync(email, ApiFactory.Password));

        var delete = await self.DeleteAsync($"/api/users/{id}");
        var update = await self.PutAsJsonAsync($"/api/users/{id}", new { email, firstName = "Ana", isActive = false });

        Assert.Equal(HttpStatusCode.BadRequest, delete.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, update.StatusCode);
        Assert.Equal("Você não pode desativar sua própria conta.", await update.ProblemDetail());
        Assert.False(string.IsNullOrWhiteSpace(await api.LoginAsync(email, ApiFactory.Password)));
    }

    [Fact]
    public async Task A_superadmin_cannot_change_their_own_role()
    {
        var (id, email) = await api.CreateUserAsync(UserRole.SuperAdmin);
        var self = api.ClientWithToken(await api.LoginAsync(email, ApiFactory.Password));

        var response = await self.PutAsJsonAsync($"/api/users/{id}", new { email, firstName = "Ana", role = "Admin" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Você não pode alterar seu próprio papel.", await response.ProblemDetail());
        var stored = await self.GetJson($"/api/users/{id}");
        Assert.Equal("SuperAdmin", stored.GetProperty("role").GetString());
    }

    [Fact]
    public async Task A_superadmin_can_edit_their_own_profile_when_the_role_is_unchanged()
    {
        var (id, email) = await api.CreateUserAsync(UserRole.SuperAdmin);
        var self = api.ClientWithToken(await api.LoginAsync(email, ApiFactory.Password));

        var response = await self.PutAsJsonAsync($"/api/users/{id}", new { email, firstName = "Novo", role = "SuperAdmin", isActive = true });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Email_of_an_inactive_user_cannot_be_reused()
    {
        var client = await api.ClientAsAsync(UserRole.SuperAdmin);
        var (_, email) = await api.CreateUserAsync(UserRole.Admin, active: false);

        var response = await client.PostAsJsonAsync("/api/users", new { email, password = "Another-Passw0rd!", firstName = "Ana", role = "Admin" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
