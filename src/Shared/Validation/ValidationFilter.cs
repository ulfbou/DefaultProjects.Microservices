using DefaultProjects.Shared.Extensions;

using Microsoft.AspNetCore.Http;

using System.ComponentModel.DataAnnotations;

namespace DefaultProjects.Shared.Validation;

public class ValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!context.HttpContext.Request.IsJsonType())
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return Results.BadRequest("Invalid content type. Must be application/json.");
        }

        if (!context.HttpContext.Request.TryGetJsonBody(out var result))
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return Results.BadRequest(result);
        }

        var validationContext = new ValidationContext(result!);
        var validationResults = new List<ValidationResult>();

        if (!Validator.TryValidateObject(result!, validationContext, validationResults, true))
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return Results.BadRequest(validationResults);
        }

        return await next(context);
    }
}
