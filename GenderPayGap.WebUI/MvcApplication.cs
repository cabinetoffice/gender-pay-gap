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

        public async Task InitAsync()
        {

        }

    }
}
