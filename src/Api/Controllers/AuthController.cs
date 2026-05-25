using CentralPark.Api.Extensions;
using CentralPark.Application.Features.Auth.Commands.GoogleAuth;
using CentralPark.Application.Features.Auth.Commands.Login;
using CentralPark.Application.Features.Auth.Commands.RefreshToken;
using CentralPark.Application.Features.Auth.Commands.Register;
using CentralPark.Application.Features.Auth.Commands.GoogleAuth;
using CentralPark.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CentralPark.Application.Features.Auth.Commands.GoogleAuthMock;
using Microsoft.AspNetCore.Hosting;

namespace CentralPark.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public sealed class AuthController(IMediator mediator, IWebHostEnvironment env) : ControllerBase
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

    [HttpPost("google")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    public async Task<IActionResult> GoogleAuth(GoogleAuthCommand cmd, CancellationToken ct)
    {
        return (await mediator.Send(cmd, ct)).ToActionResult();
    }

    [HttpPost("google/mock")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Auth)]
    public async Task<IActionResult> GoogleAuthMock(GoogleAuthMockCommand cmd, CancellationToken ct)
    {
        if (!env.IsDevelopment())
            return NotFound();
        return (await mediator.Send(cmd, ct)).ToActionResult();
    }
}
