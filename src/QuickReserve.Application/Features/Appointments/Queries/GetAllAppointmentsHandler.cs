// -----------------------------------------------------------------------
// <copyright file="GetAllAppointmentsHandler.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Application.Features.Appointments.Queries;

using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using QuickReserve.Application.DTOs.Responses;
using QuickReserve.Domain.Interfaces;

/// <summary>
/// Handler for <see cref="GetAllAppointmentsQuery"/>.
/// </summary>
public sealed partial class GetAllAppointmentsHandler
    : IRequestHandler<GetAllAppointmentsQuery, ApiResponse<IReadOnlyList<AppointmentResponse>>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ILogger<GetAllAppointmentsHandler> _logger;

    public GetAllAppointmentsHandler(
        IAppointmentRepository appointmentRepository,
        ILogger<GetAllAppointmentsHandler> logger)
    {
        _appointmentRepository = appointmentRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<IReadOnlyList<AppointmentResponse>>> Handle(
        GetAllAppointmentsQuery request,
        CancellationToken cancellationToken)
    {
        LogGettingAllAppointments(_logger);

        var appointments = await _appointmentRepository.GetAllAsync(cancellationToken);
        var response = appointments.Adapt<IReadOnlyList<AppointmentResponse>>();

        LogRetrievedAppointments(_logger, appointments.Count);

        return ApiResponse<IReadOnlyList<AppointmentResponse>>.Ok(response);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Getting all appointments")]
    private static partial void LogGettingAllAppointments(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Retrieved {Count} appointments")]
    private static partial void LogRetrievedAppointments(ILogger logger, int count);
}
