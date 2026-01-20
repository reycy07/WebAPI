using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebAPI.DTOs;
using WebAPI.Services.V1;

namespace WebAPI.Utilities.V1
{
    public class HATEOASAuthorsAttribute: HATEOASFilterAttribute
    {
        private readonly IGeneratorLinks generatorLinks;

        public HATEOASAuthorsAttribute(IGeneratorLinks generatorLinks)
        {
            this.generatorLinks = generatorLinks;
        }

        public override async Task OnResultExecutionAsync
            (ResultExecutingContext context , ResultExecutionDelegate next)
        {
            var includeHATEOAS = MustIncludeHATEOAS(context);

            if (!includeHATEOAS)
            {
                await next();
                return;
            }

            var result = context.Result as ObjectResult;
            var model = result!.Value as List<AuthorDTO> ?? throw new ArgumentNullException("Se esperaba una instancia de List<AuthorDTO> ");
            context.Result = new OkObjectResult(await generatorLinks.GenerateLinks(model));
            await next();
        } 
    }
}
