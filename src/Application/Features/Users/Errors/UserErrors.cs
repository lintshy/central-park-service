using CentralPark.Shared;

namespace CentralPark.Application.Features.Users.Errors;

public static class UserErrors
{
    public static readonly Error NotFound =
        new("User.NotFound", "No user was found for the given email.");

    public static readonly Error Forbidden =
        new("Auth.Forbidden", "You are not authorised to access this profile.");
}
