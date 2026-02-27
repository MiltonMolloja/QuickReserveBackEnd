// -----------------------------------------------------------------------
// <copyright file="Phone.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Domain.ValueObjects;

using System.Text.RegularExpressions;
using QuickReserve.Domain.Exceptions;

/// <summary>
/// Value Object representing a validated phone number.
/// </summary>
public sealed partial class Phone : IEquatable<Phone>
{
    private static readonly Regex PhonePattern = GeneratedPhoneRegex();

    private Phone(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the phone number value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Converts a <see cref="Phone"/> to its string representation implicitly.
    /// </summary>
    /// <param name="phone">The phone value object.</param>
    public static implicit operator string(Phone phone) => phone.Value;

    /// <summary>
    /// Creates a new <see cref="Phone"/> instance after validation.
    /// </summary>
    /// <param name="phone">The phone number string.</param>
    /// <returns>A validated <see cref="Phone"/> instance.</returns>
    /// <exception cref="InvalidPhoneException">Thrown when the phone is null, empty, or has an invalid format.</exception>
    public static Phone Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new InvalidPhoneException("El telefono no puede estar vacio.");
        }

        var cleanedPhone = CleanPhoneNumber(phone);

        return !PhonePattern.IsMatch(cleanedPhone)
            ? throw new InvalidPhoneException($"El formato del telefono '{phone}' no es valido.")
            : new Phone(cleanedPhone);
    }

    /// <inheritdoc/>
    public bool Equals(Phone? other) => other is not null && Value == other.Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Phone other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc/>
    public override string ToString() => Value;

    private static string CleanPhoneNumber(string phone) =>
        CleanPhoneRegex().Replace(phone, string.Empty);

    [GeneratedRegex(@"[\s\-\(\)]")]
    private static partial Regex CleanPhoneRegex();

    [GeneratedRegex(@"^\+?[1-9]\d{6,14}$", RegexOptions.Compiled)]
    private static partial Regex GeneratedPhoneRegex();
}
