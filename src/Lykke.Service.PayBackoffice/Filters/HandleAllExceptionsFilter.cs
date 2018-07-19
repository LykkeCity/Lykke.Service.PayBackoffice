using System;
using System.Web;
using Common.Log;
using Lykke.Common.Log;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;


namespace BackOffice.Filters
{
    public class HandleAllExceptionsFilter : IExceptionFilter
    {
        private readonly IHostingEnvironment _environment;
        private readonly ILog _log;

        public HandleAllExceptionsFilter(IHostingEnvironment environment, ILogFactory logFactory)
        {
            _environment = environment;
            _log = logFactory.CreateLog(this);
        }

        public void OnException(ExceptionContext filterContext)
        {
            var controller = filterContext.RouteData.Values["controller"].ToString();
            var action = filterContext.RouteData.Values["action"].ToString();

            var context = filterContext.HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress + ": " +
                          filterContext.HttpContext.User.Identity.Name;

            _log.WriteErrorAsync("API Exception", $"{filterContext.HttpContext.Request.Host} {filterContext.HttpContext.Request.Method} {filterContext.HttpContext.Request.Path}", context, filterContext.Exception).Wait();

            if (filterContext == null)
            {
                throw new ArgumentNullException(nameof(filterContext));
            }

            // If custom errors are disabled, we need to let the normal ASP.NET exception handler
            // execute so that the user can see useful debugging information.
            if (filterContext.ExceptionHandled || !_environment.IsDevelopment())
            {
                return;
            }

            filterContext.HttpContext.Response.Clear();
            filterContext.Result = new ObjectResult(new ExceptionData(controller, action, filterContext.Exception))
            {
                StatusCode = 500,
                DeclaredType = typeof(ExceptionData)
            };
            filterContext.ExceptionHandled = true;
        }

    }

    public class ExceptionData
    {
        public string Controller { get; private set; }
        public string Action { get; private set; }
        public Exception Exception { get; private set; }

        public ExceptionData(string controller, string action, Exception e)
        {
            Controller = controller;
            Action = action;
            Exception = e;
        }
    }
}
