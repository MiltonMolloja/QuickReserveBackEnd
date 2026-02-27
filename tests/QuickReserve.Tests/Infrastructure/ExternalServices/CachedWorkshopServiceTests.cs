// -----------------------------------------------------------------------
// <copyright file="CachedWorkshopServiceTests.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Tests.Infrastructure.ExternalServices;

using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using QuickReserve.Domain.Interfaces;
using QuickReserve.Infrastructure.Configuration;
using QuickReserve.Infrastructure.ExternalServices;
using QuickReserve.Infrastructure.ExternalServices.Models;

/// <summary>
/// Tests for <see cref="CachedWorkshopService"/> caching decorator.
/// </summary>
public sealed class CachedWorkshopServiceTests
{
    private const string CacheKey = "workshops:active";

    private readonly Mock<IDistributedCache> cacheMock;
    private readonly Mock<ILogger<CachedWorkshopService>> loggerMock;
    private readonly IOptions<TecnomApiSettings> settingsOptions;

    public CachedWorkshopServiceTests()
    {
        cacheMock = new Mock<IDistributedCache>();
        loggerMock = new Mock<ILogger<CachedWorkshopService>>();
        settingsOptions = Options.Create(new TecnomApiSettings
        {
            BaseUrl = "https://test.com/",
            CacheExpirationMinutes = 5,
        });
    }

    [Fact]
    public async Task GetActiveWorkshopsAsync_WhenCacheHit_ShouldReturnCachedData()
    {
        // Arrange
        var cachedWorkshops = new List<WorkshopInfo>
        {
            new(1, "Cached Workshop", "Address", "email@test.com", "+5491155551234"),
        };

        SetupCacheHit(cachedWorkshops);

        var service = CreateServiceWithApiResponse([]);

        // Act
        var result = await service.GetActiveWorkshopsAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Cached Workshop");
    }

    [Fact]
    public async Task GetActiveWorkshopsAsync_WhenCacheMiss_ShouldCallInnerServiceAndCache()
    {
        // Arrange
        var apiWorkshops = new List<TecnomWorkshopDto>
        {
            new() { Id = 1, Name = "Fresh Workshop", Active = true, Address = "Address" },
        };

        SetupCacheMiss();

        var service = CreateServiceWithApiResponse(apiWorkshops);

        // Act
        var result = await service.GetActiveWorkshopsAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Fresh Workshop");

        // Verify cache was set
        cacheMock.Verify(
            c => c.SetAsync(
                CacheKey,
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IsActiveWorkshopAsync_WithActivePlaceId_ShouldReturnTrue()
    {
        // Arrange
        var cachedWorkshops = new List<WorkshopInfo>
        {
            new(42, "Workshop", null, null, null),
        };

        SetupCacheHit(cachedWorkshops);

        var service = CreateServiceWithApiResponse([]);

        // Act
        var result = await service.IsActiveWorkshopAsync(42);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsActiveWorkshopAsync_WithInactivePlaceId_ShouldReturnFalse()
    {
        // Arrange
        var cachedWorkshops = new List<WorkshopInfo>
        {
            new(1, "Workshop", null, null, null),
        };

        SetupCacheHit(cachedWorkshops);

        var service = CreateServiceWithApiResponse([]);

        // Act
        var result = await service.IsActiveWorkshopAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetActiveWorkshopsAsync_WhenCacheEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        SetupCacheMiss();

        var service = CreateServiceWithApiResponse([]);

        // Act
        var result = await service.GetActiveWorkshopsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    private CachedWorkshopService CreateServiceWithApiResponse(List<TecnomWorkshopDto> apiResponse)
    {
        var json = JsonSerializer.Serialize(apiResponse);
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.com/") };

        var innerClient = new TecnomApiClient(
            httpClient,
            settingsOptions,
            Mock.Of<ILogger<TecnomApiClient>>());

        return new CachedWorkshopService(
            innerClient,
            cacheMock.Object,
            settingsOptions,
            loggerMock.Object);
    }

    private void SetupCacheHit(List<WorkshopInfo> workshops)
    {
        var cachedJson = JsonSerializer.Serialize(workshops);
        var cachedBytes = Encoding.UTF8.GetBytes(cachedJson);

        cacheMock
            .Setup(c => c.GetAsync(CacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);
    }

    private void SetupCacheMiss()
    {
        cacheMock
            .Setup(c => c.GetAsync(CacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
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
                Content = new StringContent(content, Encoding.UTF8, "application/json"),
            };

            return Task.FromResult(response);
        }
    }
}
