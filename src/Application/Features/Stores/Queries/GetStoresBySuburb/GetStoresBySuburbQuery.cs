using CentralPark.Application.Common.Markers;

namespace CentralPark.Application.Features.Stores.Queries.GetStoresBySuburb;

public sealed record GetStoresBySuburbQuery(Guid SuburbId, bool? AcceptingOrders) : IQuery<IReadOnlyList<StoreDto>>;
