using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebAPI.Utilities
{
    public class HATEOASFilterAttribute: ResultFilterAttribute
    {
        public bool MustIncludeHATEOAS(ResultExecutingContext context)
        { 
            if (context.Result is not ObjectResult result || !IsSuccessfulyResponse(result)) return false;

            var validateHeader = context.HttpContext.Request.Headers;
            if (!validateHeader.TryGetValue("IncludeHATEOAS", out var header) ) return false;

            return string.Equals(header, "Y", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsSuccessfulyResponse(ObjectResult result)
        {
            if(result.Value is null) return false;

            if(result.StatusCode.HasValue && !result.StatusCode.Value.ToString().StartsWith("2")) return false;


            return true;
        }
    }
}

