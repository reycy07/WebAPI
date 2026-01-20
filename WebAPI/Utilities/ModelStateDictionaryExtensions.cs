using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebAPI.Utilities
{
    public static class ModelStateDictionaryExtensions
    {
        public static BadRequestObjectResult BuildProblemDetail(this ModelStateDictionary modelState)
        {
            var detailProblems = new ValidationProblemDetails(modelState)
            {
                Title = "Uno o mas validaciones no pasaron.",
                Status = StatusCodes.Status400BadRequest
            };

            return new BadRequestObjectResult(detailProblems);
        }
    }
}
