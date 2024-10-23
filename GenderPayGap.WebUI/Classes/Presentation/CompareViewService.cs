using System.Collections.Generic;
using System.Web;
using GenderPayGap.Core;
using GenderPayGap.Extensions;
using Microsoft.AspNetCore.Http;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;

namespace GenderPayGap.WebUI.Classes.Presentation
{

    public interface ICompareViewService
    {

        List<string> ComparedEmployers { get; }
        
        int BasketItemCount { get; }

        void AddToBasket(string encEmployerId);

        void RemoveFromBasket(string encEmployerId);

        void ClearBasket();

        void LoadComparedEmployersFromCookie();

        void SaveComparedEmployersToCookie();

        bool BasketContains(string encEmployerId);

    }

    public class CompareViewService : ICompareViewService
    {

        private readonly HttpContext httpContext;

        public List<string> ComparedEmployers { get; } = new List<string>();

        public int BasketItemCount => ComparedEmployers.Count;

        public int MaxCompareBasketCount => Global.MaxCompareBasketCount;

        public CompareViewService(IHttpContextAccessor httpContext)
        {
            this.httpContext = httpContext.HttpContext;
        }

        public void LoadComparedEmployersFromCookie()
        {
            string value = httpContext.GetRequestCookieValue(CookieNames.LastCompareQuery);

            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            string[] employerIds = value.SplitI(",");

            ComparedEmployers.Clear();
            ComparedEmployers.AddRange(employerIds);
        }

        public void SaveComparedEmployersToCookie()
        {
            //Save into the cookie
            httpContext.SetResponseCookie(
                CookieNames.LastCompareQuery,
                string.Join(',', ComparedEmployers),
                VirtualDateTime.Now.AddMonths(1),
                secure: true);
        }

        public void AddToBasket(string encEmployerId)
        {
            int newBasketCount = ComparedEmployers.Count + 1;
            if (newBasketCount <= MaxCompareBasketCount)
            {
                ComparedEmployers.Add(encEmployerId);
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

    }

}
