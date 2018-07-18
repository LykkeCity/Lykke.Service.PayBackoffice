﻿using System;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BackOffice.Filters;
using BackOffice.Middleware;
using Autofac.Extensions.DependencyInjection;
using BackOffice.ModelBinders;
using BackOffice.Settings;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.BackofficeMembership.Client.Filters;
using Microsoft.AspNetCore.Mvc;
using Lykke.Logs;
using Lykke.SettingsReader;

namespace BackOffice
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        public IContainer ApplicationContainer { get; private set; }
        private ILog _log;
        private IHealthNotifier _healthNotifier;


        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                //.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
                .AddEnvironmentVariables();

            builder.AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            Configuration = builder.Build();
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(o =>
            {
                o.Filters.Add(new HandleAllExceptionsFilterFactory());
                o.Filters.Add(new ForceLogoutUserFilterFactory());
                o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                o.ModelBinderProviders.Insert(0, new DecimalInvariantModelBinderProvider());
            });

            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddAuthentication("Cookie")
                .AddCookie("Cookie", options =>
                {
                    options.LoginPath = new PathString("/Home/Index/");
                    options.AccessDeniedPath = new PathString("/Home/Index/");
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(1);
                });

            var settings = Configuration.LoadSettings<BackOfficeBundle>();

            services.AddLykkeLogging
            (
                settings.ConnectionString(x => x.PayBackOffice.Db.LogsConnString),
                "LogBackoffce",
                settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                settings.CurrentValue.SlackNotifications.AzureQueue.QueueName
            );

            ApplicationContainer = Dependencies.BindDependecies(services, Configuration);

            _log = ApplicationContainer.Resolve<ILogFactory>().CreateLog(this);
            _healthNotifier = ApplicationContainer.Resolve<IHealthNotifier>();
            _log.Info("App Stated");

            return new AutofacServiceProvider(ApplicationContainer);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
        {
            app.UseDeveloperExceptionPage();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
            }

            app.UseStaticFiles();

            app.UseMiddleware<LocalizationMiddleware>();

            app.UseAuthentication();

            app.UseAntiforgery();

            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "changePassword", template: "{action=ChangePassword}/{id}", defaults: new { controller = "Home" });
                routes.MapRoute(name: "changePin", template: "{action=ChangePin}/{id}", defaults: new { controller = "Home" });
                routes.MapRoute(name: "areaRoute", template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute(name: "Default", template: "{controller=Home}/{action=Index}/{id?}");
            });

            appLifetime.ApplicationStopped.Register(CleanUp);
        }

        private void CleanUp()
        {
            _log?.Info("Cleaning up...");

            ApplicationContainer.Dispose();

            _log?.Info("Cleaned up");
        }
    }
}
