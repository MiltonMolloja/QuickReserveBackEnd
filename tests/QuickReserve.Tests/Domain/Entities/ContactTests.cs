// -----------------------------------------------------------------------
// <copyright file="ContactTests.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Tests.Domain.Entities;

using FluentAssertions;
using QuickReserve.Domain.Entities;
using QuickReserve.Domain.Exceptions;

public class ContactTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var contact = Contact.Create("Juan Perez", "juan@email.com", "+5491155551234");

        contact.Name.Should().Be("Juan Perez");
        contact.Email.Value.Should().Be("juan@email.com");
        contact.Whatsapp.Value.Should().Be("+5491155551234");
    }

    [Fact]
    public void Create_ShouldTrimName()
    {
        var contact = Contact.Create("  Juan Perez  ", "juan@email.com", "+5491155551234");

        contact.Name.Should().Be("Juan Perez");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyName_ShouldThrowDomainException(string? emptyName)
    {
        var act = () => Contact.Create(emptyName!, "juan@email.com", "+5491155551234");

        act.Should().Throw<DomainException>()
            .WithMessage("*nombre*vacio*");
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldThrowInvalidEmailException()
    {
        var act = () => Contact.Create("Juan", "invalid-email", "+5491155551234");

        act.Should().Throw<InvalidEmailException>();
    }

    [Fact]
    public void Create_WithInvalidWhatsapp_ShouldThrowInvalidWhatsappException()
    {
        var act = () => Contact.Create("Juan", "juan@email.com", "123");

        act.Should().Throw<InvalidWhatsappException>();
    }
}
