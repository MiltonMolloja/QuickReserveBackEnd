// -----------------------------------------------------------------------
// <copyright file="InvalidLicensePlateException.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Domain.Exceptions;

/// <summary>
/// Exception thrown when a license plate format is invalid.
/// </summary>
public sealed class InvalidLicensePlateException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidLicensePlateException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public InvalidLicensePlateException(string message)
        : base(message)
    {
    }
}
