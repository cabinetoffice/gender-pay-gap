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
            IFileRepository fileRepository,
            ISearchRepository<EmployerSearchModel> searchRepository,
        )
        {
            Global.FileRepository = fileRepository;
            Global.SearchRepository = searchRepository;
        }

        public double SessionTimeOutMinutes => Config.GetAppSetting("SessionTimeOut").ToInt32(20);

        public async Task InitAsync()
        {
            //Copy AppData to remote file storage
            if (!Config.IsProduction())
            {
                await Core.Classes.Extensions.PushRemoteFileAsync(Global.FileRepository, Filenames.SicCodes, Global.DataPath);
                await Core.Classes.Extensions.PushRemoteFileAsync(Global.FileRepository, Filenames.SicSections, Global.DataPath);
            }
        }

    }
}
