using CentralPark.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentralPark.Infrastructure.Persistence.Configurations;

public sealed class StoreViewConfiguration : IEntityTypeConfiguration<StoreView>
{
    public void Configure(EntityTypeBuilder<StoreView> builder)
    {
        builder.ToTable("StoreViews");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Source).HasMaxLength(50);
        // append-only — no RowVersion needed
        builder.Ignore(v => v.RowVersion);

        builder.HasOne<Infrastructure.Identity.ApplicationUser>()
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Store>()
            .WithMany()
            .HasForeignKey(v => v.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => v.StoreId);
        builder.HasIndex(v => v.UserId);
        builder.HasIndex(v => v.CreatedAt);
    }
}
