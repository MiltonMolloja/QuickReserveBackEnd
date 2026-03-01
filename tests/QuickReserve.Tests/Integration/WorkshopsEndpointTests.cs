// -----------------------------------------------------------------------
// <copyright file="WorkshopsEndpointTests.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Tests.Integration;

using System.Net;
using System.Text.Json;
using FluentAssertions;
using QuickReserve.Application.DTOs.Responses;

/// <summary>
/// Integration tests for the /api/workshops endpoints.
/// </summary>
public sealed class WorkshopsEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private readonly HttpClient client;

    public WorkshopsEndpointTests(CustomWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithActiveWorkshops()
    {
        // Act
        var response = await client.GetAsync("/api/workshops");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<WorkshopResponse>>>(content, JsonOptions);

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Only active workshops (2 out of 3 in mock)
        result.Data!.Should().HaveCount(2);
        result.Data.Should().OnlyContain(w => w.Id == 1 || w.Id == 2);
    }

    [Fact]
    public async Task GetAll_ShouldNotReturnInactiveWorkshops()
    {
        // Act
        var response = await client.GetAsync("/api/workshops");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<WorkshopResponse>>>(content, JsonOptions);

        result!.Data.Should().NotContain(w => w.Id == 3);
    }

    [Fact]
    public async Task GetAll_ShouldMapFieldsCorrectly()
    {
        // Act
        var response = await client.GetAsync("/api/workshops");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<WorkshopResponse>>>(content, JsonOptions);

        var central = result!.Data!.First(w => w.Id == 1);
        central.Name.Should().Be("Taller Central");
        central.Address.Should().Be("Av. Corrientes 1234");
        central.Email.Should().Be("central@test.com");
        central.Phone.Should().Be("+5491155551234");
    }

    [Fact]
    public async Task Response_ShouldIncludeCorrelationIdHeader()
    {
        // Act
        var response = await client.GetAsync("/api/workshops");

        // Assert
        response.Headers.Should().ContainKey("X-Correlation-ID");
    }
}
