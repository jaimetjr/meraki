using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration.EntityConfigurations
{
    public class TestimonialConfig : IEntityTypeConfiguration<Testimonial>
    {
        public void Configure(EntityTypeBuilder<Testimonial> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.AuthorName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.AuthorAvatarUrl)
                .IsRequired();

            builder.Property(t => t.AuthorBadge)
                .HasMaxLength(50);

            builder.Property(t => t.Rating)
                .IsRequired();

            builder.Property(t => t.Content)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(t => t.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
        }
    }
}

