// -----------------------------------------------------------------------
// <copyright file="DependencyInjection.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Application;

using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using QuickReserve.Application.Common.Behaviors;
using QuickReserve.Application.Mappings;
using QuickReserve.Domain.Services;

/// <summary>
/// Extension methods for registering Application layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Application layer services to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Mapster configuration
        MappingConfig.Configure();

        // MediatR + Pipeline Behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // FluentValidation - register all validators from this assembly
        services.AddValidatorsFromAssembly(assembly);

        // Domain Services
        services.AddScoped<AppointmentDomainService>();

        return services;
    }
}
