using CentralPark.Application.Common.Interfaces;
using CentralPark.Application.Features.Stores.Errors;
using CentralPark.Domain.Entities;
using CentralPark.Shared;
using MediatR;

namespace CentralPark.Application.Features.StoreViews.Commands.RecordStoreView;

public sealed class RecordStoreViewCommandHandler(IUnitOfWork uow, IStoreRepository storeRepository)
    : IRequestHandler<RecordStoreViewCommand, Result>
{
    public async Task<Result> Handle(RecordStoreViewCommand cmd, CancellationToken ct)
    {
        if (!await storeRepository.ExistsAsync(cmd.StoreId, ct))
            return Result.Failure(StoreErrors.NotFound);

        var view = StoreView.Create(cmd.StoreId, cmd.UserId, cmd.Source);
        await uow.Repository<StoreView>().AddAsync(view, ct);
        return Result.Success();
    }
}
