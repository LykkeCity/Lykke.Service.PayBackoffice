using System;
using System.Threading.Tasks;
using BackOffice.Extensions;
using BackOffice.Services;
using Core.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BackOffice.Middleware
{
    public class LocalizationMiddleware
    {
        private readonly RequestDelegate _next;

        public LocalizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var taskUpdateFilter = UsersCache.UpdateUsersAndRoles();

            DetectLanguage(context.Request);

            await taskUpdateFilter;
            await _next.Invoke(context);
        }


        private static void DetectLanguage(HttpRequest request)
        {
            var lang = request.Cookies[ControllerLangExtention.LangCookie];
            ControllerLangExtention.SetThread(lang);
        }
    }
}
