using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public class UserDTO
    {
        [EmailAddress]
        public required string Email { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
