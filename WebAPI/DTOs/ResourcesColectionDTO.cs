namespace WebAPI.DTOs
{
    public class ResourcesColectionDTO<T>: ResourceDTO where T: ResourceDTO
    {
        public IEnumerable<T> Values { get; set; } = [];
    }
}
