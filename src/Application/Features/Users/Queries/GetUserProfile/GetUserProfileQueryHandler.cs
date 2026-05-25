using CentralPark.Application.Common.Interfaces;
using CentralPark.Application.Features.Users.Errors;
using CentralPark.Shared;
using MediatR;

namespace CentralPark.Application.Features.Users.Queries.GetUserProfile;

public sealed class GetUserProfileQueryHandler(
    IIdentityService identityService,
    IUserProfileRepository userProfileRepository)
    : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery query, CancellationToken ct)
    {
        var userIdResult = await identityService.FindUserIdByEmailAsync(query.Email, ct);
        if (!userIdResult.IsSuccess)
            return Result<UserProfileDto>.Failure(UserErrors.NotFound);

        if (userIdResult.Value != query.RequesterId)
            return Result<UserProfileDto>.Failure(UserErrors.Forbidden);

        var profile = await userProfileRepository.GetFullProfileByUserIdAsync(userIdResult.Value, ct);
        if (profile is null)
            return Result<UserProfileDto>.Failure(UserErrors.NotFound);

        return Result<UserProfileDto>.Success(profile);
    }
}
