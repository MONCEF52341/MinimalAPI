using FluentAssertions;
using Xunit;
using MinimalAPI.Features.Test.Handlers;
using MinimalAPI.Features.Test.Models;
using MinimalAPI.Shared;

namespace MinimalAPI.Tests.Features.Test.Handlers;

/// <summary>
/// Tests unitaires pour TestHandler
/// </summary>
public class TestHandlerTests
{
    [Fact]
    public void Handle_ValidRequest_Should_ReturnSuccess()
    {
        // Arrange
        var request = new TestRequest("Hello World");
        var version = "v1";

        // Act
        var result = TestHandler.Handle(request, version);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Message.Should().Be("HELLO WORLD");
        result.Value.Version.Should().Be("v1");
        result.Value.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Handle_RequestWithLowerCase_Should_ConvertToUpperCase()
    {
        // Arrange
        var request = new TestRequest("test message");
        var version = "v2";

        // Act
        var result = TestHandler.Handle(request, version);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Message.Should().Be("TEST MESSAGE");
        result.Value.Version.Should().Be("v2");
    }

    [Fact]
    public void Handle_RequestWithSpecialCharacters_Should_PreserveCharacters()
    {
        // Arrange
        var request = new TestRequest("Hello @World! #123");
        var version = "v1";

        // Act
        var result = TestHandler.Handle(request, version);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Message.Should().Be("HELLO @WORLD! #123");
    }

    [Fact]
    public void Handle_RequestWithEmptyMessage_Should_ReturnSuccess()
    {
        // Arrange
        var request = new TestRequest("");
        var version = "v1";

        // Act
        var result = TestHandler.Handle(request, version);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Message.Should().Be("");
    }

    [Fact]
    public void Handle_RequestWithLongMessage_Should_ReturnSuccess()
    {
        // Arrange
        var longMessage = new string('a', 1000);
        var request = new TestRequest(longMessage);
        var version = "v1";

        // Act
        var result = TestHandler.Handle(request, version);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Message.Should().Be(longMessage.ToUpperInvariant());
    }

    [Theory]
    [InlineData("v1")]
    [InlineData("v2")]
    [InlineData("v3")]
    [InlineData("latest")]
    public void Handle_DifferentVersions_Should_ReturnCorrectVersion(string version)
    {
        // Arrange
        var request = new TestRequest("Test");

        // Act
        var result = TestHandler.Handle(request, version);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Version.Should().Be(version);
    }
}

