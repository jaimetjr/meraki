using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration.EntityConfigurations
{
    public class CourseConfig : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(c => c.Image);

            builder.Property(c => c.Instructor)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Type)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(c => c.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(c => c.Modality)
                .HasConversion<string>();

            // Ensure StartDate is always UTC for PostgreSQL compatibility
            builder.Property(c => c.StartDate)
                .HasConversion(
                    v => v.HasValue && v.Value.Kind != DateTimeKind.Utc 
                        ? (DateTime?)DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
                        : v,
                    v => v);

            // Ensure EndDate is always UTC for PostgreSQL compatibility
            builder.Property(c => c.EndDate)
                .HasConversion(
                    v => v.HasValue && v.Value.Kind != DateTimeKind.Utc 
                        ? (DateTime?)DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
                        : v,
                    v => v);

            builder.Property(c => c.Link)
                .HasMaxLength(300);

            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.OwnsOne(c => c.Price, price =>
            {
                price.Property(p => p.Amount).HasColumnName("PriceAmount").IsRequired();
                price.Property(p => p.Currency).HasColumnName("PriceCurrency").IsRequired();
            });

        }
    }

}
