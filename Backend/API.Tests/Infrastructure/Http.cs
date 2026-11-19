using System.Net.Http.Json;
using System.Text.Json;

namespace API.Tests.Infrastructure;

/// <summary>
/// Request/response helpers. Tests talk to the API only through HTTP + JSON so they survive
/// internal restructuring (no references to DTO or service types).
/// </summary>
public static class Http
{
    public static string Unique(string prefix) => $"{prefix} {Guid.NewGuid():N}"[..Math.Min(prefix.Length + 9, 50)];

    public static MultipartFormDataContent Form(params (string Name, string? Value)[] fields)
    {
        var form = new MultipartFormDataContent();
        foreach (var (name, value) in fields)
        {
            if (value is not null) form.Add(new StringContent(value), name);
        }
        return form;
    }

    public static async Task<JsonElement> Json(this HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Expected success but got {(int)response.StatusCode}: {text}");
        return JsonDocument.Parse(text).RootElement.Clone();
    }

    public static async Task<JsonElement> GetJson(this HttpClient client, string url) =>
        await (await client.GetAsync(url)).Json();

    public static string Str(this JsonElement element, string property) =>
        element.GetProperty(property).GetString()!;

    public static Guid Id(this JsonElement element) => element.GetProperty("id").GetGuid();

    public static async Task<Guid> CreateAndGetId(this HttpClient client, string url, object body)
    {
        var response = await client.PostAsJsonAsync(url, body);
        return (await response.Json()).Id();
    }
}
