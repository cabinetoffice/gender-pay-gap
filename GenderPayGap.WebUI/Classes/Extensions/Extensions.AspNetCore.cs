using Autofac;
using GenderPayGap.Core;
using Microsoft.AspNetCore.Builder;

namespace GenderPayGap.WebUI.Classes
{
    public static partial class Extensions
    {

        public static IApplicationBuilder UseMvCApplication(this IApplicationBuilder app)
        {
            Program.MvcApplication = Global.ContainerIoC.Resolve<IMvcApplication>();

            return app;
        }

    }
}
