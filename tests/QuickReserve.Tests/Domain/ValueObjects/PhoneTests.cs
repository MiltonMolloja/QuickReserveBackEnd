// -----------------------------------------------------------------------
// <copyright file="PhoneTests.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Tests.Domain.ValueObjects;

using FluentAssertions;
using QuickReserve.Domain.Exceptions;
using QuickReserve.Domain.ValueObjects;

public class PhoneTests
{
    [Theory]
    [InlineData("+5491155551234")]
    [InlineData("5491155551234")]
    [InlineData("+541155551234")]
    public void Create_WithValidPhone_ShouldSucceed(string validPhone)
    {
        var phone = Phone.Create(validPhone);

        phone.Value.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("+54 911 5555 1234", "+5491155551234")]
    [InlineData("+54 (911) 5555-1234", "+5491155551234")]
    public void Create_ShouldCleanFormatting(string input, string expected)
    {
        var phone = Phone.Create(input);

        phone.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyPhone_ShouldThrowInvalidPhoneException(string? invalidPhone)
    {
        var act = () => Phone.Create(invalidPhone!);

        act.Should().Throw<InvalidPhoneException>()
            .WithMessage("*vacio*");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abc")]
    [InlineData("0000000")]
    public void Create_WithInvalidFormat_ShouldThrowInvalidPhoneException(string invalidPhone)
    {
        var act = () => Phone.Create(invalidPhone);

        act.Should().Throw<InvalidPhoneException>()
            .WithMessage("*formato*");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeTrue()
    {
        var phone1 = Phone.Create("+5491155551234");
        var phone2 = Phone.Create("+54 911 5555 1234");

        phone1.Should().Be(phone2);
    }
}
