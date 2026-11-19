using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration.EntityConfigurations
{
    public class TherapistConfig : IEntityTypeConfiguration<Therapist>
    {
        public void Configure(EntityTypeBuilder<Therapist> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(Therapist.NameMaxLength);

            builder.Property(t => t.Bio)
                .IsRequired()
                .HasMaxLength(Therapist.BioMaxLength);

            builder.Property(t => t.Image);

            builder.Property(t => t.Experience)
                .IsRequired();

            builder.Property(t => t.Education)
                .IsRequired();

            builder.Property(t => t.ProId)
                .HasMaxLength(Therapist.ProIdMaxLength);

            builder.HasMany(t => t.Specialties)
                .WithMany()
                .UsingEntity(j => j.ToTable("SpecialtyTherapist"));

            builder.Property(t => t.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

        }
    }

}
