// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using QuickReserve.API.Middleware;
using QuickReserve.Application;
using QuickReserve.Infrastructure;
using QuickReserve.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------
// Serilog Configuration
// -----------------------------------------------------------------------
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithProcessId()
        .WriteTo.Console();

    // Elasticsearch sink (only when URI is configured and not in Testing)
    var elasticUri = context.Configuration["Elasticsearch:Uri"];
    if (!string.IsNullOrEmpty(elasticUri) && !context.HostingEnvironment.IsEnvironment("Testing"))
    {
        configuration.WriteTo.Elasticsearch(
            new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(elasticUri))
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = Serilog.Sinks.Elasticsearch.AutoRegisterTemplateVersion.ESv8,
                IndexFormat = $"quickreserve-logs-{{0:yyyy.MM.dd}}",
            });
    }
});

// -----------------------------------------------------------------------
// Services Registration
// -----------------------------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower);

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

// CORS for Angular frontend (configurable via environment variable)
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200", "https://localhost:4200"];

builder.Services.AddCors(options =>
    options.AddPolicy("AllowAngular", policy =>
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()));

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

// Application & Infrastructure layers (CQRS + MediatR, EF Core, Tecnom API)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// -----------------------------------------------------------------------
// Middleware Pipeline
// -----------------------------------------------------------------------
var app = builder.Build();

// Custom middleware (early in pipeline)
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

// Serilog request logging
app.UseSerilogRequestLogging(options =>
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
    });

// Swagger enabled in all environments (challenge demo)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "QuickReserve API v1");
    c.RoutePrefix = "swagger";
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAngular");
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});

// Seed sample data (in-memory DB requires seeding on every startup)
await AppointmentSeeder.SeedAsync(app.Services);

await app.RunAsync();
