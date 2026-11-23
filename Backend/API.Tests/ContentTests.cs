using System.Net;
using System.Text.Json;
using API.Tests.Infrastructure;
using Domain.Enums;

namespace API.Tests;

/// <summary>Multipart (form) resources: therapists, services, courses, testimonials.</summary>
public class ContentTests(ApiFactory api)
{
    private static string JsonArray(params object[] items) => JsonSerializer.Serialize(items);

    [Fact]
    public async Task Therapist_is_created_with_the_services_they_offer()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var serviceName = Http.Unique("Reiki");
        var serviceId = (await (await client.PostAsync("/api/services", Http.Form(
            ("Name", serviceName), ("Description", "Desc")))).Json()).Id();
        var name = Http.Unique("Therapist");

        var response = await client.PostAsync("/api/therapists", Http.Form(
            ("Name", name), ("Bio", "Bio text"), ("Experience", "10 anos"), ("Education", "Psicologia"),
            ("services", JsonArray(new { id = serviceId, name = serviceName }))));

        var id = (await response.Json()).Id();
        var therapist = await api.CreateClient().GetJson($"/api/therapists/{id}");
        Assert.Equal(name, therapist.Str("name"));
        Assert.Equal("Bio text", therapist.Str("bio"));
        var service = Assert.Single(therapist.GetProperty("services").EnumerateArray());
        Assert.Equal(serviceId, service.Id());
        Assert.Equal(serviceName, service.Str("name"));
    }

    [Fact]
    public async Task Therapist_update_changes_fields_and_delete_hides_it()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var id = (await (await client.PostAsync("/api/therapists", Http.Form(
            ("Name", Http.Unique("Before")), ("Bio", "Bio"), ("Experience", "1"), ("Education", "E")))).Json()).Id();
        var newName = Http.Unique("After");

        var update = await client.PutAsync($"/api/therapists/{id}", Http.Form(
            ("Name", newName), ("Bio", "New bio"), ("Experience", "2"), ("Education", "E2")));

        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);
        Assert.Equal(newName, (await api.CreateClient().GetJson($"/api/therapists/{id}")).Str("name"));

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/therapists/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.CreateClient().GetAsync($"/api/therapists/{id}")).StatusCode);
    }

    [Fact]
    public async Task Service_is_created_with_its_benefits_duration_and_price()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var name = Http.Unique("Service");

        // Benefits are typed one per line in the admin: surrounding spaces and empty lines are dropped.
        var response = await client.PostAsync("/api/services", Http.Form(
            ("Name", name), ("Description", "Desc"), ("Duration", "60 min"), ("Price", "150"), ("Currency", "BRL"),
            ("benefits", JsonArray("  Relaxamento ", "", "Equilíbrio energético"))));

        var id = (await response.Json()).Id();
        var service = await api.CreateClient().GetJson($"/api/services/{id}");
        Assert.Equal(name, service.Str("name"));
        Assert.Equal("60 min", service.Str("duration"));
        Assert.Equal(150m, service.GetProperty("price").GetDecimal());
        Assert.Equal("BRL", service.Str("currency"));
        Assert.Equal(["Relaxamento", "Equilíbrio energético"],
            service.GetProperty("benefits").EnumerateArray().Select(b => b.GetString()));

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/services/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.CreateClient().GetAsync($"/api/services/{id}")).StatusCode);
    }

    [Fact]
    public async Task A_service_s_price_is_optional()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var name = Http.Unique("Service");
        var id = (await (await client.PostAsync("/api/services", Http.Form(("Name", name), ("Description", "Desc")))).Json()).Id();
        Assert.Equal(JsonValueKind.Null, (await api.CreateClient().GetJson($"/api/services/{id}")).GetProperty("price").ValueKind);

        await client.PutAsync($"/api/services/{id}", Http.Form(("Name", name), ("Description", "Desc"), ("Price", "80")));
        Assert.Equal(80m, (await api.CreateClient().GetJson($"/api/services/{id}")).GetProperty("price").GetDecimal());

        // The admin always sends every field, so an update without a price removes it ("sob consulta").
        await client.PutAsync($"/api/services/{id}", Http.Form(("Name", name), ("Description", "Desc")));
        Assert.Equal(JsonValueKind.Null, (await api.CreateClient().GetJson($"/api/services/{id}")).GetProperty("price").ValueKind);
    }

    [Theory]
    [InlineData("/api/categories")]
    [InlineData("/api/benefits")]
    [InlineData("/api/specialties")]
    [InlineData("/api/services/category/Terapia")]
    [InlineData("/api/therapists/specialty/Reiki")]
    public async Task Removed_content_types_have_no_endpoints(string url)
    {
        var response = await api.CreateClient().GetAsync(url);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Course_is_created_and_deleted()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var title = Http.Unique("Course");

        var response = await client.PostAsync("/api/courses", Http.Form(
            ("Title", title), ("Description", "Desc"), ("Instructor", "Ana"),
            ("Type", "Curso"), ("Status", "Disponivel"),
            ("StartDate", "2027-03-01T10:00:00Z"), ("Modality", "Online"), ("Price", "200"), ("Currency", "BRL")));

        var id = (await response.Json()).Id();
        var course = await api.CreateClient().GetJson($"/api/courses/{id}");
        Assert.Equal(title, course.Str("title"));
        Assert.Equal("Ana", course.Str("instructor"));
        Assert.Equal(200m, course.GetProperty("price").GetDecimal());

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/courses/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.CreateClient().GetAsync($"/api/courses/{id}")).StatusCode);
    }

    [Fact]
    public async Task Testimonial_is_created_and_deleted()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var author = Http.Unique("Author");

        var response = await client.PostAsync("/api/testimonials", Http.Form(
            ("AuthorName", author), ("Rating", "4"), ("Content", "Great")));

        var id = (await response.Json()).Id();
        var testimonial = await api.CreateClient().GetJson($"/api/testimonials/{id}");
        Assert.Equal(author, testimonial.Str("authorName"));
        Assert.Equal(4, testimonial.GetProperty("rating").GetInt32());

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/testimonials/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.CreateClient().GetAsync($"/api/testimonials/{id}")).StatusCode);
    }
}
