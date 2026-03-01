// -----------------------------------------------------------------------
// <copyright file="ContactRequestValidator.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Application.Validators;

using FluentValidation;
using QuickReserve.Application.DTOs.Requests;

/// <summary>
/// Validator for <see cref="ContactRequest"/>.
/// </summary>
public sealed class ContactRequestValidator : AbstractValidator<ContactRequest>
{
    public ContactRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre es requerido.")
            .MaximumLength(200)
            .WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El email es requerido.")
            .EmailAddress()
            .WithMessage("El formato del email no es valido.")
            .MaximumLength(254)
            .WithMessage("El email no puede exceder 254 caracteres.");

        RuleFor(x => x.Whatsapp)
            .NotEmpty()
            .WithMessage("El telefono es requerido.")
            .Matches(@"^\+?[1-9][\d\s\-\(\)]{6,20}$")
            .WithMessage("El formato del telefono no es valido.");
    }
}
