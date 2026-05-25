using CentralPark.Application.Common.Interfaces;
using CentralPark.Application.Features.Users.Queries.GetUserProfile;
using Microsoft.EntityFrameworkCore;

namespace CentralPark.Infrastructure.Persistence.Repositories;

public sealed class UserProfileRepository(ApplicationDbContext context) : IUserProfileRepository
{
    public async Task<UserProfileDto?> GetFullProfileByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null) return null;

        var profile = await context.UserProfiles
            .AsNoTracking()
            .Include(p => p.HomeSuburb)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null) return null;

        var customerProfile = await context.CustomerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(cp => cp.UserId == userId, ct);

        IReadOnlyList<FavouriteStoreDto> favouriteStores = [];
        IReadOnlyList<RecentOrderDto> recentOrders = [];

        if (customerProfile is not null)
        {
            favouriteStores = await context.Favourites
                .AsNoTracking()
                .Where(f => f.CustomerProfileId == customerProfile.Id)
                .Include(f => f.Store)
                    .ThenInclude(s => s!.Suburb)
                .Select(f => new FavouriteStoreDto(
                    f.StoreId,
                    f.Store!.Name,
                    f.Store.Suburb!.Name,
                    f.Store.IsAcceptingOrders))
                .ToListAsync(ct);

            recentOrders = await context.Orders
                .AsNoTracking()
                .Where(o => o.CustomerProfileId == customerProfile.Id)
                .Include(o => o.Store)
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .Select(o => new RecentOrderDto(
                    o.Id,
                    o.Store!.Name,
                    o.Status.ToString(),
                    o.TotalAmount,
                    o.Items.Count,
                    o.CreatedAt))
                .ToListAsync(ct);
        }

        SuburbDto? homeSuburb = profile.HomeSuburb is not null
            ? new SuburbDto(
                profile.HomeSuburb.Id,
                profile.HomeSuburb.Name,
                profile.HomeSuburb.PostCode,
                profile.HomeSuburb.State,
                profile.HomeSuburb.GeoBoundaries)
            : null;

        return new UserProfileDto(
            userId,
            user.Email!,
            profile.FirstName,
            profile.LastName,
            profile.PhoneNumber,
            profile.AvatarUrl,
            homeSuburb,
            favouriteStores,
            recentOrders);
    }
}
