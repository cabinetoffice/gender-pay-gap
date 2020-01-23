using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace GenderPayGap
{
    public class ControllerExtension : Controller
    {

        public readonly IHttpCache Cache;


        public readonly IHttpSession Session;

        public ControllerExtension(IHttpCache cache, IHttpSession session)
        {
            Cache = cache;
            Session = session;
        }

        public string UserHostAddress => HttpContext.GetUserHostAddress();
        public Uri RequestUrl => HttpContext.GetUri();
        public Uri UrlReferrer => HttpContext.GetUrlReferrer();

        public List<string> PageHistory
        {
            get
            {
                string pageHistory = Session["PageHistory"]?.ToString();
                return string.IsNullOrWhiteSpace(pageHistory)
                    ? new List<string>()
                    : JsonConvert.DeserializeObject<List<string>>(pageHistory);
            }
            set
            {
                if (value == null || !value.Any())
                {
                    Session.Remove("PageHistory");
                }
                else
                {
                    Session["PageHistory"] = JsonConvert.SerializeObject(value);
                }
            }
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //Ensure the session is loaded
            await Session.LoadAsync();

            try
            {
                await base.OnActionExecutionAsync(context, next);
            }
            finally
            {
                //Ensure the session data is saved
                await Session.SaveAsync();
            }
        }

    }
}
