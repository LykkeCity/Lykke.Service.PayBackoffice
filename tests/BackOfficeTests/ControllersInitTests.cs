using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Autofac;
using NUnit.Framework;
using Moq;
using BackOffice;
using BackOffice.Binders;

namespace BackOfficeTests
{
    [TestFixture]
    public class ControllersInitTests
    {
        [Test]
        public void AllBackOfficeControllersTest()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddEnvironmentVariables();
            var configuration = configBuilder.Build();
            configuration["ConnectionStrings:ConnectionString"] = configuration["SettingsUrlForBackOfficeTests"];

            var binder = new AzureBinder();
            var builder = binder.Bind(configuration, null, true);
            builder.RegisterInstance<Microsoft.AspNetCore.Http.IHttpContextAccessor>(new Microsoft.AspNetCore.Http.HttpContextAccessor());
            builder.RegisterInstance(new Mock<IHostingEnvironment>().Object).As<IHostingEnvironment>();
            var controllerTypes = typeof(Startup).Assembly
                .GetExportedTypes()
                .Where(t => t.IsSubclassOf(typeof(Controller)))
                .ToList();
            foreach (var type in controllerTypes)
                builder.RegisterType(type);

            var ioc = builder.Build();

            var failedControllers = new List<string>();
            foreach (var type in controllerTypes)
            {
                try
                {
                    ioc.Resolve(type);
                }
                catch (Exception)
                {
                    failedControllers.Add(type.Name);
                }
            }
            if (failedControllers.Count > 0)
                throw new Exception($"These WalletApi controllers can't be instantiated: {string.Join(",", failedControllers)}");
            Assert.IsTrue(true);
        }
    }
}
