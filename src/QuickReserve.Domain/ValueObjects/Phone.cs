// -----------------------------------------------------------------------
// <copyright file="Whatsapp.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Domain.ValueObjects;

using System.Text.RegularExpressions;
using QuickReserve.Domain.Exceptions;

/// <summary>
/// Value Object representing a validated whatsapp number.
/// </summary>
public sealed partial class Whatsapp : IEquatable<Whatsapp>
{
    private static readonly Regex WhatsappPattern = GeneratedWhatsappRegex();

    private Whatsapp(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the whatsapp number value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Converts a <see cref="Whatsapp"/> to its string representation implicitly.
    /// </summary>
    /// <param name="whatsapp">The whatsapp value object.</param>
    public static implicit operator string(Whatsapp whatsapp) => whatsapp.Value;

    /// <summary>
    /// Creates a new <see cref="Whatsapp"/> instance after validation.
    /// </summary>
    /// <param name="whatsapp">The whatsapp number string.</param>
    /// <returns>A validated <see cref="Whatsapp"/> instance.</returns>
    /// <exception cref="InvalidWhatsappException">Thrown when the whatsapp is null, empty, or has an invalid format.</exception>
    public static Whatsapp Create(string whatsapp)
    {
        if (string.IsNullOrWhiteSpace(whatsapp))
        {
            throw new InvalidWhatsappException("El telefono no puede estar vacio.");
        }

        var cleanedWhatsapp = CleanWhatsappNumber(whatsapp);

        return !WhatsappPattern.IsMatch(cleanedWhatsapp)
            ? throw new InvalidWhatsappException($"El formato del telefono '{whatsapp}' no es valido.")
            : new Whatsapp(cleanedWhatsapp);
    }

    /// <inheritdoc/>
    public bool Equals(Whatsapp? other) => other is not null && Value == other.Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Whatsapp other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc/>
    public override string ToString() => Value;

    private static string CleanWhatsappNumber(string whatsapp) =>
        CleanWhatsappRegex().Replace(whatsapp, string.Empty);

    [GeneratedRegex(@"[\s\-\(\)]")]
    private static partial Regex CleanWhatsappRegex();

    [GeneratedRegex(@"^\+?[1-9]\d{6,14}$", RegexOptions.Compiled)]
    private static partial Regex GeneratedWhatsappRegex();
}
