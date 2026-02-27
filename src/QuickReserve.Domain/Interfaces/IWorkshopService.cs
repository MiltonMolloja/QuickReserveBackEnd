// -----------------------------------------------------------------------
// <copyright file="IWorkshopService.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Domain.Interfaces;

/// <summary>
/// Port for workshop-related operations (implemented by infrastructure layer).
/// </summary>
public interface IWorkshopService
{
    /// <summary>
    /// Checks whether a workshop with the given place ID exists and is active.
    /// </summary>
    /// <param name="placeId">The workshop place ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the workshop is active; otherwise false.</returns>
    Task<bool> IsActiveWorkshopAsync(int placeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active workshops from the external service.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of active workshop information.</returns>
    Task<IReadOnlyList<WorkshopInfo>> GetActiveWorkshopsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Record representing workshop information from the external API.
/// </summary>
/// <param name="Id">The workshop ID.</param>
/// <param name="Name">The workshop name.</param>
/// <param name="Address">The workshop address.</param>
/// <param name="Email">The workshop email.</param>
/// <param name="Whatsapp">The workshop WhatsApp number.</param>
public sealed record WorkshopInfo(
    int Id,
    string Name,
    string? Address,
    string? Email,
    string? Whatsapp);
