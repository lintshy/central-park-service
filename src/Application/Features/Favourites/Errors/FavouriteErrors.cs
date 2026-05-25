using CentralPark.Shared;

namespace CentralPark.Application.Features.Favourites.Errors;

public static class FavouriteErrors
{
    public static readonly Error NotFound =
        new("Favourite.NotFound", "This store is not in your favourites.");

    public static readonly Error AlreadyExists =
        new("Favourite.AlreadyExists", "This store is already in your favourites.");
}
