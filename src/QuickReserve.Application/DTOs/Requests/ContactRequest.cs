// -----------------------------------------------------------------------
// <copyright file="ContactRequest.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Application.DTOs.Requests;

/// <summary>
/// Request DTO for contact information.
/// </summary>
public sealed record ContactRequest
{
    public string Name { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;
}
