using DefaultProjects.Shared.Validation;

using FluentInjections.Validation;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DefaultProjects.Shared.Extensions;

public static class IEndpointConventionBuilderExtensions
{
    public static IEndpointConventionBuilder WithValidation(this IEndpointConventionBuilder builder)
    {
        Guard.NotNull(builder, nameof(builder));

        if (builder is RouteHandlerBuilder routeHandlerBuilder)
        {
            routeHandlerBuilder.AddEndpointFilter<ValidationFilter>();
            return routeHandlerBuilder;
        }

        throw new InvalidOperationException("Cannot add validation to non-route endpoints");
    }
}
