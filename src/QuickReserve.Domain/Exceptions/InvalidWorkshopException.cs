// -----------------------------------------------------------------------
// <copyright file="InvalidWorkshopException.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Domain.Exceptions;

/// <summary>
/// Exception thrown when a workshop does not exist or is not active.
/// </summary>
public sealed class InvalidWorkshopException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidWorkshopException"/> class.
    /// </summary>
    /// <param name="placeId">The workshop place ID that was invalid.</param>
    public InvalidWorkshopException(int placeId)
        : base($"El taller con ID {placeId} no existe o no esta activo.")
    {
        PlaceId = placeId;
    }

    /// <summary>
    /// Gets the workshop place ID that caused the exception.
    /// </summary>
    public int PlaceId { get; }
}
