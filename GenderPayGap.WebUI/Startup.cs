using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Features.AttributeFilters;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.BackgroundJobs;
using GenderPayGap.WebUI.BackgroundJobs.HangfireConfiguration;
using GenderPayGap.WebUI.BusinessLogic.Abstractions;
using GenderPayGap.WebUI.BusinessLogic.Services;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Classes.Presentation;
using GenderPayGap.WebUI.Classes.Services;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.ExternalServices;
using GenderPayGap.WebUI.ExternalServices.CompaniesHouse;
using GenderPayGap.WebUI.ExternalServices.FileRepositories;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Repositories;
using GenderPayGap.WebUI.Search;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using StackExchange.Redis;

namespace GenderPayGap.WebUI
{
    public class Startup
    {

        public static Action<IServiceCollection> ConfigureTestServices;
        public static Action<ContainerBuilder> ConfigureTestContainer;

        // ConfigureServices is where you register dependencies. This gets
        // called by the runtime before the ConfigureContainer method, below.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add services to the collection. Don't build or return 
            // any IServiceProvider or the ConfigureContainer method
            // won't get called.

            //Allow handler for caching of http responses
            services.AddResponseCaching();

            //Make sure the application uses the X-Forwarded-Proto header
            services.Configure<ForwardedHeadersOptions>(
                options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor;
                });

            //Add a dedicated httpclient for Google Analytics tracking with exponential retry policy
            services.AddHttpClient<IWebTracker, GoogleAnalyticsTracker>(nameof(IWebTracker), GoogleAnalyticsTracker.SetupHttpClient)
                .SetHandlerLifetime(TimeSpan.FromMinutes(10))
                .AddPolicyHandler(GoogleAnalyticsTracker.GetRetryPolicy());

            //Add a dedicated httpclient for Companies house API with exponential retry policy
            services.AddHttpClient<ICompaniesHouseAPI, CompaniesHouseAPI>(nameof(ICompaniesHouseAPI), CompaniesHouseAPI.SetupHttpClient)
                .SetHandlerLifetime(TimeSpan.FromMinutes(10))
                .AddPolicyHandler(CompaniesHouseAPI.GetRetryPolicy());

            //Allow creation of a static http context anywhere
            services.AddHttpContextAccessor();

            services.AddControllersWithViews(
                    options =>
                    {
                        options.AddStringTrimmingProvider(); //Add modelstate binder to trim input 
                        options.ModelMetadataDetailsProviders.Add(
                            new TrimModelBinder()); //Set DisplayMetadata to input empty strings as null
                        options.ModelMetadataDetailsProviders.Add(
                            new DefaultResourceValidationMetadataProvider()); // sets default resource type to use for display text and error messages
                        options.Filters.Add<ErrorHandlingFilter>();
                    })
                .AddControllersAsServices() // Add controllers as services so attribute filters be resolved in contructors.
                .AddJsonOptions(options =>
                {
                    // By default, ASP.Net's JSON serialiser converts property names to camelCase (because javascript typically uses camelCase)
                    // But, some of our javascript code uses PascalCase (e.g. the homepage auto-complete)
                    // These options tell ASP.Net to use the original C# property names, without changing the case
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                })
                .AddDataAnnotationsLocalization(
                    options => { options.DataAnnotationLocalizerProvider = DataAnnotationLocalizerProvider.DefaultResourceHandler; });

            IMvcBuilder mvcBuilder = services.AddRazorPages();

            if (Config.IsLocal())
            {
                mvcBuilder.AddRazorRuntimeCompilation();
            }

            //Add antiforgery token by default to forms making sure the Secure flag is always set
            services.AddAntiforgery(
                options =>
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                });

            //Add services needed for sessions
            services.AddSession(
                o =>
                {
                    o.Cookie.IsEssential = true; //This is required otherwise session will not load
                    o.Cookie.SecurePolicy = CookieSecurePolicy.Always; //Equivalent to <httpCookies requireSSL="true" /> from Web.Config
                    o.Cookie.HttpOnly = true; //Session cookie should not be accessible by client-side scripts
                    o.IdleTimeout = TimeSpan.FromDays(30); // This is how long the session DATA is kept, not how long the cookie lasts
                });

            //Add the distributed redis cache
            AddRedisCache(services);

            DataProtectionKeysHelper.AddDataProtectionKeyStorage(services);
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = new PathString("/login");
                    options.LogoutPath = new PathString("/logout");
                    options.AccessDeniedPath = new PathString("/error/403");
                    // ...
                });

            services.AddHostedService<SearchCacheUpdaterService>();

            HangfireConfigurationHelper.ConfigureServices(services);

            //Override any test services
            ConfigureTestServices?.Invoke(services);

            //Create Inversion of Control container
            Global.ContainerIoC = BuildContainerIoC(services);

            // Create the IServiceProvider based on the container.
            return new AutofacServiceProvider(Global.ContainerIoC);
        }

        private static void AddRedisCache(IServiceCollection services, string applicationDiscriminator = null)
        {
            //Add distributed cache service backed by Redis cache
            if (Debugger.IsAttached || Config.IsEnvironment("Local"))
            {
                //Use a memory cache
                services.AddDistributedMemoryCache();
            }
            else
            {
                if (Global.VcapServices != null && Global.VcapServices.Redis != null)
                {
                    VcapRedis redisConfiguration = Global.VcapServices.Redis.First(b => b.Name.EndsWith("-cache"));

                    services.AddStackExchangeRedisCache(
                        options =>
                        {
                            options.ConfigurationOptions = new ConfigurationOptions
                            {
                                EndPoints = { { redisConfiguration.Credentials.Host, redisConfiguration.Credentials.Port } },
                                AbortOnConnectFail = false
                            };
                        });
                }
                else
                {
                    throw new ArgumentNullException("VCAP_SERVICES", "Cannot find 'VCAP_SERVICES' config setting");
                }
            }
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

            //Register the configuration
            builder.RegisterInstance(Config.Configuration).SingleInstance();

            builder.Register(c => new SqlRepository(new GpgDatabaseContext(Global.DatabaseConnectionString, true)))
                .As<IDataRepository>()
                .InstancePerLifetimeScope();

            builder.RegisterType<CompaniesHouseAPI>()
                .As<ICompaniesHouseAPI>()
                .SingleInstance()
                .WithParameter(
                    (p, ctx) => p.ParameterType == typeof(HttpClient),
                    (p, ctx) => ctx.Resolve<IHttpClientFactory>().CreateClient(nameof(ICompaniesHouseAPI)));

            if (!Config.IsLocal())
            {
                VcapAwsS3Bucket fileStorageBucketConfiguration = Global.VcapServices.AwsS3Bucket.First(b => b.Name.EndsWith("-filestorage"));

                builder.Register(c => new AwsFileRepository(fileStorageBucketConfiguration))
                    .As<IFileRepository>()
                    .SingleInstance();
            }
            else
            {
                string localStorageRoot = @"..\..\..\..\Temp\";
                builder.Register(c => new SystemFileRepository(localStorageRoot)).As<IFileRepository>().SingleInstance();
            }

            // BL Services
            builder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();
            builder.RegisterType<RegistrationRepository>().As<RegistrationRepository>().InstancePerLifetimeScope();
            builder.RegisterType<OrganisationService>().As<OrganisationService>().InstancePerLifetimeScope();
            builder.RegisterType<ReturnService>().As<ReturnService>().InstancePerLifetimeScope();
            builder.RegisterType<DraftReturnService>().As<DraftReturnService>().InstancePerLifetimeScope();

            builder.RegisterType<ScopeBusinessLogic>().As<IScopeBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<SubmissionBusinessLogic>().As<ISubmissionBusinessLogic>().InstancePerLifetimeScope();
            builder.RegisterType<OrganisationBusinessLogic>().As<IOrganisationBusinessLogic>().InstancePerLifetimeScope();

            builder.RegisterType<UpdateFromCompaniesHouseService>().As<UpdateFromCompaniesHouseService>().InstancePerLifetimeScope();

            // register web ui services
            builder.RegisterType<DraftFileBusinessLogic>().As<IDraftFileBusinessLogic>().InstancePerLifetimeScope();
            
            builder.RegisterType<SubmissionService>().As<ISubmissionService>().InstancePerLifetimeScope();
            builder.RegisterType<ViewingService>().As<IViewingService>().InstancePerLifetimeScope();
            builder.RegisterType<ViewingSearchService>().As<ViewingSearchService>().InstancePerLifetimeScope();
            builder.RegisterType<SearchViewService>().As<ISearchViewService>().InstancePerLifetimeScope();
            builder.RegisterType<CompareViewService>().As<ICompareViewService>().InstancePerLifetimeScope();
            builder.RegisterType<AdminSearchService>().As<AdminSearchService>().InstancePerLifetimeScope();
            builder.RegisterType<AutoCompleteSearchService>().As<AutoCompleteSearchService>().InstancePerLifetimeScope();
            builder.RegisterType<AddOrganisationSearchService>().As<AddOrganisationSearchService>().InstancePerLifetimeScope();
            builder.RegisterType<AuditLogger>().As<AuditLogger>().InstancePerLifetimeScope();

            //Register some singletons
            builder.RegisterType<InternalObfuscator>().As<IObfuscator>().SingleInstance();
            builder.RegisterType<EncryptionHandler>().As<IEncryptionHandler>().SingleInstance();
            builder.RegisterType<PinInThePostService>().As<PinInThePostService>().SingleInstance();
            builder.RegisterType<GovNotifyAPI>().As<IGovNotifyAPI>().InstancePerLifetimeScope();
            builder.RegisterType<EmailSendingService>().As<EmailSendingService>().InstancePerLifetimeScope();
            builder.RegisterType<BackgroundJobsApi>().As<IBackgroundJobsApi>().InstancePerLifetimeScope();


            //Register HttpCache and HttpSession
            builder.RegisterType<HttpSession>().As<IHttpSession>().InstancePerLifetimeScope();
            builder.RegisterType<HttpCache>().As<IHttpCache>().SingleInstance();

            // Register Action helpers
            builder.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>().SingleInstance();
            builder.Register(
                x =>
                {
                    ActionContext actionContext = x.Resolve<IActionContextAccessor>().ActionContext;
                    var factory = x.Resolve<IUrlHelperFactory>();
                    return factory.GetUrlHelper(actionContext);
                });

            //Register WebTracker
            builder.RegisterType<GoogleAnalyticsTracker>()
                .As<IWebTracker>()
                .SingleInstance()
                .WithParameter(
                    (p, ctx) => p.ParameterType == typeof(HttpClient),
                    (p, ctx) => ctx.Resolve<IHttpClientFactory>().CreateClient(nameof(IWebTracker)))
                .WithParameter("trackingId", Global.GoogleAnalyticsAccountId);

            //Register all controllers - this is required to ensure KeyFilter is resolved in constructors
            builder.RegisterAssemblyTypes(typeof(BaseController).Assembly)
                .Where(t => t.IsAssignableTo<BaseController>())
                .InstancePerLifetimeScope()
                .WithAttributeFiltering();

            //TODO: Implement AutoFac modules
            //builder.RegisterModule(new AutofacModule());

            //Override any test services
            ConfigureTestContainer?.Invoke(builder);

            IContainer container = builder.Build();

            HangfireConfigurationHelper.ConfigureIOC(container);

            return container;
        }

        // Configure is where you add middleware. This is called after
        // ConfigureContainer. You can use IApplicationBuilder.ApplicationServices
        // here if you need to resolve things from the container.
        public void Configure(IApplicationBuilder app, IApplicationLifetime lifetime)
        {
            app.UseForwardedHeaders();
            app.UseMiddleware<ExceptionMiddleware>();
            if (Config.IsLocal())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error/500");
                app.UseStatusCodePagesWithReExecute("/error/{0}");
            }
            
            /*app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.All
            });*/
            //app.UseHttpsRedirection();
            //app.UseResponseCompression(); //Disabled to use IIS compression which has better performance (see https://docs.microsoft.com/en-us/aspnet/core/performance/response-compression?view=aspnetcore-2.1)
            app.UseStaticFiles(
                new StaticFileOptions
                {
                    OnPrepareResponse = ctx =>
                    {
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
                    new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory()),
                        RequestPath = "",
                        OnPrepareResponse = ctx =>
                        {
                            //Caching static files is required to reduce connections since the default behavior of checking if a static file has changed and returning a 304 still requires a connection.
                            if (Global.StaticCacheSeconds > 0)
                            {
                                ctx.Context.SetResponseCache(Global.StaticCacheSeconds);
                            }
                        }
                    });
            }

            app.UseRouting();
            app.UseResponseCaching();
            app.UseResponseBuffering(); //required otherwise JsonResult uses chunking and adds extra characters
            app.UseStaticHttpContext(); //Temporary fix for old static HttpContext 
            app.UseSession(); //Must be before UseMvC or any middleware which requires session
            app.UseAuthentication(); //Ensure the OIDC IDentity Server authentication services execute on each http request - Must be before UseMVC
            app.UseAuthorization();

            var cookiePolicyOptions = new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Lax,
            };
            app.UseCookiePolicy(cookiePolicyOptions);
            app.UseMaintenancePageMiddleware(Global.MaintenanceMode); //Redirect to maintenance page when Maintenance mode settings = true
            app.UseSecurityHeaderMiddleware(); //Add/remove security headers from all responses

            if (!string.IsNullOrWhiteSpace(Global.BasicAuthUsername)
                && !string.IsNullOrWhiteSpace(Global.BasicAuthPassword))
            {
                // Add HTTP Basic Authentication in our non-production environments to make sure people don't accidentally stumble across the site
                // The site will still also be secured by the usual login/cookie auth - this is just an extra layer to make the site not publicly accessible
                app.UseMiddleware<BasicAuthMiddleware>();
            }

            // Prevent caching of html responses as they may contain secure info - GPG-581
            app.UseMiddleware<NoHtmlCachingMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            HangfireConfigurationHelper.ConfigureApp(app);

            lifetime.ApplicationStarted.Register(
                () =>
                {
                    // Summary:
                    //     Triggered when the application host has fully started and is about to wait for
                    //     a graceful shutdown.
                    CustomLogger.Information("Application Started");
                });
            lifetime.ApplicationStopping.Register(
                () =>
                {
                    // Summary:
                    //     Triggered when the application host is performing a graceful shutdown. Requests
                    //     may still be in flight. Shutdown will block until this event completes.
                    CustomLogger.Information("Application Stopping");
                });
        }

    }
}
