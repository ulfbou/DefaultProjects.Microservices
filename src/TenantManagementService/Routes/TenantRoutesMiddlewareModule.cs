using DefaultProjects.Shared.Constants;
using DefaultProjects.Shared.DTOs;
using DefaultProjects.Shared.Extensions;
using DefaultProjects.Shared.Interfaces;

using System.Text.Json;

using FluentInjections;

namespace DefaultProjects.Microservices.TenantManagementServices.Routes;

public partial class TenantRoutesMiddlewareModule : Module<IMiddlewareConfigurator>
{
    public override void Configure(IMiddlewareConfigurator configurator)
    {
        var app = configurator.Application;
        var service = app.ApplicationServices.GetRequiredService<ITenantService>();

        var routeBuilder = new RouteBuilder(app);

        //routeBuilder.MapPost(Tenants.Routes.CreateEndpoint, async context => await CreateTenant(service, context))
        //            .UseValidation()
        //            .RequireAuthorization();

        //routeBuilder.MapGet(Tenants.Routes.GetEndpoint, async context => await GetTenantDetails(service, context))
        //            .RequireAuthorization();

        //routeBuilder.MapPut(Tenants.Routes.UpdateEndpoint, async context => await UpdateTenant(service, context))
        //            .RequireAuthorization()
        //            .WithValidation();

        //routeBuilder.MapDelete(Tenants.Routes.DeleteEndpoint, async context => await DeleteTenant(service, context))
        //            .RequireAuthorization();

        var tenantService = app.ApplicationServices.GetRequiredService<ITenantService>();
        var userManagementService = app.ApplicationServices.GetRequiredService<IUserManagementService>();

        routeBuilder.MapPost(Tenants.Routes.CreateEndpoint, async context =>
        {
            var tenantDto = await JsonSerializer.DeserializeAsync<TenantCreationDTO>(context.Request.Body);

            if (tenantDto is null)
            {
                context.Response.StatusCode = 400;
                return;
            }

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

        routeBuilder.MapGet(Tenants.Routes.GetEndpoint, async context =>
        {
            var tenantId = context.GetRouteValue("tenantId")?.ToString();

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                context.Response.StatusCode = 400;
                return;
            }

            var tenant = await tenantService.GetAsync(tenantId, context.RequestAborted);

            if (tenant is null)
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

        routeBuilder.MapPut(Tenants.Routes.UpdateEndpoint, async context =>
        {
            var tenantId = context.GetRouteValue("tenantId")?.ToString();

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                context.Response.StatusCode = 400;
                return;
            }

            var tenantDto = await JsonSerializer.DeserializeAsync<TenantUpdateDTO>(context.Request.Body);

            if (tenantDto is null)
            {
                context.Response.StatusCode = 400;
                return;
            }

            var tenant = await tenantService.UpdateAsync(tenantId, tenantDto, context.RequestAborted);

            if (tenant is null)
            {
                context.Response.StatusCode = 404;
                return;
            }

            context.Response.StatusCode = 204;
        });

        routeBuilder.MapDelete(Tenants.Routes.DeleteEndpoint, async context =>
        {
            var tenantId = context.GetRouteValue("tenantId")?.ToString();

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                context.Response.StatusCode = 400;
                return;
            }

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

    private static async Task<IResult> CreateTenant(ITenantService service, HttpContext context)
    {
        var tenantDto = await context.Request.ReadFromJsonAsync<TenantCreationDTO>(context.RequestAborted);

        if (tenantDto is null)
        {
            return Results.BadRequest(new { message = Tenants.Messages.TenantDataRequired });
        }

        var tenant = await service.CreateAsync(tenantDto, context.RequestAborted);

        if (tenant is null)
        {
            return Results.InternalServerError(new { message = Tenants.Messages.FailedToCreate });
        }

        return Results.Created($"/tenants/{tenant.Id}", new { message = Tenants.Messages.Created });
    }

    private static async Task<IResult> GetTenantDetails(ITenantService service, HttpContext context)
    {
        var tenantId = context.Request.RouteValues["tenantId"]?.ToString();

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return Results.BadRequest(new { message = Tenants.Messages.TenantIdMissing });
        }

        var tenant = await service.GetAsync(tenantId, context.RequestAborted);

        if (tenant is null)
        {
            return Results.NotFound(new { message = Tenants.Messages.NotFound });
        }

        return Results.Ok(new
        {
            tenantId = tenant.Id,
            companyName = tenant.CompanyName,
            createdDate = tenant.CreatedDate.ToString("o"),
            plan = tenant.Plan
        });
    }

    private static async Task<IResult> UpdateTenant(ITenantService tenantService, HttpContext context)
    {
        var tenantId = context.Request.RouteValues["tenantId"]?.ToString();

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return Results.BadRequest(new { message = Tenants.Messages.TenantIdMissing });
        }

        var tenantDto = await context.Request.ReadFromJsonAsync<TenantUpdateDTO>(context.RequestAborted);

        if (tenantDto is null)
        {
            return Results.BadRequest(new { message = Tenants.Messages.TenantDataRequired });
        }

        var tenant = await tenantService.UpdateAsync(tenantId, tenantDto, context.RequestAborted);

        if (tenant is null)
        {
            return Results.NotFound(new { message = Tenants.Messages.NotFound });
        }

        return Results.Ok(new { message = Tenants.Messages.Updated });
    }

    private static async Task<IResult> DeleteTenant(ITenantService tenantService, HttpContext context)
    {
        var tenantId = context.Request.RouteValues["tenantId"]?.ToString();

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return Results.BadRequest(new { message = Tenants.Messages.TenantIdMissing });
        }

        await tenantService.DeleteAsync(tenantId, context.RequestAborted);
        return Results.Ok(new { message = Tenants.Messages.Deleted });
    }
}
