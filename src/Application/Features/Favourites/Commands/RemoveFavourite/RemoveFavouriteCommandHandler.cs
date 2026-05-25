using CentralPark.Application.Common.Interfaces;
using CentralPark.Application.Features.Favourites.Errors;
using CentralPark.Domain.Entities;
using CentralPark.Shared;
using MediatR;

namespace CentralPark.Application.Features.Favourites.Commands.RemoveFavourite;

public sealed class RemoveFavouriteCommandHandler(IUnitOfWork uow)
    : IRequestHandler<RemoveFavouriteCommand, Result>
{
    public async Task<Result> Handle(RemoveFavouriteCommand cmd, CancellationToken ct)
    {
        var customerProfiles = await uow.Repository<CustomerProfile>()
            .FindAsync(cp => cp.UserId == cmd.RequesterId, ct);
        var customerProfile = customerProfiles.FirstOrDefault();
        if (customerProfile is null)
            return Result.Failure(FavouriteErrors.NotFound);

        var favourites = await uow.Repository<Favourite>()
            .FindAsync(f => f.StoreId == cmd.StoreId && f.CustomerProfileId == customerProfile.Id, ct);
        var favourite = favourites.FirstOrDefault();
        if (favourite is null)
            return Result.Failure(FavouriteErrors.NotFound);

        uow.Repository<Favourite>().Remove(favourite);
        return Result.Success();
    }
}
