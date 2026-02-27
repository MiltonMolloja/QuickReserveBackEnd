// -----------------------------------------------------------------------
// <copyright file="AppointmentsEndpointTests.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Tests.Integration;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using QuickReserve.Application.DTOs.Requests;
using QuickReserve.Application.DTOs.Responses;

/// <summary>
/// Integration tests for the /api/appointments endpoints.
/// </summary>
public sealed class AppointmentsEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private readonly HttpClient client;

    public AppointmentsEndpointTests(CustomWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithEmptyList()
    {
        // Act
        var response = await client.GetAsync("/api/appointments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<AppointmentResponse>>>(content, JsonOptions);

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateAppointmentRequest
        {
            PlaceId = 1, // Active workshop in mock
            AppointmentAt = DateTime.UtcNow.AddDays(7),
            ServiceType = "Mantenimiento",
            Contact = new ContactRequest
            {
                Name = "Juan Perez",
                Email = "juan@example.com",
                Phone = "+5491155551234",
            },
            Vehicle = new VehicleRequest
            {
                Make = "Toyota",
                Model = "Corolla",
                Year = 2023,
                LicensePlate = "AB123CD",
            },
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/appointments", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<AppointmentResponse>>(content, JsonOptions);

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PlaceId.Should().Be(1);
        result.Data.Contact.Should().NotBeNull();
        result.Data.Contact!.Name.Should().Be("Juan Perez");
    }

    [Fact]
    public async Task Create_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange - missing required fields
        var request = new CreateAppointmentRequest
        {
            PlaceId = 0, // Invalid
            AppointmentAt = DateTime.UtcNow.AddDays(-1), // Past date
            ServiceType = string.Empty, // Empty
            Contact = null!, // Null
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/appointments", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_ThenGetAll_ShouldReturnCreatedAppointment()
    {
        // Arrange
        var request = new CreateAppointmentRequest
        {
            PlaceId = 2, // Active workshop
            AppointmentAt = DateTime.UtcNow.AddDays(14),
            ServiceType = "Revision",
            Contact = new ContactRequest
            {
                Name = "Maria Garcia",
                Email = "maria@example.com",
                Phone = "+5491166662345",
            },
        };

        // Act - Create
        var createResponse = await client.PostAsJsonAsync("/api/appointments", request, JsonOptions);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Get All
        var getResponse = await client.GetAsync("/api/appointments");
        var content = await getResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<AppointmentResponse>>>(content, JsonOptions);

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().Contain(a => a.PlaceId == 2);
    }

    [Fact]
    public async Task Create_WithInactiveWorkshop_ShouldReturnBadRequest()
    {
        // Arrange - PlaceId 3 is inactive in mock
        var request = new CreateAppointmentRequest
        {
            PlaceId = 3,
            AppointmentAt = DateTime.UtcNow.AddDays(7),
            ServiceType = "Mantenimiento",
            Contact = new ContactRequest
            {
                Name = "Test User",
                Email = "test@example.com",
                Phone = "+5491155551234",
            },
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/appointments", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Response_ShouldIncludeCorrelationIdHeader()
    {
        // Act
        var response = await client.GetAsync("/api/appointments");

        // Assert
        response.Headers.Should().ContainKey("X-Correlation-ID");
    }

    [Fact]
    public async Task Response_ShouldUseSnakeCaseNaming()
    {
        // Arrange
        var request = new CreateAppointmentRequest
        {
            PlaceId = 1,
            AppointmentAt = DateTime.UtcNow.AddDays(7),
            ServiceType = "Service",
            Contact = new ContactRequest
            {
                Name = "Snake Case Test",
                Email = "snake@example.com",
                Phone = "+5491155551234",
            },
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/appointments", request, JsonOptions);
        var content = await response.Content.ReadAsStringAsync();

        // Assert - verify snake_case in response body
        content.Should().Contain("place_id");
        content.Should().Contain("appointment_at");
        content.Should().Contain("service_type");
        content.Should().Contain("created_at");
    }
}
