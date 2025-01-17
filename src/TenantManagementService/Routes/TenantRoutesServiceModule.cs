using DefaultProjects.Microservices.TenantManagementServices.Services;
using DefaultProjects.Shared.Interfaces;

using FluentInjections;

namespace DefaultProjects.Microservices.TenantManagementServices.Routes;

public class TenantRoutesServiceModule : Module<IServiceConfigurator>
{
    public override void Configure(IServiceConfigurator configurator)
    {
        configurator.Services.AddSingleton<ITenantService, TenantService>();
        configurator.Services.AddRouting();
    }
}
