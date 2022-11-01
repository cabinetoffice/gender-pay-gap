using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace GenderPayGap.WebUI
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, IApplicationLifetime lifetime)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("This site is under maintenance");
            });
        }

    }
}
