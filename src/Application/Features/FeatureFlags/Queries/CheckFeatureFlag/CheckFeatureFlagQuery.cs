using CentralPark.Application.Common.Markers;

namespace CentralPark.Application.Features.FeatureFlags.Queries.CheckFeatureFlag;

public sealed record CheckFeatureFlagQuery(string Key, Guid? UserId = null) : IQuery<bool>;
