using System.Net;
using System.Text.Json;
using API.Tests.Infrastructure;
using Domain.Enums;

namespace API.Tests;

/// <summary>
/// A therapist's services and a service's benefits are replaced, not merged, on update; omitting the list keeps
/// the current one. Only existing, active services can be linked to a therapist.
/// </summary>
public class RelationsTests(ApiFactory api)
{
    private static string Json(object value) => JsonSerializer.Serialize(value);

    private static MultipartFormDataContent TherapistForm(string name, params Guid[] serviceIds) => Http.Form(
        ("Name", name), ("Bio", "Bio"), ("Experience", "5"), ("Education", "Edu"),
        ("services", Json(serviceIds.Select(id => new { id, name = "as sent by the admin UI" }).ToArray())));

    private static MultipartFormDataContent ServiceForm(string name, string? benefitsJson) => Http.Form(
        ("Name", name), ("Description", "Desc"), ("benefits", benefitsJson));

    private static async Task<Guid> CreateServiceAsync(HttpClient client, string? name = null) =>
        (await (await client.PostAsync("/api/services", ServiceForm(name ?? Http.Unique("Svc"), null))).Json()).Id();

    private static async Task<Guid> CreateTherapistAsync(HttpClient client, string name, params Guid[] serviceIds) =>
        (await (await client.PostAsync("/api/therapists", TherapistForm(name, serviceIds))).Json()).Id();

    private async Task<List<string>> BenefitsOf(Guid serviceId) =>
        (await api.CreateClient().GetJson($"/api/services/{serviceId}")).GetProperty("benefits")
            .EnumerateArray().Select(b => b.GetString()!).ToList();

    private async Task<List<Guid>> ServicesOf(Guid therapistId) =>
        (await api.CreateClient().GetJson($"/api/therapists/{therapistId}")).GetProperty("services")
            .EnumerateArray().Select(s => s.Id()).ToList();

    [Fact]
    public async Task Removing_a_service_from_a_therapist_persists()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var keep = await CreateServiceAsync(client);
        var drop = await CreateServiceAsync(client);
        var name = Http.Unique("Therapist");
        var id = await CreateTherapistAsync(client, name, keep, drop);

        var update = await client.PutAsync($"/api/therapists/{id}", TherapistForm(name, keep));

        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);
        Assert.Equal([keep], await ServicesOf(id));
    }

    [Fact]
    public async Task A_service_s_benefits_are_replaced_on_update_and_kept_when_omitted()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var name = Http.Unique("Svc");
        var id = (await (await client.PostAsync("/api/services", ServiceForm(name, Json(new[] { "Relaxamento", "Sono" })))).Json()).Id();

        await client.PutAsync($"/api/services/{id}", ServiceForm(name, Json(new[] { "Relaxamento" })));
        Assert.Equal(["Relaxamento"], await BenefitsOf(id));

        await client.PutAsync($"/api/services/{id}", ServiceForm(name, null));
        Assert.Equal(["Relaxamento"], await BenefitsOf(id));

        await client.PutAsync($"/api/services/{id}", ServiceForm(name, "[]"));
        Assert.Empty(await BenefitsOf(id));
    }

    [Fact]
    public async Task Therapist_with_an_unknown_service_is_rejected()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);

        var response = await client.PostAsync("/api/therapists", TherapistForm(Http.Unique("T"), Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Um dos serviços selecionados não existe mais. Recarregue a página e tente novamente.",
            await response.ProblemDetail());
    }

    [Fact]
    public async Task Therapist_with_a_deleted_service_is_rejected()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var serviceId = await CreateServiceAsync(client);
        await client.DeleteAsync($"/api/services/{serviceId}");

        var response = await client.PostAsync("/api/therapists", TherapistForm(Http.Unique("T"), serviceId));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task A_deleted_service_disappears_from_its_therapists()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var serviceId = await CreateServiceAsync(client);
        var therapistId = await CreateTherapistAsync(client, Http.Unique("T"), serviceId);

        await client.DeleteAsync($"/api/services/{serviceId}");

        Assert.Empty(await ServicesOf(therapistId));
    }

    [Fact]
    public async Task A_service_lists_the_active_therapists_who_offer_it()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var serviceId = await CreateServiceAsync(client);
        var offersName = Http.Unique("Offers");
        var offers = await CreateTherapistAsync(client, offersName, serviceId);
        await CreateTherapistAsync(client, Http.Unique("Other"));
        var deleted = await CreateTherapistAsync(client, Http.Unique("Deleted"), serviceId);
        await client.DeleteAsync($"/api/therapists/{deleted}");

        var therapists = (await api.CreateClient().GetJson($"/api/services/{serviceId}")).GetProperty("therapists")
            .EnumerateArray().ToList();

        var only = Assert.Single(therapists);
        Assert.Equal(offers, only.Id());
        Assert.Equal(offersName, only.Str("name"));
    }
}
