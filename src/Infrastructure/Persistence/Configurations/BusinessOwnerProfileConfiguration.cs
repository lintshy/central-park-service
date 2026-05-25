using CentralPark.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentralPark.Infrastructure.Persistence.Configurations;

public sealed class BusinessOwnerProfileConfiguration : IEntityTypeConfiguration<BusinessOwnerProfile>
{
    public void Configure(EntityTypeBuilder<BusinessOwnerProfile> builder)
    {
        builder.ToTable("BusinessOwnerProfiles");
        builder.HasKey(b => b.Id);
        builder.HasIndex(b => b.UserId).IsUnique();
        builder.Property(b => b.BusinessName).HasMaxLength(200).IsRequired();
        builder.Property(b => b.BusinessEmail).HasMaxLength(256);
        builder.Property(b => b.BusinessPhone).HasMaxLength(20);
        builder.Property(b => b.RowVersion).IsRowVersion();

        builder.HasOne<Infrastructure.Identity.ApplicationUser>()
            .WithOne()
            .HasForeignKey<BusinessOwnerProfile>(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Stores)
            .WithOne()
            .HasForeignKey(s => s.BusinessOwnerProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
