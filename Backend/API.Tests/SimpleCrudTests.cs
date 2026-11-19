using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.Tests.Infrastructure;
using Domain.Enums;

namespace API.Tests;

/// <summary>Categories, benefits and specialties share the same JSON CRUD shape.</summary>
public class SimpleCrudTests(ApiFactory api)
{
    [Theory]
    [InlineData("/api/categories", "name")]
    [InlineData("/api/benefits", "title")]
    [InlineData("/api/specialties", "name")]
    public async Task Created_item_is_returned_by_get_by_id(string resource, string field)
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var value = Http.Unique("Created");

        var id = await client.CreateAndGetId(resource, new Dictionary<string, string> { [field] = value });

        var item = await api.CreateClient().GetJson($"{resource}/{id}");
        Assert.Equal(value, item.Str(field));
    }

    [Theory]
    [InlineData("/api/categories", "name")]
    [InlineData("/api/benefits", "title")]
    [InlineData("/api/specialties", "name")]
    public async Task Update_changes_the_stored_value(string resource, string field)
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var id = await client.CreateAndGetId(resource, new Dictionary<string, string> { [field] = Http.Unique("Before") });
        var updated = Http.Unique("After");

        var response = await client.PutAsJsonAsync($"{resource}/{id}", new Dictionary<string, string> { [field] = updated });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var item = await api.CreateClient().GetJson($"{resource}/{id}");
        Assert.Equal(updated, item.Str(field));
    }

    [Theory]
    [InlineData("/api/categories", "name")]
    [InlineData("/api/benefits", "title")]
    [InlineData("/api/specialties", "name")]
    public async Task Deleted_item_disappears_from_get_and_list(string resource, string field)
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var id = await client.CreateAndGetId(resource, new Dictionary<string, string> { [field] = Http.Unique("Doomed") });

        var response = await client.DeleteAsync($"{resource}/{id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.CreateClient().GetAsync($"{resource}/{id}")).StatusCode);
        var list = await api.CreateClient().GetJson(resource);
        Assert.DoesNotContain(list.EnumerateArray(), e => e.Id() == id);
    }

    [Theory]
    [InlineData("/api/categories")]
    [InlineData("/api/benefits")]
    [InlineData("/api/specialties")]
    public async Task Get_unknown_id_is_not_found(string resource)
    {
        var response = await api.CreateClient().GetAsync($"{resource}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
