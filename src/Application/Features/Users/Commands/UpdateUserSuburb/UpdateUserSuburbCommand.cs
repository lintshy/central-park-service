using CentralPark.Application.Common.Markers;
using CentralPark.Application.Features.Users.Queries.GetUserProfile;

namespace CentralPark.Application.Features.Users.Commands.UpdateUserSuburb;

public sealed record UpdateUserSuburbCommand(
    string Email,
    Guid HomeSuburbId,
    Guid RequesterId) : ICommand<SuburbDto>;
