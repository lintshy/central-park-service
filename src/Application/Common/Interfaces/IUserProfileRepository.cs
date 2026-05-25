using CentralPark.Application.Features.Users.Queries.GetUserProfile;

namespace CentralPark.Application.Common.Interfaces;

public interface IUserProfileRepository
{
    Task<UserProfileDto?> GetFullProfileByUserIdAsync(Guid userId, CancellationToken ct = default);
}
