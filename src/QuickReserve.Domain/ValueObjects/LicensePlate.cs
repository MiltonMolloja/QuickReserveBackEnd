// -----------------------------------------------------------------------
// <copyright file="LicensePlate.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Domain.ValueObjects;

using System.Text.RegularExpressions;
using QuickReserve.Domain.Exceptions;

/// <summary>
/// Value Object representing a validated Argentine license plate.
/// Supports old format (ABC123) and new Mercosur format (AB123CD).
/// </summary>
public sealed partial class LicensePlate : IEquatable<LicensePlate>
{
    private static readonly Regex LicensePlatePattern = GeneratedLicensePlateRegex();

    private LicensePlate(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the license plate value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Converts a <see cref="LicensePlate"/> to its string representation implicitly.
    /// </summary>
    /// <param name="plate">The license plate value object.</param>
    public static implicit operator string?(LicensePlate? plate) => plate?.Value;

    /// <summary>
    /// Creates a new <see cref="LicensePlate"/> instance after validation.
    /// Returns null if the input is null or empty (license plate is optional).
    /// </summary>
    /// <param name="licensePlate">The license plate string.</param>
    /// <returns>A validated <see cref="LicensePlate"/> instance, or null if input is empty.</returns>
    /// <exception cref="InvalidLicensePlateException">Thrown when the license plate has an invalid format.</exception>
    public static LicensePlate? Create(string? licensePlate)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
        {
            return null;
        }

        var upperPlate = licensePlate.Trim().ToUpperInvariant();

        return !LicensePlatePattern.IsMatch(upperPlate)
            ? throw new InvalidLicensePlateException(
                $"El formato de la patente '{licensePlate}' no es valido. Use formato ABC123 o AB123CD.")
            : new LicensePlate(upperPlate);
    }

    /// <inheritdoc/>
    public bool Equals(LicensePlate? other) => other is not null && Value == other.Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is LicensePlate other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc/>
    public override string ToString() => Value;

    [GeneratedRegex(@"^([A-Z]{3}\d{3}|[A-Z]{2}\d{3}[A-Z]{2})$", RegexOptions.Compiled)]
    private static partial Regex GeneratedLicensePlateRegex();
}
