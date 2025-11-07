using System.ComponentModel.DataAnnotations;
using WebAPI.Validations;

namespace WebAPI.Entities
{
    public class Author
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "{0} filed is required")]
        [StringLength(150, ErrorMessage = "{0} filed should be {1} caracthers or less")]
        [FirstLetterUppercase]
        public required string Names { get; set; }

        [Required(ErrorMessage = "{0} filed is required")]
        [StringLength(150, ErrorMessage = "{0} filed should be {1} caracthers or less")]
        [FirstLetterUppercase]
        public required string LastNames { get; set; }

        [StringLength(20, ErrorMessage = "{0} filed should be {1} caracthers or less")]
        public string? Identification { get; set; }

        public List<Book> Books { get; set; } = new List<Book>();

    }
}
