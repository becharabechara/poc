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

public class GetNoteUseCaseTests
{
    private readonly Mock<INoteRepository> _noteRepositoryMock;
    private readonly GetNoteUseCase _useCase;

    public GetNoteUseCaseTests()
    {
        _noteRepositoryMock = new Mock<INoteRepository>();
        _useCase = new GetNoteUseCase(_noteRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingNote_ShouldReturnNoteResponse()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var note = new Note("Test Title", "Test Content", new[] { new Tag("tag1") });
        var noteType = note.GetType();
        var idField = noteType.GetField("<Id>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        idField?.SetValue(note, noteId);

        _noteRepositoryMock.Setup(r => r.GetByIdAsync(noteId))
            .ReturnsAsync(note);

        // Act
        var result = await _useCase.ExecuteAsync(noteId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(noteId, result!.Id);
        Assert.Equal("Test Title", result.Title);
        Assert.Equal("Test Content", result.Content);
        Assert.Single(result.Tags);
        Assert.Equal("tag1", result.Tags.First());
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingNote_ShouldReturnNull()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        _noteRepositoryMock.Setup(r => r.GetByIdAsync(noteId))
            .ReturnsAsync((Note?)null);

        // Act
        var result = await _useCase.ExecuteAsync(noteId);

        // Assert
        Assert.Null(result);
    }
}