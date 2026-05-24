using CentralPark.Api.Extensions;
using CentralPark.Application.Features.FeatureFlags.Commands.CreateFeatureFlag;
using CentralPark.Application.Features.FeatureFlags.Commands.DeleteFeatureFlag;
using CentralPark.Application.Features.FeatureFlags.Commands.UpdateFeatureFlag;
using CentralPark.Application.Features.FeatureFlags.Queries.CheckFeatureFlag;
using CentralPark.Application.Features.FeatureFlags.Queries.GetAllFeatureFlags;
using CentralPark.Application.Features.FeatureFlags.Queries.GetFeatureFlagByKey;
using CentralPark.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralPark.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/feature-flags")]
[Produces("application/json")]
[Authorize(Policy = Policies.AuthenticatedUser)]
public sealed class FeatureFlagsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Policies.FeatureFlagManager)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => (await mediator.Send(new GetAllFeatureFlagsQuery(), ct)).ToActionResult();

    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey(string key, CancellationToken ct)
        => (await mediator.Send(new GetFeatureFlagByKeyQuery(key), ct)).ToActionResult();

    [HttpGet("{key}/check")]
    public async Task<IActionResult> Check(string key, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        Guid.TryParse(userId, out var guid);
        return (await mediator.Send(new CheckFeatureFlagQuery(key, guid == Guid.Empty ? null : guid), ct))
            .ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = Policies.FeatureFlagManager)]
    public async Task<IActionResult> Create(CreateFeatureFlagCommand cmd, CancellationToken ct)
    {
        var result = await mediator.Send(cmd, ct);
        return result.ToCreatedResult("GetFeatureFlagByKey", new { key = cmd.Key });
    }

    [HttpPut("{key}")]
    [Authorize(Policy = Policies.FeatureFlagManager)]
    public async Task<IActionResult> Update(string key, [FromBody] UpdateFeatureFlagRequest body, CancellationToken ct)
        => (await mediator.Send(new UpdateFeatureFlagCommand(key, body.IsEnabled, body.Description), ct))
            .ToActionResult();

    [HttpDelete("{key}")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> Delete(string key, CancellationToken ct)
        => (await mediator.Send(new DeleteFeatureFlagCommand(key), ct)).ToActionResult();
}

public sealed record UpdateFeatureFlagRequest(bool IsEnabled, string? Description);
