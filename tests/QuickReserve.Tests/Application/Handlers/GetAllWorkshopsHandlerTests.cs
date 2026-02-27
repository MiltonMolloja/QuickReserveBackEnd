// -----------------------------------------------------------------------
// <copyright file="GetAllWorkshopsHandlerTests.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Tests.Application.Handlers;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using QuickReserve.Application.Features.Workshops.Queries;
using QuickReserve.Application.Mappings;
using QuickReserve.Domain.Interfaces;

public class GetAllWorkshopsHandlerTests
{
    private readonly Mock<IWorkshopService> _workshopServiceMock;
    private readonly GetAllWorkshopsHandler _sut;

    public GetAllWorkshopsHandlerTests()
    {
        _workshopServiceMock = new Mock<IWorkshopService>();
        var logger = Mock.Of<ILogger<GetAllWorkshopsHandler>>();

        MappingConfig.Configure();

        _sut = new GetAllWorkshopsHandler(_workshopServiceMock.Object, logger);
    }

    [Fact]
    public async Task Handle_WithWorkshops_ShouldReturnSuccess()
    {
        // Arrange
        var workshops = new List<WorkshopInfo>
        {
            new(1, "Taller Norte", "Av. Libertador 1234", "norte@email.com", "+5491111111111"),
            new(2, "Taller Sur", "Av. Rivadavia 5678", null, null),
        };

        _workshopServiceMock
            .Setup(w => w.GetActiveWorkshopsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(workshops);

        // Act
        var result = await _sut.Handle(new GetAllWorkshopsQuery(), CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data![0].Name.Should().Be("Taller Norte");
        result.Data[1].Name.Should().Be("Taller Sur");
    }

    [Fact]
    public async Task Handle_WhenServiceThrows_ShouldReturnFailure()
    {
        // Arrange
        _workshopServiceMock
            .Setup(w => w.GetActiveWorkshopsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        var result = await _sut.Handle(new GetAllWorkshopsQuery(), CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("talleres"));
    }
}
