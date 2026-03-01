// -----------------------------------------------------------------------
// <copyright file="WhatsappTests.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Tests.Domain.ValueObjects;

using FluentAssertions;
using QuickReserve.Domain.Exceptions;
using QuickReserve.Domain.ValueObjects;

public class WhatsappTests
{
    [Theory]
    [InlineData("+5491155551234")]
    [InlineData("5491155551234")]
    [InlineData("+541155551234")]
    public void Create_WithValidWhatsapp_ShouldSucceed(string validWhatsapp)
    {
        var whatsapp = Whatsapp.Create(validWhatsapp);

        whatsapp.Value.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("+54 911 5555 1234", "+5491155551234")]
    [InlineData("+54 (911) 5555-1234", "+5491155551234")]
    public void Create_ShouldCleanFormatting(string input, string expected)
    {
        var whatsapp = Whatsapp.Create(input);

        whatsapp.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyWhatsapp_ShouldThrowInvalidWhatsappException(string? invalidWhatsapp)
    {
        var act = () => Whatsapp.Create(invalidWhatsapp!);

        act.Should().Throw<InvalidWhatsappException>()
            .WithMessage("*vacio*");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abc")]
    [InlineData("0000000")]
    public void Create_WithInvalidFormat_ShouldThrowInvalidWhatsappException(string invalidWhatsapp)
    {
        var act = () => Whatsapp.Create(invalidWhatsapp);

        act.Should().Throw<InvalidWhatsappException>()
            .WithMessage("*formato*");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeTrue()
    {
        var whatsapp1 = Whatsapp.Create("+5491155551234");
        var whatsapp2 = Whatsapp.Create("+54 911 5555 1234");

        whatsapp1.Should().Be(whatsapp2);
    }
}
