using DefaultProjects.Microservices.TenantManagementServices.Services;
using DefaultProjects.Shared.DTOs;
using DefaultProjects.Shared.Interfaces;

using FluentInjections;

using System.Text.Json;

namespace DefaultProjects.Microservices.TenantManagementServices.Routes;

public class TenantServiceRoutesModule : Module<IServiceConfigurator>
{
    public override void Configure(IServiceConfigurator configurator)
    {
        configurator.Services.AddSingleton<ITenantService, TenantService>();
        configurator.Services.AddRouting();
    }
}

public class TenantMiddlewareRoutesModule : Module<IMiddlewareConfigurator>
{

    public override void Configure(IMiddlewareConfigurator configurator)
    {
        var app = configurator.Application;

        var tenantService = app.ApplicationServices.GetRequiredService<ITenantService>();
        var userManagementService = app.ApplicationServices.GetRequiredService<IUserManagementService>();

        var routeBuilder = new RouteBuilder(app);

        routeBuilder.MapPost("/api/tenants", async context =>
        {
            var tenantDto = await JsonSerializer.DeserializeAsync<TenantCreationDTO>(context.Request.Body);
            var tenant = await tenantService.CreateAsync(tenantDto, context.RequestAborted);
            var adminUser = await userManagementService.GetUserAsync(tenantDto.AdminEmail, context.RequestAborted);

            var response = new
            {
                tenantId = tenant.Id,
                adminUserId = adminUser?.Id
            };

            context.Response.ContentType = "application/json";
            await JsonSerializer.SerializeAsync(context.Response.Body, response);
        });

        routeBuilder.MapGet("/api/tenants/{tenantId}", async context =>
        {
            var tenantId = context.GetRouteValue("tenantId").ToString();
            var tenant = await tenantService.GetAsync(tenantId, context.RequestAborted);

            if (tenant == null)
            {
                context.Response.StatusCode = 404;
                return;
            }

            var response = new
            {
                tenantId = tenant.Id,
                companyName = tenant.CompanyName,
                createdDate = tenant.CreatedDate.ToString("o"),
                plan = tenant.Plan
            };

            context.Response.ContentType = "application/json";
            await JsonSerializer.SerializeAsync(context.Response.Body, response);
        });

        routeBuilder.MapPut("/api/tenants/{tenantId}", async context =>
        {
            var tenantId = context.GetRouteValue("tenantId").ToString();
            var tenantDto = await JsonSerializer.DeserializeAsync<TenantCreationDTO>(context.Request.Body);
            var tenant = await tenantService.UpdateAsync(tenantId, tenantDto, context.RequestAborted);

            if (tenant == null)
            {
                context.Response.StatusCode = 404;
                return;
            }

            context.Response.StatusCode = 204;
        });

        routeBuilder.MapDelete("/api/tenants/{tenantId}", async context =>
        {
            var tenantId = context.GetRouteValue("tenantId").ToString();
            await tenantService.DeleteAsync(tenantId, context.RequestAborted);

            var response = new
            {
                message = "Tenant deleted successfully."
            };

            context.Response.ContentType = "application/json";
            await JsonSerializer.SerializeAsync(context.Response.Body, response);
        });

        app.UseRouter(routeBuilder.Build());
    }
}
