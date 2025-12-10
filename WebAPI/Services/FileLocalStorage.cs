
namespace WebAPI.Services
{
    public class FileLocalStorage : IFileStorage
    {
        private readonly IWebHostEnvironment env;
        private readonly IHttpContextAccessor httpContextAccessor;

        public FileLocalStorage(
                IWebHostEnvironment env, 
                IHttpContextAccessor httpContextAccessor
            )
        {
            this.env = env;
            this.httpContextAccessor = httpContextAccessor;
        }
        public Task Delete(string? route, string container)
        {
            if (string.IsNullOrEmpty(route))
            {
                return Task.CompletedTask;
            }

            var fileName = Path.GetFileName(route);
            var fileFolder = Path.Combine(env.WebRootPath, container, fileName);

            if (File.Exists(fileFolder))
            {
                File.Delete(fileFolder);
            }

            return Task.CompletedTask;
        }

        public async Task<string> Store(string container, IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            string folder = Path.Combine(env.WebRootPath, container);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string route = Path.Combine(folder, fileName);
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                var content = ms.ToArray();
                await File.WriteAllBytesAsync(route, content);
            }

            var request = httpContextAccessor.HttpContext!.Request;
            var url = $"{request.Scheme}://{request.Host}";
            var fileUrl = Path.Combine(url, container, fileName).Replace("\\", "/");


            return fileUrl;

        }
    }
}
