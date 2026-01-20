using WebAPI.DTOs;

namespace WebAPI.Routes
{
    public record RouteInfo(string Route, string Method, string Description, object Author);
    public class AuthorRoutes
    {
        public List<RouteInfo> Routes { get; set; }
        public AuthorRoutes()
        {
            this.Routes = new List<RouteInfo>();
        }

        public List<RouteInfo> BuildRouteList(AuthorDTO authorDTO)
        {
            this.Routes.Add(new RouteInfo(Route: "GetAuthorV1", Method: "GET", Description: "self", Author: new { id = authorDTO.Id }));
            this.Routes.Add(new RouteInfo(Route: "UpdateAuthorV1", Method: "PUT", Description: "update-author", Author: new { id = authorDTO.Id }));
            this.Routes.Add(new RouteInfo(Route: "PatchAuthorV1", Method: "PATCH", Description: "patch-author", Author: new { id = authorDTO.Id }));
            this.Routes.Add(new RouteInfo(Route: "DeleteAuthorV1", Method: "DELETE", Description: "delete-author", Author: new { id = authorDTO.Id }));
            return this.Routes;
        }
    }
}
