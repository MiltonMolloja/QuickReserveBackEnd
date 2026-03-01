// -----------------------------------------------------------------------
// <copyright file="Contact.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Domain.Entities;

using QuickReserve.Domain.Exceptions;
using QuickReserve.Domain.ValueObjects;

/// <summary>
/// Entity representing contact information for an appointment.
/// </summary>
public sealed class Contact
{
    // EF Core parameterless constructor
    private Contact()
    {
        Name = string.Empty;
        Email = null!;
        Whatsapp = null!;
    }

    private Contact(string name, Email email, Whatsapp whatsapp)
    {
        Name = name;
        Email = email;
        Whatsapp = whatsapp;
    }

    /// <summary>
    /// Gets the contact name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the contact email.
    /// </summary>
    public Email Email { get; private set; }

    /// <summary>
    /// Gets the contact whatsapp number.
    /// </summary>
    public Whatsapp Whatsapp { get; private set; }

    /// <summary>
    /// Creates a new <see cref="Contact"/> instance with validated data.
    /// </summary>
    /// <param name="name">The contact name.</param>
    /// <param name="email">The contact email address.</param>
    /// <param name="whatsapp">The contact whatsapp number.</param>
    /// <returns>A new <see cref="Contact"/> instance.</returns>
    /// <exception cref="DomainException">Thrown when the name is empty.</exception>
    public static Contact Create(string name, string email, string whatsapp)
    {
        return string.IsNullOrWhiteSpace(name)
            ? throw new DomainException("El nombre del contacto no puede estar vacio.")
            : new Contact(
                name.Trim(),
                Email.Create(email),
                Whatsapp.Create(whatsapp));
    }
}
