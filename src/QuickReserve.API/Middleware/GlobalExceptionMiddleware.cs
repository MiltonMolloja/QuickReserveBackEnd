// -----------------------------------------------------------------------
// <copyright file="GlobalExceptionMiddleware.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.API.Middleware;

using System.Net;
using System.Text.Json;
using QuickReserve.Application.DTOs.Responses;
using QuickReserve.Domain.Exceptions;

/// <summary>
/// Middleware that catches unhandled exceptions and returns a consistent
/// <see cref="ApiResponse{T}"/> error response.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private readonly RequestDelegate next;
    private readonly ILogger<GlobalExceptionMiddleware> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger.</param>
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            InvalidWorkshopException workshopEx => (HttpStatusCode.BadRequest, workshopEx.Message),
            DomainException domainEx => (HttpStatusCode.BadRequest, domainEx.Message),
            HttpRequestException => (HttpStatusCode.ServiceUnavailable, "Error al conectar con el servicio externo."),
            TaskCanceledException => (HttpStatusCode.RequestTimeout, "La solicitud ha expirado."),
            _ => (HttpStatusCode.InternalServerError, "Ha ocurrido un error interno."),
        };

        logger.LogError(exception, "Exception caught: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.Fail(message);
        var json = JsonSerializer.Serialize(response, JsonOptions);

        await context.Response.WriteAsync(json);
    }
}
