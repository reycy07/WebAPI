using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public class EditClaimDTO
    {
        [EmailAddress]
        [Required]
        public required string Email { get; set; }

    }
}
