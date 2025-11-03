using Microsoft.AspNetCore.Mvc;
using SmartNotes.Application.DTOs;
using SmartNotes.Application.UseCases;
using System.ComponentModel.DataAnnotations;

namespace SmartNotes.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class NotesController : ControllerBase
{
    private readonly CreateNoteUseCase _createNoteUseCase;
    private readonly GetAllNotesUseCase _getAllNotesUseCase;
    private readonly GetNoteUseCase _getNoteUseCase;
    private readonly UpdateNoteUseCase _updateNoteUseCase;
    private readonly DeleteNoteUseCase _deleteNoteUseCase;
    private readonly SearchNotesUseCase _searchNotesUseCase;
    private readonly ILogger<NotesController> _logger;

    public NotesController(
        CreateNoteUseCase createNoteUseCase,
        GetAllNotesUseCase getAllNotesUseCase,
        GetNoteUseCase getNoteUseCase,
        UpdateNoteUseCase updateNoteUseCase,
        DeleteNoteUseCase deleteNoteUseCase,
        SearchNotesUseCase searchNotesUseCase,
        ILogger<NotesController> logger)
    {
        _createNoteUseCase = createNoteUseCase;
        _getAllNotesUseCase = getAllNotesUseCase;
        _getNoteUseCase = getNoteUseCase;
        _updateNoteUseCase = updateNoteUseCase;
        _deleteNoteUseCase = deleteNoteUseCase;
        _searchNotesUseCase = searchNotesUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all notes from the system
    /// </summary>
    /// <returns>A collection of all notes</returns>
    /// <response code="200">Returns the list of notes</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NoteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<NoteResponse>>> GetAllNotes()
    {
        try
        {
            _logger.LogInformation("Retrieving all notes");
            var notes = await _getAllNotesUseCase.ExecuteAsync();
            return Ok(notes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all notes");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving notes" });
        }
    }

    /// <summary>
    /// Retrieves a specific note by its ID
    /// </summary>
    /// <param name="id">The unique identifier of the note</param>
    /// <returns>The requested note</returns>
    /// <response code="200">Returns the requested note</response>
    /// <response code="404">If the note is not found</response>
    /// <response code="400">If the ID format is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NoteResponse>> GetNote([Required] Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving note with ID: {NoteId}", id);
            var note = await _getNoteUseCase.ExecuteAsync(id);
            
            if (note == null)
            {
                _logger.LogWarning("Note with ID {NoteId} not found", id);
                return NotFound(new { error = $"Note with ID {id} not found" });
            }

            return Ok(note);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving note with ID: {NoteId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving the note" });
        }
    }

    /// <summary>
    /// Creates a new note
    /// </summary>
    /// <param name="request">The note creation request</param>
    /// <returns>The created note</returns>
    /// <response code="201">Returns the newly created note</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(NoteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NoteResponse>> CreateNote([FromBody, Required] CreateNoteRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for note creation: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating new note with title: {Title}", request.Title);
            var note = await _createNoteUseCase.ExecuteAsync(request);
            
            return CreatedAtAction(
                nameof(GetNote), 
                new { id = note.Id }, 
                note);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for note creation: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating note");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while creating the note" });
        }
    }

    /// <summary>
    /// Updates an existing note
    /// </summary>
    /// <param name="id">The unique identifier of the note to update</param>
    /// <param name="request">The note update request</param>
    /// <returns>The updated note</returns>
    /// <response code="200">Returns the updated note</response>
    /// <response code="400">If the request is invalid or ID mismatch</response>
    /// <response code="404">If the note is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(NoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NoteResponse>> UpdateNote([Required] Guid id, [FromBody, Required] UpdateNoteRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for note update: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            if (id != request.Id)
            {
                _logger.LogWarning("ID mismatch in update request. Route ID: {RouteId}, Request ID: {RequestId}", id, request.Id);
                return BadRequest(new { error = "ID in route does not match ID in request body" });
            }

            _logger.LogInformation("Updating note with ID: {NoteId}", id);
            var note = await _updateNoteUseCase.ExecuteAsync(request);
            
            if (note == null)
            {
                _logger.LogWarning("Note with ID {NoteId} not found for update", id);
                return NotFound(new { error = $"Note with ID {id} not found" });
            }

            return Ok(note);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for note update: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating note with ID: {NoteId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while updating the note" });
        }
    }

    /// <summary>
    /// Deletes a note by its ID
    /// </summary>
    /// <param name="id">The unique identifier of the note to delete</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">If the note was successfully deleted</response>
    /// <response code="404">If the note is not found</response>
    /// <response code="400">If the ID format is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteNote([Required] Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting note with ID: {NoteId}", id);
            var success = await _deleteNoteUseCase.ExecuteAsync(id);
            
            if (!success)
            {
                _logger.LogWarning("Note with ID {NoteId} not found for deletion", id);
                return NotFound(new { error = $"Note with ID {id} not found" });
            }

            _logger.LogInformation("Successfully deleted note with ID: {NoteId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting note with ID: {NoteId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while deleting the note" });
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>Health status</returns>
    /// <response code="200">Service is healthy</response>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}