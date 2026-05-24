using CentralPark.Application.Common.Interfaces;
using CentralPark.Application.Features.FeatureFlags.Errors;
using CentralPark.Domain.Entities;
using CentralPark.Shared;
using MediatR;

namespace CentralPark.Application.Features.FeatureFlags.Commands.UpdateFeatureFlag;

public sealed class UpdateFeatureFlagCommandHandler(IUnitOfWork uow)
    : IRequestHandler<UpdateFeatureFlagCommand, Result>
{
    public async Task<Result> Handle(UpdateFeatureFlagCommand cmd, CancellationToken ct)
    {
        var flags = await uow.Repository<FeatureFlag>()
            .FindAsync(f => f.Key == cmd.Key.ToLowerInvariant(), ct);

        var flag = flags.FirstOrDefault();
        if (flag is null)
            return Result.Failure(FeatureFlagErrors.NotFound(cmd.Key));

        flag.SetEnabled(cmd.IsEnabled);
        flag.UpdateDescription(cmd.Description);
        uow.Repository<FeatureFlag>().Update(flag);

        return Result.Success();
    }
}
