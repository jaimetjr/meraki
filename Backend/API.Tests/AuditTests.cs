using API.Tests.Infrastructure;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace API.Tests;

public class AuditTests(ApiFactory api)
{
    private static MultipartFormDataContent ServiceForm(string name) => Http.Form(("Name", name), ("Description", "Desc"));

    [Fact]
    public async Task Creating_content_records_the_author_and_an_audit_log_row()
    {
        var (adminId, email) = await api.CreateUserAsync(UserRole.Admin);
        var client = api.ClientWithToken(await api.LoginAsync(email, ApiFactory.Password));

        var serviceId = (await (await client.PostAsync("/api/services", ServiceForm(Http.Unique("Audited")))).Json()).Id();

        await using var db = api.CreateDbContext();
        var service = await db.Services.SingleAsync(s => s.Id == serviceId);
        Assert.Equal(adminId, service.CreatedBy);
        var log = await db.AuditLogs.SingleAsync(a => a.EntityId == serviceId);
        Assert.Equal("Service", log.EntityType);
        Assert.Equal("Create", log.Action);
        Assert.Equal(adminId, log.UserId);
    }

    [Fact]
    public async Task Updating_content_records_who_updated_it()
    {
        var (adminId, email) = await api.CreateUserAsync(UserRole.Admin);
        var client = api.ClientWithToken(await api.LoginAsync(email, ApiFactory.Password));
        var other = await api.ClientAsAsync(UserRole.SuperAdmin);
        var serviceId = (await (await other.PostAsync("/api/services", ServiceForm(Http.Unique("Audited")))).Json()).Id();

        await client.PutAsync($"/api/services/{serviceId}", ServiceForm(Http.Unique("Renamed")));

        await using var db = api.CreateDbContext();
        var service = await db.Services.SingleAsync(s => s.Id == serviceId);
        Assert.Equal(adminId, service.UpdatedBy);
        Assert.Contains(await db.AuditLogs.Where(a => a.EntityId == serviceId).ToListAsync(),
            a => a.Action == "Update" && a.UserId == adminId);
    }
}
