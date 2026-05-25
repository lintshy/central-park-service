using CentralPark.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentralPark.Infrastructure.Persistence.Configurations;

public sealed class SuburbConfiguration : IEntityTypeConfiguration<Suburb>
{
    public void Configure(EntityTypeBuilder<Suburb> builder)
    {
        builder.ToTable("Suburbs");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
        builder.Property(s => s.PostCode).HasMaxLength(10).IsRequired();
        builder.Property(s => s.State).HasMaxLength(10).IsRequired();
        builder.Property(s => s.Country).HasMaxLength(5).IsRequired().HasDefaultValue("AU");
        builder.Property(s => s.CentroidLat).HasPrecision(9, 6);
        builder.Property(s => s.CentroidLng).HasPrecision(9, 6);
        builder.Property(s => s.GeoBoundaries).HasColumnType("nvarchar(max)");
        builder.Property(s => s.RowVersion).IsRowVersion();
        builder.HasIndex(s => s.PostCode);
        builder.HasIndex(s => new { s.Name, s.State });
    }
}
