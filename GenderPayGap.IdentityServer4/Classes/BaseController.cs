using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.IdentityServer4.Classes
{
    public class BaseController : Controller
    {

        protected readonly IDistributedCache _cache;
        protected readonly IEventService _events;
        protected readonly ILogger _Logger;

        public BaseController(
            IEventService events,
            IDistributedCache cache,
            ILogger logger)
        {
            _events = events;
            _cache = cache;
            _Logger = logger;
        }

        public string ActionName => ControllerContext.RouteData.Values["action"].ToString();

        public string ControllerName => ControllerContext.RouteData.Values["controller"].ToString();

    }
}
