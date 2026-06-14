using Remp.API.Middlewares;

namespace Remp.API.Extensions;

public static class MiddlewareExtensions
{
     public static IApplicationBuilder UseExceptionMiddleware(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }
}
