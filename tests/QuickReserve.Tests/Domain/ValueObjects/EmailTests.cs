// -----------------------------------------------------------------------
// <copyright file="EmailTests.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Tests.Domain.ValueObjects;

using FluentAssertions;
using QuickReserve.Domain.Exceptions;
using QuickReserve.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.org")]
    [InlineData("user+tag@example.co.uk")]
    public void Create_WithValidEmail_ShouldSucceed(string validEmail)
    {
        var email = Email.Create(validEmail);

        email.Value.Should().Be(validEmail.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyEmail_ShouldThrowInvalidEmailException(string? invalidEmail)
    {
        var act = () => Email.Create(invalidEmail!);

        act.Should().Throw<InvalidEmailException>()
            .WithMessage("*vacio*");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user@.com")]
    public void Create_WithInvalidFormat_ShouldThrowInvalidEmailException(string invalidEmail)
    {
        var act = () => Email.Create(invalidEmail);

        act.Should().Throw<InvalidEmailException>()
            .WithMessage("*formato*");
    }

    [Fact]
    public void Create_ShouldNormalizeToLowerCase()
    {
        var email = Email.Create("TEST@EXAMPLE.COM");

        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeTrue()
    {
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("TEST@EXAMPLE.COM");

        email1.Should().Be(email2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldBeFalse()
    {
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("other@example.com");

        email1.Should().NotBe(email2);
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnStringValue()
    {
        var email = Email.Create("test@example.com");
        string result = email;

        result.Should().Be("test@example.com");
    }
}
