using CentralPark.Application.Common.Interfaces;
using CentralPark.Application.Features.FeatureFlags.Queries;
using CentralPark.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CentralPark.Infrastructure.Persistence;

namespace CentralPark.Infrastructure.FeatureFlags;

public sealed class FeatureFlagService(ApplicationDbContext context) : IFeatureFlagService
{
    public async Task<bool> IsEnabledAsync(string key, CancellationToken ct = default)
    {
        var flag = await context.FeatureFlags
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Key == key.ToLowerInvariant(), ct);

        return flag?.IsEnabled ?? false;
    }

    public async Task<bool> IsEnabledForUserAsync(string key, Guid userId, CancellationToken ct = default)
    {
        var flag = await context.FeatureFlags
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Key == key.ToLowerInvariant(), ct);

        if (flag is null) return false;

        var userIdStr = userId.ToString();
        if (flag.RolloutOverrides.TryGetValue(userIdStr, out var overrideValue))
            return overrideValue;

        return flag.IsEnabled;
    }

    public async Task<IReadOnlyList<FeatureFlagDto>> GetAllAsync(CancellationToken ct = default)
    {
        var flags = await context.FeatureFlags.AsNoTracking().ToListAsync(ct);
        return flags.Select(f => new FeatureFlagDto(
            f.Id, f.Key, f.IsEnabled, f.Description, f.RolloutOverrides)).ToList().AsReadOnly();
    }

    public async Task SetAsync(string key, bool isEnabled, CancellationToken ct = default)
    {
        var flag = await context.FeatureFlags
            .FirstOrDefaultAsync(f => f.Key == key.ToLowerInvariant(), ct);

        if (flag is null)
        {
            var newFlag = FeatureFlag.Create(key, isEnabled);
            context.FeatureFlags.Add(newFlag);
        }
        else
        {
            flag.SetEnabled(isEnabled);
        }

        await context.SaveChangesAsync(ct);
    }
}
