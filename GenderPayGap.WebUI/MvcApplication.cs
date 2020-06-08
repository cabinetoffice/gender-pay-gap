using System.Threading.Tasks;
using Autofac;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;

namespace GenderPayGap.WebUI
{
    public interface IMvcApplication
    {

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

        public async Task InitAsync()
        {

        }

    }
}
