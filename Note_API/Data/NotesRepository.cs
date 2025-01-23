using Dapper;
using Note_API.Models;

namespace Note_API.Data
{
    public class NotesRepository
    {
        private readonly DbConnection _dbConnection;
        private readonly ILogger<NotesRepository> _logger;

        public NotesRepository(DbConnection dbConnection, ILogger<NotesRepository> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task<IEnumerable<Note>> GetAllNotesAsync(string? search = null, string? sortBy = "CreatedAt", bool ascending = false)
        {
            try
            {
                using var connection = _dbConnection.CreateConnection();

                var validSortColumns = new HashSet<string> { "Title", "CreatedAt" };
                if (!validSortColumns.Contains(sortBy))
                    throw new ArgumentException("Invalid sort column.", nameof(sortBy));

                var sql = $@"
                    SELECT * FROM Notes
                    WHERE (@Search IS NULL OR Title LIKE '%' + @Search + '%' OR Content LIKE '%' + @Search + '%')
                    ORDER BY {(sortBy == "Title" ? "Title" : "CreatedAt")} {(ascending ? "ASC" : "DESC")}";

                return await connection.QueryAsync<Note>(sql, new { Search = search });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notes with search: {Search}, sortBy: {SortBy}, ascending: {Ascending}", search, sortBy, ascending);
                throw new ApplicationException("An error occurred while retrieving notes.", ex);
            }
        }

        public async Task<Note?> GetNoteByIdAsync(int id)
        {
            try
            {
                using var connection = _dbConnection.CreateConnection();
                return await connection.QueryFirstOrDefaultAsync<Note>(
                    "SELECT * FROM Notes WHERE Id = @Id",
                    new { Id = id }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving note with ID {Id}", id);
                throw new ApplicationException($"An error occurred while retrieving the note with ID {id}.", ex);
            }
        }

        public async Task<int> CreateNoteAsync(Note note)
        {
            try
            {
                using var connection = _dbConnection.CreateConnection();

                var sql = @"
                    INSERT INTO Notes (Title, Content, CreatedAt) 
                    VALUES (@Title, @Content, @CreatedAt); 
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var newId = await connection.ExecuteScalarAsync<int>(sql, note);
                _logger.LogInformation("Note created with ID {Id}", newId);

                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating note: {Title}", note.Title);
                throw new ApplicationException("An error occurred while creating the note.", ex);
            }
        }

        public async Task<bool> UpdateNoteAsync(Note note)
        {
            try
            {
                using var connection = _dbConnection.CreateConnection();

                var sql = @"
                    UPDATE Notes 
                    SET Title = @Title, Content = @Content, UpdatedAt = @UpdatedAt 
                    WHERE Id = @Id";

                var rowsAffected = await connection.ExecuteAsync(sql, note);
                if (rowsAffected > 0)
                {
                    _logger.LogInformation("Note updated successfully with ID {Id}", note.Id);
                    return true;
                }

                _logger.LogWarning("No rows affected while updating note with ID {Id}", note.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating note with ID {Id}", note.Id);
                throw new ApplicationException($"An error occurred while updating the note with ID {note.Id}.", ex);
            }
        }

        public async Task<bool> DeleteNoteAsync(int id)
        {
            try
            {
                using var connection = _dbConnection.CreateConnection();

                var rowsAffected = await connection.ExecuteAsync(
                    "DELETE FROM Notes WHERE Id = @Id",
                    new { Id = id }
                );

                if (rowsAffected > 0)
                {
                    _logger.LogInformation("Note with ID {Id} deleted successfully", id);
                    return true;
                }

                _logger.LogWarning("No rows affected while deleting note with ID {Id}", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting note with ID {Id}", id);
                throw new ApplicationException($"An error occurred while deleting the note with ID {id}.", ex);
            }
        }
    }
}
