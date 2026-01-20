using WebAPI.DTOs;

namespace WebAPI.Services.V1
{
    public interface IGeneratorLinks
    {
        Task GenerateLinks(AuthorDTO authorDTO);
        Task<ResourcesColectionDTO<AuthorDTO>> GenerateLinks(List<AuthorDTO> authorsDTO);
    }
}