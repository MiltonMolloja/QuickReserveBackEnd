// -----------------------------------------------------------------------
// <copyright file="TecnomApiClient.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Infrastructure.ExternalServices;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuickReserve.Domain.Interfaces;
using QuickReserve.Infrastructure.Configuration;
using QuickReserve.Infrastructure.ExternalServices.Models;

/// <summary>
/// HTTP client for the Tecnom CRM API. Implements <see cref="IWorkshopService"/>
/// to fetch workshop data from the external service.
/// </summary>
public sealed class TecnomApiClient : IWorkshopService
{
    private readonly HttpClient httpClient;
    private readonly ILogger<TecnomApiClient> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TecnomApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client (configured via HttpClientFactory).</param>
    /// <param name="settings">The Tecnom API settings.</param>
    /// <param name="logger">The logger.</param>
    public TecnomApiClient(
        HttpClient httpClient,
        IOptions<TecnomApiSettings> settings,
        ILogger<TecnomApiClient> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;

        // Configure Basic Auth
        var credentials = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{settings.Value.Username}:{settings.Value.Password}"));
        this.httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);
    }

    /// <inheritdoc/>
    public async Task<bool> IsActiveWorkshopAsync(int placeId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Checking if workshop {PlaceId} is active", placeId);

        var workshops = await GetActiveWorkshopsAsync(cancellationToken);
        return workshops.Any(w => w.Id == placeId);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<WorkshopInfo>> GetActiveWorkshopsAsync(CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Fetching workshops from Tecnom API");

        var response = await httpClient.GetAsync("places/workshops", cancellationToken);
        response.EnsureSuccessStatusCode();

        var workshops = await response.Content.ReadFromJsonAsync<List<TecnomWorkshopDto>>(cancellationToken)
            ?? [];

        var activeWorkshops = workshops
            .Where(w => w.Active)
            .Select(w => new WorkshopInfo(w.Id, w.Name, w.Address, w.Email, w.Whatsapp))
            .ToList();

        logger.LogDebug("Retrieved {Count} active workshops", activeWorkshops.Count);

        return activeWorkshops;
    }
}
