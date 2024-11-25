using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
using Microsoft.AspNetCore.Http;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;

namespace GenderPayGap.WebUI.Classes.Presentation
{

    public class CompareViewService
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
            if (!httpContext.Request.Cookies.TryGetValue(CookieNames.LastCompareQuery, out string cookieValue))
            {
                cookieValue = "";
            }

            if (!string.IsNullOrWhiteSpace(cookieValue))
            {
                string[] employerIds = cookieValue.Split(",");

                ComparedEmployers.Clear();
                foreach (string employerId in employerIds)
                {
                    if (string.IsNullOrWhiteSpace(employerId))
                    {
                        // An empty string is not a valid organisation ID, despite what the Obfuscator might tell you!
                    }
                    else if (long.TryParse(employerId, out long parsedOrganisationId))
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
            HttpContextHelper.SetResponseCookie(
                httpContext,
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
