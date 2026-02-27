// -----------------------------------------------------------------------
// <copyright file="CreateAppointmentCommand.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Application.Features.Appointments.Commands;

using MediatR;
using QuickReserve.Application.DTOs.Requests;
using QuickReserve.Application.DTOs.Responses;

/// <summary>
/// Command to create a new appointment. Wraps the request DTO.
/// </summary>
/// <param name="Request">The appointment creation request.</param>
public sealed record CreateAppointmentCommand(CreateAppointmentRequest Request)
    : IRequest<ApiResponse<AppointmentResponse>>;
