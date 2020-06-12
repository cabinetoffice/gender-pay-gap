using System.Threading.Tasks;
using Autofac;

namespace GenderPayGap.WebUI
{
    public interface IMvcApplication
    {

        Task InitAsync();

    }

    public class MvcApplication : IMvcApplication
    {

        public static IContainer ContainerIoC;

        public async Task InitAsync()
        {

        }

    }
}
