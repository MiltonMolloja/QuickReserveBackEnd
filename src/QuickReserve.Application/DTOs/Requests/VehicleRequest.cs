// -----------------------------------------------------------------------
// <copyright file="VehicleRequest.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Application.DTOs.Requests;

/// <summary>
/// Request DTO for vehicle information (optional).
/// </summary>
public sealed record VehicleRequest
{
    public string? Make { get; init; }

    public string? Model { get; init; }

    public int? Year { get; init; }

    public string? LicensePlate { get; init; }
}
