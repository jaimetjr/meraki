using API.Tests.Infrastructure;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace API.Tests;

public class AuditTests(ApiFactory api)
{
    [Fact]
    public async Task Creating_content_records_the_author_and_an_audit_log_row()
    {
        var (adminId, email) = await api.CreateUserAsync(UserRole.Admin);
        var client = api.ClientWithToken(await api.LoginAsync(email, ApiFactory.Password));

        var categoryId = await client.CreateAndGetId("/api/categories", new { name = Http.Unique("Audited") });

        await using var db = api.CreateDbContext();
        var category = await db.Categories.SingleAsync(c => c.Id == categoryId);
        Assert.Equal(adminId, category.CreatedBy);
        var log = await db.AuditLogs.SingleAsync(a => a.EntityId == categoryId);
        Assert.Equal("Category", log.EntityType);
        Assert.Equal("Create", log.Action);
        Assert.Equal(adminId, log.UserId);
    }

    [Fact]
    public async Task Updating_content_records_who_updated_it()
    {
        var (adminId, email) = await api.CreateUserAsync(UserRole.Admin);
        var client = api.ClientWithToken(await api.LoginAsync(email, ApiFactory.Password));
        var other = await api.ClientAsAsync(UserRole.SuperAdmin);
        var categoryId = await other.CreateAndGetId("/api/categories", new { name = Http.Unique("Audited") });

        await client.PutAsync($"/api/categories/{categoryId}",
            System.Net.Http.Json.JsonContent.Create(new { name = Http.Unique("Renamed") }));

        await using var db = api.CreateDbContext();
        var category = await db.Categories.SingleAsync(c => c.Id == categoryId);
        Assert.Equal(adminId, category.UpdatedBy);
        Assert.Contains(await db.AuditLogs.Where(a => a.EntityId == categoryId).ToListAsync(),
            a => a.Action == "Update" && a.UserId == adminId);
    }
}
