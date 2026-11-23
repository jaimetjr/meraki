using System.Net;
using API.Tests.Infrastructure;
using Domain.Enums;

namespace API.Tests;

public class AuthorizationTests(ApiFactory api)
{
    [Theory]
    [InlineData("POST", "/api/therapists")]
    [InlineData("POST", "/api/services")]
    [InlineData("POST", "/api/courses")]
    [InlineData("POST", "/api/testimonials")]
    [InlineData("PUT", "/api/services/00000000-0000-0000-0000-000000000001")]
    [InlineData("DELETE", "/api/therapists/00000000-0000-0000-0000-000000000001")]
    [InlineData("GET", "/api/users")]
    [InlineData("POST", "/api/users")]
    [InlineData("GET", "/api/consultoriome/professionals")]
    public async Task Anonymous_requests_to_protected_endpoints_are_unauthorized(string method, string url)
    {
        var response = await api.CreateClient().SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("GET", "/api/users")]
    [InlineData("POST", "/api/users")]
    [InlineData("PUT", "/api/users/00000000-0000-0000-0000-000000000001/password")]
    public async Task Admin_cannot_use_user_management(string method, string url)
    {
        var client = await api.ClientAsAsync(UserRole.Admin);

        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/therapists")]
    [InlineData("/api/services")]
    [InlineData("/api/courses")]
    [InlineData("/api/testimonials")]
    public async Task Public_lists_are_readable_anonymously(string url)
    {
        var response = await api.CreateClient().GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
