// -----------------------------------------------------------------------
// <copyright file="AppointmentDomainServiceTests.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Tests.Domain.Services;

using FluentAssertions;
using Moq;
using QuickReserve.Domain.Exceptions;
using QuickReserve.Domain.Interfaces;
using QuickReserve.Domain.Services;

public class AppointmentDomainServiceTests
{
    private readonly Mock<IWorkshopService> _workshopServiceMock;
    private readonly AppointmentDomainService _sut;

    public AppointmentDomainServiceTests()
    {
        _workshopServiceMock = new Mock<IWorkshopService>();
        _sut = new AppointmentDomainService(_workshopServiceMock.Object);
    }

    [Fact]
    public async Task CreateAppointmentAsync_WithActiveWorkshop_ShouldSucceed()
    {
        // Arrange
        _workshopServiceMock
            .Setup(x => x.IsActiveWorkshopAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var futureDate = DateTime.UtcNow.AddDays(7);

        // Act
        var appointment = await _sut.CreateAppointmentAsync(
            placeId: 123,
            appointmentAt: futureDate,
            serviceType: "Mantenimiento",
            contactName: "Juan Perez",
            contactEmail: "juan@email.com",
            contactWhatsapp: "+5491155551234");

        // Assert
        appointment.Should().NotBeNull();
        appointment.PlaceId.Should().Be(123);
        appointment.Contact.Name.Should().Be("Juan Perez");
        _workshopServiceMock.Verify(
            x => x.IsActiveWorkshopAsync(123, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAppointmentAsync_WithInactiveWorkshop_ShouldThrowInvalidWorkshopException()
    {
        // Arrange
        _workshopServiceMock
            .Setup(x => x.IsActiveWorkshopAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var futureDate = DateTime.UtcNow.AddDays(7);

        // Act
        var act = () => _sut.CreateAppointmentAsync(
            placeId: 999,
            appointmentAt: futureDate,
            serviceType: "Mantenimiento",
            contactName: "Juan Perez",
            contactEmail: "juan@email.com",
            contactWhatsapp: "+5491155551234");

        // Assert
        await act.Should().ThrowAsync<InvalidWorkshopException>()
            .Where(ex => ex.PlaceId == 999);
    }

    [Fact]
    public async Task CreateAppointmentAsync_WithVehicle_ShouldIncludeVehicleData()
    {
        // Arrange
        _workshopServiceMock
            .Setup(x => x.IsActiveWorkshopAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var futureDate = DateTime.UtcNow.AddDays(7);

        // Act
        var appointment = await _sut.CreateAppointmentAsync(
            placeId: 123,
            appointmentAt: futureDate,
            serviceType: "Mantenimiento",
            contactName: "Juan Perez",
            contactEmail: "juan@email.com",
            contactWhatsapp: "+5491155551234",
            vehicleMake: "Toyota",
            vehicleModel: "Corolla",
            vehicleYear: 2022,
            vehicleLicensePlate: "AB123CD");

        // Assert
        appointment.Vehicle.Should().NotBeNull();
        appointment.Vehicle!.Make.Should().Be("Toyota");
        appointment.Vehicle.LicensePlate!.Value.Should().Be("AB123CD");
    }

    [Fact]
    public async Task CreateAppointmentAsync_WithPastDate_ShouldThrowDomainException()
    {
        // Arrange
        _workshopServiceMock
            .Setup(x => x.IsActiveWorkshopAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var pastDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var act = () => _sut.CreateAppointmentAsync(
            placeId: 123,
            appointmentAt: pastDate,
            serviceType: "Mantenimiento",
            contactName: "Juan Perez",
            contactEmail: "juan@email.com",
            contactWhatsapp: "+5491155551234");

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*fecha*futura*");
    }
}
