// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

// ---------------------------------------------------------------------------
// Serilog Bootstrap Logger (captures startup errors)
// ---------------------------------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting QuickReserve API");

    var builder = WebApplication.CreateBuilder(args);

    // -----------------------------------------------------------------------
    // Serilog Configuration
    // -----------------------------------------------------------------------
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithProcessId()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(
            new Uri(context.Configuration["Elasticsearch:Uri"] ?? "http://localhost:9200"))
        {
            AutoRegisterTemplate = true,
            AutoRegisterTemplateVersion = Serilog.Sinks.Elasticsearch.AutoRegisterTemplateVersion.ESv8,
            IndexFormat = $"quickreserve-logs-{{0:yyyy.MM.dd}}",
        }));

    // -----------------------------------------------------------------------
    // Services Registration
    // -----------------------------------------------------------------------
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
        });

    builder.Services.AddEndpointsApiExplorer();

    // Swagger / OpenAPI
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
        {
            Title = "QuickReserve API",
            Version = "v1",
            Description = "API para reserva de turnos en talleres - Challenge Tecnom",
            Contact = new Microsoft.OpenApi.OpenApiContact
            {
                Name = "Milton",
            },
        });

        // Include XML comments
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    // CORS for Angular frontend
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngular", policy =>
        {
            policy
                .WithOrigins("http://localhost:4200", "https://localhost:4200")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    // Health Checks
    var healthChecks = builder.Services.AddHealthChecks();

    var redisConnection = builder.Configuration.GetConnectionString("Redis");
    if (!string.IsNullOrEmpty(redisConnection))
    {
        healthChecks.AddRedis(redisConnection, name: "redis", tags: ["infrastructure"]);
    }

    var tecnomBaseUrl = builder.Configuration["TecnomApi:BaseUrl"];
    if (!string.IsNullOrEmpty(tecnomBaseUrl))
    {
        healthChecks.AddUrlGroup(
            new Uri($"{tecnomBaseUrl}places/workshops"),
            name: "tecnom-api",
            tags: ["external"]);
    }

    // TODO: Fase 2-3 - builder.Services.AddApplication();
    // TODO: Fase 4   - builder.Services.AddInfrastructure(builder.Configuration);

    // -----------------------------------------------------------------------
    // Middleware Pipeline
    // -----------------------------------------------------------------------
    var app = builder.Build();

    // Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
        };
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "QuickReserve API v1");
            c.RoutePrefix = string.Empty; // Swagger at root
        });
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAngular");
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// -----------------------------------------------------------------------
// Partial class for integration tests (WebApplicationFactory)
// -----------------------------------------------------------------------
#pragma warning disable CA1050 // Declare types in namespaces

/// <summary>
/// Entry point for the QuickReserve API. Partial class to enable WebApplicationFactory in integration tests.
/// </summary>
public partial class Program
{
}
#pragma warning restore CA1050 // Declare types in namespaces
