using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public class BookCreateDTO
    {
        [Required]
        [StringLength(250, ErrorMessage = "El campo {0} debe tener {1 caracteres o menos}")]
        public required string Title { get; set; }
        public int AuthorId { get; set; }
    }
}
