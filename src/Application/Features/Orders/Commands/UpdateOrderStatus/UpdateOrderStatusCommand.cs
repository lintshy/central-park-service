using CentralPark.Application.Common.Markers;

namespace CentralPark.Application.Features.Orders.Commands.UpdateOrderStatus;

public sealed record UpdateOrderStatusCommand(
    Guid OrderId,
    string NewStatus,
    Guid RequesterId) : ICommand<UpdateOrderStatusResponse>;

public sealed record UpdateOrderStatusResponse(Guid OrderId, string Status, DateTime UpdatedAt);
