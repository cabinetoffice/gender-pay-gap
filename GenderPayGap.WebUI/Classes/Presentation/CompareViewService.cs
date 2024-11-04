using System.Collections.Generic;
using System.Web;
using GenderPayGap.Core;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
using Microsoft.AspNetCore.Http;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;

namespace GenderPayGap.WebUI.Classes.Presentation
{

    public interface ICompareViewService
    {

        List<long> ComparedEmployers { get; }
        
        int BasketItemCount { get; }

        void AddToBasket(long organisationId);

        void RemoveFromBasket(long organisationId);

        void ClearBasket();

        void LoadComparedEmployersFromCookie();

        void SaveComparedEmployersToCookie();
        void SaveComparedEmployersToCookieIfAnyAreObfuscated();

        bool BasketContains(long organisationId);

    }

    public class CompareViewService : ICompareViewService
    {

        private readonly HttpContext httpContext;
        private bool areAnyIdsObfuscated = false;

        public List<long> ComparedEmployers { get; } = new List<long>();

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
            foreach (string employerId in employerIds)
            {
                if (long.TryParse(employerId, out long parsedOrganisationId))
                {
                    ComparedEmployers.Add(parsedOrganisationId);
                }
                else
                {
                    areAnyIdsObfuscated = true;
                    ComparedEmployers.Add(Obfuscator.DeObfuscate(employerId));
                }
            }
        }

        public void SaveComparedEmployersToCookieIfAnyAreObfuscated()
        {
            if (areAnyIdsObfuscated)
            {
                SaveComparedEmployersToCookie();
            }
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
                if (!ComparedEmployers.Contains(organisationId))
                {
                    ComparedEmployers.Add(organisationId);
                }
            }
        }

        public void RemoveFromBasket(long organisationId)
        {
            ComparedEmployers.Remove(organisationId);
        }

        public void ClearBasket()
        {
            ComparedEmployers.Clear();
        }

        public bool BasketContains(long organisationId)
        {
            return ComparedEmployers.Contains(organisationId);
        }

    }

}
