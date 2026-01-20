using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace WebAPI.Swagger
{
    public class VersionGroupConventional : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var controllerNameSpace = controller.ControllerType.Namespace;
            var version = controllerNameSpace!.Split(".").Last().ToLower();
            controller.ApiExplorer.GroupName = version;
        }
    }
}
