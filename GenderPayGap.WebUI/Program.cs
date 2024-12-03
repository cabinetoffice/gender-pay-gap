using System.Diagnostics;
using System.Globalization;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.BackgroundJobs;
using GenderPayGap.WebUI.BackgroundJobs.HangfireConfiguration;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.ExternalServices;
using GenderPayGap.WebUI.ExternalServices.CompaniesHouse;
using GenderPayGap.WebUI.ExternalServices.FileRepositories;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Repositories;
using GenderPayGap.WebUI.Search;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Serilog;

namespace GenderPayGap.WebUI
{
    public class Program
    {

        public static IServiceProvider DependencyInjectionServiceProvider;

        public static void Main(string[] args)
        {
            InitialiseSentry();
            
            // Add a handler for unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
            SetupSerilogLogger();

            SetThreadCultureToParseUkDates();
            
            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            
            ConfigureServices(builder.Services);
            SetupDependencyInjection(builder);

            WebApplication app = builder.Build();
            
            ConfigureApp(app);

            DependencyInjectionServiceProvider = app.Services;
            
            app.Run();

            // // Create the web host
            // IWebHost host = BuildWebHost(args);
            //
            // // Run the webhost
            // host.Run();
        }

        private static void InitialiseSentry()
        {
            if (Global.LogToSentry)
            {
                SentrySdk.Init(options =>
                {
                    options.Dsn = "https://7aa7b734d6b24c549432c396a5c42465@o4504651047501824.ingest.us.sentry.io/4504672991641600";
                    options.Environment = Config.EnvironmentName;
                    options.Release = BuildNumberHelper.GetBuildNumber();
                });
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            Console.WriteLine($"UNHANDLED EXCEPTION ({Console.Title}): {ex.Message}{Environment.NewLine}{ExceptionDetailsHelper.GetDetailsText(ex)}");
            Debug.WriteLine($"UNHANDLED EXCEPTION ({Console.Title}): {ex.Message}{Environment.NewLine}{ExceptionDetailsHelper.GetDetailsText(ex)}");

            throw ex;
        }

        private static void SetupSerilogLogger()
        {
           
            // Log to Console
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
           
            Log.Information("Serilog logger setup complete");
        }

        private static void SetThreadCultureToParseUkDates()
        {
            // Culture is required so UK dates can be parsed correctly
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            
            //Add a dedicated httpclient for Google Analytics tracking with exponential retry policy
            services.AddHttpClient<GoogleAnalyticsTracker, GoogleAnalyticsTracker>(nameof(GoogleAnalyticsTracker), GoogleAnalyticsTracker.SetupHttpClient)
                .SetHandlerLifetime(TimeSpan.FromMinutes(10))
                .AddPolicyHandler(GoogleAnalyticsTracker.GetRetryPolicy());

            //Add a dedicated httpclient for Companies house API with exponential retry policy
            services.AddHttpClient<CompaniesHouseAPI, CompaniesHouseAPI>(nameof(CompaniesHouseAPI), CompaniesHouseAPI.SetupHttpClient)
                .SetHandlerLifetime(TimeSpan.FromMinutes(10))
                .AddPolicyHandler(CompaniesHouseAPI.GetRetryPolicy());
            
            services.AddControllersWithViews(
                    options =>
                    {
                        AddStringTrimmingProvider(options); //Add modelstate binder to trim input 
                        options.ModelMetadataDetailsProviders.Add(
                            new TrimModelBinder()); //Set DisplayMetadata to input empty strings as null
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
                });
            
            // Add anti-forgery token by default to forms making sure the Secure flag is always set
            services.AddAntiforgery(
                options =>
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                });

            DataProtectionKeysHelper.AddDataProtectionKeyStorage(services);
            
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = new PathString("/login");
                    options.LogoutPath = new PathString("/logout");
                    options.AccessDeniedPath = new PathString("/error/403");
                });
            
            HangfireConfigurationHelper.ConfigureServices(services);
        }

        private static void SetupDependencyInjection(WebApplicationBuilder builder)
        {
            // The big ones
            // - Database
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            builder.Services.AddScoped<IDataRepository>(_ =>
            {
                var gpgDatabaseContext = new GpgDatabaseContext(Global.DatabaseConnectionString, true);
                return new SqlRepository(gpgDatabaseContext);
            });
            
            // - File repository
            if (!Config.IsLocal())
            {
                builder.Services.AddSingleton<IFileRepository>(_ => new AwsFileRepository(
                    bucketName: Global.S3BucketName,
                    awsAccessKeyId: Global.S3BucketAwsAccessKeyId,
                    awsSecretAccessKey: Global.S3BucketAwsSecretAccessKey,
                    awsRegion: Global.S3BucketAwsRegion
                ));
            }
            else
            {
                const string localStorageRoot = @"..\..\..\..\Temp\";
                builder.Services.AddSingleton<IFileRepository>(_ => new SystemFileRepository(localStorageRoot));
            }
            
            // Background Jobs
            builder.Services.AddScoped<IBackgroundJobsApi, BackgroundJobsApi>();
            
            // External Services
            builder.Services.AddSingleton<CompaniesHouseAPI>(serviceProvider =>
            {
                IHttpClientFactory httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
                HttpClient httpClient = httpClientFactory.CreateClient(nameof(CompaniesHouseAPI));
                return new CompaniesHouseAPI(httpClient);
            });
            builder.Services.AddScoped<IGovNotifyAPI, GovNotifyAPI>();
            builder.Services.AddScoped<PostcodesIoApi>();
            
            // Helpers
            builder.Services.AddSingleton<GoogleAnalyticsTracker>(serviceProvider =>
            {
                IHttpClientFactory httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
                HttpClient httpClient = httpClientFactory.CreateClient(nameof(GoogleAnalyticsTracker));
                return new GoogleAnalyticsTracker(httpClient, Global.GoogleAnalyticsAccountId);
            });
            
            // Repositories
            builder.Services.AddScoped<RegistrationRepository>();
            builder.Services.AddScoped<UserRepository>();
            
            // Services
            builder.Services.AddScoped<AuditLogger>();
            builder.Services.AddScoped<ComparisonBasketService>();
            builder.Services.AddScoped<DraftReturnService>();
            builder.Services.AddScoped<EmailSendingService>();
            builder.Services.AddScoped<OrganisationService>();
            builder.Services.AddSingleton<PinInThePostService>();
            builder.Services.AddScoped<ReturnService>();
            builder.Services.AddScoped<UpdateFromCompaniesHouseService>();
            
            // Search
            builder.Services.AddScoped<AddOrganisationSearchService>();
            builder.Services.AddScoped<AdminSearchService>();
            builder.Services.AddScoped<AutoCompleteSearchService>();
            builder.Services.AddHostedService<SearchCacheUpdaterService>();
            builder.Services.AddScoped<ViewingSearchService>();
        }

        private static void AddStringTrimmingProvider(MvcOptions option)
        {
            IModelBinderProvider binderToFind = option.ModelBinderProviders.FirstOrDefault(x => x.GetType() == typeof(SimpleTypeModelBinderProvider));
            if (binderToFind != null)
            {
                int index = option.ModelBinderProviders.IndexOf(binderToFind);
                option.ModelBinderProviders.Insert(index, new TrimmingModelBinderProvider());
            }
        }
        
        private static void ConfigureApp(WebApplication app)
        {
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PORT")))
            {
                app.Urls.Add($"http://*:{Environment.GetEnvironmentVariable("PORT")}/");
            }
            
            app.UseStaticFiles();

            if (Config.IsLocal())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error/500");
                app.UseStatusCodePagesWithReExecute("/error/{0}");
            }
            
            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });
            
            app.UseMiddleware<MaintenancePageMiddleware>(Global.MaintenanceMode); // Redirect to maintenance page when Maintenance mode settings = true
            app.UseMiddleware<SecurityHeaderMiddleware>(); // Add/remove security headers from all responses
            app.UseMiddleware<NoHtmlCachingMiddleware>(); // Add "Cache-Control: no-store" header to all HTML responses
            
            if (!string.IsNullOrWhiteSpace(Global.BasicAuthUsername)
                && !string.IsNullOrWhiteSpace(Global.BasicAuthPassword))
            {
                // Add HTTP Basic Authentication in our non-production environments to make sure people don't accidentally stumble across the site.
                // The site will still also be secured by the usual login/cookie auth - this is just an extra layer to make the site not publicly accessible
                app.UseMiddleware<BasicAuthMiddleware>();
            }

            app.MapControllers();
            
            app.Lifetime.ApplicationStarted.Register(
                () =>
                {
                    // Summary:
                    //     Triggered when the application host has fully started and is about to wait for
                    //     a graceful shutdown.
                    CustomLogger.Information("Application Started");
                });
            app.Lifetime.ApplicationStopping.Register(
                () =>
                {
                    // Summary:
                    //     Triggered when the application host is performing a graceful shutdown. Requests
                    //     may still be in flight. Shutdown will block until this event completes.
                    CustomLogger.Information("Application Stopping");
                });
            
            HangfireConfigurationHelper.ConfigureApp(app);
        }

    }
}
