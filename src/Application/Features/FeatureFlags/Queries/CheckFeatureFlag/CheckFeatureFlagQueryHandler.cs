using CentralPark.Application.Common.Interfaces;
using CentralPark.Shared;
using MediatR;

namespace CentralPark.Application.Features.FeatureFlags.Queries.CheckFeatureFlag;

public sealed class CheckFeatureFlagQueryHandler(IFeatureFlagService featureFlagService)
    : IRequestHandler<CheckFeatureFlagQuery, Result<bool>>
{
    public async Task<Result<bool>> Handle(CheckFeatureFlagQuery query, CancellationToken ct)
    {
        var enabled = query.UserId.HasValue
            ? await featureFlagService.IsEnabledForUserAsync(query.Key, query.UserId.Value, ct)
            : await featureFlagService.IsEnabledAsync(query.Key, ct);

        return Result<bool>.Success(enabled);
    }
}
