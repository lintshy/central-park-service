using CentralPark.Shared.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace CentralPark.Api.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddApplicationAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(opts =>
        {
            opts.AddPolicy(Policies.AdminOnly, p => p.RequireRole(Roles.Admin));
            opts.AddPolicy(Policies.FeatureFlagManager,
                p => p.RequireRole(Roles.Admin, Roles.FeatureManager));
            opts.AddPolicy(Policies.AuthenticatedUser,
                p => p.RequireAuthenticatedUser());
        });

        return services;
    }
}
