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
                .HasMaxLength(Testimonial.AuthorNameMaxLength);

            builder.Property(t => t.AuthorAvatarUrl)
                .IsRequired();

            builder.Property(t => t.AuthorBadge)
                .HasMaxLength(Testimonial.AuthorBadgeMaxLength);

            builder.Property(t => t.Rating)
                .IsRequired();

            builder.Property(t => t.Content)
                .IsRequired()
                .HasMaxLength(Testimonial.ContentMaxLength);

            builder.Property(t => t.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
        }
    }
}

