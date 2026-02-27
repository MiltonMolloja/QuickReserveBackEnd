// -----------------------------------------------------------------------
// <copyright file="VehicleTests.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Tests.Domain.Entities;

using FluentAssertions;
using QuickReserve.Domain.Entities;
using QuickReserve.Domain.Exceptions;

public class VehicleTests
{
    [Fact]
    public void Create_WithAllFields_ShouldSucceed()
    {
        var vehicle = Vehicle.Create("Toyota", "Corolla", 2022, "AB123CD");

        vehicle.Should().NotBeNull();
        vehicle!.Make.Should().Be("Toyota");
        vehicle.Model.Should().Be("Corolla");
        vehicle.Year.Should().Be(2022);
        vehicle.LicensePlate!.Value.Should().Be("AB123CD");
    }

    [Fact]
    public void Create_WithPartialFields_ShouldSucceed()
    {
        var vehicle = Vehicle.Create("Toyota", null, null, null);

        vehicle.Should().NotBeNull();
        vehicle!.Make.Should().Be("Toyota");
        vehicle.Model.Should().BeNull();
        vehicle.Year.Should().BeNull();
        vehicle.LicensePlate.Should().BeNull();
    }

    [Fact]
    public void Create_WithAllNullFields_ShouldReturnNull()
    {
        var vehicle = Vehicle.Create(null, null, null, null);

        vehicle.Should().BeNull();
    }

    [Fact]
    public void Create_WithAllEmptyFields_ShouldReturnNull()
    {
        var vehicle = Vehicle.Create(string.Empty, "  ", null, string.Empty);

        vehicle.Should().BeNull();
    }

    [Fact]
    public void Create_WithInvalidLicensePlate_ShouldThrowException()
    {
        var act = () => Vehicle.Create("Toyota", "Corolla", 2022, "INVALID");

        act.Should().Throw<InvalidLicensePlateException>();
    }

    [Fact]
    public void Create_ShouldTrimFields()
    {
        var vehicle = Vehicle.Create("  Toyota  ", "  Corolla  ", 2022, null);

        vehicle.Should().NotBeNull();
        vehicle!.Make.Should().Be("Toyota");
        vehicle.Model.Should().Be("Corolla");
    }
}
