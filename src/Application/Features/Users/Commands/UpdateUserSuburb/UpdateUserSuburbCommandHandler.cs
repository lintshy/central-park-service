using CentralPark.Application.Common.Interfaces;
using CentralPark.Application.Features.Users.Errors;
using CentralPark.Application.Features.Users.Queries.GetUserProfile;
using CentralPark.Domain.Entities;
using CentralPark.Shared;
using MediatR;

namespace CentralPark.Application.Features.Users.Commands.UpdateUserSuburb;

public sealed class UpdateUserSuburbCommandHandler(
    IIdentityService identityService,
    IUnitOfWork uow)
    : IRequestHandler<UpdateUserSuburbCommand, Result<SuburbDto>>
{
    public async Task<Result<SuburbDto>> Handle(UpdateUserSuburbCommand cmd, CancellationToken ct)
    {
        var userIdResult = await identityService.FindUserIdByEmailAsync(cmd.Email, ct);
        if (!userIdResult.IsSuccess)
            return Result<SuburbDto>.Failure(UserErrors.NotFound);

        if (userIdResult.Value != cmd.RequesterId)
            return Result<SuburbDto>.Failure(UserErrors.Forbidden);

        var suburb = await uow.Repository<Suburb>().GetByIdAsync(cmd.HomeSuburbId, ct);
        if (suburb is null)
            return Result<SuburbDto>.Failure(new Error("Suburb.NotFound", "The specified suburb was not found."));

        var profiles = await uow.Repository<UserProfile>().FindAsync(p => p.UserId == userIdResult.Value, ct);
        var profile = profiles.FirstOrDefault();
        if (profile is null)
            return Result<SuburbDto>.Failure(UserErrors.NotFound);

        profile.UpdateHomeSuburb(suburb.Id);
        uow.Repository<UserProfile>().Update(profile);

        return Result<SuburbDto>.Success(new SuburbDto(suburb.Id, suburb.Name, suburb.PostCode, suburb.State, null));
    }
}
