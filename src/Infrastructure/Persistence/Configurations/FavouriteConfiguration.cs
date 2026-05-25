using CentralPark.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentralPark.Infrastructure.Persistence.Configurations;

public sealed class FavouriteConfiguration : IEntityTypeConfiguration<Favourite>
{
    public void Configure(EntityTypeBuilder<Favourite> builder)
    {
        builder.ToTable("Favourites");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.RowVersion).IsRowVersion();
        builder.HasIndex(f => new { f.CustomerProfileId, f.StoreId }).IsUnique();

        builder.HasOne(f => f.Store)
            .WithMany()
            .HasForeignKey(f => f.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
