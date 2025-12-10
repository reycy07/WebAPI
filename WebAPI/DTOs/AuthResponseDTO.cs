namespace WebAPI.DTOs
{
    public class AuthResponseDTO
    {
        public required string Token { get; set; }
        public DateTime Expiration { get; set; }

    }
}
