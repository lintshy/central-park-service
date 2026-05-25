using CentralPark.Application.Features.Stores.Queries.GetStoresBySuburb;

namespace CentralPark.Application.Common.Interfaces;

public interface IStoreRepository
{
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<StoreDto>> GetBySuburbAsync(Guid suburbId, bool? acceptingOrders, CancellationToken ct = default);
}
