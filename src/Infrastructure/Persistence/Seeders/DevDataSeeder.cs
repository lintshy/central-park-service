using CentralPark.Domain.Entities;
using CentralPark.Domain.Enums;
using CentralPark.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CentralPark.Infrastructure.Persistence.Seeders;

public sealed class DevDataSeeder(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    ILogger<DevDataSeeder> logger)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await context.Suburbs.AnyAsync(ct))
        {
            logger.LogInformation("Dev seed already applied — skipping");
            return;
        }

        logger.LogInformation("Applying dev seed data...");

        var suburbs = SeedSuburbs();
        await context.Suburbs.AddRangeAsync(suburbs, ct);
        await context.SaveChangesAsync(ct);

        var (customer, owner, both) = await SeedUsersAsync();
        await SeedProfilesAsync(customer, owner, both, suburbs, ct);
        await context.SaveChangesAsync(ct);

        var stores = await SeedStoresAsync(owner, both, suburbs, ct);
        await SeedOrdersAndFavouritesAsync(customer, both, stores, ct);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Dev seed complete");
    }

    // ── Suburbs ──────────────────────────────────────────────────────────────

    private static List<Suburb> SeedSuburbs() =>
    [
        Suburb.Create("Bondi Beach",  "2026", "NSW"),
        Suburb.Create("Surry Hills",  "2010", "NSW"),
        Suburb.Create("Newtown",      "2042", "NSW"),
        Suburb.Create("Manly",        "2095", "NSW"),
        Suburb.Create("Glebe",        "2037", "NSW"),
        Suburb.Create("Paddington",   "2021", "NSW"),
        Suburb.Create("Balmain",      "2041", "NSW"),
        Suburb.Create("Mosman",       "2088", "NSW"),
    ];

    // ── Users ─────────────────────────────────────────────────────────────────

    private async Task<(ApplicationUser customer, ApplicationUser owner, ApplicationUser both)> SeedUsersAsync()
    {
        var customer = await EnsureUserAsync("customer@dev.test", "Dev_Password123!");
        var owner    = await EnsureUserAsync("owner@dev.test",    "Dev_Password123!");
        var both     = await EnsureUserAsync("both@dev.test",     "Dev_Password123!");
        return (customer, owner, both);
    }

    private async Task<ApplicationUser> EnsureUserAsync(string email, string password)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null) return existing;

        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                $"Failed to create seed user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        return user;
    }

    // ── Profiles ──────────────────────────────────────────────────────────────

    private async Task SeedProfilesAsync(
        ApplicationUser customer,
        ApplicationUser owner,
        ApplicationUser both,
        List<Suburb> suburbs,
        CancellationToken ct)
    {
        var bondi    = suburbs.First(s => s.Name == "Bondi Beach");
        var newtown  = suburbs.First(s => s.Name == "Newtown");
        var surry    = suburbs.First(s => s.Name == "Surry Hills");

        // customer@dev.test — customer only
        var customerProfile = UserProfile.Create(customer.Id, "Alex", "Customer");
        customerProfile.UpdateHomeSuburb(bondi.Id);
        await context.UserProfiles.AddAsync(customerProfile, ct);
        await context.CustomerProfiles.AddAsync(CustomerProfile.Create(customer.Id), ct);

        // owner@dev.test — business owner only
        var ownerProfile = UserProfile.Create(owner.Id, "Sam", "Owner");
        ownerProfile.UpdateHomeSuburb(surry.Id);
        await context.UserProfiles.AddAsync(ownerProfile, ct);
        await context.BusinessOwnerProfiles.AddAsync(BusinessOwnerProfile.Create(owner.Id, "Sam's Markets"), ct);

        // both@dev.test — customer AND business owner
        var bothProfile = UserProfile.Create(both.Id, "Jordan", "Both");
        bothProfile.UpdateHomeSuburb(newtown.Id);
        await context.UserProfiles.AddAsync(bothProfile, ct);
        await context.CustomerProfiles.AddAsync(CustomerProfile.Create(both.Id), ct);
        var bothOwnerProfile = BusinessOwnerProfile.Create(both.Id, "Jordan's Fresh Co.");
        await context.BusinessOwnerProfiles.AddAsync(bothOwnerProfile, ct);
    }

    // ── Stores ────────────────────────────────────────────────────────────────

    private async Task<List<Store>> SeedStoresAsync(
        ApplicationUser owner,
        ApplicationUser both,
        List<Suburb> suburbs,
        CancellationToken ct)
    {
        var bondi   = suburbs.First(s => s.Name == "Bondi Beach");
        var newtown = suburbs.First(s => s.Name == "Newtown");
        var surry   = suburbs.First(s => s.Name == "Surry Hills");

        var ownerBop = await context.BusinessOwnerProfiles
            .FirstAsync(b => b.UserId == owner.Id, ct);
        var bothBop = await context.BusinessOwnerProfiles
            .FirstAsync(b => b.UserId == both.Id, ct);

        var store1 = Store.Create(
            ownerBop.Id, bondi.Id,
            "Bondi Beach Farmers Market",
            "Queen Elizabeth Drive", "2026",
            "Sam Owner");
        store1.SetAcceptingOrders(true);

        var store2 = Store.Create(
            ownerBop.Id, surry.Id,
            "Surry Hills Weekend Market",
            "Crown Street", "2010",
            "Sam Owner");
        store2.SetAcceptingOrders(false);

        var store3 = Store.Create(
            bothBop.Id, newtown.Id,
            "Jordan's Fresh Produce",
            "King Street", "2042",
            "Jordan Both");
        store3.SetAcceptingOrders(true);

        await context.Stores.AddRangeAsync([store1, store2, store3], ct);
        await context.SaveChangesAsync(ct);

        await SeedStoreHoursAsync([store1, store2, store3], ct);

        return [store1, store2, store3];
    }

    private async Task SeedStoreHoursAsync(List<Store> stores, CancellationToken ct)
    {
        var weekendOpen  = (TimeOnly.FromTimeSpan(TimeSpan.FromHours(7)),
                            TimeOnly.FromTimeSpan(TimeSpan.FromHours(13)));

        foreach (var store in stores)
        {
            var hours = new List<StoreHours>
            {
                StoreHours.CreateClosed(store.Id, DayOfWeek.Monday),
                StoreHours.CreateClosed(store.Id, DayOfWeek.Tuesday),
                StoreHours.CreateClosed(store.Id, DayOfWeek.Wednesday),
                StoreHours.CreateClosed(store.Id, DayOfWeek.Thursday),
                StoreHours.CreateClosed(store.Id, DayOfWeek.Friday),
                StoreHours.CreateOpen(store.Id, DayOfWeek.Saturday, weekendOpen.Item1, weekendOpen.Item2),
                StoreHours.CreateOpen(store.Id, DayOfWeek.Sunday,   weekendOpen.Item1, weekendOpen.Item2),
            };
            await context.StoreHours.AddRangeAsync(hours, ct);
        }
    }

    // ── Orders & Favourites ───────────────────────────────────────────────────

    private async Task SeedOrdersAndFavouritesAsync(
        ApplicationUser customer,
        ApplicationUser both,
        List<Store> stores,
        CancellationToken ct)
    {
        var customerCp = await context.CustomerProfiles.FirstAsync(c => c.UserId == customer.Id, ct);
        var bothCp     = await context.CustomerProfiles.FirstAsync(c => c.UserId == both.Id, ct);

        var bondiStore   = stores[0];
        var newtonStore  = stores[2];

        // Favourites
        await context.Favourites.AddRangeAsync(
        [
            Favourite.Create(customerCp.Id, bondiStore.Id),
            Favourite.Create(customerCp.Id, newtonStore.Id),
            Favourite.Create(bothCp.Id,     bondiStore.Id),
        ], ct);

        // Completed order for customer
        var completedOrder = Order.Create(customerCp.Id, bondiStore.Id, "Leave at the door");
        completedOrder.AddItem("Heirloom Tomatoes", 2, 8.50m);
        completedOrder.AddItem("Sourdough Loaf",    1, 12.00m);
        completedOrder.UpdateStatus(OrderStatus.Completed);
        await context.Orders.AddAsync(completedOrder, ct);

        // Pending order for both-user
        var pendingOrder = Order.Create(bothCp.Id, bondiStore.Id);
        pendingOrder.AddItem("Organic Eggs", 1, 9.00m);
        await context.Orders.AddAsync(pendingOrder, ct);
    }
}
