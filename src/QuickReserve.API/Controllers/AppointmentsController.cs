// -----------------------------------------------------------------------
// <copyright file="AppointmentsController.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using QuickReserve.Application.DTOs.Requests;
using QuickReserve.Application.DTOs.Responses;
using QuickReserve.Application.Features.Appointments.Commands;
using QuickReserve.Application.Features.Appointments.Queries;

/// <summary>
/// Controller for managing workshop appointments (turnos).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class AppointmentsController : ControllerBase
{
    private readonly IMediator mediator;
    private readonly ILogger<AppointmentsController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppointmentsController"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR mediator.</param>
    /// <param name="logger">The logger.</param>
    public AppointmentsController(IMediator mediator, ILogger<AppointmentsController> logger)
    {
        this.mediator = mediator;
        this.logger = logger;
    }

    /// <summary>
    /// Gets all appointments.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all appointments.</returns>
    /// <response code="200">Appointments retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AppointmentResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        logger.LogInformation("GET /api/appointments");

        var result = await mediator.Send(new GetAllAppointmentsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new appointment.
    /// </summary>
    /// <param name="request">The appointment creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created appointment.</returns>
    /// <response code="201">Appointment created successfully.</response>
    /// <response code="400">Invalid data or inactive workshop.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("POST /api/appointments for place {PlaceId}", request.PlaceId);

        var result = await mediator.Send(new CreateAppointmentCommand(request), cancellationToken);

        return !result.Success
            ? BadRequest(result)
            : CreatedAtAction(nameof(GetAll), result);
    }
}
