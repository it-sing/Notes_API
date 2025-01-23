using Note_API.Data;
using Note_API.Models;

namespace Note_API.Services
{
    public class NoteService
    {
        private readonly NotesRepository _repository;

        public NoteService(NotesRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        public async Task<IEnumerable<Note>> GetAllNotesAsync(string? search = null, string? sortBy = "CreatedAt", bool ascending = false)
        {
            if (string.IsNullOrEmpty(sortBy) || !IsValidSortColumn(sortBy))
            {
                throw new ArgumentException("Invalid sort column", nameof(sortBy));
            }

            return await _repository.GetAllNotesAsync(search, sortBy, ascending);
        }

        public async Task<Note?> GetNoteByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid note ID", nameof(id));

            return await _repository.GetNoteByIdAsync(id);
        }

        public async Task<int> CreateNoteAsync(Note note)
        {
            if (note == null) throw new ArgumentNullException(nameof(note));

            if (string.IsNullOrWhiteSpace(note.Title)) throw new ArgumentException("Title is required", nameof(note));

            return await _repository.CreateNoteAsync(note);
        }

        public async Task<bool> UpdateNoteAsync(Note note)
        {
            if (note == null) throw new ArgumentNullException(nameof(note));

            var existingNote = await _repository.GetNoteByIdAsync(note.Id);
            if (existingNote == null) throw new KeyNotFoundException($"Note with ID {note.Id} not found");

            return await _repository.UpdateNoteAsync(note);
        }

        public async Task<bool> DeleteNoteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid note ID", nameof(id));

            var note = await _repository.GetNoteByIdAsync(id);
            if (note == null) return false;

            try
            {
                return await _repository.DeleteNoteAsync(id);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // Helpe function to validate allowed sort columns
        private bool IsValidSortColumn(string sortBy)
        {
            var validColumns = new List<string> { "CreatedAt", "Title", "UpdatedAt" };
            return validColumns.Contains(sortBy);
        }
    }
}
