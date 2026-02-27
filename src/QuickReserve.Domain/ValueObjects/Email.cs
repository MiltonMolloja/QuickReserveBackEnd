// -----------------------------------------------------------------------
// <copyright file="Email.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Domain.ValueObjects;

using System.Text.RegularExpressions;
using QuickReserve.Domain.Exceptions;

/// <summary>
/// Value Object representing a validated email address.
/// </summary>
public sealed partial class Email : IEquatable<Email>
{
    private static readonly Regex EmailPattern = GeneratedEmailRegex();

    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the email address value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Converts an <see cref="Email"/> to its string representation implicitly.
    /// </summary>
    /// <param name="email">The email value object.</param>
    public static implicit operator string(Email email) => email.Value;

    /// <summary>
    /// Creates a new <see cref="Email"/> instance after validation.
    /// </summary>
    /// <param name="email">The email address string.</param>
    /// <returns>A validated <see cref="Email"/> instance.</returns>
    /// <exception cref="InvalidEmailException">Thrown when the email is null, empty, or has an invalid format.</exception>
    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidEmailException("El email no puede estar vacio.");
        }

        var trimmedEmail = email.Trim().ToLowerInvariant();

        return !EmailPattern.IsMatch(trimmedEmail)
            ? throw new InvalidEmailException($"El formato del email '{email}' no es valido.")
            : new Email(trimmedEmail);
    }

    /// <inheritdoc/>
    public bool Equals(Email? other) => other is not null && Value == other.Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Email other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc/>
    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex GeneratedEmailRegex();
}
