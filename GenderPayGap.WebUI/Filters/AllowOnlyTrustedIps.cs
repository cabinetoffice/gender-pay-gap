using System;
using System.Net;
using System.Web;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GenderPayGap.Core.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AllowOnlyTrustedIps : ActionFilterAttribute
    {

        public enum IpRangeTypes
        {

            None = 0,
            EhrcIPRange = 1,

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
                    _commaSeparatedListOfTrustedIps = Global.EhrcIPRange;
                    break;
                case IpRangeTypes.None:
                default:
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
                string controllerMessagePart = context.Controller == null || string.IsNullOrWhiteSpace(context.Controller.ToString())
                    ? "an unknown controller"
                    : $"controller {context.Controller}";

                string forbiddingReasonMessagePart = string.IsNullOrWhiteSpace(userHostAddress)
                    ? "since it was not possible to read its host address information"
                    : $"for address {userHostAddress} as it is not part of the configured ips {_ipRange}";

                CustomLogger.Warning($"Access to {controllerMessagePart} was forbidden {forbiddingReasonMessagePart}");
            }
            catch (Exception ex)
            {
                // Don't care if there was an error during logging
                // It's more important that the code continues
            }
        }

    }
}
