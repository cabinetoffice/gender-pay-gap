using System.Collections.Generic;
using System.Linq;
using System.Web;
using GenderPayGap.Core;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Cookies;
using Microsoft.AspNetCore.Http;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;

namespace GenderPayGap.WebUI.Classes.Presentation
{

    public interface ICompareViewService
    {

        List<string> ComparedEmployers { get; }
        
        int MaxCompareBasketShareCount { get; }

        int MaxCompareBasketCount { get; }

        int BasketItemCount { get; }

        void AddToBasket(string encEmployerId);

        void AddRangeToBasket(string[] encEmployerIds);

        void RemoveFromBasket(string encEmployerId);

        void ClearBasket();

        void LoadComparedEmployersFromCookie();

        void SaveComparedEmployersToCookie(HttpRequest request);

        bool BasketContains(string encEmployerId);

    }

    public class CompareViewService : ICompareViewService
    {

        public CompareViewService(IHttpContextAccessor httpContext)
        {
            HttpContext = httpContext.HttpContext;
            ComparedEmployers = new List<string>();
        }

        public void LoadComparedEmployersFromCookie()
        {
            string value = HttpContext.GetRequestCookieValue(CookieNames.LastCompareQuery);

            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            string[] employerIds = value.SplitI(",");

            ClearBasket();

            if (employerIds.Any())
            {
                AddRangeToBasket(employerIds);
            }
        }

        public void SaveComparedEmployersToCookie(HttpRequest request)
        {
            CookieSettings cookieSettings = CookieHelper.GetCookieSettingsCookie(request);
            if (cookieSettings.RememberSettings)
            {
                //Save into the cookie
                HttpContext.SetResponseCookie(
                    CookieNames.LastCompareQuery,
                    string.Join(',', ComparedEmployers),
                    VirtualDateTime.Now.AddMonths(1),
                    secure: true);
            }
        }

        #region Dependencies

        public HttpContext HttpContext { get; }

        #endregion

        #region Properties
        
        public List<string> ComparedEmployers { get; }

        public int BasketItemCount => ComparedEmployers.Count;

        public int MaxCompareBasketCount => Global.MaxCompareBasketCount;

        public int MaxCompareBasketShareCount => Global.MaxCompareBasketShareCount;

        #endregion

        #region Basket Methods

        public void AddToBasket(string encEmployerId)
        {
            int newBasketCount = ComparedEmployers.Count + 1;
            if (newBasketCount <= MaxCompareBasketCount)
            {
                ComparedEmployers.Add(encEmployerId);
            }
        }

        public void AddRangeToBasket(string[] encEmployerIds)
        {
            int newBasketCount = ComparedEmployers.Count + encEmployerIds.Length;
            if (newBasketCount <= MaxCompareBasketCount)
            {
                ComparedEmployers.AddRange(encEmployerIds);
            }
        }

        public void RemoveFromBasket(string encEmployerId)
        {
            ComparedEmployers.Remove(encEmployerId);
        }

        public void ClearBasket()
        {
            ComparedEmployers.Clear();
        }

        public bool BasketContains(string encEmployerId)
        {
            return ComparedEmployers.Contains(encEmployerId);
        }

        #endregion

    }

}
