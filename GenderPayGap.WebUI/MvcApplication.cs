using System.Threading.Tasks;
using Autofac;
using Autofac.Features.AttributeFilters;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;

namespace GenderPayGap.WebUI
{
    public interface IMvcApplication
    {

        double SessionTimeOutMinutes { get; }

        Task InitAsync();

    }

    public class MvcApplication : IMvcApplication
    {

        public static IContainer ContainerIoC;

        public MvcApplication(
            IFileRepository fileRepository
        )
        {
            Global.FileRepository = fileRepository;
        }

        public double SessionTimeOutMinutes => Config.GetAppSetting("SessionTimeOut").ToInt32(20);

        public async Task InitAsync()
        {

        }

    }
}
