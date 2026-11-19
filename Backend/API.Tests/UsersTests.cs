using System.Net;
using System.Net.Http.Json;
using API.Tests.Infrastructure;
using Domain.Enums;

namespace API.Tests;

public class UsersTests(ApiFactory api)
{
    [Fact]
    public async Task SuperAdmin_creates_a_user_who_can_log_in()
    {
        var client = await api.ClientAsAsync(UserRole.SuperAdmin);
        var email = $"new-{Guid.NewGuid():N}@test.local";

        var response = await client.PostAsJsonAsync("/api/users",
            new { email, password = "Another-Passw0rd!", firstName = "Ana", lastName = "Souza", role = "Admin" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var token = await api.LoginAsync(email, "Another-Passw0rd!");
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task Creating_a_user_with_an_existing_email_is_a_conflict()
    {
        var client = await api.ClientAsAsync(UserRole.SuperAdmin);
        var (_, email) = await api.CreateUserAsync(UserRole.Admin);

        var response = await client.PostAsJsonAsync("/api/users", new { email, password = "Another-Passw0rd!", role = "Admin" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Password_change_replaces_the_old_password()
    {
        var client = await api.ClientAsAsync(UserRole.SuperAdmin);
        var (id, email) = await api.CreateUserAsync(UserRole.Admin);

        var response = await client.PutAsJsonAsync($"/api/users/{id}/password", new { password = "Changed-Passw0rd!" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(await api.LoginAsync(email, "Changed-Passw0rd!")));
        var old = await api.CreateClient().PostAsJsonAsync("/api/auth/login", new { email, password = ApiFactory.Password });
        Assert.Equal(HttpStatusCode.Unauthorized, old.StatusCode);
    }
}
