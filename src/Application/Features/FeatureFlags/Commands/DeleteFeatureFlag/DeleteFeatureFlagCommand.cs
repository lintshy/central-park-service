using CentralPark.Application.Common.Markers;

namespace CentralPark.Application.Features.FeatureFlags.Commands.DeleteFeatureFlag;

public sealed record DeleteFeatureFlagCommand(string Key) : ICommand;
