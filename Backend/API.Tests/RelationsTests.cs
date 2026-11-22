using System.Net;
using System.Text.Json;
using API.Tests.Infrastructure;
using Domain.Enums;

namespace API.Tests;

/// <summary>Updates replace a relation list; only existing, active IDs are accepted.</summary>
public class RelationsTests(ApiFactory api)
{
    private static string Json(object value) => JsonSerializer.Serialize(value);

    private static MultipartFormDataContent TherapistForm(string name, params Guid[] specialtyIds) => Http.Form(
        ("Name", name), ("Bio", "Bio"), ("Experience", "5"), ("Education", "Edu"),
        ("specialties", Json(specialtyIds.Select(id => new { id, name = "as sent by the admin UI" }).ToArray())));

    private static MultipartFormDataContent ServiceForm(string name, Guid categoryId, string benefitsJson) => Http.Form(
        ("Name", name), ("Description", "Desc"), ("Price", "10"), ("Currency", "BRL"),
        ("category", Json(new { id = categoryId, name = "as sent by the admin UI" })), ("benefits", benefitsJson));

    [Fact]
    public async Task Removing_a_specialty_from_a_therapist_persists()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var keep = await client.CreateAndGetId("/api/specialties", new { name = Http.Unique("Keep") });
        var drop = await client.CreateAndGetId("/api/specialties", new { name = Http.Unique("Drop") });
        var name = Http.Unique("Therapist");
        var id = (await (await client.PostAsync("/api/therapists", TherapistForm(name, keep, drop))).Json()).Id();

        var update = await client.PutAsync($"/api/therapists/{id}", TherapistForm(name, keep));

        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);
        var specialties = (await api.CreateClient().GetJson($"/api/therapists/{id}")).GetProperty("specialties")
            .EnumerateArray().Select(s => s.Id()).ToList();
        Assert.Equal([keep], specialties);
    }

    [Fact]
    public async Task Removing_a_benefit_from_a_service_persists()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var categoryId = await client.CreateAndGetId("/api/categories", new { name = Http.Unique("Cat") });
        var keep = await client.CreateAndGetId("/api/benefits", new { title = Http.Unique("Keep") });
        var drop = await client.CreateAndGetId("/api/benefits", new { title = Http.Unique("Drop") });
        var name = Http.Unique("Svc");
        var id = (await (await client.PostAsync("/api/services",
            ServiceForm(name, categoryId, Json(new[] { new { id = keep, title = "x" }, new { id = drop, title = "x" } })))).Json()).Id();

        var update = await client.PutAsync($"/api/services/{id}", ServiceForm(name, categoryId, Json(new[] { new { id = keep, title = "x" } })));

        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);
        var benefits = (await api.CreateClient().GetJson($"/api/services/{id}")).GetProperty("benefits")
            .EnumerateArray().Select(b => b.Id()).ToList();
        Assert.Equal([keep], benefits);
    }

    [Fact]
    public async Task Therapist_with_an_unknown_specialty_is_rejected()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);

        var response = await client.PostAsync("/api/therapists", TherapistForm(Http.Unique("T"), Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Therapist_with_a_deleted_specialty_is_rejected()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var specialtyId = await client.CreateAndGetId("/api/specialties", new { name = Http.Unique("Gone") });
        await client.DeleteAsync($"/api/specialties/{specialtyId}");

        var response = await client.PostAsync("/api/therapists", TherapistForm(Http.Unique("T"), specialtyId));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Service_benefit_without_an_id_is_rejected_instead_of_created()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var categoryId = await client.CreateAndGetId("/api/categories", new { name = Http.Unique("Cat") });
        var title = Http.Unique("Inline");

        var response = await client.PostAsync("/api/services",
            ServiceForm(Http.Unique("Svc"), categoryId, Json(new[] { new { title } })));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var benefits = await api.CreateClient().GetJson("/api/benefits");
        Assert.DoesNotContain(benefits.EnumerateArray(), b => b.Str("title") == title);
    }

    [Fact]
    public async Task Deleting_a_category_that_services_use_is_a_conflict()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var categoryId = await client.CreateAndGetId("/api/categories", new { name = Http.Unique("Used") });
        await (await client.PostAsync("/api/services", ServiceForm(Http.Unique("Svc"), categoryId, "[]"))).Json();

        var response = await client.DeleteAsync($"/api/categories/{categoryId}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(
            "Esta categoria é usada por 1 serviço(s). Mova-os para outra categoria antes de excluí-la.",
            await response.ProblemDetail());
        Assert.Equal(HttpStatusCode.OK, (await api.CreateClient().GetAsync($"/api/categories/{categoryId}")).StatusCode);
    }
}
