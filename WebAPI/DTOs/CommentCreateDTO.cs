using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public class CommentCreateDTO
    {
        [Required]
        public required string Body { get; set; }
    }
}
