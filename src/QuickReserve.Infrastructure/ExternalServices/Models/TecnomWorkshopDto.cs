// -----------------------------------------------------------------------
// <copyright file="TecnomWorkshopDto.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Infrastructure.ExternalServices.Models;

using System.Text.Json.Serialization;

/// <summary>
/// DTO representing a workshop from the Tecnom CRM API response.
/// </summary>
public sealed record TecnomWorkshopDto
{
    /// <summary>
    /// Gets the workshop ID.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>
    /// Gets the workshop name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the workshop address.
    /// </summary>
    [JsonPropertyName("address")]
    public string? Address { get; init; }

    /// <summary>
    /// Gets the workshop email.
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; init; }

    /// <summary>
    /// Gets the workshop whatsapp number (from phone field in Tecnom API).
    /// </summary>
    [JsonPropertyName("phone")]
    public string? Whatsapp { get; init; }

    /// <summary>
    /// Gets a value indicating whether the workshop is active.
    /// </summary>
    [JsonPropertyName("active")]
    public bool Active { get; init; }
}
