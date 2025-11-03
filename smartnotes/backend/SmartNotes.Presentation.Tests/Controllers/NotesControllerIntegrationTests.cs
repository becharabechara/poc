using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SmartNotes.Application.DTOs;
using SmartNotes.Infrastructure.Data;
using SmartNotes.Presentation.Tests.Helpers;

namespace SmartNotes.Presentation.Tests.Controllers;

public class NotesControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public NotesControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task CreateNote_WithValidData_ShouldReturnCreatedNote()
    {
        // Arrange
        var request = new CreateNoteRequest
        {
            Title = "Test Note",
            Content = "This is a test note content",
            Tags = new[] { "test", "integration" }
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/notes", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdNote = JsonSerializer.Deserialize<NoteResponse>(responseContent, _jsonOptions);

        createdNote.Should().NotBeNull();
        createdNote!.Id.Should().NotBeEmpty();
        createdNote.Title.Should().Be(request.Title);
        createdNote.Content.Should().Be(request.Content);
        createdNote.Tags.Should().BeEquivalentTo(request.Tags);
        createdNote.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        createdNote.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify location header
        response.Headers.Location.Should().NotBeNull();
        var locationString = response.Headers.Location!.ToString();
        locationString.Should().Contain("/api/");
        locationString.Should().Contain(createdNote.Id.ToString());
    }

    [Fact]
    public async Task CreateNote_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateNoteRequest
        {
            Title = "", // Invalid: empty title
            Content = "This is a test note content",
            Tags = new[] { "test" }
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/notes", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetNote_WithValidId_ShouldReturnNote()
    {
        // Arrange - Create a note first
        var createRequest = new CreateNoteRequest
        {
            Title = "Test Note for Get",
            Content = "Content for get test",
            Tags = new[] { "get", "test" }
        };

        var createResponse = await CreateNoteAsync(createRequest);
        var createdNote = await DeserializeResponseAsync<NoteResponse>(createResponse);

        // Act
        var response = await _client.GetAsync($"/api/notes/{createdNote.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var retrievedNote = JsonSerializer.Deserialize<NoteResponse>(responseContent, _jsonOptions);

        retrievedNote.Should().NotBeNull();
        retrievedNote!.Id.Should().Be(createdNote.Id);
        retrievedNote.Title.Should().Be(createRequest.Title);
        retrievedNote.Content.Should().Be(createRequest.Content);
        retrievedNote.Tags.Should().BeEquivalentTo(createRequest.Tags);
    }

    [Fact]
    public async Task GetNote_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/notes/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllNotes_ShouldReturnAllNotes()
    {
        // Arrange - Clean database and create test notes
        await ClearDatabaseAsync();
        
        var note1Request = new CreateNoteRequest { Title = "Note 1", Content = "Content 1", Tags = new[] { "tag1" } };
        var note2Request = new CreateNoteRequest { Title = "Note 2", Content = "Content 2", Tags = new[] { "tag2" } };

        await CreateNoteAsync(note1Request);
        await CreateNoteAsync(note2Request);

        // Act
        var response = await _client.GetAsync("/api/notes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var notes = JsonSerializer.Deserialize<List<NoteResponse>>(responseContent, _jsonOptions);

        notes.Should().NotBeNull();
        notes!.Should().HaveCount(2);
        notes.Should().Contain(n => n.Title == "Note 1");
        notes.Should().Contain(n => n.Title == "Note 2");
    }

    [Fact]
    public async Task UpdateNote_WithValidData_ShouldReturnUpdatedNote()
    {
        // Arrange - Create a note first
        var createRequest = new CreateNoteRequest
        {
            Title = "Original Title",
            Content = "Original Content",
            Tags = new[] { "original" }
        };

        var createResponse = await CreateNoteAsync(createRequest);
        var createdNote = await DeserializeResponseAsync<NoteResponse>(createResponse);

        var updateRequest = new UpdateNoteRequest
        {
            Id = createdNote.Id,
            Title = "Updated Title",
            Content = "Updated Content",
            Tags = new[] { "updated", "modified" }
        };

        var json = JsonSerializer.Serialize(updateRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/notes/{createdNote.Id}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var updatedNote = JsonSerializer.Deserialize<NoteResponse>(responseContent, _jsonOptions);

        updatedNote.Should().NotBeNull();
        updatedNote!.Id.Should().Be(createdNote.Id);
        updatedNote.Title.Should().Be(updateRequest.Title);
        updatedNote.Content.Should().Be(updateRequest.Content);
        updatedNote.Tags.Should().BeEquivalentTo(updateRequest.Tags);
        updatedNote.UpdatedAt.Should().BeAfter(createdNote.UpdatedAt);
    }

    [Fact]
    public async Task UpdateNote_WithMismatchedId_ShouldReturnBadRequest()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var differentId = Guid.NewGuid();

        var updateRequest = new UpdateNoteRequest
        {
            Id = differentId,
            Title = "Updated Title",
            Content = "Updated Content",
            Tags = new[] { "updated" }
        };

        var json = JsonSerializer.Serialize(updateRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/notes/{noteId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteNote_WithValidId_ShouldReturnNoContent()
    {
        // Arrange - Create a note first
        var createRequest = new CreateNoteRequest
        {
            Title = "Note to Delete",
            Content = "This note will be deleted",
            Tags = new[] { "delete" }
        };

        var createResponse = await CreateNoteAsync(createRequest);
        var createdNote = await DeserializeResponseAsync<NoteResponse>(createResponse);

        // Act
        var response = await _client.DeleteAsync($"/api/notes/{createdNote.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify note is actually deleted
        var getResponse = await _client.GetAsync($"/api/notes/{createdNote.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteNote_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/notes/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SearchNotes_WithKeyword_ShouldReturnMatchingNotes()
    {
        // Arrange - Clear database and create test notes
        await ClearDatabaseAsync();

        var note1Request = new CreateNoteRequest { Title = "JavaScript Tutorial", Content = "Learn JavaScript programming", Tags = new[] { "programming" } };
        var note2Request = new CreateNoteRequest { Title = "Python Basics", Content = "Introduction to Python", Tags = new[] { "programming" } };
        var note3Request = new CreateNoteRequest { Title = "Cooking Recipe", Content = "How to cook pasta", Tags = new[] { "cooking" } };

        await CreateNoteAsync(note1Request);
        await CreateNoteAsync(note2Request);
        await CreateNoteAsync(note3Request);

        // Act
        var response = await _client.GetAsync("/api/notes/search?keyword=programming");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var searchResults = JsonSerializer.Deserialize<List<NoteResponse>>(responseContent, _jsonOptions);

        searchResults.Should().NotBeNull();
        searchResults!.Should().HaveCountGreaterOrEqualTo(1);
        searchResults.Should().Contain(n => n.Title.Contains("JavaScript") || n.Content.Contains("JavaScript") || n.Content.Contains("programming"));
    }

    [Fact]
    public async Task SearchNotes_WithTags_ShouldReturnMatchingNotes()
    {
        // Arrange - Clear database and create test notes
        await ClearDatabaseAsync();

        var note1Request = new CreateNoteRequest { Title = "Note 1", Content = "Content 1", Tags = new[] { "work", "urgent" } };
        var note2Request = new CreateNoteRequest { Title = "Note 2", Content = "Content 2", Tags = new[] { "personal", "urgent" } };
        var note3Request = new CreateNoteRequest { Title = "Note 3", Content = "Content 3", Tags = new[] { "work", "normal" } };

        await CreateNoteAsync(note1Request);
        await CreateNoteAsync(note2Request);
        await CreateNoteAsync(note3Request);

        // Act
        var response = await _client.GetAsync("/api/notes/search?tags=urgent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var searchResults = JsonSerializer.Deserialize<List<NoteResponse>>(responseContent, _jsonOptions);

        searchResults.Should().NotBeNull();
        searchResults!.Should().HaveCount(2);
        searchResults.Should().OnlyContain(n => n.Tags.Contains("urgent"));
    }

    // Helper methods
    private async Task<HttpResponseMessage> CreateNoteAsync(CreateNoteRequest request)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _client.PostAsync("/api/notes", content);
    }

    private async Task<T> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, _jsonOptions)!;
    }

    private async Task ClearDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SmartNotesDbContext>();
        
        context.Notes.RemoveRange(context.Notes);
        await context.SaveChangesAsync();
    }
}