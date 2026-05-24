using CentralPark.Api.Extensions;
using CentralPark.Application.Features.Auth.Commands.Login;
using CentralPark.Application.Features.Auth.Commands.RefreshToken;
using CentralPark.Application.Features.Auth.Commands.Register;
using CentralPark.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CentralPark.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    public async Task<IActionResult> Register(RegisterCommand cmd, CancellationToken ct)
    {
        var result = await mediator.Send(cmd, ct);
        return result.ToCreatedResult("GetUserById", new { id = result.Value });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    public async Task<IActionResult> Login(LoginCommand cmd, CancellationToken ct)
        => (await mediator.Send(cmd, ct)).ToActionResult();

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    public async Task<IActionResult> Refresh(RefreshTokenCommand cmd, CancellationToken ct)
        => (await mediator.Send(cmd, ct)).ToActionResult();
}
