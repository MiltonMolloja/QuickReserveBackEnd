// -----------------------------------------------------------------------
// <copyright file="WorkshopResponse.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Application.DTOs.Responses;

/// <summary>
/// Response DTO for workshop information.
/// </summary>
public sealed record WorkshopResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Address { get; init; }

    public string? Email { get; init; }

    public string? Whatsapp { get; init; }
}
