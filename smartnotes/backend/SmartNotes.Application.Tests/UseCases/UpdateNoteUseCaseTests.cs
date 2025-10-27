using System;
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

public class UpdateNoteUseCaseTests
{
    private readonly Mock<INoteRepository> _noteRepositoryMock;
    private readonly UpdateNoteUseCase _useCase;

    public UpdateNoteUseCaseTests()
    {
        _noteRepositoryMock = new Mock<INoteRepository>();
        _useCase = new UpdateNoteUseCase(_noteRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingNote_ShouldUpdateAndReturnResponse()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var existingNote = new Note("Old Title", "Old Content", new[] { new Tag("old") });
        var noteType = existingNote.GetType();
        var idField = noteType.GetField("<Id>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        idField?.SetValue(existingNote, noteId);

        var request = new UpdateNoteRequest
        {
            Id = noteId,
            Title = "New Title",
            Content = "New Content",
            Tags = new List<string> { "new" }
        };

        _noteRepositoryMock.Setup(r => r.GetByIdAsync(noteId))
            .ReturnsAsync(existingNote);
        _noteRepositoryMock.Setup(r => r.UpdateAsync(existingNote))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(noteId, result!.Id);
        Assert.Equal("New Title", result.Title);
        Assert.Equal("New Content", result.Content);
        Assert.Single(result.Tags);
        Assert.Equal("new", result.Tags.First());

        _noteRepositoryMock.Verify(r => r.UpdateAsync(existingNote), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingNote_ShouldReturnNull()
    {
        // Arrange
        var request = new UpdateNoteRequest
        {
            Id = Guid.NewGuid(),
            Title = "Title",
            Content = "Content",
            Tags = new List<string>()
        };

        _noteRepositoryMock.Setup(r => r.GetByIdAsync(request.Id))
            .ReturnsAsync((Note?)null);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.Null(result);
        _noteRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Note>()), Times.Never);
    }
}