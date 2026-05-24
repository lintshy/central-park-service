using CentralPark.Application.Common.Interfaces;
using CentralPark.Domain.Entities;
using CentralPark.Shared;
using MediatR;

namespace CentralPark.Application.Features.FeatureFlags.Commands.CreateFeatureFlag;

public sealed class CreateFeatureFlagCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CreateFeatureFlagCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateFeatureFlagCommand cmd, CancellationToken ct)
    {
        var flag = FeatureFlag.Create(cmd.Key, cmd.IsEnabled, cmd.Description);
        await uow.Repository<FeatureFlag>().AddAsync(flag, ct);
        return Result<Guid>.Success(flag.Id);
    }
}
