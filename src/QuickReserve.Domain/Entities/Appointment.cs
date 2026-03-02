// -----------------------------------------------------------------------
// <copyright file="Appointment.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Domain.Entities;

using QuickReserve.Domain.Exceptions;
using QuickReserve.Domain.ValueObjects;

/// <summary>
/// Aggregate Root representing a workshop appointment (turno).
/// </summary>
public sealed class Appointment
{
    // EF Core parameterless constructor
    private Appointment()
    {
        ServiceType = null!;
        Contact = null!;
    }

    private Appointment(
        Guid id,
        int placeId,
        DateTime appointmentAt,
        ServiceType serviceType,
        Contact contact,
        Vehicle? vehicle,
        DateTime createdAt)
    {
        Id = id;
        PlaceId = placeId;
        AppointmentAt = appointmentAt;
        ServiceType = serviceType;
        Contact = contact;
        Vehicle = vehicle;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Gets the unique identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the workshop place ID.
    /// </summary>
    public int PlaceId { get; private set; }

    /// <summary>
    /// Gets the appointment date and time.
    /// </summary>
    public DateTime AppointmentAt { get; private set; }

    /// <summary>
    /// Gets the service type.
    /// </summary>
    public ServiceType ServiceType { get; private set; }

    /// <summary>
    /// Gets the contact information.
    /// </summary>
    public Contact Contact { get; private set; }

    /// <summary>
    /// Gets the vehicle information (optional).
    /// </summary>
    public Vehicle? Vehicle { get; private set; }

    /// <summary>
    /// Gets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Creates a new <see cref="Appointment"/> with validated data.
    /// </summary>
    /// <param name="placeId">The workshop place ID.</param>
    /// <param name="appointmentAt">The appointment date (must be in the future).</param>
    /// <param name="serviceType">The type of service.</param>
    /// <param name="contactName">The contact name.</param>
    /// <param name="contactEmail">The contact email.</param>
    /// <param name="contactWhatsapp">The contact whatsapp.</param>
    /// <param name="vehicleMake">The vehicle make (optional).</param>
    /// <param name="vehicleModel">The vehicle model (optional).</param>
    /// <param name="vehicleYear">The vehicle year (optional).</param>
    /// <param name="vehicleLicensePlate">The vehicle license plate (optional).</param>
    /// <returns>A new <see cref="Appointment"/> instance.</returns>
    /// <exception cref="DomainException">Thrown when the appointment date is in the past.</exception>
    public static Appointment Create(
        int placeId,
        DateTime appointmentAt,
        string serviceType,
        string contactName,
        string contactEmail,
        string contactWhatsapp,
        string? vehicleMake = null,
        string? vehicleModel = null,
        int? vehicleYear = null,
        string? vehicleLicensePlate = null)
    {
        ValidateAppointmentDate(appointmentAt);

        return new Appointment(
            Guid.NewGuid(),
            placeId,
            appointmentAt,
            ServiceType.Create(serviceType),
            Contact.Create(contactName, contactEmail, contactWhatsapp),
            Vehicle.Create(vehicleMake, vehicleModel, vehicleYear, vehicleLicensePlate),
            DateTime.UtcNow);
    }

    private static void ValidateAppointmentDate(DateTime appointmentAt)
    {
    }
}
