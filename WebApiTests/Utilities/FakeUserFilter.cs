using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace WebApiTests.Utilities
{
    public class FakeUserFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //before action
            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim("email", "example@gmail.com")
            }, "test"));

            await next();
            //after action
        }
    }
}
