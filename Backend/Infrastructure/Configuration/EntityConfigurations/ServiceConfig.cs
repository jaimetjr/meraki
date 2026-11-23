using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configuration.EntityConfigurations
{
    public class ServiceConfig : IEntityTypeConfiguration<Service>
    {
        public void Configure(EntityTypeBuilder<Service> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(Service.NameMaxLength);

            builder.Property(s => s.Description)
                .IsRequired()
                .HasMaxLength(Service.DescriptionMaxLength);

            builder.Property(s => s.Image);

            builder.Property(s => s.LongDescription)
                .HasMaxLength(Service.LongDescriptionMaxLength);

            builder.Property(s => s.Duration)
                .HasMaxLength(Service.DurationMaxLength);

            // Optional ("sob consulta"): both columns are null when there is no price.
            builder.OwnsOne(s => s.Price, price =>
            {
                price.Property(p => p.Amount).HasColumnName("PriceAmount").IsRequired();
                price.Property(p => p.Currency).HasColumnName("PriceCurrency").IsRequired();
            });

            // A PostgreSQL text[] column, in the order the admin typed them.
            builder.Property(s => s.Benefits)
                .IsRequired();

            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

        }
    }

}
