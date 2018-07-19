using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BackOffice.Filters
{
    public class HandleAllExceptionsFilterFactory : IFilterFactory
    {
        public bool IsReusable => true;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var environment = (IHostingEnvironment)serviceProvider.GetService(typeof(IHostingEnvironment));
            var logFactory = (ILogFactory)serviceProvider.GetService(typeof(ILogFactory));
            return new HandleAllExceptionsFilter(environment, logFactory);
        }
    }
}
