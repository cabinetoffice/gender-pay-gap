using Microsoft.AspNetCore.HttpOverrides;

namespace HoldingPage
{
    public class Program
    {

        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            
            ConfigureServices(builder.Services);

            WebApplication app = builder.Build();
            
            ConfigureApp(app);

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            
            services.AddControllersWithViews()
                .AddControllersAsServices() // Add controllers as services so attribute filters be resolved in contructors.
                .AddJsonOptions(options =>
                {
                    // By default, ASP.Net's JSON serialiser converts property names to camelCase (because javascript typically uses camelCase)
                    // But, some of our javascript code uses PascalCase (e.g. the homepage auto-complete)
                    // These options tell ASP.Net to use the original C# property names, without changing the case
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                });

            // Configure forwarded headers - this is so that the anti-forgery middleware (see below) is allowed to set a "Secure only" cookie
            services.Configure<ForwardedHeadersOptions>(
                options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                });
        }

        private static void ConfigureApp(WebApplication app)
        {
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PORT")))
            {
                app.Urls.Add($"http://*:{Environment.GetEnvironmentVariable("PORT")}/");
            }

            app.UseForwardedHeaders();
            
            app.UseStaticFiles();

            app.UseExceptionHandler("/error/500");
            app.UseStatusCodePagesWithReExecute("/error/{0}");
            
            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
        }

    }
}
