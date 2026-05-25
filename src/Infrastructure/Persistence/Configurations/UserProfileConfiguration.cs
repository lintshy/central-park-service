using CentralPark.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentralPark.Infrastructure.Persistence.Configurations;

public sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");
        builder.HasKey(u => u.Id);
        builder.HasIndex(u => u.UserId).IsUnique();
        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.RowVersion).IsRowVersion();

        builder.HasOne(u => u.HomeSuburb)
            .WithMany()
            .HasForeignKey(u => u.HomeSuburbId)
            .OnDelete(DeleteBehavior.SetNull);

        // FK to AspNetUsers — no navigation property on ApplicationUser side
        builder.HasOne<Infrastructure.Identity.ApplicationUser>()
            .WithOne()
            .HasForeignKey<UserProfile>(u => u.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
