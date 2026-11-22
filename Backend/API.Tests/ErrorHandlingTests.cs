using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.Tests.Infrastructure;
using Domain.Enums;

namespace API.Tests;

/// <summary>User mistakes come back as 400/409 ProblemDetails; only real failures are 500, and those never leak internals.</summary>
public class ErrorHandlingTests(ApiFactory api)
{
    private const string ProblemJson = "application/problem+json";

    private static async Task<JsonElement> AssertProblem(HttpResponseMessage response, HttpStatusCode status)
    {
        var text = await response.Content.ReadAsStringAsync();
        Assert.True(status == response.StatusCode, $"Expected {(int)status} but got {(int)response.StatusCode}: {text}");
        Assert.Equal(ProblemJson, response.Content.Headers.ContentType?.MediaType);
        return JsonDocument.Parse(text).RootElement.Clone();
    }

    private static IEnumerable<string> ErrorKeys(JsonElement problem) =>
        problem.GetProperty("errors").EnumerateObject().Select(p => p.Name.ToLowerInvariant());

    private async Task<(Guid Id, string Name)> NewCategory(HttpClient client)
    {
        var name = Http.Unique("Cat");
        return (await client.CreateAndGetId("/api/categories", new { name }), name);
    }

    [Fact]
    public async Task Validation_errors_return_problem_details_keyed_by_field()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);

        var response = await client.PostAsJsonAsync("/api/categories", new { name = "" });

        var problem = await AssertProblem(response, HttpStatusCode.BadRequest);
        Assert.Contains("name", ErrorKeys(problem));
    }

    [Fact]
    public async Task Validation_errors_explain_the_problem_in_portuguese_using_the_form_field_name()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);

        var response = await client.PostAsJsonAsync("/api/categories", new { name = "" });

        // The admin shows "detail" as it is, so it must be a sentence the clinic staff understands.
        var problem = await AssertProblem(response, HttpStatusCode.BadRequest);
        Assert.Equal("'Nome' deve ser informado.", problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Text_longer_than_its_database_column_is_a_400_not_a_crash()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var (categoryId, categoryName) = await NewCategory(client);

        var response = await client.PostAsync("/api/services", Http.Form(
            ("Name", Http.Unique("Svc")), ("Description", "Desc"), ("Price", "10"), ("Currency", "BRL"),
            ("LongDescription", new string('x', 1001)),
            ("category", JsonSerializer.Serialize(new { id = categoryId, name = categoryName }))));

        var problem = await AssertProblem(response, HttpStatusCode.BadRequest);
        Assert.Contains("longdescription", ErrorKeys(problem));
    }

    [Fact]
    public async Task Domain_rule_violation_returns_400_with_the_rule_message()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);

        var response = await client.PostAsync("/api/courses", Http.Form(
            ("Title", Http.Unique("Course")), ("Description", "Desc"), ("Instructor", "Ana"),
            ("Type", "Curso"), ("Status", "Disponivel"),
            ("StartDate", "2020-01-01T10:00:00Z"), ("Modality", "Online"), ("Price", "10"), ("Currency", "BRL")));

        var problem = await AssertProblem(response, HttpStatusCode.BadRequest);
        Assert.Equal("A data de início não pode estar no passado.", problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Unexpected_errors_return_a_generic_500_without_internal_details()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);

        // ConsultorioME credentials are not configured in tests, so resolving the client fails.
        var response = await client.GetAsync("/api/consultoriome/professionals");

        var problem = await AssertProblem(response, HttpStatusCode.InternalServerError);
        Assert.DoesNotContain("ClientId", problem.GetRawText());
    }

    [Fact]
    public async Task Decimal_form_values_are_parsed_independently_of_server_culture()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var (categoryId, categoryName) = await NewCategory(client);

        var response = await client.PostAsync("/api/services", Http.Form(
            ("Name", Http.Unique("Svc")), ("Description", "Desc"), ("Price", "150.5"), ("Currency", "BRL"),
            ("category", JsonSerializer.Serialize(new { id = categoryId, name = categoryName }))));

        var id = (await response.Json()).Id();
        var service = await api.CreateClient().GetJson($"/api/services/{id}");
        Assert.Equal(150.5m, service.GetProperty("price").GetDecimal());
    }

    [Fact]
    public async Task Malformed_json_in_a_form_field_is_a_400_on_that_field()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);

        var response = await client.PostAsync("/api/services", Http.Form(
            ("Name", Http.Unique("Svc")), ("Description", "Desc"), ("Price", "10"), ("Currency", "BRL"),
            ("category", "not-json")));

        var problem = await AssertProblem(response, HttpStatusCode.BadRequest);
        Assert.Contains("category", ErrorKeys(problem));
    }

    [Fact]
    public async Task Service_with_an_unknown_category_is_a_400()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);

        var response = await client.PostAsync("/api/services", Http.Form(
            ("Name", Http.Unique("Svc")), ("Description", "Desc"), ("Price", "10"), ("Currency", "BRL"),
            ("category", JsonSerializer.Serialize(new { id = Guid.NewGuid(), name = "Ghost" }))));

        await AssertProblem(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Unknown_course_status_filter_is_a_400()
    {
        var response = await api.CreateClient().GetAsync("/api/courses/status/Bogus");

        await AssertProblem(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Course_enum_values_are_accepted_in_any_case()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);

        var response = await client.PostAsync("/api/courses", Http.Form(
            ("Title", Http.Unique("Course")), ("Description", "Desc"), ("Instructor", "Ana"),
            ("Type", "curso"), ("Status", "disponivel"),
            ("StartDate", "2027-03-01T10:00:00Z"), ("Modality", "online"), ("Price", "10"), ("Currency", "BRL")));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task User_with_an_empty_password_is_rejected()
    {
        var client = await api.ClientAsAsync(UserRole.SuperAdmin);

        var response = await client.PostAsJsonAsync("/api/users",
            new { email = $"x-{Guid.NewGuid():N}@test.local", password = "", firstName = "Ana", role = "Admin" });

        var problem = await AssertProblem(response, HttpStatusCode.BadRequest);
        Assert.Contains("password", ErrorKeys(problem));
    }

    [Fact]
    public async Task User_with_an_invalid_email_is_rejected()
    {
        var client = await api.ClientAsAsync(UserRole.SuperAdmin);

        var response = await client.PostAsJsonAsync("/api/users",
            new { email = "not-an-email", password = "Valid-Passw0rd!", firstName = "Ana", role = "Admin" });

        var problem = await AssertProblem(response, HttpStatusCode.BadRequest);
        Assert.Contains("email", ErrorKeys(problem));
    }

    [Fact]
    public async Task Wrong_login_returns_a_401_problem()
    {
        var response = await api.CreateClient().PostAsJsonAsync("/api/auth/login",
            new { email = "nobody@test.local", password = "whatever-123" });

        var problem = await AssertProblem(response, HttpStatusCode.Unauthorized);
        Assert.Equal("E-mail ou senha inválidos.", problem.GetProperty("detail").GetString());
    }
}
