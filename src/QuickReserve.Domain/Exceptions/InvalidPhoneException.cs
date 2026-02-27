// -----------------------------------------------------------------------
// <copyright file="InvalidPhoneException.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Domain.Exceptions;

/// <summary>
/// Exception thrown when a phone number is invalid.
/// </summary>
public sealed class InvalidPhoneException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidPhoneException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public InvalidPhoneException(string message)
        : base(message)
    {
    }
}
