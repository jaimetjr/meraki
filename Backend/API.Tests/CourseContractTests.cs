using System.Net;
using API.Tests.Infrastructure;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace API.Tests;

/// <summary>What the API returns for a course can be sent straight back; started courses stay editable.</summary>
public class CourseContractTests(ApiFactory api)
{
    private static MultipartFormDataContent CourseForm(string title, string status, string type, string modality, string startDate) => Http.Form(
        ("Title", title), ("Description", "Desc"), ("Instructor", "Ana"), ("Type", type), ("Status", status),
        ("StartDate", startDate), ("Modality", modality), ("Price", "10"), ("Currency", "BRL"));

    [Fact]
    public async Task Course_enums_are_returned_as_identifiers()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var id = (await (await client.PostAsync("/api/courses",
            CourseForm(Http.Unique("C"), "InscricoesAbertas", "Mentoria", "Hibrido", "2027-05-01T10:00:00Z"))).Json()).Id();

        var course = await api.CreateClient().GetJson($"/api/courses/{id}");

        Assert.Equal("InscricoesAbertas", course.Str("status"));
        Assert.Equal("Mentoria", course.Str("type"));
        Assert.Equal("Hibrido", course.Str("modality"));
    }

    [Fact]
    public async Task A_course_read_from_the_api_can_be_saved_back_unchanged()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var id = (await (await client.PostAsync("/api/courses",
            CourseForm(Http.Unique("C"), "EmBreve", "Curso", "Online", "2027-05-01T10:00:00Z"))).Json()).Id();
        var course = await api.CreateClient().GetJson($"/api/courses/{id}");

        var response = await client.PutAsync($"/api/courses/{id}", CourseForm(
            course.Str("title"), course.Str("status"), course.Str("type"), course.Str("modality"), "2027-05-01T10:00:00Z"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task A_course_that_already_started_can_still_be_edited()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var id = (await (await client.PostAsync("/api/courses",
            CourseForm(Http.Unique("C"), "Disponivel", "Curso", "Online", "2027-05-01T10:00:00Z"))).Json()).Id();
        await using (var db = api.CreateDbContext())
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE \"Courses\" SET \"StartDate\" = {new DateTime(2020, 1, 1, 10, 0, 0, DateTimeKind.Utc)} WHERE \"Id\" = {id}");
        }
        var newTitle = Http.Unique("Renamed");

        var response = await client.PutAsync($"/api/courses/{id}",
            CourseForm(newTitle, "Disponivel", "Curso", "Online", "2020-01-01T10:00:00Z"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(newTitle, (await api.CreateClient().GetJson($"/api/courses/{id}")).Str("title"));
    }

    [Fact]
    public async Task Moving_a_course_start_date_into_the_past_is_rejected()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var id = (await (await client.PostAsync("/api/courses",
            CourseForm(Http.Unique("C"), "Disponivel", "Curso", "Online", "2027-05-01T10:00:00Z"))).Json()).Id();

        var response = await client.PutAsync($"/api/courses/{id}",
            CourseForm(Http.Unique("C"), "Disponivel", "Curso", "Online", "2020-01-01T10:00:00Z"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
