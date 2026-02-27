// -----------------------------------------------------------------------
// <copyright file="IAppointmentRepository.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Domain.Interfaces;

using QuickReserve.Domain.Entities;

/// <summary>
/// Repository port for appointment persistence operations.
/// </summary>
public interface IAppointmentRepository
{
    /// <summary>
    /// Gets an appointment by its unique identifier.
    /// </summary>
    /// <param name="id">The appointment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The appointment if found; otherwise null.</returns>
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all appointments ordered by creation date descending.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of appointments.</returns>
    Task<IReadOnlyList<Appointment>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new appointment to the repository.
    /// </summary>
    /// <param name="appointment">The appointment to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The persisted appointment.</returns>
    Task<Appointment> AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
}
