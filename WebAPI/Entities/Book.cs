using System.ComponentModel.DataAnnotations;

namespace WebAPI.Entities
{
    public class Book
    {
        public int Id { get; set; }
        [Required]
        [StringLength(250, ErrorMessage = "El campo {0} debe tener {1 caracteres o menos}")]
        public required string Title { get; set; }
        public List<AuthorBook> Authors { get; set; } = [];  
        public List<Comment> Comments { get; set; } = [];
    }
}
