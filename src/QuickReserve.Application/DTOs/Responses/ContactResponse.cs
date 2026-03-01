// -----------------------------------------------------------------------
// <copyright file="ContactResponse.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Application.DTOs.Responses;

/// <summary>
/// Response DTO for contact information.
/// </summary>
public sealed record ContactResponse
{
    public string Name { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Whatsapp { get; init; } = string.Empty;
}
