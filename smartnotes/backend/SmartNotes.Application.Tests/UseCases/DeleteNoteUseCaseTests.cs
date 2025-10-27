using System;
using System.Threading.Tasks;
using Moq;
using SmartNotes.Application.Ports;
using SmartNotes.Application.UseCases;
using SmartNotes.Domain.Entities;
using SmartNotes.Domain.ValueObjects;
using Xunit;

namespace SmartNotes.Application.Tests.UseCases;

public class DeleteNoteUseCaseTests
{
    private readonly Mock<INoteRepository> _noteRepositoryMock;
    private readonly DeleteNoteUseCase _useCase;

    public DeleteNoteUseCaseTests()
    {
        _noteRepositoryMock = new Mock<INoteRepository>();
        _useCase = new DeleteNoteUseCase(_noteRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingNote_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var note = new Note("Title", "Content", new[] { new Tag("tag") });
        var noteType = note.GetType();
        var idField = noteType.GetField("<Id>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        idField?.SetValue(note, noteId);

        _noteRepositoryMock.Setup(r => r.GetByIdAsync(noteId))
            .ReturnsAsync(note);
        _noteRepositoryMock.Setup(r => r.DeleteAsync(noteId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(noteId);

        // Assert
        Assert.True(result);
        _noteRepositoryMock.Verify(r => r.DeleteAsync(noteId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingNote_ShouldReturnFalse()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        _noteRepositoryMock.Setup(r => r.GetByIdAsync(noteId))
            .ReturnsAsync((Note?)null);

        // Act
        var result = await _useCase.ExecuteAsync(noteId);

        // Assert
        Assert.False(result);
        _noteRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}