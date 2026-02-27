// -----------------------------------------------------------------------
// <copyright file="VehicleRequestValidator.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Application.Validators;

using FluentValidation;
using QuickReserve.Application.DTOs.Requests;

/// <summary>
/// Validator for <see cref="VehicleRequest"/>.
/// </summary>
public sealed class VehicleRequestValidator : AbstractValidator<VehicleRequest>
{
    public VehicleRequestValidator()
    {
        RuleFor(x => x.Make)
            .MaximumLength(100)
            .WithMessage("La marca no puede exceder 100 caracteres.");

        RuleFor(x => x.Model)
            .MaximumLength(100)
            .WithMessage("El modelo no puede exceder 100 caracteres.");

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
            .When(x => x.Year.HasValue)
            .WithMessage($"El ano debe estar entre 1900 y {DateTime.UtcNow.Year + 1}.");

        RuleFor(x => x.LicensePlate)
            .Matches(@"^([A-Za-z]{3}\d{3}|[A-Za-z]{2}\d{3}[A-Za-z]{2})$")
            .When(x => !string.IsNullOrWhiteSpace(x.LicensePlate))
            .WithMessage("El formato de la patente no es valido. Use ABC123 o AB123CD.");
    }
}
