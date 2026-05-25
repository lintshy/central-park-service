using System.Security.Claims;
using CentralPark.Api.Extensions;
using CentralPark.Application.Features.StoreViews.Commands.RecordStoreView;
using CentralPark.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralPark.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/store-views")]
[Produces("application/json")]
[Authorize(Policy = Policies.AuthenticatedUser)]
public sealed class StoreViewsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Record(RecordStoreViewRequest body, CancellationToken ct)
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = Guid.TryParse(raw, out var id) ? id : Guid.Empty;
        return (await mediator.Send(new RecordStoreViewCommand(body.StoreId, userId, body.Source), ct))
            .ToAcceptedResult();
    }
}

public sealed record RecordStoreViewRequest(Guid StoreId, string? Source);
