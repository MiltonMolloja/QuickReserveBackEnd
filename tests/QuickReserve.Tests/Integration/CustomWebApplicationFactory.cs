// -----------------------------------------------------------------------
// <copyright file="CustomWebApplicationFactory.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Tests.Integration;

using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuickReserve.Domain.Interfaces;
using QuickReserve.Infrastructure.ExternalServices;
using QuickReserve.Infrastructure.ExternalServices.Models;

/// <summary>
/// Custom WebApplicationFactory that replaces external dependencies
/// (Tecnom API, Redis, Elasticsearch) with in-memory test doubles.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Gets the list of workshops returned by the mock Tecnom API.
    /// </summary>
    public List<TecnomWorkshopDto> MockWorkshops { get; } =
    [
        new() { Id = 1, Name = "Taller Central", Active = true, Address = "Av. Corrientes 1234", Email = "central@test.com", Whatsapp = "+5491155551234" },
        new() { Id = 2, Name = "Taller Norte", Active = true, Address = "Av. Cabildo 5678" },
        new() { Id = 3, Name = "Taller Inactivo", Active = false, Address = "Calle Falsa 123" },
    ];

    /// <inheritdoc/>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Override configuration to avoid connecting to real services
        builder.ConfigureAppConfiguration((context, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Redis"] = string.Empty,
                ["Elasticsearch:Uri"] = string.Empty,
                ["TecnomApi:BaseUrl"] = "https://fake-tecnom-api.test/",
                ["TecnomApi:Username"] = "test",
                ["TecnomApi:Password"] = "test",
                ["TecnomApi:CacheExpirationMinutes"] = "5",
                ["TecnomApi:TimeoutSeconds"] = "10",
                ["TecnomApi:RetryCount"] = "1",
            }));

        builder.ConfigureServices(services =>
        {
            // Remove the real CachedWorkshopService and TecnomApiClient
            services.RemoveAll<IWorkshopService>();
            services.RemoveAll<CachedWorkshopService>();
            services.RemoveAll<TecnomApiClient>();

            // Register a simple mock workshop service
            var workshops = MockWorkshops;
            services.AddSingleton<IWorkshopService>(new MockWorkshopService(workshops));
        });
    }

    /// <summary>
    /// In-memory mock implementation of <see cref="IWorkshopService"/>.
    /// </summary>
    private sealed class MockWorkshopService : IWorkshopService
    {
        private readonly List<TecnomWorkshopDto> workshops;

        public MockWorkshopService(List<TecnomWorkshopDto> workshops)
        {
            this.workshops = workshops;
        }

        public Task<bool> IsActiveWorkshopAsync(int placeId, CancellationToken cancellationToken = default)
        {
            var isActive = workshops.Any(w => w.Id == placeId && w.Active);
            return Task.FromResult(isActive);
        }

        public Task<IReadOnlyList<WorkshopInfo>> GetActiveWorkshopsAsync(CancellationToken cancellationToken = default)
        {
            var active = workshops
                .Where(w => w.Active)
                .Select(w => new WorkshopInfo(w.Id, w.Name, w.Address, w.Email, w.Whatsapp))
                .ToList();

            return Task.FromResult<IReadOnlyList<WorkshopInfo>>(active);
        }
    }
}
