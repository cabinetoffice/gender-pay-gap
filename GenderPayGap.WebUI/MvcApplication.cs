using System.Threading.Tasks;
using Autofac;
using Autofac.Features.AttributeFilters;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebUI
{
    public interface IMvcApplication
    {

        double SessionTimeOutMinutes { get; }

        ILogger Logger { get; }
        IQueue SendNotifyEmailQueue { get; set; }
        IQueue ExecuteWebjobQueue { get; }

        Task InitAsync();

    }

    public class MvcApplication : IMvcApplication
    {

        public static IContainer ContainerIoC;

        public MvcApplication(
            ILogger<MvcApplication> logger,
            IFileRepository fileRepository,
            ISearchRepository<EmployerSearchModel> searchRepository,
            [KeyFilter(QueueNames.SendNotifyEmail)]
            IQueue sendNotifyEmailQueue,
            [KeyFilter(QueueNames.ExecuteWebJob)] IQueue executeWebjobQueue
        )
        {
            Logger = logger;

            Global.FileRepository = fileRepository;
            Global.SearchRepository = searchRepository;

            SendNotifyEmailQueue = sendNotifyEmailQueue;
            ExecuteWebjobQueue = executeWebjobQueue;
        }

        public ILogger Logger { get; }
        public IQueue SendNotifyEmailQueue { get; set; }
        public IQueue ExecuteWebjobQueue { get; }

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
