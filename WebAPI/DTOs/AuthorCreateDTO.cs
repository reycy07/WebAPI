using System.ComponentModel.DataAnnotations;
using WebAPI.Validations;

namespace WebAPI.DTOs
{
    public class AuthorCreateDTO
    {
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
    }
}
