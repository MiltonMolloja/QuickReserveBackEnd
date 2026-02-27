// -----------------------------------------------------------------------
// <copyright file="GetAllWorkshopsHandler.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Application.Features.Workshops.Queries;

using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using QuickReserve.Application.DTOs.Responses;
using QuickReserve.Domain.Interfaces;

/// <summary>
/// Handler for <see cref="GetAllWorkshopsQuery"/>.
/// </summary>
public sealed class GetAllWorkshopsHandler
    : IRequestHandler<GetAllWorkshopsQuery, ApiResponse<IReadOnlyList<WorkshopResponse>>>
{
    private readonly IWorkshopService _workshopService;
    private readonly ILogger<GetAllWorkshopsHandler> _logger;

    public GetAllWorkshopsHandler(
        IWorkshopService workshopService,
        ILogger<GetAllWorkshopsHandler> logger)
    {
        _workshopService = workshopService;
        _logger = logger;
    }

    public async Task<ApiResponse<IReadOnlyList<WorkshopResponse>>> Handle(
        GetAllWorkshopsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all active workshops");

        try
        {
            var workshops = await _workshopService.GetActiveWorkshopsAsync(cancellationToken);
            var response = workshops.Adapt<IReadOnlyList<WorkshopResponse>>();

            _logger.LogInformation("Retrieved {Count} active workshops", workshops.Count);

            return ApiResponse<IReadOnlyList<WorkshopResponse>>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workshops from external API");
            return ApiResponse<IReadOnlyList<WorkshopResponse>>.Fail(
                "Error al obtener los talleres. Intente nuevamente.");
        }
    }
}
