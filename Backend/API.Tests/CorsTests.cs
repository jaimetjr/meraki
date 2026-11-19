using API.Tests.Infrastructure;

namespace API.Tests;

/// <summary>Allowed frontend origins come from Cors:AllowedOrigins configuration.</summary>
public class CorsTests(ApiFactory api)
{
    private async Task<HttpResponseMessage> Preflight(string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/services");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "authorization");
        return await api.CreateClient().SendAsync(request);
    }

    [Fact]
    public async Task The_configured_origin_is_allowed()
    {
        var response = await Preflight(ApiFactory.AllowedOrigin);

        Assert.Equal([ApiFactory.AllowedOrigin], response.Headers.GetValues("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Other_origins_are_not_allowed()
    {
        var response = await Preflight("https://evil.test");

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }
}
