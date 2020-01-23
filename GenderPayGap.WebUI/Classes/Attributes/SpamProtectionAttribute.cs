using System;
using System.Web;
using GenderPayGap.Core;
using GenderPayGap.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GenderPayGap.WebUI.Classes
{
    public class SpamProtectionAttribute : ActionFilterAttribute
    {

        private readonly int _minimumSeconds;

        public SpamProtectionAttribute(int minimumSeconds = 10)
        {
            _minimumSeconds = minimumSeconds;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //If the model state isnt valid then return
            if (!filterContext.ModelState.IsValid)
            {
                return;
            }

            DateTime remoteTime = DateTime.MinValue;

            try
            {
                remoteTime = Encryption.DecryptData(filterContext.HttpContext.GetParams("SpamProtectionTimeStamp")).FromSmallDateTime(true);
                if (remoteTime.AddSeconds(_minimumSeconds) < VirtualDateTime.Now)
                {
                    return;
                }
            }
            catch { }

            if (Global.SkipSpamProtection)
            {
                return;
            }

            throw new HttpException(429, "Too Many Requests");
        }

    }
}
