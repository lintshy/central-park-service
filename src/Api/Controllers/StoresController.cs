using CentralPark.Api.Extensions;
using CentralPark.Application.Features.Stores.Queries.GetStoresBySuburb;
using CentralPark.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralPark.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/stores")]
[Produces("application/json")]
[Authorize(Policy = Policies.AuthenticatedUser)]
public sealed class StoresController(IMediator mediator) : ControllerBase
{
    [HttpGet("{suburbId:guid}")]
    public async Task<IActionResult> GetBySuburb(
        Guid suburbId,
        [FromQuery] bool? acceptingOrders,
        CancellationToken ct)
        => (await mediator.Send(new GetStoresBySuburbQuery(suburbId, acceptingOrders), ct)).ToActionResult();
}
