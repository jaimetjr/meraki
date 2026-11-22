using System.Net;
using System.Net.Http.Json;
using API.Tests.Infrastructure;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace API.Tests;

public class SeedingTests(ApiFactory api)
{
    private static readonly SuperAdminSeed Owner = new() { Email = "owner@clinic.test", Password = "Owner-Passw0rd!" };

    [Fact]
    public async Task The_superadmin_from_configuration_can_log_in()
    {
        var response = await api.CreateClient().PostAsJsonAsync("/api/auth/login",
            new { email = ApiFactory.SeedSuperAdminEmail, password = ApiFactory.SeedSuperAdminPassword });

        var body = await response.Json();
        Assert.Equal("SuperAdmin", body.Str("role"));
    }

    [Fact]
    public async Task The_old_hard_coded_admin_account_no_longer_exists()
    {
        var response = await api.CreateClient().PostAsJsonAsync("/api/auth/login",
            new { email = "admin@meraki.com", password = "Admin123!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Without_demo_content_only_the_superadmin_is_seeded()
    {
        await using var db = await api.CreateFreshDatabaseAsync();

        await Seeder.SeedAsync(db, Owner, includeDemoContent: false, NullLogger.Instance);

        Assert.Equal(0, await db.Therapists.CountAsync());
        Assert.Equal(0, await db.Services.CountAsync());
        Assert.Equal(0, await db.Courses.CountAsync());
        Assert.Equal(0, await db.Testimonials.CountAsync());
        var user = await db.Users.SingleAsync();
        Assert.Equal((Owner.Email, UserRole.SuperAdmin, true), (user.Email, user.Role, user.IsActive));
    }

    [Fact]
    public async Task Demo_content_is_seeded_without_image_urls()
    {
        await using var db = await api.CreateFreshDatabaseAsync();

        await Seeder.SeedAsync(db, Owner, includeDemoContent: true, NullLogger.Instance);

        Assert.NotEmpty(await db.Therapists.ToListAsync());
        Assert.NotEmpty(await db.Services.ToListAsync());
        Assert.All(await db.Therapists.ToListAsync(), t => Assert.Null(t.Image));
        Assert.All(await db.Services.ToListAsync(), s => Assert.Null(s.Image));
        Assert.All(await db.Courses.ToListAsync(), c => Assert.Null(c.Image));
        Assert.All(await db.Testimonials.ToListAsync(), t => Assert.Null(t.AuthorAvatarUrl));
    }

    [Fact]
    public async Task Seeding_twice_does_not_duplicate_anything()
    {
        await using var db = await api.CreateFreshDatabaseAsync();
        await Seeder.SeedAsync(db, Owner, includeDemoContent: true, NullLogger.Instance);
        var therapists = await db.Therapists.CountAsync();

        await Seeder.SeedAsync(db, Owner, includeDemoContent: true, NullLogger.Instance);

        Assert.Equal(therapists, await db.Therapists.CountAsync());
        Assert.Equal(1, await db.Users.CountAsync());
    }

    [Fact]
    public async Task A_configured_but_inactive_superadmin_is_reactivated_to_recover_from_a_lockout()
    {
        await using var db = await api.CreateFreshDatabaseAsync();
        var locked = new User(Guid.NewGuid(), Owner.Email!, BCrypt.Net.BCrypt.HashPassword("whatever-123"), UserRole.Admin);
        locked.Deactivate();
        db.Users.Add(locked);
        await db.SaveChangesAsync();

        await Seeder.SeedAsync(db, Owner, includeDemoContent: false, NullLogger.Instance);

        db.ChangeTracker.Clear();
        var user = await db.Users.SingleAsync();
        Assert.Equal((UserRole.SuperAdmin, true), (user.Role, user.IsActive));
    }

    [Fact]
    public async Task No_superadmin_is_created_when_an_active_one_already_exists()
    {
        await using var db = await api.CreateFreshDatabaseAsync();
        db.Users.Add(new User(Guid.NewGuid(), "existing@clinic.test", BCrypt.Net.BCrypt.HashPassword("whatever-123"), UserRole.SuperAdmin));
        await db.SaveChangesAsync();

        await Seeder.SeedAsync(db, Owner, includeDemoContent: false, NullLogger.Instance);

        Assert.Equal(["existing@clinic.test"], await db.Users.Select(u => u.Email).ToListAsync());
    }

    [Fact]
    public async Task Without_superadmin_configuration_no_user_is_created()
    {
        await using var db = await api.CreateFreshDatabaseAsync();

        await Seeder.SeedAsync(db, new SuperAdminSeed(), includeDemoContent: false, NullLogger.Instance);

        Assert.Equal(0, await db.Users.CountAsync());
    }
}
