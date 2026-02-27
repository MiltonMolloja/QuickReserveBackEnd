// -----------------------------------------------------------------------
// <copyright file="AppointmentRepository.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using QuickReserve.Domain.Entities;
using QuickReserve.Domain.Interfaces;

/// <summary>
/// EF Core implementation of <see cref="IAppointmentRepository"/>.
/// </summary>
public sealed class AppointmentRepository : IAppointmentRepository
{
    private readonly AppDbContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppointmentRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public AppointmentRepository(AppDbContext context)
    {
        this.context = context;
    }

    /// <inheritdoc/>
    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Appointment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Appointments
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Appointment> AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        await context.Appointments.AddAsync(appointment, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return appointment;
    }
}
