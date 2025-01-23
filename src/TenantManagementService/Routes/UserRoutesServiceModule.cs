using DefaultProjects.Microservices.TenantManagementServices.Services;
using DefaultProjects.Shared.Interfaces;

using FluentInjections;

namespace DefaultProjects.Microservices.TenantManagementServices.Routes;

public partial class TenantRoutesMiddlewareModule
{
    public class UserRoutesServiceModule : Module<IServiceConfigurator>
    {
        public override void Configure(IServiceConfigurator configurator)
        {
            configurator.Bind<ITenantService>()
                        .To<TenantService>()
                        .AsSingleton();
            configurator.Services.AddRouting();
        }
    }
}
