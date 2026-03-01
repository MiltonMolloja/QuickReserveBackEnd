// -----------------------------------------------------------------------
// <copyright file="AppointmentTests.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Tests.Domain.Entities;

using FluentAssertions;
using QuickReserve.Domain.Entities;
using QuickReserve.Domain.Exceptions;

public class AppointmentTests
{
    private static readonly DateTime FutureDate = DateTime.UtcNow.AddDays(7);

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var appointment = Appointment.Create(
            placeId: 123,
            appointmentAt: FutureDate,
            serviceType: "Mantenimiento",
            contactName: "Juan Perez",
            contactEmail: "juan@email.com",
            contactWhatsapp: "+5491155551234");

        appointment.Id.Should().NotBeEmpty();
        appointment.PlaceId.Should().Be(123);
        appointment.AppointmentAt.Should().Be(FutureDate);
        appointment.ServiceType.Value.Should().Be("Mantenimiento");
        appointment.Contact.Name.Should().Be("Juan Perez");
        appointment.Contact.Email.Value.Should().Be("juan@email.com");
        appointment.Contact.Whatsapp.Value.Should().Be("+5491155551234");
        appointment.Vehicle.Should().BeNull();
        appointment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithVehicle_ShouldIncludeVehicle()
    {
        var appointment = Appointment.Create(
            placeId: 123,
            appointmentAt: FutureDate,
            serviceType: "Mantenimiento",
            contactName: "Juan Perez",
            contactEmail: "juan@email.com",
            contactWhatsapp: "+5491155551234",
            vehicleMake: "Toyota",
            vehicleModel: "Corolla",
            vehicleYear: 2022,
            vehicleLicensePlate: "AB123CD");

        appointment.Vehicle.Should().NotBeNull();
        appointment.Vehicle!.Make.Should().Be("Toyota");
        appointment.Vehicle.Model.Should().Be("Corolla");
        appointment.Vehicle.Year.Should().Be(2022);
        appointment.Vehicle.LicensePlate!.Value.Should().Be("AB123CD");
    }

    [Fact]
    public void Create_WithPastDate_ShouldThrowDomainException()
    {
        var pastDate = DateTime.UtcNow.AddDays(-1);

        var act = () => Appointment.Create(
            placeId: 123,
            appointmentAt: pastDate,
            serviceType: "Mantenimiento",
            contactName: "Juan Perez",
            contactEmail: "juan@email.com",
            contactWhatsapp: "+5491155551234");

        act.Should().Throw<DomainException>()
            .WithMessage("*fecha*futura*");
    }

    [Fact]
    public void Create_WithEmptyServiceType_ShouldThrowDomainException()
    {
        var act = () => Appointment.Create(
            placeId: 123,
            appointmentAt: FutureDate,
            serviceType: string.Empty,
            contactName: "Juan Perez",
            contactEmail: "juan@email.com",
            contactWhatsapp: "+5491155551234");

        act.Should().Throw<DomainException>()
            .WithMessage("*tipo de servicio*");
    }

    [Fact]
    public void Create_WithEmptyContactName_ShouldThrowDomainException()
    {
        var act = () => Appointment.Create(
            placeId: 123,
            appointmentAt: FutureDate,
            serviceType: "Mantenimiento",
            contactName: string.Empty,
            contactEmail: "juan@email.com",
            contactWhatsapp: "+5491155551234");

        act.Should().Throw<DomainException>()
            .WithMessage("*nombre*");
    }

    [Fact]
    public void Create_EachCallShouldGenerateUniqueId()
    {
        var appointment1 = Appointment.Create(
            placeId: 123,
            appointmentAt: FutureDate,
            serviceType: "Mantenimiento",
            contactName: "Juan",
            contactEmail: "juan@email.com",
            contactWhatsapp: "+5491155551234");

        var appointment2 = Appointment.Create(
            placeId: 123,
            appointmentAt: FutureDate,
            serviceType: "Mantenimiento",
            contactName: "Juan",
            contactEmail: "juan@email.com",
            contactWhatsapp: "+5491155551234");

        appointment1.Id.Should().NotBe(appointment2.Id);
    }
}
