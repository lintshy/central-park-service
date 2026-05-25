using CentralPark.Application.Common.Markers;

namespace CentralPark.Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string? AvatarUrl) : ICommand<CreateUserResponse>;

public sealed record CreateUserResponse(Guid UserId, string Email, bool IsNewUser);
