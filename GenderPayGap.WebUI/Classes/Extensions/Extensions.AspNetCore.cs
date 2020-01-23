using Autofac;
using Microsoft.AspNetCore.Builder;

namespace GenderPayGap.WebUI.Classes
{
    public static partial class Extensions
    {

        public static IApplicationBuilder UseMvCApplication(this IApplicationBuilder app)
        {
            Program.MvcApplication = MvcApplication.ContainerIoC.Resolve<IMvcApplication>();

            return app;
        }

    }
}
