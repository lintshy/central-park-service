using System.Reflection;
using CentralPark.Application.Common.Behaviours;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CentralPark.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(TransactionBehaviour<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
