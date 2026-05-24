using CentralPark.Application.Common.Markers;

namespace CentralPark.Application.Features.FeatureFlags.Queries.GetAllFeatureFlags;

public sealed record GetAllFeatureFlagsQuery : IQuery<IReadOnlyList<FeatureFlagDto>>;
