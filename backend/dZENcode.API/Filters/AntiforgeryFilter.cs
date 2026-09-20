using Microsoft.AspNetCore.Antiforgery;

namespace dZENcode.API.Filters;

public sealed class AntiforgeryFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        IAntiforgery antiforgery = context.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();

        await antiforgery.ValidateRequestAsync(context.HttpContext);

        return await next(context);
    }
}
