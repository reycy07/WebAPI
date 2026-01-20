using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;

namespace WebAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1")]
    [Authorize]
    public class RouteController:ControllerBase
    {
        private readonly IAuthorizationService authorizationService;

        public RouteController(IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
        }
        [HttpGet(Name = "GetRootV1")]
        [AllowAnonymous]
        public async Task <IEnumerable<DataHATEOASDTO>> Get()
        {
            var dataHATEOAS = new List<DataHATEOASDTO>();

            var isAdmin = await authorizationService.AuthorizeAsync(User, "isAdmin");

            dataHATEOAS.Add(new DataHATEOASDTO(Link: Url.Link("GetRootV1", new { })!, Description: "self", Method: "GET"));

            dataHATEOAS.Add(new DataHATEOASDTO(Link: Url.Link("GetAuthorsV1", new { })!, Description: "get-authors", Method: "GET"));

            dataHATEOAS.Add(new DataHATEOASDTO(Link: Url.Link("GetBooksV1", new { })!, Description: "get-book", Method: "GET"));


            dataHATEOAS.Add(new DataHATEOASDTO(Link: Url.Link("LoginV1", new { })!, Description: "login-user", Method: "POST"));
            dataHATEOAS.Add(new DataHATEOASDTO(Link: Url.Link("UpdatetUserV1", new { })!, Description: "update-user", Method: "PUT"));

            if (User.Identity!.IsAuthenticated)
            {
                dataHATEOAS.Add(new DataHATEOASDTO(Link: Url.Link("RegisterNewUserV1", new { })!, Description: "register-user", Method: "POST"));
                dataHATEOAS.Add(new DataHATEOASDTO(Link: Url.Link("RegenerateTokenV1", new { })!, Description: "review-token", Method: "GET"));
            }

            // Actions for only admins users able to use
            if (isAdmin.Succeeded)
            {
                dataHATEOAS.Add(new DataHATEOASDTO(Link: Url.Link("GetUsersV1", new { })!, Description: "get-user", Method: "GET"));
                dataHATEOAS.Add(new DataHATEOASDTO(Link: Url.Link("CreateAuthorWithPictureV1", new { })!, Description: "create-author-with-picture", Method: "POST"));
                dataHATEOAS.Add(new DataHATEOASDTO(Link: Url.Link("CreateAuthorV1", new { })!, Description: "create-author", Method: "POST"));
                dataHATEOAS.Add(new DataHATEOASDTO(Link: Url.Link("CreateAuthorsV1", new { })!, Description: "create-authors", Method: "POST"));
                dataHATEOAS.Add(new DataHATEOASDTO(Link: Url.Link("CreateBookV1", new { })!, Description: "create-book", Method: "POST"));
            }

            return dataHATEOAS;
        }
    }
}
