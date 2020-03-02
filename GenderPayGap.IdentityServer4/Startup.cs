using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Queues;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.IdentityServer4.Classes;
using GenderPayGap.Infrastructure.AzureQueues.Extensions;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace GenderPayGap.IdentityServer4
{
    public class Startup
    {

        public static Action<IServiceCollection> ConfigureTestServices;
        public static Action<ContainerBuilder> ConfigureTestContainer;

        private static string _SiteAuthority;
        private readonly IHostingEnvironment env;
        private readonly ILogger logger;

        public Startup(IHostingEnvironment env, ILogger<Startup> logger)
        {
            this.env = env;
            this.logger = logger;
        }

        public static string SiteAuthority
        {
            get
            {
                if (_SiteAuthority == null)
                {
                    _SiteAuthority = Config.SiteAuthority;
                }

                return _SiteAuthority;
            }
            set => _SiteAuthority = value;
        }

        // ConfigureServices is where you register dependencies. This gets
        // called by the runtime before the ConfigureContainer method, below.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add services to the collection. Don't build or return 
            // any IServiceProvider or the ConfigureContainer method
            // won't get called.

            #region Configure identity server

            services.AddSingleton<IEventSink, AuditEventSink>();

            IIdentityServerBuilder builder = services.AddIdentityServer(
                    options => {
                        options.Events.RaiseSuccessEvents = true;
                        options.Events.RaiseFailureEvents = true;
                        options.Events.RaiseErrorEvents = true;
                        options.UserInteraction.LoginUrl = "/sign-in";
                        options.UserInteraction.LogoutUrl = "/sign-out";
                        options.UserInteraction.ErrorUrl = "/error";
                    })
                .AddInMemoryClients(Clients.Get())
                .AddInMemoryIdentityResources(Resources.GetIdentityResources())
                //.AddInMemoryApiResources(Resources.GetApiResources())
                .AddCustomUserStore();

            if (Debugger.IsAttached || Config.IsDevelopment() || Config.IsLocal())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                builder.AddSigningCredential(LoadCertificate());
            }

            #endregion

            //Allow caching of http responses
            services.AddResponseCaching();

            //This is to allow access to the current http context anywhere
            services.AddHttpContextAccessor();

            services.AddMvc(options => { options.AddCacheProfiles(); })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddRazorOptions(
                    // we need to explicitly set AllowRecompilingViewsOnFileChange because we use a custom environment "Local" for local dev 
                    // https://docs.microsoft.com/en-us/aspnet/core/mvc/views/view-compilation?view=aspnetcore-2.2#runtime-compilation
                    options => options.AllowRecompilingViewsOnFileChange = env.IsDevelopment() || Config.IsLocal());

            //Add the distributed redis cache
            services.AddRedisCache();

            //This may now be required 
            services.AddHttpsRedirection(options => { options.HttpsPort = 443; });

            //Override any test services
            ConfigureTestServices?.Invoke(services);

            //Create Inversion of Control container
            Program.ContainerIoC = BuildContainerIoC(services);

            // Create the IServiceProvider based on the container.
            return Program.ContainerIoC.Resolve<IServiceProvider>();
        }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you. If you
        // need a reference to the container, you need to use the
        // "Without ConfigureContainer" mechanism shown later.
        public IContainer BuildContainerIoC(IServiceCollection services)
        {
            var builder = new ContainerBuilder();

            // Note that Populate is basically a foreach to add things
            // into Autofac that are in the collection. If you register
            // things in Autofac BEFORE Populate then the stuff in the
            // ServiceCollection can override those things; if you register
            // AFTER Populate those registrations can override things
            // in the ServiceCollection. Mix and match as needed.
            builder.Populate(services);

            //Set the default encrytpion key
            Encryption.SetDefaultEncryptionKey(Config.GetAppSetting("DefaultEncryptionKey"));

            //Register the configuration
            builder.RegisterInstance(Config.Configuration).SingleInstance();

            builder.Register(c => new SqlRepository(new GpgDatabaseContext(Global.DatabaseConnectionString)))
                .As<IDataRepository>()
                .InstancePerLifetimeScope();

            // Register storage
            string azureStorageShareName = Config.GetAppSetting("AzureStorageShareName");
            string localStorageRoot = Config.GetAppSetting("LocalStorageRoot");
            if (string.IsNullOrWhiteSpace(localStorageRoot))
            {
                //Exponential retry policy is recommended for background tasks - see https://docs.microsoft.com/en-us/azure/architecture/best-practices/retry-service-specific#azure-storage
                builder.Register(
                        c => new AzureFileRepository(
                            Global.AzureStorageConnectionString,
                            azureStorageShareName,
                            new ExponentialRetry(TimeSpan.FromSeconds(3), 10)))
                    .As<IFileRepository>()
                    .SingleInstance();
            }
            else
            {
                builder.Register(c => new SystemFileRepository(localStorageRoot)).As<IFileRepository>().SingleInstance();
            }

            // Register queues (without key filtering)
            builder.Register(c => new LogEventQueue(Global.AzureStorageConnectionString, c.Resolve<IFileRepository>())).SingleInstance();
            builder.Register(c => new LogRecordQueue(Global.AzureStorageConnectionString, c.Resolve<IFileRepository>())).SingleInstance();

            // Register Audit Logger
            builder.RegisterType<AuditLogger>().As<AuditLogger>().InstancePerLifetimeScope();

            // Register Action helpers
            builder.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>().SingleInstance();

            //Override any test services
            ConfigureTestContainer?.Invoke(builder);

            return builder.Build();
        }

        public void Configure(IApplicationBuilder app, IApplicationLifetime lifetime, ILoggerFactory loggerFactory, ILogger<Startup> logger)
        {
            loggerFactory.UseLogEventQueueLogger(app.ApplicationServices);

            app.UseMiddleware<ExceptionMiddleware>();
            if (Debugger.IsAttached || Config.IsDevelopment() || Config.IsLocal())
            {
                IdentityModelEventSource.ShowPII = true;

                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error/500"); //This must use a subdirectory as must start with '/'
                app.UseStatusCodePagesWithReExecute("/error/{0}");
            }

            app.UseIdentityServer();
            app.UseStaticFiles(
                new StaticFileOptions {
                    OnPrepareResponse = ctx => {
                        //Caching static files is required to reduce connections since the default behavior of checking if a static file has changed and returning a 304 still requires a connection.
                        if (Global.StaticCacheSeconds > 0)
                        {
                            ctx.Context.SetResponseCache(Global.StaticCacheSeconds);
                        }
                    }
                }); //For the wwwroot folder

            // Include un-bundled js + css folders to serve the source files in dev environment
            if (Config.IsLocal())
            {
                app.UseStaticFiles(
                    new StaticFileOptions {
                        FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory()),
                        RequestPath = "",
                        OnPrepareResponse = ctx => {
                            //Caching static files is required to reduce connections since the default behavior of checking if a static file has changed and returning a 304 still requires a connection.
                            if (Global.StaticCacheSeconds > 0)
                            {
                                ctx.Context.SetResponseCache(Global.StaticCacheSeconds);
                            }
                        }
                    });
            }

            app.UseStaticHttpContext(); //Temporary fix for old static HttpContext 
            app.UseMaintenancePageMiddleware(Global.MaintenanceMode); //Redirect to maintenance page when Maintenance mode settings = true
            app.UseStickySessionMiddleware(Global.StickySessions); //Enable/Disable sticky sessions based on  
            app.UseSecurityHeaderMiddleware(); //Add/remove security headers from all responses
            app.UseMvcWithDefaultRoute();

            lifetime.ApplicationStarted.Register(
                () => {
                    // Summary:
                    //     Triggered when the application host has fully started and is about to wait for
                    //     a graceful shutdown.
                    logger.LogInformation("Application Started");
                });
            lifetime.ApplicationStopping.Register(
                () => {
                    // Summary:
                    //     Triggered when the application host is performing a graceful shutdown. Requests
                    //     may still be in flight. Shutdown will block until this event completes.
                    logger.LogInformation("Application Stopping");
                });
        }

        private X509Certificate2 LoadCertificate()
        {
            //Load the site certificate
            string certThumprint = Config.Configuration["WEBSITE_LOAD_CERTIFICATES"].SplitI(";").FirstOrDefault();
            if (string.IsNullOrWhiteSpace(certThumprint))
            {
                certThumprint = Config.Configuration["CertThumprint"].SplitI(";").FirstOrDefault();
            }

            X509Certificate2 cert = null;
            if (!string.IsNullOrWhiteSpace(certThumprint))
            {
                cert = HttpsCertificate.LoadCertificateFromThumbprint(certThumprint);
                logger.LogInformation(
                    $"Successfully loaded certificate '{cert.FriendlyName}' expiring '{cert.GetExpirationDateString()}' from thumbprint '{certThumprint}'");
            }
            else
            {
                string certPath = Path.Combine(Directory.GetCurrentDirectory(), @"LocalHost.pfx");
                cert = HttpsCertificate.LoadCertificateFromFile(certPath, "LocalHost");
                logger.LogInformation(
                    $"Successfully loaded certificate '{cert.FriendlyName}' expiring '{cert.GetExpirationDateString()}' from file '{certPath}'");
            }

            if (Global.CertExpiresWarningDays > 0)
            {
                DateTime expires = cert.GetExpirationDateString().ToDateTime();
                if (expires < VirtualDateTime.UtcNow)
                {
                    logger.LogError(
                        $"The website certificate for '{Global.ExternalHost}' expired on {expires.ToFriendlyDate()} and needs replacing immediately.");
                }
                else
                {
                    TimeSpan remainingTime = expires - VirtualDateTime.Now;

                    if (expires < VirtualDateTime.UtcNow.AddDays(Global.CertExpiresWarningDays))
                    {
                        logger.LogWarning(
                            $"The website certificate for '{SiteAuthority}' is due expire on {expires.ToFriendlyDate()} and will need replacing within {remainingTime.ToFriendly(maxParts: 2)}.");
                    }
                }
            }

            return cert;
        }

    }
}
