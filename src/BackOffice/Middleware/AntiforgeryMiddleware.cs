using System;
using System.Threading.Tasks;
using BackOffice.Controllers;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BackOffice.Middleware
{
    public class AntiforgeryMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAntiforgery _antiforgery;

        public AntiforgeryMiddleware(RequestDelegate next, IAntiforgery antiforgery)
        {
            _next = next;
            _antiforgery = antiforgery;
        }

        public Task Invoke(HttpContext context)
        {
            if (string.Equals(context.Request.Path, "/", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(context.Request.Path, $"/BackOffice/{nameof(BackOfficeController.Layout)}", StringComparison.OrdinalIgnoreCase))
            {
                var tokens = _antiforgery.GetAndStoreTokens(context);
                context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken,
                    new CookieOptions { HttpOnly = false });
            }
            return _next(context);
        }
    }

    public static class AntiforgeryMiddlewareExtensions
    {
        public static IApplicationBuilder UseAntiforgery(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AntiforgeryMiddleware>();
        }
    }
}