// -----------------------------------------------------------------------
// <copyright file="CreateAppointmentValidatorTests.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Tests.Application.Validators;

using FluentAssertions;
using FluentValidation.TestHelper;
using QuickReserve.Application.DTOs.Requests;
using QuickReserve.Application.Validators;

public class CreateAppointmentValidatorTests
{
    private readonly CreateAppointmentValidator _validator = new();

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
    public async Task Validate_WithValidRequest_ShouldPass()
    {
        var result = await _validator.TestValidateAsync(ValidRequest);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithZeroPlaceId_ShouldFail()
    {
        var request = ValidRequest with { PlaceId = 0 };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.PlaceId);
    }

    [Fact]
    public async Task Validate_WithPastDate_ShouldFail()
    {
        var request = ValidRequest with { AppointmentAt = DateTime.UtcNow.AddDays(-1) };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.AppointmentAt);
    }

    [Fact]
    public async Task Validate_WithEmptyServiceType_ShouldFail()
    {
        var request = ValidRequest with { ServiceType = string.Empty };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.ServiceType);
    }

    [Fact]
    public async Task Validate_WithNullContact_ShouldFail()
    {
        var request = ValidRequest with { Contact = null! };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Contact);
    }

    [Fact]
    public async Task Validate_WithInvalidContactEmail_ShouldFail()
    {
        var request = ValidRequest with
        {
            Contact = new ContactRequest
            {
                Name = "Juan",
                Email = "invalid-email",
                Phone = "+5491155551234",
            },
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor("Contact.Email");
    }

    [Fact]
    public async Task Validate_WithValidVehicle_ShouldPass()
    {
        var request = ValidRequest with
        {
            Vehicle = new VehicleRequest
            {
                Make = "Toyota",
                Model = "Corolla",
                Year = 2022,
                LicensePlate = "AB123CD",
            },
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithInvalidVehicleLicensePlate_ShouldFail()
    {
        var request = ValidRequest with
        {
            Vehicle = new VehicleRequest
            {
                Make = "Toyota",
                LicensePlate = "INVALID",
            },
        };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor("Vehicle.LicensePlate");
    }

    [Fact]
    public async Task Validate_WithNullVehicle_ShouldPass()
    {
        var request = ValidRequest with { Vehicle = null };

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
