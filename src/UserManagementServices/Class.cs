using DefaultProjects.Microservices.UserManagementServices.Services;
using DefaultProjects.Shared.Interfaces;

namespace DefaultProjects.Microservices.UserManagementServices.Routes;

public class UserRoutesModule : Module<IMiddlewareConfigurator>
{
    public void Configure(IServiceConfigurator configurator)
    {
        configurator.Services.AddSingleton<IUserManagementService, UserManagementService>();
        configurator.Services.AddRouting();
    }
}
