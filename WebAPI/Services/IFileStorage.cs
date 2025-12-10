namespace WebAPI.Services
{
    public interface IFileStorage
    {
        Task Delete(string? route, string container);
        Task<string> Store(string container, IFormFile file);
        async Task<string> Edit(string? route, string container, IFormFile file)
        {
            await Delete(route, container);
            return await Store(container, file);
        }
    }
}
