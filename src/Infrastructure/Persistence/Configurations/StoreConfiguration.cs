using CentralPark.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentralPark.Infrastructure.Persistence.Configurations;

public sealed class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("Stores");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(1000);
        builder.Property(s => s.StreetAddress).HasMaxLength(300).IsRequired();
        builder.Property(s => s.PostCode).HasMaxLength(10).IsRequired();
        builder.Property(s => s.Latitude).HasPrecision(9, 6);
        builder.Property(s => s.Longitude).HasPrecision(9, 6);
        builder.Property(s => s.OwnerDisplayName).HasMaxLength(200).IsRequired();
        builder.Property(s => s.OwnerContactEmail).HasMaxLength(256);
        builder.Property(s => s.OwnerContactPhone).HasMaxLength(20);
        builder.Property(s => s.IsActive).HasDefaultValue(true);
        builder.Property(s => s.RowVersion).IsRowVersion();

        builder.HasOne(s => s.Suburb)
            .WithMany()
            .HasForeignKey(s => s.SuburbId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Hours)
            .WithOne()
            .HasForeignKey(h => h.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.SuburbId);
        builder.HasIndex(s => s.IsActive);
    }
}
