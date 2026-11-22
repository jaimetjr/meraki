using System.Net;
using API.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;

namespace API.Tests;

/// <summary>
/// In production the built Angular site sits in the API's wwwroot, so one service serves both: the site's
/// files, the site's own routes (e.g. a refresh on /admin/users) and /api.
/// </summary>
public class SiteHostingTests(ApiFactory api)
{
    private HttpClient SiteClient()
    {
        var root = Directory.CreateTempSubdirectory("meraki-site-").FullName;
        File.WriteAllText(Path.Combine(root, "index.html"), "<!doctype html><title>meraki-site</title>");
        File.WriteAllText(Path.Combine(root, "main.js"), "console.log('meraki-site-js');");
        return api.WithWebHostBuilder(builder => builder.UseSetting(WebHostDefaults.WebRootKey, root)).CreateClient();
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/cursos")]
    [InlineData("/admin/users")]
    public async Task Site_pages_load_including_a_refresh_on_a_deep_link(string path)
    {
        var response = await SiteClient().GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("meraki-site", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task The_site_files_are_served()
    {
        var response = await SiteClient().GetAsync("/main.js");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("meraki-site-js", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("/api/does-not-exist")]
    [InlineData("/missing.js")]
    public async Task Unknown_api_routes_and_missing_files_are_404_not_the_site(string path)
    {
        var response = await SiteClient().GetAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task The_api_still_answers_next_to_the_site()
    {
        var response = await SiteClient().GetAsync("/api/therapists");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("[", (await response.Content.ReadAsStringAsync()).TrimStart());
    }
}
