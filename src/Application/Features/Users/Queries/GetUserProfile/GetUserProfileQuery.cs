using CentralPark.Application.Common.Markers;

namespace CentralPark.Application.Features.Users.Queries.GetUserProfile;

public sealed record GetUserProfileQuery(string Email, Guid RequesterId) : IQuery<UserProfileDto>;
