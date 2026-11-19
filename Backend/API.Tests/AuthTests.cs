using System.Net;
using System.Net.Http.Json;
using API.Tests.Infrastructure;
using Domain.Enums;

namespace API.Tests;

public class AuthTests(ApiFactory api)
{
    [Fact]
    public async Task Login_with_valid_credentials_returns_token_and_role()
    {
        var (id, email) = await api.CreateUserAsync(UserRole.Admin);

        var response = await api.CreateClient().PostAsJsonAsync("/api/auth/login", new { email, password = ApiFactory.Password });

        var body = await response.Json();
        Assert.False(string.IsNullOrWhiteSpace(body.Str("token")));
        Assert.Equal("Admin", body.Str("role"));
        Assert.Equal(id, body.GetProperty("userId").GetGuid());
    }

    [Fact]
    public async Task Login_with_wrong_password_is_unauthorized()
    {
        var (_, email) = await api.CreateUserAsync(UserRole.Admin);

        var response = await api.CreateClient().PostAsJsonAsync("/api/auth/login", new { email, password = "wrong-password" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_of_inactive_user_is_unauthorized()
    {
        var (_, email) = await api.CreateUserAsync(UserRole.Admin, active: false);

        var response = await api.CreateClient().PostAsJsonAsync("/api/auth/login", new { email, password = ApiFactory.Password });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Token_from_login_is_accepted_by_a_protected_endpoint()
    {
        var (_, email) = await api.CreateUserAsync(UserRole.SuperAdmin);
        var token = await api.LoginAsync(email, ApiFactory.Password);

        var response = await api.ClientWithToken(token).GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
