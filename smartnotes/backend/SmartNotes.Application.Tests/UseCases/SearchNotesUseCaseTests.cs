using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using SmartNotes.Application.DTOs;
using SmartNotes.Application.Ports;
using SmartNotes.Application.UseCases;
using SmartNotes.Domain.Entities;
using SmartNotes.Domain.ValueObjects;
using Xunit;

namespace SmartNotes.Application.Tests.UseCases;

public class SearchNotesUseCaseTests
{
    private readonly Mock<INoteRepository> _noteRepositoryMock;
    private readonly SearchNotesUseCase _useCase;

    public SearchNotesUseCaseTests()
    {
        _noteRepositoryMock = new Mock<INoteRepository>();
        _useCase = new SearchNotesUseCase(_noteRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithKeywordAndTags_ShouldSearchAndReturnResults()
    {
        // Arrange
        var request = new SearchNotesRequest
        {
            Keyword = "test",
            Tags = new List<string> { "tag1" }
        };

        var notes = new List<Note>
        {
            new Note("Test Note", "Content", new[] { new Tag("tag1") })
        };

        _noteRepositoryMock.Setup(r => r.SearchAsync("test", It.Is<IEnumerable<string>>(t => t.Contains("tag1"))))
            .ReturnsAsync(notes);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal("Test Note", resultList[0].Title);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullKeywordAndTags_ShouldSearchWithNulls()
    {
        // Arrange
        var request = new SearchNotesRequest
        {
            Keyword = null,
            Tags = null
        };

        var notes = new List<Note>
        {
            new Note("Note", "Content", new[] { new Tag("tag") })
        };

        _noteRepositoryMock.Setup(r => r.SearchAsync(null, null))
            .ReturnsAsync(notes);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyResults_ShouldReturnEmptyList()
    {
        // Arrange
        var request = new SearchNotesRequest
        {
            Keyword = "nonexistent",
            Tags = new List<string>()
        };

        _noteRepositoryMock.Setup(r => r.SearchAsync("nonexistent", It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<Note>());

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SearchNotesUseCase(null!));
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _useCase.ExecuteAsync(null!));
    }

    [Fact]
    public async Task ExecuteAsync_WithNotesHavingEmptyTags_ShouldHandleGracefully()
    {
        // Arrange
        var request = new SearchNotesRequest
        {
            Keyword = "test",
            Tags = new List<string>()
        };

        var notes = new List<Note>
        {
            new Note("Test Note", "Content", new List<Tag>()) // Empty tags
        };

        _noteRepositoryMock.Setup(r => r.SearchAsync("test", It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(notes);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal("Test Note", resultList[0].Title);
        Assert.Empty(resultList[0].Tags);
    }
}