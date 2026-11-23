using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Therapist> Therapists => Set<Therapist>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Soft-deleted content is invisible to every query (including Include and Find-by-id).
        // Users are deliberately not filtered: inactive users stay listed so they can be reactivated.
        modelBuilder.Entity<Service>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<Therapist>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<Course>().HasQueryFilter(e => e.IsActive);
        modelBuilder.Entity<Testimonial>().HasQueryFilter(e => e.IsActive);

        ConfigureAuditFor<Service>(modelBuilder);
        ConfigureAuditFor<Therapist>(modelBuilder);
        ConfigureAuditFor<Course>(modelBuilder);
        ConfigureAuditFor<Testimonial>(modelBuilder);
    }

    private static void ConfigureAuditFor<TEntity>(ModelBuilder modelBuilder) where TEntity : Entity
    {
        var entity = modelBuilder.Entity<TEntity>();

        entity.HasOne<User>()
              .WithMany()
              .HasForeignKey(e => e.CreatedBy)
              .OnDelete(DeleteBehavior.SetNull);

        entity.HasOne<User>()
              .WithMany()
              .HasForeignKey(e => e.UpdatedBy)
              .OnDelete(DeleteBehavior.SetNull);
    }
}
