using Microsoft.AspNetCore.Mvc;
using Note_API.DTOs;
using Note_API.Models;
using Note_API.Services;

namespace Note_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotesController : ControllerBase
    {
        private readonly NoteService _noteService;

        public NotesController(NoteService noteService)
        {
            _noteService = noteService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllNotes(
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = "CreatedAt",
            [FromQuery] bool ascending = false)
        {
            var notes = await _noteService.GetAllNotesAsync(search, sortBy, ascending);

            var noteDtos = notes.Select(note => new NoteReadDto
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            });

            return Ok(noteDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNoteById(int id)
        {
            var note = await _noteService.GetNoteByIdAsync(id);
            if (note == null) return NotFound();

            var noteDto = new NoteReadDto
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            };

            return Ok(noteDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] CreateNoteDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var note = new Note
            {
                Title = dto.Title,
                Content = dto.Content,
            };

            var newId = await _noteService.CreateNoteAsync(note);

            var createdNote = await _noteService.GetNoteByIdAsync(newId);
            var createdNoteDto = new CreateNoteDto
            {
                Title = createdNote.Title,
                Content = createdNote.Content,
            };

            return CreatedAtAction(nameof(GetNoteById), new { id = newId }, createdNoteDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] NoteUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingNote = await _noteService.GetNoteByIdAsync(id);
            if (existingNote == null) return NotFound();

            existingNote.Title = dto.Title;
            existingNote.Content = dto.Content;

            var updated = await _noteService.UpdateNoteAsync(existingNote);
            if (!updated) return StatusCode(500, "An error occurred while updating the note.");

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var existingNote = await _noteService.GetNoteByIdAsync(id);
            if (existingNote == null) return NotFound();

            var deleted = await _noteService.DeleteNoteAsync(id);
            if (!deleted) return StatusCode(500, "An error occurred while deleting the note.");

            return NoContent();
        }
    }
}
