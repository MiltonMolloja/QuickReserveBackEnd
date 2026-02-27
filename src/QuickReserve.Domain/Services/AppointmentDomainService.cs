// -----------------------------------------------------------------------
// <copyright file="AppointmentDomainService.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Domain.Services;

using QuickReserve.Domain.Entities;
using QuickReserve.Domain.Exceptions;
using QuickReserve.Domain.Interfaces;

/// <summary>
/// Domain service that orchestrates appointment creation with workshop validation.
/// </summary>
public sealed class AppointmentDomainService
{
    private readonly IWorkshopService _workshopService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppointmentDomainService"/> class.
    /// </summary>
    /// <param name="workshopService">The workshop service port.</param>
    public AppointmentDomainService(IWorkshopService workshopService)
    {
        _workshopService = workshopService;
    }

    /// <summary>
    /// Creates a new appointment after validating that the workshop is active.
    /// </summary>
    /// <param name="placeId">The workshop place ID.</param>
    /// <param name="appointmentAt">The appointment date.</param>
    /// <param name="serviceType">The service type.</param>
    /// <param name="contactName">The contact name.</param>
    /// <param name="contactEmail">The contact email.</param>
    /// <param name="contactPhone">The contact phone.</param>
    /// <param name="vehicleMake">The vehicle make (optional).</param>
    /// <param name="vehicleModel">The vehicle model (optional).</param>
    /// <param name="vehicleYear">The vehicle year (optional).</param>
    /// <param name="vehicleLicensePlate">The vehicle license plate (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A new <see cref="Appointment"/> instance.</returns>
    /// <exception cref="InvalidWorkshopException">Thrown when the workshop is not active.</exception>
    public async Task<Appointment> CreateAppointmentAsync(
        int placeId,
        DateTime appointmentAt,
        string serviceType,
        string contactName,
        string contactEmail,
        string contactPhone,
        string? vehicleMake = null,
        string? vehicleModel = null,
        int? vehicleYear = null,
        string? vehicleLicensePlate = null,
        CancellationToken cancellationToken = default)
    {
        // Validate that the workshop exists and is active
        var isActiveWorkshop = await _workshopService.IsActiveWorkshopAsync(placeId, cancellationToken);

        if (!isActiveWorkshop)
        {
            throw new InvalidWorkshopException(placeId);
        }

        // Create the appointment (domain validations are inside the entity)
        return Appointment.Create(
            placeId,
            appointmentAt,
            serviceType,
            contactName,
            contactEmail,
            contactPhone,
            vehicleMake,
            vehicleModel,
            vehicleYear,
            vehicleLicensePlate);
    }
}
