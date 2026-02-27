// -----------------------------------------------------------------------
// <copyright file="CachedWorkshopService.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Infrastructure.ExternalServices;

using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuickReserve.Domain.Interfaces;
using QuickReserve.Infrastructure.Configuration;

/// <summary>
/// Decorator that adds distributed caching (Redis/Memory) on top of
/// <see cref="TecnomApiClient"/> for workshop data.
/// </summary>
public sealed partial class CachedWorkshopService : IWorkshopService
{
    private const string CacheKey = "workshops:active";

    private readonly TecnomApiClient _innerService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedWorkshopService> _logger;
    private readonly TecnomApiSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachedWorkshopService"/> class.
    /// </summary>
    /// <param name="innerService">The inner workshop service (Tecnom API client).</param>
    /// <param name="cache">The distributed cache.</param>
    /// <param name="settings">The Tecnom API settings.</param>
    /// <param name="logger">The logger.</param>
    public CachedWorkshopService(
        TecnomApiClient innerService,
        IDistributedCache cache,
        IOptions<TecnomApiSettings> settings,
        ILogger<CachedWorkshopService> logger)
    {
        _innerService = innerService;
        _cache = cache;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<bool> IsActiveWorkshopAsync(int placeId, CancellationToken cancellationToken = default)
    {
        var workshops = await GetActiveWorkshopsAsync(cancellationToken);
        return workshops.Any(w => w.Id == placeId);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<WorkshopInfo>> GetActiveWorkshopsAsync(CancellationToken cancellationToken = default)
    {
        // Try to get from cache (graceful fallback if cache is unavailable)
        try
        {
            var cachedData = await _cache.GetStringAsync(CacheKey, cancellationToken);

            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogDebug("Workshops retrieved from cache");
                return JsonSerializer.Deserialize<List<WorkshopInfo>>(cachedData) ?? [];
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache unavailable, falling back to API");
        }

        // Cache miss or cache unavailable - fetch from API
        _logger.LogDebug("Fetching workshops from API");
        var workshops = await _innerService.GetActiveWorkshopsAsync(cancellationToken);

        // Try to store in cache (best-effort, don't fail if cache is down)
        try
        {
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.CacheExpirationMinutes),
            };

            await _cache.SetStringAsync(
                CacheKey,
                JsonSerializer.Serialize(workshops),
                cacheOptions,
                cancellationToken);

            LogWorkshopsCached(_logger, _settings.CacheExpirationMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to store workshops in cache");
        }

        return workshops;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Workshops cached for {Minutes} minutes")]
    private static partial void LogWorkshopsCached(ILogger logger, int minutes);
}
