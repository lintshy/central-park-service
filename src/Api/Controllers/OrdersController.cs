using System.Security.Claims;
using CentralPark.Api.Extensions;
using CentralPark.Application.Features.Orders.Commands.PlaceOrder;
using CentralPark.Application.Features.Orders.Commands.UpdateOrderStatus;
using CentralPark.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralPark.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/orders")]
[Produces("application/json")]
[Authorize(Policy = Policies.AuthenticatedUser)]
public sealed class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Place(PlaceOrderRequest body, CancellationToken ct)
    {
        var requesterId = GetRequesterId();
        var cmd = new PlaceOrderCommand(
            requesterId,
            body.StoreId,
            body.Notes,
            body.Items.Select(i => new OrderItemRequest(i.ProductName, i.Quantity, i.UnitPrice)).ToList());

        var result = await mediator.Send(cmd, ct);
        if (!result.IsSuccess) return result.ToActionResult();
        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPut("{orderId:guid}")]
    public async Task<IActionResult> UpdateStatus(
        Guid orderId,
        [FromBody] UpdateOrderStatusRequest body,
        CancellationToken ct)
    {
        var requesterId = GetRequesterId();
        return (await mediator.Send(new UpdateOrderStatusCommand(orderId, body.Status, requesterId), ct))
            .ToActionResult();
    }

    private Guid GetRequesterId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }
}

public sealed record PlaceOrderRequest(Guid StoreId, string? Notes, IReadOnlyList<OrderItemDto> Items);
public sealed record OrderItemDto(string ProductName, int Quantity, decimal UnitPrice);
public sealed record UpdateOrderStatusRequest(string Status);
