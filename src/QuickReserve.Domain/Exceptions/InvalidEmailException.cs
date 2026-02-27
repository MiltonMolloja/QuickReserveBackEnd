// -----------------------------------------------------------------------
// <copyright file="InvalidEmailException.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Domain.Exceptions;

/// <summary>
/// Exception thrown when an email address is invalid.
/// </summary>
public sealed class InvalidEmailException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidEmailException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public InvalidEmailException(string message)
        : base(message)
    {
    }
}
