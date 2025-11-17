using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public class CommentDTO
    {
        public Guid Id { get; set; }
        public required string Body { get; set; }
        public DateTime PublishDate { get; set; }

        public int BookId { get; set; }
    }
}
