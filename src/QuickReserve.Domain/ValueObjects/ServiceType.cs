// -----------------------------------------------------------------------
// <copyright file="ServiceType.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Domain.ValueObjects;

using QuickReserve.Domain.Exceptions;

/// <summary>
/// Value Object representing a service type for an appointment.
/// Known types are normalized; unknown types are accepted as-is.
/// </summary>
public sealed class ServiceType : IEquatable<ServiceType>
{
    private static readonly HashSet<string> ValidServiceTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Mantenimiento",
        "Reparacion",
        "Revision",
        "Diagnostico",
        "Service",
        "Otro",
    };

    private ServiceType(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the service type value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Converts a <see cref="ServiceType"/> to its string representation implicitly.
    /// </summary>
    /// <param name="serviceType">The service type value object.</param>
    public static implicit operator string(ServiceType serviceType) => serviceType.Value;

    /// <summary>
    /// Creates a new <see cref="ServiceType"/> instance after validation.
    /// Known service types are normalized to their canonical form.
    /// </summary>
    /// <param name="serviceType">The service type string.</param>
    /// <returns>A validated <see cref="ServiceType"/> instance.</returns>
    /// <exception cref="DomainException">Thrown when the service type is null or empty.</exception>
    public static ServiceType Create(string serviceType)
    {
        if (string.IsNullOrWhiteSpace(serviceType))
        {
            throw new DomainException("El tipo de servicio no puede estar vacio.");
        }

        var trimmedType = serviceType.Trim();

        // Normalize known types, accept unknown types as-is
        var normalizedType = ValidServiceTypes
            .FirstOrDefault(v => v.Equals(trimmedType, StringComparison.OrdinalIgnoreCase))
            ?? trimmedType;

        return new ServiceType(normalizedType);
    }

    /// <inheritdoc/>
    public bool Equals(ServiceType? other) =>
        other is not null && Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ServiceType other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Value.ToUpperInvariant().GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc/>
    public override string ToString() => Value;
}
