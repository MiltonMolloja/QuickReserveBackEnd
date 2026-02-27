// -----------------------------------------------------------------------
// <copyright file="TecnomApiClientTests.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Tests.Infrastructure.ExternalServices;

using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using QuickReserve.Infrastructure.Configuration;
using QuickReserve.Infrastructure.ExternalServices;
using QuickReserve.Infrastructure.ExternalServices.Models;

/// <summary>
/// Tests for <see cref="TecnomApiClient"/> using a mock HTTP handler.
/// </summary>
public sealed class TecnomApiClientTests
{
    private readonly Mock<ILogger<TecnomApiClient>> loggerMock = new();

    private readonly TecnomApiSettings settings = new()
    {
        BaseUrl = "https://api.test.com/",
        Username = "testuser",
        Password = "testpass",
        TimeoutSeconds = 30,
        RetryCount = 3,
        CacheExpirationMinutes = 5,
    };

    [Fact]
    public async Task GetActiveWorkshopsAsync_ShouldReturnOnlyActiveWorkshops()
    {
        // Arrange
        var workshops = new List<TecnomWorkshopDto>
        {
            new() { Id = 1, Name = "Workshop A", Active = true, Address = "Calle 1" },
            new() { Id = 2, Name = "Workshop B", Active = false, Address = "Calle 2" },
            new() { Id = 3, Name = "Workshop C", Active = true, Address = "Calle 3" },
        };

        var client = CreateClientWithResponse(HttpStatusCode.OK, workshops);

        // Act
        var result = await client.GetActiveWorkshopsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(w => w.Id == 1 || w.Id == 3);
    }

    [Fact]
    public async Task GetActiveWorkshopsAsync_ShouldMapFieldsCorrectly()
    {
        // Arrange
        var workshops = new List<TecnomWorkshopDto>
        {
            new()
            {
                Id = 1,
                Name = "Taller Central",
                Active = true,
                Address = "Av. Corrientes 1234",
                Email = "taller@test.com",
                Whatsapp = "+5491155551234",
            },
        };

        var client = CreateClientWithResponse(HttpStatusCode.OK, workshops);

        // Act
        var result = await client.GetActiveWorkshopsAsync();

        // Assert
        result.Should().HaveCount(1);
        var workshop = result[0];
        workshop.Id.Should().Be(1);
        workshop.Name.Should().Be("Taller Central");
        workshop.Address.Should().Be("Av. Corrientes 1234");
        workshop.Email.Should().Be("taller@test.com");
        workshop.Whatsapp.Should().Be("+5491155551234");
    }

    [Fact]
    public async Task GetActiveWorkshopsAsync_WhenNoWorkshops_ShouldReturnEmptyList()
    {
        // Arrange
        var client = CreateClientWithResponse(HttpStatusCode.OK, new List<TecnomWorkshopDto>());

        // Act
        var result = await client.GetActiveWorkshopsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveWorkshopsAsync_WhenAllInactive_ShouldReturnEmptyList()
    {
        // Arrange
        var workshops = new List<TecnomWorkshopDto>
        {
            new() { Id = 1, Name = "Inactive A", Active = false },
            new() { Id = 2, Name = "Inactive B", Active = false },
        };

        var client = CreateClientWithResponse(HttpStatusCode.OK, workshops);

        // Act
        var result = await client.GetActiveWorkshopsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task IsActiveWorkshopAsync_WithActivePlaceId_ShouldReturnTrue()
    {
        // Arrange
        var workshops = new List<TecnomWorkshopDto>
        {
            new() { Id = 42, Name = "Active Workshop", Active = true },
        };

        var client = CreateClientWithResponse(HttpStatusCode.OK, workshops);

        // Act
        var result = await client.IsActiveWorkshopAsync(42);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsActiveWorkshopAsync_WithInactivePlaceId_ShouldReturnFalse()
    {
        // Arrange
        var workshops = new List<TecnomWorkshopDto>
        {
            new() { Id = 42, Name = "Inactive Workshop", Active = false },
        };

        var client = CreateClientWithResponse(HttpStatusCode.OK, workshops);

        // Act
        var result = await client.IsActiveWorkshopAsync(42);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsActiveWorkshopAsync_WithNonExistentPlaceId_ShouldReturnFalse()
    {
        // Arrange
        var workshops = new List<TecnomWorkshopDto>
        {
            new() { Id = 1, Name = "Workshop", Active = true },
        };

        var client = CreateClientWithResponse(HttpStatusCode.OK, workshops);

        // Act
        var result = await client.IsActiveWorkshopAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetActiveWorkshopsAsync_WhenApiReturnsError_ShouldThrowHttpRequestException()
    {
        // Arrange
        var client = CreateClientWithResponse(HttpStatusCode.InternalServerError, string.Empty);

        // Act
        var act = () => client.GetActiveWorkshopsAsync();

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public void Constructor_ShouldSetBasicAuthHeader()
    {
        // Arrange & Act
        var httpClient = new HttpClient { BaseAddress = new Uri(settings.BaseUrl) };
        _ = new TecnomApiClient(httpClient, Options.Create(settings), loggerMock.Object);

        // Assert
        httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull();
        httpClient.DefaultRequestHeaders.Authorization!.Scheme.Should().Be("Basic");

        var expectedCredentials = Convert.ToBase64String(
            System.Text.Encoding.ASCII.GetBytes($"{settings.Username}:{settings.Password}"));
        httpClient.DefaultRequestHeaders.Authorization.Parameter.Should().Be(expectedCredentials);
    }

    private TecnomApiClient CreateClientWithResponse<T>(HttpStatusCode statusCode, T content)
    {
        var json = JsonSerializer.Serialize(content);
        var handler = new MockHttpMessageHandler(statusCode, json);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(settings.BaseUrl) };

        return new TecnomApiClient(httpClient, Options.Create(settings), loggerMock.Object);
    }

    /// <summary>
    /// Simple mock HTTP message handler for testing.
    /// </summary>
    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode statusCode;
        private readonly string content;

        public MockHttpMessageHandler(HttpStatusCode statusCode, string content)
        {
            this.statusCode = statusCode;
            this.content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json"),
            };

            return Task.FromResult(response);
        }
    }
}
