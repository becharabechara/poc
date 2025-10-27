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

public class GetAllNotesUseCaseTests
{
    private readonly Mock<INoteRepository> _noteRepositoryMock;
    private readonly GetAllNotesUseCase _useCase;

    public GetAllNotesUseCaseTests()
    {
        _noteRepositoryMock = new Mock<INoteRepository>();
        _useCase = new GetAllNotesUseCase(_noteRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithNotes_ShouldReturnAllNoteResponses()
    {
        // Arrange
        var notes = new List<Note>
        {
            new Note("Title 1", "Content 1", new[] { new Tag("tag1") }),
            new Note("Title 2", "Content 2", new[] { new Tag("tag2") })
        };

        _noteRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(notes);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal("Title 1", resultList[0].Title);
        Assert.Equal("Title 2", resultList[1].Title);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoNotes_ShouldReturnEmptyList()
    {
        // Arrange
        _noteRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Note>());

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GetAllNotesUseCase(null!));
    }

    [Fact]
    public async Task ExecuteAsync_WithNotesHavingNullTags_ShouldHandleGracefully()
    {
        // Arrange
        var notes = new List<Note>
        {
            new Note("Title 1", "Content 1", new List<Tag>()) // Empty tags
        };

        _noteRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(notes);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal("Title 1", resultList[0].Title);
        Assert.Empty(resultList[0].Tags);
    }
}