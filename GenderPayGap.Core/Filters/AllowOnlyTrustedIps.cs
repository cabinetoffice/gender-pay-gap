using System;
using System.Net;
using System.Web;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.Core.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AllowOnlyTrustedIps : ActionFilterAttribute
    {

        public enum IpRangeTypes
        {

            None = 0,
            EhrcIPRange = 1,
            TrustedIPDomains = 2

        }

        private readonly string _commaSeparatedListOfTrustedIps;
        private readonly IpRangeTypes _ipRange;
        private readonly bool _useHtmlFormattedResponse;

        public AllowOnlyTrustedIps(IpRangeTypes ipRange = IpRangeTypes.None, bool useHtmlFormattedResponse = true)
        {
            _ipRange = ipRange;
            _useHtmlFormattedResponse = useHtmlFormattedResponse;

            switch (_ipRange)
            {
                case IpRangeTypes.EhrcIPRange:
                    _commaSeparatedListOfTrustedIps = Config.GetAppSetting("EhrcIPRange");
                    break;
                case IpRangeTypes.None:
                case IpRangeTypes.TrustedIPDomains:
                default:
                    _commaSeparatedListOfTrustedIps = Config.GetAppSetting("TrustedIPDomains");
                    break;
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (string.IsNullOrWhiteSpace(_commaSeparatedListOfTrustedIps))
            {
                return;
            }

            string userHostAddress = context.HttpContext.GetUserHostAddress();
            if (string.IsNullOrWhiteSpace(userHostAddress) || !userHostAddress.IsTrustedAddress(_commaSeparatedListOfTrustedIps.SplitI()))
            {
                LogAttempt(context, userHostAddress);
                context.Result = ReturnForbidden($"IP {userHostAddress} is not trusted");
            }

            base.OnActionExecuting(context);
        }

        private ActionResult ReturnForbidden(string errorMessage)
        {
            ActionResult result;

            if (_useHtmlFormattedResponse)
            {
                result = new HttpForbiddenResult(errorMessage);
            }
            else
            {
                result = new HttpStatusCodeResult(HttpStatusCode.Forbidden, errorMessage);
            }

            return result;
        }

        private void LogAttempt(ActionExecutingContext context, string userHostAddress)
        {
            try
            {
                var logger = context.HttpContext.RequestServices?.GetService<ILogger<AllowOnlyTrustedIps>>();
                if (logger == null)
                {
                    return;
                }

                string controllerMessagePart = context.Controller == null || string.IsNullOrWhiteSpace(context.Controller.ToString())
                    ? "an unknown controller"
                    : $"controller {context.Controller}";

                string forbiddingReasonMessagePart = string.IsNullOrWhiteSpace(userHostAddress)
                    ? "since it was not possible to read its host address information"
                    : $"for address {userHostAddress} as it is not part of the configured ips {_ipRange}";

                logger.LogWarning($"Access to {controllerMessagePart} was forbidden {forbiddingReasonMessagePart}");
            }
            catch (Exception ex)
            {
                // Don't care if there was an error during logging
                // It's more important that the code continues
            }
        }

    }
}
