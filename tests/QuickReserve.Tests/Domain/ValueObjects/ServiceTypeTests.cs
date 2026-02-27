// -----------------------------------------------------------------------
// <copyright file="ServiceTypeTests.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Tests.Domain.ValueObjects;

using FluentAssertions;
using QuickReserve.Domain.Exceptions;
using QuickReserve.Domain.ValueObjects;

public class ServiceTypeTests
{
    [Theory]
    [InlineData("Mantenimiento")]
    [InlineData("Reparacion")]
    [InlineData("Revision")]
    [InlineData("Diagnostico")]
    [InlineData("Service")]
    [InlineData("Otro")]
    public void Create_WithKnownType_ShouldNormalize(string knownType)
    {
        var serviceType = ServiceType.Create(knownType);

        serviceType.Value.Should().Be(knownType);
    }

    [Fact]
    public void Create_WithKnownTypeDifferentCase_ShouldNormalize()
    {
        var serviceType = ServiceType.Create("mantenimiento");

        serviceType.Value.Should().Be("Mantenimiento");
    }

    [Fact]
    public void Create_WithUnknownType_ShouldAcceptAsIs()
    {
        var serviceType = ServiceType.Create("Pintura");

        serviceType.Value.Should().Be("Pintura");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyType_ShouldThrowDomainException(string? emptyType)
    {
        var act = () => ServiceType.Create(emptyType!);

        act.Should().Throw<DomainException>()
            .WithMessage("*tipo de servicio*vacio*");
    }

    [Fact]
    public void Equals_WithSameValueDifferentCase_ShouldBeTrue()
    {
        var type1 = ServiceType.Create("Mantenimiento");
        var type2 = ServiceType.Create("mantenimiento");

        type1.Should().Be(type2);
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnStringValue()
    {
        var serviceType = ServiceType.Create("Revision");
        string result = serviceType;

        result.Should().Be("Revision");
    }
}
