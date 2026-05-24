using CentralPark.Application.Common.Interfaces;
using CentralPark.Application.Features.FeatureFlags.Errors;
using CentralPark.Domain.Entities;
using CentralPark.Shared;
using MediatR;

namespace CentralPark.Application.Features.FeatureFlags.Commands.DeleteFeatureFlag;

public sealed class DeleteFeatureFlagCommandHandler(IUnitOfWork uow)
    : IRequestHandler<DeleteFeatureFlagCommand, Result>
{
    public async Task<Result> Handle(DeleteFeatureFlagCommand cmd, CancellationToken ct)
    {
        var flags = await uow.Repository<FeatureFlag>()
            .FindAsync(f => f.Key == cmd.Key.ToLowerInvariant(), ct);

        var flag = flags.FirstOrDefault();
        if (flag is null)
            return Result.Failure(FeatureFlagErrors.NotFound(cmd.Key));

        uow.Repository<FeatureFlag>().Remove(flag);
        return Result.Success();
    }
}
