using FluentAssertions;
using Xunit;
using MinimalAPI.Shared;

namespace MinimalAPI.Tests.Shared;

/// <summary>
/// Tests unitaires pour le pattern Result<T>
/// </summary>
public class ResultTests
{
    [Fact]
    public void Success_Should_CreateSuccessfulResult()
    {
        // Act
        var result = Result<string>.Success("test");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("test");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_Should_CreateFailedResult()
    {
        // Act
        var result = Result<string>.Failure("error message");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().Be("error message");
    }

    [Fact]
    public void Match_WithSuccess_Should_CallOnSuccess()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var value = result.Match(
            onSuccess: x => x * 2,
            onFailure: _ => 0
        );

        // Assert
        value.Should().Be(84);
    }

    [Fact]
    public void Match_WithFailure_Should_CallOnFailure()
    {
        // Arrange
        var result = Result<int>.Failure("error");

        // Act
        var value = result.Match(
            onSuccess: x => x * 2,
            onFailure: error => error.Length
        );

        // Assert
        value.Should().Be(5); // "error".Length
    }

    [Fact]
    public void Map_WithSuccess_Should_TransformValue()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var mapped = result.Map(x => x.ToString());

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be("42");
    }

    [Fact]
    public void Map_WithFailure_Should_PreserveError()
    {
        // Arrange
        var result = Result<int>.Failure("error");

        // Act
        var mapped = result.Map(x => x.ToString());

        // Assert
        mapped.IsSuccess.Should().BeFalse();
        mapped.Error.Should().Be("error");
    }

    [Fact]
    public void Bind_WithSuccess_Should_ChainResults()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var bound = result.Bind(x => Result<string>.Success(x.ToString()));

        // Assert
        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("42");
    }

    [Fact]
    public void Bind_WithFailure_Should_PreserveError()
    {
        // Arrange
        var result = Result<int>.Failure("error");

        // Act
        var bound = result.Bind(x => Result<string>.Success(x.ToString()));

        // Assert
        bound.IsSuccess.Should().BeFalse();
        bound.Error.Should().Be("error");
    }

    [Fact]
    public void Bind_WithSuccessButNextFails_Should_ReturnFailure()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var bound = result.Bind(_ => Result<string>.Failure("chain error"));

        // Assert
        bound.IsSuccess.Should().BeFalse();
        bound.Error.Should().Be("chain error");
    }
}

