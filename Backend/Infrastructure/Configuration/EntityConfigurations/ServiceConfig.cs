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
                .HasMaxLength(100);

            builder.Property(s => s.Description)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(s => s.Image);

            builder.Property(s => s.LongDescription)
                .HasMaxLength(1000);

            builder.Property(s => s.Duration)
                .HasMaxLength(50);

            builder.OwnsOne(s => s.Price, price =>
            {
                price.Property(p => p.Amount).HasColumnName("PriceAmount").IsRequired();
                price.Property(p => p.Currency).HasColumnName("PriceCurrency").IsRequired();
            });

            builder.HasOne(s => s.Category)
                .WithMany()
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.Benefits)
                    .WithMany(b => b.Services)
                    .UsingEntity<Dictionary<string, object>>(
                        "ServiceBenefit",
                        j => j.HasOne<Benefit>().WithMany().HasForeignKey("BenefitId"),
                        j => j.HasOne<Service>().WithMany().HasForeignKey("ServiceId"),
                        j =>
                        {
                            j.HasKey("ServiceId", "BenefitId");
                            j.ToTable("ServiceBenefit");
                        });

            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

        }
    }

}
