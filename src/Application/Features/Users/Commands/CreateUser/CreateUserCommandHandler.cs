using CentralPark.Application.Common.Interfaces;
using CentralPark.Domain.Entities;
using CentralPark.Shared;
using MediatR;

namespace CentralPark.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler(
    IIdentityService identityService,
    IUnitOfWork uow)
    : IRequestHandler<CreateUserCommand, Result<CreateUserResponse>>
{
    public async Task<Result<CreateUserResponse>> Handle(CreateUserCommand cmd, CancellationToken ct)
    {
        var existingResult = await identityService.FindUserIdByEmailAsync(cmd.Email, ct);
        if (existingResult.IsSuccess)
            return Result<CreateUserResponse>.Success(
                new CreateUserResponse(existingResult.Value, cmd.Email, IsNewUser: false));

        var registerResult = await identityService.RegisterExternalAsync(cmd.Email, ct);
        if (!registerResult.IsSuccess)
            return Result<CreateUserResponse>.Failure(registerResult.Error);

        var userId = registerResult.Value;
        var profile = UserProfile.Create(userId, cmd.FirstName, cmd.LastName, cmd.AvatarUrl);
        var customerProfile = CustomerProfile.Create(userId);

        await uow.Repository<UserProfile>().AddAsync(profile, ct);
        await uow.Repository<CustomerProfile>().AddAsync(customerProfile, ct);

        return Result<CreateUserResponse>.Success(
            new CreateUserResponse(userId, cmd.Email, IsNewUser: true));
    }
}
