using CentralPark.Application.Common.Markers;
using CentralPark.Application.Features.Users.Queries.GetUserProfile;

namespace CentralPark.Application.Features.Suburbs.Queries;

public sealed record GetSuburbsQuery() : IQuery<IReadOnlyList<SuburbDto>>
{
    public string? Search { get; init; }
    public string? State { get; init; }
}
