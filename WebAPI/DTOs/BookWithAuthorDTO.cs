namespace WebAPI.DTOs
{
    public class BookWithAuthorDTO: BookDTO
    {
        public int AuthorId { get; set; }
        public required string AuthorName { get; set; }
    }
}
