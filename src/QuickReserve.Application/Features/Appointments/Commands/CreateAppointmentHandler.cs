// -----------------------------------------------------------------------
// <copyright file="CreateAppointmentHandler.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Application.Features.Appointments.Commands;

using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using QuickReserve.Application.DTOs.Responses;
using QuickReserve.Domain.Exceptions;
using QuickReserve.Domain.Interfaces;
using QuickReserve.Domain.Services;

/// <summary>
/// Handler for <see cref="CreateAppointmentCommand"/>.
/// Validates the request, creates the appointment via domain service, and persists it.
/// </summary>
public sealed partial class CreateAppointmentHandler
    : IRequestHandler<CreateAppointmentCommand, ApiResponse<AppointmentResponse>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly AppointmentDomainService _domainService;
    private readonly IValidator<DTOs.Requests.CreateAppointmentRequest> _validator;
    private readonly ILogger<CreateAppointmentHandler> _logger;

    public CreateAppointmentHandler(
        IAppointmentRepository appointmentRepository,
        AppointmentDomainService domainService,
        IValidator<DTOs.Requests.CreateAppointmentRequest> validator,
        ILogger<CreateAppointmentHandler> logger)
    {
        _appointmentRepository = appointmentRepository;
        _domainService = domainService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ApiResponse<AppointmentResponse>> Handle(
        CreateAppointmentCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        LogCreatingAppointment(_logger, request.PlaceId);

        // Validate request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Validation failed for appointment: {Errors}", string.Join(", ", errors));
            return ApiResponse<AppointmentResponse>.Fail(errors);
        }

        try
        {
            // Create appointment via domain service (validates workshop is active)
            var appointment = await _domainService.CreateAppointmentAsync(
                request.PlaceId,
                request.AppointmentAt,
                request.ServiceType,
                request.Contact.Name,
                request.Contact.Email,
                request.Contact.Phone,
                request.Vehicle?.Make,
                request.Vehicle?.Model,
                request.Vehicle?.Year,
                request.Vehicle?.LicensePlate,
                cancellationToken);

            // Persist
            await _appointmentRepository.AddAsync(appointment, cancellationToken);

            LogAppointmentCreated(_logger, appointment.Id);

            // Map to response
            var response = appointment.Adapt<AppointmentResponse>();
            return ApiResponse<AppointmentResponse>.Ok(response);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain validation failed: {Message}", ex.Message);
            return ApiResponse<AppointmentResponse>.Fail(ex.Message);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Creating appointment for place {PlaceId}")]
    private static partial void LogCreatingAppointment(ILogger logger, int placeId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Appointment {AppointmentId} created successfully")]
    private static partial void LogAppointmentCreated(ILogger logger, Guid appointmentId);
}
