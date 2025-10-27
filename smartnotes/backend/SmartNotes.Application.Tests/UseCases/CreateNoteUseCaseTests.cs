using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using SmartNotes.Application.DTOs;
using SmartNotes.Application.Ports;
using SmartNotes.Application.UseCases;
using SmartNotes.Domain.Entities;
using Xunit;

namespace SmartNotes.Application.Tests.UseCases;

public class CreateNoteUseCaseTests
{
    private readonly Mock<INoteRepository> _noteRepositoryMock;
    private readonly CreateNoteUseCase _useCase;

    public CreateNoteUseCaseTests()
    {
        _noteRepositoryMock = new Mock<INoteRepository>();
        _useCase = new CreateNoteUseCase(_noteRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldCreateNoteAndReturnResponse()
    {
        // Arrange
        var request = new CreateNoteRequest
        {
            Title = "Test Note",
            Content = "Test Content",
            Tags = new List<string> { "tag1", "tag2" }
        };

        Note? capturedNote = null;
        _noteRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Note>()))
            .Callback<Note>(note => capturedNote = note)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.Content, result.Content);
        Assert.Equal(request.Tags, result.Tags);

        _noteRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Note>()), Times.Once);
        Assert.NotNull(capturedNote);
        Assert.Equal(request.Title, capturedNote!.Title);
        Assert.Equal(request.Content, capturedNote.Content);
        Assert.Equal(2, capturedNote.Tags!.Count);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyTags_ShouldCreateNoteWithEmptyTags()
    {
        // Arrange
        var request = new CreateNoteRequest
        {
            Title = "Test Note",
            Content = "Test Content",
            Tags = new List<string>()
        };

        _noteRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Note>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Tags);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullTags_ShouldCreateNoteWithEmptyTags()
    {
        // Arrange
        var request = new CreateNoteRequest
        {
            Title = "Test Note",
            Content = "Test Content",
            Tags = null
        };

        _noteRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Note>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Tags);
    }
}