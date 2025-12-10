namespace WebAPI.DTOs
{
    public class ResultDTO
    {
        public required string Hash { get; set; }
        public required byte[] Salt {  get; set; }
    }
}
