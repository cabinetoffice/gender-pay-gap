using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.Core.Interfaces
{
    public interface IWebTracker
    {

        Task TrackPageViewAsync(Controller controller, string pageTitle = null, string pageUrl = null);

    }
}
