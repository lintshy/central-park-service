using CentralPark.Application.Common.Markers;

namespace CentralPark.Application.Features.Favourites.Commands.RemoveFavourite;

public sealed record RemoveFavouriteCommand(Guid StoreId, Guid RequesterId) : ICommand;
