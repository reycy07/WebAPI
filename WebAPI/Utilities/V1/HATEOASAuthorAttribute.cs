using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebAPI.DTOs;
using WebAPI.Entities;
using WebAPI.Services.V1;

namespace WebAPI.Utilities.V1
{
    public class HATEOASAuthorAttribute : HATEOASFilterAttribute
    {
        private readonly IGeneratorLinks generatorLinks;

        public HATEOASAuthorAttribute(IGeneratorLinks generatorLinks)
        {
            this.generatorLinks = generatorLinks;
        }

        public override async Task OnResultExecutionAsync
            (ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var includeHATEOAS = MustIncludeHATEOAS(context);

            if (!includeHATEOAS)
            {
                await next();
                return;
            }

            var result = context.Result as ObjectResult;
            var model = result!.Value as AuthorDTO ?? throw new ArgumentException("Se esperaba una instancia de AuthorDTO");
            await generatorLinks.GenerateLinks(model);
            await next();
            
        }
    } 
}
