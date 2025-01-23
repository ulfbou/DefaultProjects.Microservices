using DefaultProjects.Microservices.UserManagementServices.Services;
using DefaultProjects.Shared.Interfaces;

using FluentInjections;

namespace DefaultProjects.Microservices.UserManagementServices.Routes
{
    public class UserRoutesServiceModule : Module<IServiceConfigurator>
    {
        public override void Configure(IServiceConfigurator configurator)
        {
            configurator.Bind<IUserManagementService>()
                        .To<UserManagementService>()
                        .AsSingleton();
            configurator.Services.AddRouting();
        }
    }
}
