using CentralPark.Application.Common.Markers;

namespace CentralPark.Application.Features.Orders.Commands.PlaceOrder;

public sealed record PlaceOrderCommand(
    Guid RequesterId,
    Guid StoreId,
    string? Notes,
    IReadOnlyList<OrderItemRequest> Items) : ICommand<PlaceOrderResponse>;

public sealed record PlaceOrderResponse(Guid OrderId, string Status, decimal TotalAmount, DateTime PlacedAt);
