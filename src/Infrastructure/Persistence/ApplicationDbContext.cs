using CentralPark.Domain.Entities;
using CentralPark.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CentralPark.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>(options)
{
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<Suburb> Suburbs => Set<Suburb>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    public DbSet<BusinessOwnerProfile> BusinessOwnerProfiles => Set<BusinessOwnerProfile>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<StoreHours> StoreHours => Set<StoreHours>();
    public DbSet<Favourite> Favourites => Set<Favourite>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<StoreView> StoreViews => Set<StoreView>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
