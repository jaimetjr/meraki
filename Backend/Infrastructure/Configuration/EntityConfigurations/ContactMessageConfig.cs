using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration.EntityConfigurations;

public class ContactMessageConfig : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(ContactMessage.NameMaxLength);
        builder.Property(m => m.Email).IsRequired().HasMaxLength(ContactMessage.EmailMaxLength);
        builder.Property(m => m.Phone).IsRequired().HasMaxLength(ContactMessage.PhoneMaxLength);
        builder.Property(m => m.ServiceName).IsRequired().HasMaxLength(ContactMessage.ServiceNameMaxLength);
        builder.Property(m => m.Message).HasMaxLength(ContactMessage.MessageMaxLength);
        builder.Property(m => m.ConsentGivenAt).IsRequired();
        builder.HasIndex(m => m.CreatedAt);
    }
}
