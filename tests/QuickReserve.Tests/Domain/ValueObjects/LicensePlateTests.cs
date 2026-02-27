// -----------------------------------------------------------------------
// <copyright file="LicensePlateTests.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Tests.Domain.ValueObjects;

using FluentAssertions;
using QuickReserve.Domain.Exceptions;
using QuickReserve.Domain.ValueObjects;

public class LicensePlateTests
{
    [Theory]
    [InlineData("ABC123")]
    [InlineData("AB123CD")]
    [InlineData("abc123")]
    [InlineData("ab123cd")]
    public void Create_WithValidPlate_ShouldSucceed(string validPlate)
    {
        var plate = LicensePlate.Create(validPlate);

        plate.Should().NotBeNull();
        plate!.Value.Should().Be(validPlate.ToUpperInvariant());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyPlate_ShouldReturnNull(string? emptyPlate)
    {
        var plate = LicensePlate.Create(emptyPlate);

        plate.Should().BeNull();
    }

    [Theory]
    [InlineData("A1")]
    [InlineData("ABCDEF")]
    [InlineData("123456")]
    [InlineData("AB12CD3")]
    public void Create_WithInvalidFormat_ShouldThrowInvalidLicensePlateException(string invalidPlate)
    {
        var act = () => LicensePlate.Create(invalidPlate);

        act.Should().Throw<InvalidLicensePlateException>()
            .WithMessage("*formato*patente*");
    }

    [Fact]
    public void Create_ShouldNormalizeToUpperCase()
    {
        var plate = LicensePlate.Create("ab123cd");

        plate!.Value.Should().Be("AB123CD");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeTrue()
    {
        var plate1 = LicensePlate.Create("AB123CD");
        var plate2 = LicensePlate.Create("ab123cd");

        plate1.Should().Be(plate2);
    }

    [Fact]
    public void ImplicitConversion_NullPlate_ShouldReturnNull()
    {
        LicensePlate? plate = null;
        string? result = plate;

        result.Should().BeNull();
    }
}
