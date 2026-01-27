using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class AppDbContext :DbContext
    {
        public DbSet<Service> Services => Set<Service>();
        public DbSet<Therapist> Therapists => Set<Therapist>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Benefit> Benefits => Set<Benefit>();
        public DbSet<Specialty> Specialties => Set<Specialty>();
        public DbSet<Testimonial> Testimonials => Set<Testimonial>();
        public DbSet<User> Users => Set<User>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            ConfigureAuditFor<Service>(modelBuilder);
            ConfigureAuditFor<Therapist>(modelBuilder);
            ConfigureAuditFor<Course>(modelBuilder);
            ConfigureAuditFor<Category>(modelBuilder);
            ConfigureAuditFor<Benefit>(modelBuilder);
            ConfigureAuditFor<Specialty>(modelBuilder);
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
}
