using System;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GenderPayGap.IdentityServer4.Classes
{
    public static class HtmlHelpers
    {

        public static HtmlString PageIdentifier(this IHtmlHelper htmlHelper)
        {
            return new HtmlString(
                $"Date:{VirtualDateTime.Now}, Version:{Global.Version}, File Date:{Global.AssemblyDate.ToLocalTime()}, Environment:{Config.EnvironmentName}, Machine:{Environment.MachineName}, Instance:{Global.AzureInstanceId}, {Global.AssemblyCopyright}");
        }

        #region Validation messages

        public static async Task<IHtmlContent> CustomValidationSummaryAsync(this IHtmlHelper helper,
            bool excludePropertyErrors = true,
            string validationSummaryMessage = "The following errors were detected",
            object htmlAttributes = null)
        {
            helper.ViewBag.ValidationSummaryMessage = validationSummaryMessage;
            helper.ViewBag.ExcludePropertyErrors = excludePropertyErrors;

            return await helper.PartialAsync("_ValidationSummary");
        }

        #endregion

    }
}
