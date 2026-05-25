using CentralPark.Application.Common.Interfaces;
using CentralPark.Domain.Entities;
using CentralPark.Shared;
using MediatR;

namespace CentralPark.Application.Features.Stores.Queries.GetStoresBySuburb;

public sealed class GetStoresBySuburbQueryHandler(IUnitOfWork uow, IStoreRepository storeRepository)
    : IRequestHandler<GetStoresBySuburbQuery, Result<IReadOnlyList<StoreDto>>>
{
    public async Task<Result<IReadOnlyList<StoreDto>>> Handle(GetStoresBySuburbQuery query, CancellationToken ct)
    {
        var suburb = await uow.Repository<Suburb>().GetByIdAsync(query.SuburbId, ct);
        if (suburb is null)
            return Result<IReadOnlyList<StoreDto>>.Failure(
                new Error("Suburb.NotFound", "The specified suburb was not found."));

        var stores = await storeRepository.GetBySuburbAsync(query.SuburbId, query.AcceptingOrders, ct);
        return Result<IReadOnlyList<StoreDto>>.Success(stores);
    }
}
