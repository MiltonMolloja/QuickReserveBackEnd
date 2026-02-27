// -----------------------------------------------------------------------
// <copyright file="GetAllWorkshopsQuery.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Application.Features.Workshops.Queries;

using MediatR;
using QuickReserve.Application.DTOs.Responses;

/// <summary>
/// Query to get all active workshops from the external API.
/// </summary>
public sealed record GetAllWorkshopsQuery : IRequest<ApiResponse<IReadOnlyList<WorkshopResponse>>>;
