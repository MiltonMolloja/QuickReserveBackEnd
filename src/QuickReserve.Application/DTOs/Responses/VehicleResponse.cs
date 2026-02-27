// -----------------------------------------------------------------------
// <copyright file="VehicleResponse.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Application.DTOs.Responses;

/// <summary>
/// Response DTO for vehicle information.
/// </summary>
public sealed record VehicleResponse
{
    public string? Make { get; init; }

    public string? Model { get; init; }

    public int? Year { get; init; }

    public string? LicensePlate { get; init; }
}
