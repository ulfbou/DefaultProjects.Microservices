using DefaultProjects.Microservices.TenantManagementServices.Routes;
using DefaultProjects.Microservices.TenantManagementServices.Services;
using DefaultProjects.Shared.Constants;
using DefaultProjects.Shared.DTOs;
using DefaultProjects.Shared.Extensions;
using DefaultProjects.Shared.Interfaces;
using DefaultProjects.Shared.Models;
using DefaultProjects.Shared.Validation;

using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using System.Text.Json;

namespace DefaultProjects.Microservices.TenantManagementServices.Routes;

public static class TenantManagementApiExtensions
{
    public static void MapTenantManagementEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var service = endpoints.ServiceProvider.GetRequiredService<ITenantService>();

        endpoints.MapPost(Tenants.Routes.CreateEndpoint, async context => await CreateTenant(service, context))
                 .RequireAuthorization()
                 .WithValidation();

        endpoints.MapGet(Tenants.Routes.GetEndpoint, async context => await GetTenantDetails(service, context))
                 .RequireAuthorization();

        endpoints.MapPut(Tenants.Routes.UpdateEndpoint, async context => await UpdateTenant(service, context))
                 .RequireAuthorization()
                 .WithValidation();

        endpoints.MapDelete(Tenants.Routes.DeleteEndpoint, DeleteTenant)
                 .RequireAuthorization();
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
