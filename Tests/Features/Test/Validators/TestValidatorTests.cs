using FluentAssertions;
using Xunit;
using MinimalAPI.Features.Test.Models;
using MinimalAPI.Features.Test.Validators;
using MinimalAPI.Shared;

namespace MinimalAPI.Tests.Features.Test.Validators;

/// <summary>
/// Tests unitaires pour TestValidator
/// </summary>
public class TestValidatorTests
{
    [Fact]
    public void Validate_ValidRequest_Should_ReturnSuccess()
    {
        // Arrange
        var request = new TestRequest("Valid message");

        // Act
        var result = TestValidator.Validate(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(request);
    }

    [Fact]
    public void Validate_RequestWithNullMessage_Should_ReturnFailure()
    {
        // Arrange
        var request = new TestRequest(null!);

        // Act
        var result = TestValidator.Validate(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("vide");
    }

    [Fact]
    public void Validate_RequestWithEmptyMessage_Should_ReturnFailure()
    {
        // Arrange
        var request = new TestRequest("");

        // Act
        var result = TestValidator.Validate(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("vide");
    }

    [Fact]
    public void Validate_RequestWithWhitespaceOnly_Should_ReturnFailure()
    {
        // Arrange
        var request = new TestRequest("   ");

        // Act
        var result = TestValidator.Validate(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("vide");
    }

    [Fact]
    public void Validate_RequestWithMessageLength100_Should_ReturnSuccess()
    {
        // Arrange
        var message = new string('a', 100);
        var request = new TestRequest(message);

        // Act
        var result = TestValidator.Validate(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_RequestWithMessageLength101_Should_ReturnFailure()
    {
        // Arrange
        var message = new string('a', 101);
        var request = new TestRequest(message);

        // Act
        var result = TestValidator.Validate(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("100 caractères");
    }

    [Fact]
    public void Validate_RequestWithMessageLength1000_Should_ReturnFailure()
    {
        // Arrange
        var message = new string('a', 1000);
        var request = new TestRequest(message);

        // Act
        var result = TestValidator.Validate(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("100 caractères");
    }

    [Fact]
    public void Validate_RequestWithValidMessageAndOptionalField_Should_ReturnSuccess()
    {
        // Arrange
        var request = new TestRequest("Valid message", "Optional value");

        // Act
        var result = TestValidator.Validate(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(request);
    }
}

