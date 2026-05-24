namespace CentralPark.Application.Features.FeatureFlags.Queries;

public sealed record FeatureFlagDto(
    Guid Id,
    string Key,
    bool IsEnabled,
    string? Description,
    Dictionary<string, bool> RolloutOverrides);
