// -----------------------------------------------------------------------
// <copyright file="AppointmentResponse.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Application.DTOs.Responses;

/// <summary>
/// Response DTO for an appointment.
/// </summary>
public sealed record AppointmentResponse
{
    public Guid Id { get; init; }

    public int PlaceId { get; init; }

    public DateTime AppointmentAt { get; init; }

    public string ServiceType { get; init; } = string.Empty;

    public ContactResponse Contact { get; init; } = null!;

    public VehicleResponse? Vehicle { get; init; }

    public DateTime CreatedAt { get; init; }
}
