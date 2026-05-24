using CentralPark.Shared;

namespace CentralPark.Application.Features.FeatureFlags.Errors;

public static class FeatureFlagErrors
{
    public static Error NotFound(string key) =>
        new("FeatureFlag.NotFound", $"Feature flag '{key}' was not found.");

    public static Error FeatureDisabled(string key) =>
        new("FeatureFlag.Disabled", $"Feature '{key}' is currently disabled.");
}
