using System.Collections.Generic;
using System.Web;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Extensions;
using Microsoft.AspNetCore.Http;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;

namespace GenderPayGap.WebUI.Classes.Presentation
{

    public interface ICompareViewService
    {

        List<string> ComparedEmployers { get; }
        
        int BasketItemCount { get; }

        void AddToBasket(long organisationId);

        void RemoveFromBasket(long organisationId);

        void ClearBasket();

        void LoadComparedEmployersFromCookie();

        void SaveComparedEmployersToCookie();

        bool BasketContains(long organisationId);

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

        public void AddToBasket(long organisationId)
        {
            int newBasketCount = ComparedEmployers.Count + 1;
            if (newBasketCount <= MaxCompareBasketCount)
            {
                ComparedEmployers.Add(Obfuscator.Obfuscate(organisationId));
            }
        }

        public void RemoveFromBasket(long organisationId)
        {
            ComparedEmployers.Remove(Obfuscator.Obfuscate(organisationId));
        }

        public void ClearBasket()
        {
            ComparedEmployers.Clear();
        }

        public bool BasketContains(long organisationId)
        {
            return ComparedEmployers.Contains(Obfuscator.Obfuscate(organisationId));
        }

    }

}
