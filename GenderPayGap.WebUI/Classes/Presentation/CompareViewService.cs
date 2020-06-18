using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models.Cookies;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.AspNetCore.Http;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;

namespace GenderPayGap.WebUI.Classes.Presentation
{

    public interface ICompareViewService
    {

        Lazy<SessionList<string>> ComparedEmployers { get; }

        int MaxCompareBasketShareCount { get; }

        int MaxCompareBasketCount { get; }

        string LastComparedEmployerList { get; set; }

        string SortColumn { get; set; }

        bool SortAscending { get; set; }

        int BasketItemCount { get; }

        void AddToBasket(string encEmployerId);

        void AddRangeToBasket(string[] encEmployerIds);

        void RemoveFromBasket(string encEmployerId);

        void ClearBasket();

        void LoadComparedEmployersFromCookie();

        void SaveComparedEmployersToCookie(HttpRequest request);

        bool BasketContains(params string[] encEmployerIds);

    }

    public class CompareViewService : ICompareViewService
    {

        public CompareViewService(IHttpContextAccessor httpContext, IHttpSession session)
        {
            HttpContext = httpContext.HttpContext;
            Session = session;
            ComparedEmployers = new Lazy<SessionList<string>>(CreateCompareSessionList(Session));
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
            IList<string> employerIds = ComparedEmployers.Value.ToList();

            CookieSettings cookieSettings = CookieHelper.GetCookieSettingsCookie(request);
            if (cookieSettings.RememberSettings)
            {
                //Save into the cookie
                HttpContext.SetResponseCookie(
                    CookieNames.LastCompareQuery,
                    employerIds.ToDelimitedString(),
                    VirtualDateTime.Now.AddMonths(1),
                    secure: true);
            }
        }

        private SessionList<string> CreateCompareSessionList(IHttpSession session)
        {
            return new SessionList<string>(
                session,
                nameof(CompareViewService),
                nameof(ComparedEmployers));
        }

        #region Dependencies

        public HttpContext HttpContext { get; }

        public IHttpSession Session { get; }

        #endregion

        #region Properties

        public Lazy<SessionList<string>> ComparedEmployers { get; }

        public string LastComparedEmployerList
        {
            get => Session["LastComparedEmployerList"].ToStringOrNull();
            set => Session["LastComparedEmployerList"] = value;
        }

        public string SortColumn
        {
            get => Session["SortColumn"].ToStringOrNull();
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Session.Remove("SortColumn");
                }
                else
                {
                    Session["SortColumn"] = value;
                }
            }
        }

        public bool SortAscending
        {
            get => Session["SortAscending"].ToBoolean(true);
            set => Session["SortAscending"] = value;
        }

        public int BasketItemCount => ComparedEmployers.Value.Count;

        public int MaxCompareBasketCount => Global.MaxCompareBasketCount;

        public int MaxCompareBasketShareCount => Global.MaxCompareBasketShareCount;

        #endregion

        #region Basket Methods

        public void AddToBasket(string encEmployerId)
        {
            int newBasketCount = ComparedEmployers.Value.Count + 1;
            if (newBasketCount <= MaxCompareBasketCount)
            {
                ComparedEmployers.Value.Add(encEmployerId);
            }
        }

        public void AddRangeToBasket(string[] encEmployerIds)
        {
            int newBasketCount = ComparedEmployers.Value.Count + encEmployerIds.Length;
            if (newBasketCount <= MaxCompareBasketCount)
            {
                ComparedEmployers.Value.Add(encEmployerIds);
            }
        }

        public void RemoveFromBasket(string encEmployerId)
        {
            ComparedEmployers.Value.Remove(encEmployerId);
        }

        public void ClearBasket()
        {
            ComparedEmployers.Value.Clear();
        }

        public bool BasketContains(params string[] encEmployerIds)
        {
            return ComparedEmployers.Value.Contains(encEmployerIds);
        }

        #endregion

    }

}
