// -----------------------------------------------------------------------
// <copyright file="InvalidWhatsappException.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Domain.Exceptions;

/// <summary>
/// Exception thrown when a whatsapp number is invalid.
/// </summary>
public sealed class InvalidWhatsappException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidWhatsappException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public InvalidWhatsappException(string message)
        : base(message)
    {
    }
}
