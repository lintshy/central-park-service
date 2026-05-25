using CentralPark.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentralPark.Infrastructure.Persistence.Configurations;

public sealed class StoreHoursConfiguration : IEntityTypeConfiguration<StoreHours>
{
    public void Configure(EntityTypeBuilder<StoreHours> builder)
    {
        builder.ToTable("StoreHours");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.DayOfWeek).IsRequired();
        builder.Property(h => h.RowVersion).IsRowVersion();
        builder.HasIndex(h => new { h.StoreId, h.DayOfWeek }).IsUnique();
    }
}
