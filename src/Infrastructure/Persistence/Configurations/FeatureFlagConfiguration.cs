using CentralPark.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentralPark.Infrastructure.Persistence.Configurations;

public sealed class FeatureFlagConfiguration : IEntityTypeConfiguration<FeatureFlag>
{
    public void Configure(EntityTypeBuilder<FeatureFlag> builder)
    {
        builder.ToTable("FeatureFlags");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Key)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(f => f.Key).IsUnique();

        builder.Property(f => f.Description)
            .HasMaxLength(500);

        builder.Property(f => f.RolloutOverrides)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(v,
                    (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, bool>());

        builder.Property(f => f.RowVersion)
            .IsRowVersion();
    }
}
