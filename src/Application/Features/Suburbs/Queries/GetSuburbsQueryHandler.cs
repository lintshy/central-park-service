using System.Linq.Expressions;
using CentralPark.Application.Common.Interfaces;
using CentralPark.Application.Features.Users.Queries.GetUserProfile;
using CentralPark.Domain.Entities;
using CentralPark.Shared;
using MediatR;

namespace CentralPark.Application.Features.Suburbs.Queries;

public sealed class GetSuburbsQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetSuburbsQuery, Result<IReadOnlyList<SuburbDto>>>
{
    public async Task<Result<IReadOnlyList<SuburbDto>>> Handle(GetSuburbsQuery query, CancellationToken ct)
    {
        Expression<Func<Suburb, bool>> predicate = s =>
            (string.IsNullOrWhiteSpace(query.Search) || query.Search.Length < 2 ||
                s.Name.Contains(query.Search) || s.PostCode.Contains(query.Search)) &&
            (string.IsNullOrWhiteSpace(query.State) ||
                s.State == query.State);

        var suburbs = await uow.Repository<Suburb>().FindAsync(predicate, ct);

        return Result<IReadOnlyList<SuburbDto>>.Success(
            suburbs.Select(s => new SuburbDto(s.Id, s.Name, s.PostCode, s.State, null)).ToList());
    }
}
