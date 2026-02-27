// -----------------------------------------------------------------------
// <copyright file="GetAllAppointmentsQuery.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Application.Features.Appointments.Queries;

using MediatR;
using QuickReserve.Application.DTOs.Responses;

/// <summary>
/// Query to get all appointments.
/// </summary>
public sealed record GetAllAppointmentsQuery : IRequest<ApiResponse<IReadOnlyList<AppointmentResponse>>>;
