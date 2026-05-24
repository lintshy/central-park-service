using CentralPark.Application.Common.Markers;

namespace CentralPark.Application.Features.FeatureFlags.Queries.GetFeatureFlagByKey;

public sealed record GetFeatureFlagByKeyQuery(string Key) : IQuery<FeatureFlagDto>;
