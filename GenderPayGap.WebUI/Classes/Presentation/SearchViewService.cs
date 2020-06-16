using System.Collections.Specialized;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models.Search;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Classes.Presentation
{

    public interface ISearchViewService
    {

        SearchViewModel LastSearchResults { get; set; }

        string LastSearchParameters { get; }

        string GetLastSearchUrl();

    }

    public class SearchViewService : ISearchViewService
    {

        public SearchViewService(IHttpSession session, IUrlHelper urlHelper)
        {
            Session = session;
            UrlHelper = urlHelper;
        }

        /// <summary>
        ///     Returns the relative Url of the last search including querystring
        /// </summary>
        public string GetLastSearchUrl()
        {
            NameValueCollection routeValues = LastSearchParameters?.FromQueryString();
            string actionUrl = UrlHelper.Action(nameof(ViewingController.SearchResults), "Viewing");
            string routeQuery = routeValues?.ToQueryString(true);
            return $"{actionUrl}?{routeQuery}";
        }

        #region Dependencies

        public IHttpSession Session { get; }

        public IUrlHelper UrlHelper { get; }

        #endregion

        #region Properties

        public SearchViewModel LastSearchResults { get; set; }

        /// <summary>
        ///     last search querystring
        /// </summary>
        public string LastSearchParameters
        {
            get => Session["LastSearchParameters"] as string;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Session.Remove("LastSearchParameters");
                }
                else
                {
                    Session["LastSearchParameters"] = value;
                }
            }
        }

        #endregion

    }


}
