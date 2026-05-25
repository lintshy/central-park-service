using CentralPark.Api.Extensions;
using CentralPark.Application.Features.Suburbs.Queries;
using CentralPark.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralPark.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/suburbs")]
[Produces("application/json")]
[Authorize(Policy = Policies.AuthenticatedUser)]
public sealed class SuburbsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? state,
        CancellationToken ct)
        => (await mediator.Send(new GetSuburbsQuery { Search = search, State = state }, ct)).ToActionResult();
}
