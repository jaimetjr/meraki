using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration.EntityConfigurations
{
    public class SpecialtyConfig : IEntityTypeConfiguration<Specialty>
    {
        public void Configure(EntityTypeBuilder<Specialty> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(Specialty.NameMaxLength);

            builder.Property(s => s.Description)
                .HasMaxLength(Specialty.DescriptionMaxLength);

            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

        }
    }

}
