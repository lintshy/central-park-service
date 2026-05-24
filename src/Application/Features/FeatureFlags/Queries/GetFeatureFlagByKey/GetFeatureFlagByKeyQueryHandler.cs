using CentralPark.Application.Common.Interfaces;
using CentralPark.Application.Features.FeatureFlags.Errors;
using CentralPark.Domain.Entities;
using CentralPark.Shared;
using MediatR;

namespace CentralPark.Application.Features.FeatureFlags.Queries.GetFeatureFlagByKey;

public sealed class GetFeatureFlagByKeyQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetFeatureFlagByKeyQuery, Result<FeatureFlagDto>>
{
    public async Task<Result<FeatureFlagDto>> Handle(GetFeatureFlagByKeyQuery query, CancellationToken ct)
    {
        var flags = await uow.Repository<FeatureFlag>()
            .FindAsync(f => f.Key == query.Key.ToLowerInvariant(), ct);

        var flag = flags.FirstOrDefault();
        if (flag is null)
            return Result<FeatureFlagDto>.Failure(FeatureFlagErrors.NotFound(query.Key));

        return Result<FeatureFlagDto>.Success(new FeatureFlagDto(
            flag.Id, flag.Key, flag.IsEnabled, flag.Description, flag.RolloutOverrides));
    }
}
