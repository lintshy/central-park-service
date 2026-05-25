using System.Security.Claims;
using CentralPark.Api.Extensions;
using CentralPark.Application.Features.Users.Commands.CreateUser;
using CentralPark.Application.Features.Users.Commands.UpdateUserSuburb;
using CentralPark.Application.Features.Users.Queries.GetUserProfile;
using CentralPark.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralPark.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/users")]
[Produces("application/json")]
[Authorize(Policy = Policies.AuthenticatedUser)]
public sealed class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet("{email}")]
    public async Task<IActionResult> GetProfile(string email, CancellationToken ct)
    {
        var requesterId = GetRequesterId();
        return (await mediator.Send(new GetUserProfileQuery(email, requesterId), ct)).ToActionResult();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create(CreateUserCommand cmd, CancellationToken ct)
    {
        var result = await mediator.Send(cmd, ct);
        if (!result.IsSuccess) return result.ToActionResult();
        return result.Value!.IsNewUser
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : Ok(result.Value);
    }

    [HttpPut("{email}")]
    public async Task<IActionResult> UpdateSuburb(string email, [FromBody] UpdateSuburbRequest body, CancellationToken ct)
    {
        var requesterId = GetRequesterId();
        return (await mediator.Send(new UpdateUserSuburbCommand(email, body.HomeSuburbId, requesterId), ct))
            .ToActionResult();
    }

    private Guid GetRequesterId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }
}

public sealed record UpdateSuburbRequest(Guid HomeSuburbId);
