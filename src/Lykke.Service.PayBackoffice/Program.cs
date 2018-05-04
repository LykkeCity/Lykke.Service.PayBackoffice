using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BackOffice.Binders;
using Common.IocContainer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BackOffice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .ConfigureServices(collection => collection.AddSingleton<IDependencyBinder>(new AzureBinder()))
                .UseStartup<Startup>()
                //.UseUrls("http://localhost:55560/")
                .Build();

            host.Run();
        }
    }
}
