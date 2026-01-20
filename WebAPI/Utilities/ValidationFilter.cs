using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.DTOs;

namespace WebAPI.Utilities
{
    public class ValidationFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext dbContext;

        public ValidationFilter(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if(!context.ActionArguments.TryGetValue("BookCreateDTO", out var value) || value is not BookCreateDTO bookCreateDTO)
            {
                context.ModelState.AddModelError(string.Empty, "El modelo enviado no es válido");
                context.Result = context.ModelState.BuildProblemDetail();
                return;

            }

            if (bookCreateDTO.AuthorsIds is null || bookCreateDTO.AuthorsIds.Count == 0)
            {
                context.ModelState.AddModelError(nameof(bookCreateDTO.AuthorsIds), "No se puede crear un libro sin autores");
                context.Result = context.ModelState.BuildProblemDetail();
                return;
            }

            var authorsIdsExist = await dbContext.Authors
                                  .Where(x => bookCreateDTO.AuthorsIds.Contains(x.Id))
                                  .Select(x => x.Id).ToListAsync();
            if (authorsIdsExist.Count != bookCreateDTO.AuthorsIds.Count)
            {
                var authorsNoExist = bookCreateDTO.AuthorsIds.Except(authorsIdsExist);
                var authorsNoExistString = string.Join(",", authorsNoExist);
                var errorMessage = $"Los siguientes autores no existen: {authorsNoExistString}";
                context.ModelState.AddModelError(nameof(bookCreateDTO.AuthorsIds), errorMessage);
                context.Result=context.ModelState.BuildProblemDetail();
                return;
            }
            await next();
        }
    }
}
