using System.ComponentModel.DataAnnotations;

namespace Note_API.DTOs
{
    public class NoteUpdateDto
    {
        [Required]
        [MaxLength(100)] 
        public string Title { get; set; } = string.Empty;

        public string? Content { get; set; }
    }
}
