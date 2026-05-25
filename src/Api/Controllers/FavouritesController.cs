using System.Security.Claims;
using CentralPark.Api.Extensions;
using CentralPark.Application.Features.Favourites.Commands.RemoveFavourite;
using CentralPark.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralPark.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/favourites")]
[Produces("application/json")]
[Authorize(Policy = Policies.AuthenticatedUser)]
public sealed class FavouritesController(IMediator mediator) : ControllerBase
{
    // TODO: POST /favourites — add a store to favourites (AddFavouriteCommand)
    // Request body: { storeId: Guid }
    // Response 201: { favouriteId, storeId, createdAt }
    // Errors: 404 Store.NotFound, 409 Favourite.AlreadyExists

    [HttpDelete("{storeId:guid}")]
    public async Task<IActionResult> Remove(Guid storeId, CancellationToken ct)
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var requesterId = Guid.TryParse(raw, out var id) ? id : Guid.Empty;
        return (await mediator.Send(new RemoveFavouriteCommand(storeId, requesterId), ct)).ToActionResult();
    }
}
