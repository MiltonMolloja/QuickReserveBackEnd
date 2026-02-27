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
public sealed class CachedWorkshopService : IWorkshopService
{
    private const string CacheKey = "workshops:active";

    private readonly IWorkshopService innerService;
    private readonly IDistributedCache cache;
    private readonly ILogger<CachedWorkshopService> logger;
    private readonly TecnomApiSettings settings;

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
        this.innerService = innerService;
        this.cache = cache;
        this.settings = settings.Value;
        this.logger = logger;
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
        // Try to get from cache
        var cachedData = await cache.GetStringAsync(CacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedData))
        {
            logger.LogDebug("Workshops retrieved from cache");
            return JsonSerializer.Deserialize<List<WorkshopInfo>>(cachedData) ?? [];
        }

        // Cache miss - fetch from API
        logger.LogDebug("Cache miss, fetching workshops from API");
        var workshops = await innerService.GetActiveWorkshopsAsync(cancellationToken);

        // Store in cache
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(settings.CacheExpirationMinutes),
        };

        await cache.SetStringAsync(
            CacheKey,
            JsonSerializer.Serialize(workshops),
            cacheOptions,
            cancellationToken);

        logger.LogDebug("Workshops cached for {Minutes} minutes", settings.CacheExpirationMinutes);

        return workshops;
    }
}
