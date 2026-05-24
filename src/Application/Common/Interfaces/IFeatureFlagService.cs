using CentralPark.Application.Features.FeatureFlags.Queries;

namespace CentralPark.Application.Common.Interfaces;

public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string key, CancellationToken ct = default);
    Task<bool> IsEnabledForUserAsync(string key, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<FeatureFlagDto>> GetAllAsync(CancellationToken ct = default);
    Task SetAsync(string key, bool isEnabled, CancellationToken ct = default);
}
