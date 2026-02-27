// -----------------------------------------------------------------------
// <copyright file="WorkshopsController.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuickReserve.Application.DTOs.Responses;
using QuickReserve.Application.Features.Workshops.Queries;

/// <summary>
/// Controller for querying active workshops from the Tecnom CRM API.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class WorkshopsController : ControllerBase
{
    private readonly IMediator mediator;
    private readonly ILogger<WorkshopsController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkshopsController"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR mediator.</param>
    /// <param name="logger">The logger.</param>
    public WorkshopsController(IMediator mediator, ILogger<WorkshopsController> logger)
    {
        this.mediator = mediator;
        this.logger = logger;
    }

    /// <summary>
    /// Gets all active workshops.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of active workshops.</returns>
    /// <response code="200">Workshops retrieved successfully.</response>
    /// <response code="503">External service unavailable.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<WorkshopResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<WorkshopResponse>>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        logger.LogInformation("GET /api/workshops");

        var result = await mediator.Send(new GetAllWorkshopsQuery(), cancellationToken);

        return !result.Success
            ? StatusCode(StatusCodes.Status503ServiceUnavailable, result)
            : Ok(result);
    }
}
