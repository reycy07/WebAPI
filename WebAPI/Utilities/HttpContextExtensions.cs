using Microsoft.EntityFrameworkCore;

namespace WebAPI.Utilities
{
    public static class HttpContextExtensions
    {
        public async static Task
            InsertPaginationParamsInHeader<T>(this HttpContext httpContext, IQueryable<T> queryable)
        {
            if(httpContext is null) {  throw new ArgumentNullException(nameof(httpContext));}

            double qty = await queryable.CountAsync();
            httpContext.Response.Headers.Append("total-qty-recodrs", qty.ToString());
        }
    }
}
