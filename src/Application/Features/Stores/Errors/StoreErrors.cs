using CentralPark.Shared;

namespace CentralPark.Application.Features.Stores.Errors;

public static class StoreErrors
{
    public static readonly Error NotFound =
        new("Store.NotFound", "The store was not found.");

    public static readonly Error NotAcceptingOrders =
        new("Store.NotAcceptingOrders", "This store is not currently accepting orders.");
}
