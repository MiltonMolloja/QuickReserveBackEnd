// -----------------------------------------------------------------------
// <copyright file="TecnomApiSettings.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for the Tecnom CRM API.
/// Bound from the "TecnomApi" section in appsettings.json.
/// </summary>
public sealed class TecnomApiSettings
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "TecnomApi";

    /// <summary>
    /// Gets or sets the base URL of the Tecnom API.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username for Basic Auth.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for Basic Auth.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cache expiration in minutes.
    /// </summary>
    public int CacheExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Gets or sets the HTTP request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the number of retry attempts for transient failures.
    /// </summary>
    public int RetryCount { get; set; } = 3;
}
