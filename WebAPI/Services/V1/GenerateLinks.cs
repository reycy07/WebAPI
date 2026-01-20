using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using System;
using WebAPI.DTOs;
using WebAPI.Entities;
using WebAPI.Routes;

namespace WebAPI.Services.V1
{
    public class GeneratorLinks : IGeneratorLinks
    {
        private readonly LinkGenerator linkGenerator;
        private readonly IAuthorizationService authorizationService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly AuthorRoutes authorRoutes;

        public GeneratorLinks(
                LinkGenerator linkGenerator,
                IAuthorizationService authorizationService,
                IHttpContextAccessor httpContextAccessor,
                AuthorRoutes authorRoutes
            )
        {
            this.linkGenerator = linkGenerator;
            this.authorizationService = authorizationService;
            this.httpContextAccessor = httpContextAccessor;
            this.authorRoutes = authorRoutes;
        }

        public async Task<ResourcesColectionDTO<AuthorDTO>> GenerateLinks(List<AuthorDTO> authorsDTO)
        {
            var result = new ResourcesColectionDTO<AuthorDTO> { Values = authorsDTO };

            var user = httpContextAccessor.HttpContext!.User;
            var isAdmin = await authorizationService.AuthorizeAsync(user, "isAdmin");

            foreach (var dto in authorsDTO)
            {
                GenerateLinks(dto, isAdmin.Succeeded);
            }
            if (isAdmin.Succeeded)
            {
                var createUser = GenerateSpecificLink("CreateAuthorV1", "create-author", new { }, "POST");
                result.Links.Add(createUser);
                var createUserWithPicture = GenerateSpecificLink("CreateAuthorWithPictureV1", "create-author-with-picture", new { }, "POST");
                result.Links.Add(createUserWithPicture);

            }
            var getUserList = GenerateSpecificLink("GetAuthorsV1", "self", new { }, "GET");
            result.Links.Add(getUserList);

            return result;
        }

        public async Task GenerateLinks(AuthorDTO authorDTO)
        {
            var user = httpContextAccessor.HttpContext!.User;
            var isAdmin = await authorizationService.AuthorizeAsync(user, "isAdmin");
            GenerateLinks(authorDTO, isAdmin.Succeeded);

        }

        private void GenerateLinks(AuthorDTO authorDTO, bool isAdmin)
        {
            var routes = authorRoutes.BuildRouteList(authorDTO);
            foreach (var route in routes)
            {
                if (isAdmin)
                {
                    var item = GenerateSpecificLink(route.Route, route.Description, route.Author, route.Method);
                    authorDTO.Links.Add(
                            item
                        );
                }
            }
        }

        private DataHATEOASDTO GenerateSpecificLink(string route, string description, object author, string method)
        {
            DataHATEOASDTO dataHATEOASDTO = new(
                    Link: linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext!, route, author)!,
                    Description: description,
                    Method: method
                    );

            return dataHATEOASDTO;
        }
    }
}
