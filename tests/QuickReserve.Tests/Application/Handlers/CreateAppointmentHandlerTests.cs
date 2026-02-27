// -----------------------------------------------------------------------
// <copyright file="CreateAppointmentHandlerTests.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Tests.Application.Handlers;

using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using QuickReserve.Application.DTOs.Requests;
using QuickReserve.Application.Features.Appointments.Commands;
using QuickReserve.Application.Mappings;
using QuickReserve.Domain.Entities;
using QuickReserve.Domain.Interfaces;
using QuickReserve.Domain.Services;

public class CreateAppointmentHandlerTests
{
    private readonly Mock<IAppointmentRepository> _repositoryMock;
    private readonly Mock<IWorkshopService> _workshopServiceMock;
    private readonly Mock<IValidator<CreateAppointmentRequest>> _validatorMock;
    private readonly CreateAppointmentHandler _sut;

    public CreateAppointmentHandlerTests()
    {
        _repositoryMock = new Mock<IAppointmentRepository>();
        _workshopServiceMock = new Mock<IWorkshopService>();
        _validatorMock = new Mock<IValidator<CreateAppointmentRequest>>();

        var domainService = new AppointmentDomainService(_workshopServiceMock.Object);
        var logger = Mock.Of<ILogger<CreateAppointmentHandler>>();

        MappingConfig.Configure();

        _sut = new CreateAppointmentHandler(
            _repositoryMock.Object,
            domainService,
            _validatorMock.Object,
            logger);
    }

    private static CreateAppointmentRequest ValidRequest => new()
    {
        PlaceId = 123,
        AppointmentAt = DateTime.UtcNow.AddDays(7),
        ServiceType = "Mantenimiento",
        Contact = new ContactRequest
        {
            Name = "Juan Perez",
            Email = "juan@email.com",
            Phone = "+5491155551234",
        },
    };

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateAppointmentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _workshopServiceMock
            .Setup(w => w.IsActiveWorkshopAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment a, CancellationToken _) => a);

        var command = new CreateAppointmentCommand(ValidRequest);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PlaceId.Should().Be(123);
        result.Data.Contact.Name.Should().Be("Juan Perez");

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidationErrors_ShouldReturnFailure()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("PlaceId", "El place_id debe ser mayor a 0."),
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateAppointmentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        var command = new CreateAppointmentCommand(ValidRequest with { PlaceId = 0 });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("El place_id debe ser mayor a 0.");

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithInactiveWorkshop_ShouldReturnFailure()
    {
        // Arrange
        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateAppointmentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _workshopServiceMock
            .Setup(w => w.IsActiveWorkshopAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new CreateAppointmentCommand(ValidRequest with { PlaceId = 999 });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("999"));
    }
}
