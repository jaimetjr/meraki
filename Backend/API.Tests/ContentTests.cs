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
    public async Task Therapist_is_created_with_its_specialty()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var specialtyName = Http.Unique("Spec");
        var specialtyId = await client.CreateAndGetId("/api/specialties", new { name = specialtyName });
        var name = Http.Unique("Therapist");

        var response = await client.PostAsync("/api/therapists", Http.Form(
            ("Name", name), ("Bio", "Bio text"), ("Experience", "10 anos"), ("Education", "Psicologia"),
            ("specialties", JsonArray(new { id = specialtyId, name = specialtyName }))));

        var id = (await response.Json()).Id();
        var therapist = await api.CreateClient().GetJson($"/api/therapists/{id}");
        Assert.Equal(name, therapist.Str("name"));
        Assert.Equal("Bio text", therapist.Str("bio"));
        Assert.Contains(therapist.GetProperty("specialties").EnumerateArray(), s => s.Id() == specialtyId);
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
    public async Task Service_is_created_with_category_benefit_and_price()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var categoryName = Http.Unique("Cat");
        var categoryId = await client.CreateAndGetId("/api/categories", new { name = categoryName });
        var benefitTitle = Http.Unique("Benefit");
        var benefitId = await client.CreateAndGetId("/api/benefits", new { title = benefitTitle });
        var name = Http.Unique("Service");

        var response = await client.PostAsync("/api/services", Http.Form(
            ("Name", name), ("Description", "Desc"), ("Price", "150"), ("Currency", "BRL"),
            ("category", JsonSerializer.Serialize(new { id = categoryId, name = categoryName })),
            ("benefits", JsonArray(new { id = benefitId, title = benefitTitle }))));

        var id = (await response.Json()).Id();
        var service = await api.CreateClient().GetJson($"/api/services/{id}");
        Assert.Equal(name, service.Str("name"));
        Assert.Equal(150m, service.GetProperty("price").GetDecimal());
        Assert.Equal("BRL", service.Str("currency"));
        Assert.Equal(categoryId, service.GetProperty("category").Id());
        Assert.Contains(service.GetProperty("benefits").EnumerateArray(), b => b.Id() == benefitId);

        var byCategory = await api.CreateClient().GetJson($"/api/services/category/{Uri.EscapeDataString(categoryName)}");
        Assert.Contains(byCategory.EnumerateArray(), s => s.Id() == id);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/services/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.CreateClient().GetAsync($"/api/services/{id}")).StatusCode);
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
            ("AuthorName", author), ("AuthorAvatarUrl", "https://example.com/a.jpg"), ("Rating", "4"), ("Content", "Great")));

        var id = (await response.Json()).Id();
        var testimonial = await api.CreateClient().GetJson($"/api/testimonials/{id}");
        Assert.Equal(author, testimonial.Str("authorName"));
        Assert.Equal(4, testimonial.GetProperty("rating").GetInt32());
        Assert.Equal("https://example.com/a.jpg", testimonial.Str("authorAvatarUrl"));

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/testimonials/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await api.CreateClient().GetAsync($"/api/testimonials/{id}")).StatusCode);
    }
}
