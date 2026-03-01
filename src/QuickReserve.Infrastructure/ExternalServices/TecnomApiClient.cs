// -----------------------------------------------------------------------
// <copyright file="TecnomApiClient.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Infrastructure.ExternalServices;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuickReserve.Domain.Interfaces;
using QuickReserve.Infrastructure.Configuration;
using QuickReserve.Infrastructure.ExternalServices.Models;

/// <summary>
/// HTTP client for the Tecnom CRM API. Implements <see cref="IWorkshopService"/>
/// to fetch workshop data from the external service.
/// </summary>
public sealed partial class TecnomApiClient : IWorkshopService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TecnomApiClient> _logger;

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
        _httpClient = httpClient;
        _logger = logger;

        // Configure Basic Auth
        var credentials = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{settings.Value.Username}:{settings.Value.Password}"));
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);
    }

    /// <inheritdoc/>
    public async Task<bool> IsActiveWorkshopAsync(int placeId, CancellationToken cancellationToken = default)
    {
        LogCheckingWorkshop(_logger, placeId);

        var workshops = await GetActiveWorkshopsAsync(cancellationToken);
        return workshops.Any(w => w.Id == placeId);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<WorkshopInfo>> GetActiveWorkshopsAsync(CancellationToken cancellationToken = default)
    {
        LogFetchingWorkshops(_logger);

        var response = await _httpClient.GetAsync("places/workshops", cancellationToken);
        response.EnsureSuccessStatusCode();

        var workshops = await response.Content.ReadFromJsonAsync<List<TecnomWorkshopDto>>(cancellationToken)
            ?? [];

        var activeWorkshops = workshops
            .Where(w => w.Active)
            .Select(w => new WorkshopInfo(w.Id, w.Name, ParseFormattedAddress(w.Address), w.Email, w.Whatsapp))
            .ToList();

        LogRetrievedWorkshops(_logger, activeWorkshops.Count);

        return activeWorkshops;
    }

    /// <summary>
    /// Extracts the formatted address from a Google Places JSON string.
    /// Returns the raw value if parsing fails.
    /// </summary>
    private static string? ParseFormattedAddress(string? addressJson)
    {
        if (string.IsNullOrEmpty(addressJson))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(addressJson);
            return doc.RootElement.TryGetProperty("formatted_address", out var formatted)
                ? formatted.GetString()
                : addressJson;
        }
        catch (JsonException)
        {
            return addressJson;
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Checking if workshop {PlaceId} is active")]
    private static partial void LogCheckingWorkshop(ILogger logger, int placeId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Fetching workshops from Tecnom API")]
    private static partial void LogFetchingWorkshops(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Retrieved {Count} active workshops")]
    private static partial void LogRetrievedWorkshops(ILogger logger, int count);
}
