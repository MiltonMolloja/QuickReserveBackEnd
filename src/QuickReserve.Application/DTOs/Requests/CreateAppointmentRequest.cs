// -----------------------------------------------------------------------
// <copyright file="CreateAppointmentRequest.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Application.DTOs.Requests;

/// <summary>
/// Request DTO for creating a new appointment.
/// </summary>
public sealed record CreateAppointmentRequest
{
    public int PlaceId { get; init; }

    public DateTime AppointmentAt { get; init; }

    public string ServiceType { get; init; } = string.Empty;

    public ContactRequest Contact { get; init; } = null!;

    public VehicleRequest? Vehicle { get; init; }
}
