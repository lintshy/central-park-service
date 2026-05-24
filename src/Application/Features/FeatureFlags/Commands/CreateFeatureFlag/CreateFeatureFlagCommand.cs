using CentralPark.Application.Common.Markers;

namespace CentralPark.Application.Features.FeatureFlags.Commands.CreateFeatureFlag;

public sealed record CreateFeatureFlagCommand(string Key, bool IsEnabled, string? Description) : ICommand<Guid>;
