using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes.Attributes;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;

namespace GenderPayGap.WebUI.Classes
{
    public static class HtmlHelpers
    {

        public static async Task<IHtmlContent> PartialModelAsync<T>(this IHtmlHelper htmlHelper, T viewModel)
        {
            // extract the parial path from the model class attr
            string partialPath = viewModel.GetAttribute<PartialAttribute>().PartialPath;
            return await htmlHelper.PartialAsync(partialPath, viewModel);
        }

        public static string WithQuery(this IUrlHelper helper, string actionName, object routeValues)
        {
            var newRoute = new NameValueCollection();

            if (helper.ActionContext.HttpContext.Request.QueryString.HasValue)
            {
                var existingRoute = HttpUtility.ParseQueryString(helper.ActionContext.HttpContext.Request.QueryString.Value);
                newRoute.Add(existingRoute);
            }

            foreach (KeyValuePair<string, object> item in new RouteValueDictionary(routeValues))
            {
                newRoute[item.Key] = item.Value.ToString();
            }

            string querystring = null;
            var keys = new SortedSet<string>(newRoute.AllKeys);
            foreach (string key in keys)
            {
                foreach (string value in newRoute.GetValues(key))
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(querystring))
                    {
                        querystring += "&";
                    }

                    querystring += $"{key}={value}";
                }
            }

            return helper.Action(actionName) + "?" + querystring;
        }
        
        public static HtmlString UnpackBundle(this IHtmlHelper htmlHelper, string bundlePath, string media = "")
        {
            Bundle bundle = Bundle.ReadBundleFile("bundleconfig.json", bundlePath);
            if (bundle == null)
            {
                return null;
            }

            IEnumerable<string> outputString = bundlePath.EndsWith(".js")
                ? bundle.InputFiles.Select(inputFile => $"<script src='/{inputFile}' type='text/javascript'></script>")
                : bundle.InputFiles.Select(inputFile => $"<link rel='stylesheet' type='text/css' media='{media}' href='/{inputFile}' />");

            return new HtmlString(string.Join("\n", outputString));
        }

    }
}
