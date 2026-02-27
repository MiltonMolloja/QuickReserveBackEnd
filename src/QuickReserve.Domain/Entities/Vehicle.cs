// -----------------------------------------------------------------------
// <copyright file="Vehicle.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Domain.Entities;

using QuickReserve.Domain.ValueObjects;

/// <summary>
/// Entity representing vehicle information for an appointment (optional).
/// </summary>
public sealed class Vehicle
{
    // EF Core parameterless constructor
    private Vehicle()
    {
    }

    private Vehicle(string? make, string? model, int? year, LicensePlate? licensePlate)
    {
        Make = make;
        Model = model;
        Year = year;
        LicensePlate = licensePlate;
    }

    /// <summary>
    /// Gets the vehicle make (brand).
    /// </summary>
    public string? Make { get; private set; }

    /// <summary>
    /// Gets the vehicle model.
    /// </summary>
    public string? Model { get; private set; }

    /// <summary>
    /// Gets the vehicle year.
    /// </summary>
    public int? Year { get; private set; }

    /// <summary>
    /// Gets the vehicle license plate.
    /// </summary>
    public LicensePlate? LicensePlate { get; private set; }

    /// <summary>
    /// Creates a new <see cref="Vehicle"/> instance.
    /// Returns null if all fields are empty (vehicle is optional).
    /// </summary>
    /// <param name="make">The vehicle make.</param>
    /// <param name="model">The vehicle model.</param>
    /// <param name="year">The vehicle year.</param>
    /// <param name="licensePlate">The vehicle license plate.</param>
    /// <returns>A new <see cref="Vehicle"/> instance, or null if all fields are empty.</returns>
    public static Vehicle? Create(string? make, string? model, int? year, string? licensePlate)
    {
        if (string.IsNullOrWhiteSpace(make) &&
            string.IsNullOrWhiteSpace(model) &&
            !year.HasValue &&
            string.IsNullOrWhiteSpace(licensePlate))
        {
            return null;
        }

        return new Vehicle(
            make?.Trim(),
            model?.Trim(),
            year,
            LicensePlate.Create(licensePlate));
    }
}
