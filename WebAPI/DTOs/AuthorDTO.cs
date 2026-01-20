using WebAPI.Entities;

namespace WebAPI.DTOs
{
    public class AuthorDTO: ResourceDTO
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public string? Picture { get; set; }

    }
}
