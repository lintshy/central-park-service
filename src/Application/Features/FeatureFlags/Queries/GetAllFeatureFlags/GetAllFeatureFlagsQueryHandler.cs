using CentralPark.Application.Common.Interfaces;
using CentralPark.Shared;
using MediatR;

namespace CentralPark.Application.Features.FeatureFlags.Queries.GetAllFeatureFlags;

public sealed class GetAllFeatureFlagsQueryHandler(IFeatureFlagService featureFlagService)
    : IRequestHandler<GetAllFeatureFlagsQuery, Result<IReadOnlyList<FeatureFlagDto>>>
{
    public async Task<Result<IReadOnlyList<FeatureFlagDto>>> Handle(
        GetAllFeatureFlagsQuery query, CancellationToken ct)
    {
        var flags = await featureFlagService.GetAllAsync(ct);
        return Result<IReadOnlyList<FeatureFlagDto>>.Success(flags);
    }
}
