using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public class UserCredentialsDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
