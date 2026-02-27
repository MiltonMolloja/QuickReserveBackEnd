// -----------------------------------------------------------------------
// <copyright file="DependencyInjection.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using QuickReserve.Domain.Interfaces;
using QuickReserve.Infrastructure.Configuration;
using QuickReserve.Infrastructure.ExternalServices;
using QuickReserve.Infrastructure.Persistence;
using QuickReserve.Infrastructure.Persistence.Repositories;

/// <summary>
/// Extension methods for registering Infrastructure layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core InMemory
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("QuickReserveDb"));

        // Repository
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();

        // Tecnom API Settings (Options Pattern)
        services.Configure<TecnomApiSettings>(
            configuration.GetSection(TecnomApiSettings.SectionName));

        var tecnomSettings = configuration
            .GetSection(TecnomApiSettings.SectionName)
            .Get<TecnomApiSettings>() ?? new TecnomApiSettings();

        // HttpClient with resilience (retry + circuit breaker)
        services.AddHttpClient<TecnomApiClient>(client =>
        {
            client.BaseAddress = new Uri(tecnomSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(tecnomSettings.TimeoutSeconds);
        })
        .AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = tecnomSettings.RetryCount;
            options.Retry.Delay = TimeSpan.FromSeconds(2);

            options.CircuitBreaker.FailureRatio = 0.5;
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            options.CircuitBreaker.MinimumThroughput = 5;
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);

            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(tecnomSettings.TimeoutSeconds);
        });

        // Workshop Service with Cache (Decorator pattern)
        services.AddScoped<IWorkshopService, CachedWorkshopService>();

        // Redis Cache (or fallback to in-memory)
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "QuickReserve:";
            });
        }
        else
        {
            // Fallback to in-memory distributed cache when Redis is unavailable
            services.AddDistributedMemoryCache();
        }

        return services;
    }
}
