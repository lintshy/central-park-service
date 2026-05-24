using CentralPark.Application.Common.Markers;

namespace CentralPark.Application.Features.FeatureFlags.Commands.UpdateFeatureFlag;

public sealed record UpdateFeatureFlagCommand(string Key, bool IsEnabled, string? Description) : ICommand;
