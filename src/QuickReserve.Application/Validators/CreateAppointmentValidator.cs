// -----------------------------------------------------------------------
// <copyright file="CreateAppointmentValidator.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Application.Validators;

using FluentValidation;
using QuickReserve.Application.DTOs.Requests;

/// <summary>
/// Validator for <see cref="CreateAppointmentRequest"/>.
/// </summary>
public sealed class CreateAppointmentValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentValidator()
    {
        RuleFor(x => x.PlaceId)
            .GreaterThan(0)
            .WithMessage("El place_id debe ser mayor a 0.");

        RuleFor(x => x.AppointmentAt)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("La fecha del turno debe ser futura.");

        RuleFor(x => x.ServiceType)
            .NotEmpty()
            .WithMessage("El tipo de servicio es requerido.")
            .MaximumLength(100)
            .WithMessage("El tipo de servicio no puede exceder 100 caracteres.");

        RuleFor(x => x.Contact)
            .NotNull()
            .WithMessage("Los datos de contacto son requeridos.")
            .SetValidator(new ContactRequestValidator());

        When(x => x.Vehicle is not null, () =>
            RuleFor(x => x.Vehicle!)
                .SetValidator(new VehicleRequestValidator()));
    }
}
