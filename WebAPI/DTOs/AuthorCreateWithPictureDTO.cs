namespace WebAPI.DTOs
{
    public class AuthorCreateWithPictureDTO:AuthorCreateDTO
    {
        public IFormFile? Picture { get; set; } 
    }
}