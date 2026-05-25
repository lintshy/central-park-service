using CentralPark.Application.Common.Interfaces;
using CentralPark.Application.Features.Stores.Queries.GetStoresBySuburb;
using Microsoft.EntityFrameworkCore;

namespace CentralPark.Infrastructure.Persistence.Repositories;

public sealed class StoreRepository(ApplicationDbContext context) : IStoreRepository
{
    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await context.Stores.AnyAsync(s => s.Id == id && s.IsActive, ct);

    public async Task<IReadOnlyList<StoreDto>> GetBySuburbAsync(
        Guid suburbId,
        bool? acceptingOrders,
        CancellationToken ct = default)
    {
        var query = context.Stores
            .AsNoTracking()
            .Where(s => s.SuburbId == suburbId && s.IsActive)
            .Include(s => s.Hours);

        var filtered = acceptingOrders.HasValue
            ? query.Where(s => s.IsAcceptingOrders == acceptingOrders.Value)
            : query;

        return await filtered
            .Select(s => new StoreDto(
                s.Id,
                s.Name,
                s.Description,
                s.StreetAddress,
                s.PostCode,
                s.IsAcceptingOrders,
                s.OwnerDisplayName,
                s.OwnerContactPhone,
                s.Hours.Select(h => new StoreHoursDto(
                    (int)h.DayOfWeek,
                    h.OpenTime.HasValue ? h.OpenTime.Value.ToString("HH:mm") : null,
                    h.CloseTime.HasValue ? h.CloseTime.Value.ToString("HH:mm") : null,
                    h.IsClosed))
                .ToList()))
            .ToListAsync(ct);
    }
}
