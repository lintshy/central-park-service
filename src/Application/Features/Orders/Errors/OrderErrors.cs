using CentralPark.Shared;

namespace CentralPark.Application.Features.Orders.Errors;

public static class OrderErrors
{
    public static readonly Error NotFound =
        new("Order.NotFound", "The order was not found.");

    public static readonly Error InvalidTransition =
        new("Order.InvalidTransition", "This status transition is not permitted.");

    public static readonly Error Forbidden =
        new("Auth.Forbidden", "You are not authorised to update this order.");
}
