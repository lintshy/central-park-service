namespace CentralPark.Application.Features.Orders.Commands.PlaceOrder;

public sealed record OrderItemRequest(string ProductName, int Quantity, decimal UnitPrice);
