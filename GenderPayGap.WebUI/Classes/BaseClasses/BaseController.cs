using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace GenderPayGap.WebUI.Classes
{
    public class BaseController : Controller
    {

        #region Constructors

        public BaseController(
            IHttpCache cache,
            IHttpSession session,
            IDataRepository dataRepository,
            IWebTracker webTracker)
        {
            DataRepository = dataRepository;
            WebTracker = webTracker;
            Cache = cache;
            Session = session;
        }

        #endregion

        public string EmployerBackUrl
        {
            get => Session["EmployerBackUrl"] as string;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Session.Remove("EmployerBackUrl");
                }
                else
                {
                    Session["EmployerBackUrl"] = value;
                }
            }
        }

        public string ReportBackUrl
        {
            get => Session["ReportBackUrl"] as string;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Session.Remove("ReportBackUrl");
                }
                else
                {
                    Session["ReportBackUrl"] = value;
                }
            }
        }

        private void SaveHistory()
        {
            List<string> history = PageHistory;
            try
            {
                string previousPage = UrlReferrer == null || !RequestUrl.Host.Equals(UrlReferrer.Host) ? null : UrlReferrer.PathAndQuery;
                string currentPage = RequestUrl.PathAndQuery;

                int currentIndex = history.IndexOf(currentPage);
                int previousIndex = string.IsNullOrWhiteSpace(previousPage) ? -2 : history.IndexOf(previousPage);

                if (previousIndex == -2)
                {
                    history.Clear();
                    history.Insert(0, currentPage);
                    return;
                }

                if (currentIndex == -1 && previousIndex == 0)
                {
                    history.Insert(0, currentPage);
                    return;
                }

                if (currentIndex == -1)
                {
                    history.Clear();
                    if (previousIndex == -1)
                    {
                        history.Insert(0, previousPage);
                    }

                    history.Insert(0, currentPage);
                    return;
                }

                if (currentIndex == 0 && previousIndex == 1)
                {
                    return;
                }

                if (currentIndex > previousIndex)
                {
                    for (int i = currentIndex - 1; i >= 0; i--)
                    {
                        history.RemoveAt(i);
                    }
                }
            }
            finally
            {
                PageHistory = history;
            }
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            # region logic before action goes here

            //Pass the controller object into the ViewData 
            var controller = context.Controller as Controller;
            controller.ViewData["Controller"] = controller;

            #endregion

            // the actual action
            await Session.LoadAsync();
            try
            {
                await base.OnActionExecutionAsync(context, next);
            }
            finally
            {
                //Ensure the session data is saved
                await Session.SaveAsync();
            }

            #region logic after the action goes here

            //Save the history and action/controller names
            SaveHistory();
            
            #endregion
        }

        /// <summary>
        ///     returns true if previous action
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        protected bool WasAction(string actionName, string controllerName = null, object routeValues = null)
        {
            if (string.IsNullOrWhiteSpace(controllerName))
            {
                controllerName = ControllerName;
            }

            return !(UrlReferrer == null) && UrlReferrer.PathAndQuery.EqualsI(Url.Action(actionName, controllerName, routeValues));
        }

        protected bool WasAnyAction(params string[] actionUrls)
        {
            for (var i = 0; i < actionUrls.Length; i++)
            {
                string actionUrl = actionUrls[i].TrimI(@" /\");
                string actionName = actionUrl.AfterFirst("/");
                string controllerName = actionUrl.BeforeFirst("/", includeWhenNoSeparator: false);
                if (WasAction(actionName, controllerName))
                {
                    return true;
                }
            }

            return false;
        }

        protected bool IsAction(string actionName, string controllerName = null)
        {
            return actionName.EqualsI(ActionName) && (controllerName.EqualsI(ControllerName) || string.IsNullOrWhiteSpace(controllerName));
        }

        protected bool IsAnyAction(params string[] actionUrls)
        {
            for (var i = 0; i < actionUrls.Length; i++)
            {
                string actionUrl = actionUrls[i].TrimI(@" /\");
                string actionName = actionUrl.AfterFirst("/");
                string controllerName = actionUrl.BeforeFirst("/", includeWhenNoSeparator: false);
                if (IsAction(actionName, controllerName))
                {
                    return true;
                }
            }

            return false;
        }

        #region Dependencies

        public IDataRepository DataRepository { get; protected set; }
        public IWebTracker WebTracker { get; }
        
        public readonly IHttpCache Cache;

        public readonly IHttpSession Session;
        #endregion

        #region Properties

        public long ReportingOrganisationId
        {
            get => Session["ReportingOrganisationId"].ToInt64();
            set
            {
                _ReportingOrganisation = null;
                ReportingOrganisationStartYear = null;
                Session["ReportingOrganisationId"] = value;
            }
        }

        public int? ReportingOrganisationStartYear
        {
            get => Session["ReportingOrganisationReportStartYear"].ToInt32();
            set => Session["ReportingOrganisationReportStartYear"] = value;
        }

        public string PendingFasttrackCodes
        {
            get => (string) Session["PendingFasttrackCodes"];
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Session.Remove("PendingFasttrackCodes");
                }
                else
                {
                    Session["PendingFasttrackCodes"] = value;
                }
            }
        }

        private Organisation _ReportingOrganisation;

        public Organisation ReportingOrganisation
        {
            get
            {
                if (_ReportingOrganisation == null && ReportingOrganisationId > 0)
                {
                    _ReportingOrganisation = DataRepository.GetAll<Organisation>()
                        .FirstOrDefault(o => o.OrganisationId == ReportingOrganisationId);
                }

                return _ReportingOrganisation;
            }
            set
            {
                _ReportingOrganisation = value;
                ReportingOrganisationId = value == null ? 0 : value.OrganisationId;
            }
        }

        public virtual User CurrentUser =>
            User.Identity.IsAuthenticated
                ? DataRepository.Get<User>(LoginHelper.GetUserId(User))
                : null;

        public bool IsImpersonatingUser => LoginHelper.IsUserBeingImpersonated(User);

        #endregion

        #region Authorisation Methods

        protected User OriginalUser
        {
            get
            {
                if (LoginHelper.IsUserBeingImpersonated(User))
                {
                    long userId = LoginHelper.GetAdminImpersonatorUserId(User);
                    return DataRepository.Get<User>(userId);
                }

                return null;
            }
        }


        protected IActionResult CheckUserRegisteredOk(out User currentUser)
        {
            currentUser = null;

            //Ensure user is logged in submit or rest of registration
            if (!User.Identity.IsAuthenticated)
            {
                //Ask the user to login
                return new ChallengeResult();
            }

            //Always allow the viewing controller
            if (this is ViewingController)
            {
                return null;
            }

            //Ensure we get a valid user from the database
            currentUser = CurrentUser;
            if (currentUser == null)
            {
                throw new IdentityNotMappedException();
            }

            // When user status is retired
            if (currentUser.Status == UserStatuses.Retired)
            {
                return new ChallengeResult();
            }

            //When email not verified
            if (currentUser.EmailVerifiedDate.EqualsI(null, DateTime.MinValue))
            {
                //If email not sent
                if (currentUser.EmailVerifySendDate.EqualsI(null, DateTime.MinValue))
                {
                    
                    //Tell them to verify email
                    return View("CustomError", new ErrorViewModel(1100));
                }

                //If verification code has expired
                if (currentUser.EmailVerifySendDate.Value.AddDays(Global.EmailVerificationExpiryDays) < VirtualDateTime.Now)
                {
                    
                    //prompt user to click to request a new one
                    return View("CustomError", new ErrorViewModel(1101));
                }

                //If code min time hasnt elapsed 
                TimeSpan remainingTime = currentUser.EmailVerifySendDate.Value.AddHours(Global.EmailVerificationMinResendHours)
                                         - VirtualDateTime.Now;
                if (remainingTime > TimeSpan.Zero)
                {
                    
                    //tell them to wait
                    return View("CustomError", new ErrorViewModel(1102, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)}));
                }

                
                //Prompt user to request a new verification code
                return View("CustomError", new ErrorViewModel(1103));
            }

            //Ensure admins always routed to their home page
            if (currentUser.IsAdministrator())
            {
                if (IsAnyAction(
                    "Register/ReviewRequest",
                    "Register/ConfirmCancellation",
                    "Register/RequestAccepted",
                    "Register/RequestCancelled"))
                {
                    return null;
                }

                return RedirectToAction("AdminHomePage", "AdminHomepage");
                //return View("CustomError", new ErrorViewModel(1117));
            }

            //Ensure admin pages only available to administrators

            if (ControllerName.EqualsI("admin")
                || IsAnyAction(
                    "Register/ReviewRequest",
                    "Register/ConfirmCancellation",
                    "Register/RequestAccepted",
                    "Register/RequestCancelled"))
            {
                return new HttpForbiddenResult($"User {CurrentUser?.EmailAddress} is not an administrator");
            }

            //Allow all steps from email confirmed to organisation chosen
            if (IsAnyAction(
                "Register/OrganisationType",
                "Register/OrganisationSearch",
                "Register/ChooseOrganisation",
                "Register/AddOrganisation",
                "Register/SelectOrganisation",
                "Register/AddAddress",
                "Register/AddSector",
                "Register/AddContact",
                "Register/ConfirmOrganisation",
                "Register/RequestReceived",
                "Register/EnterFasttrackCodes"))
            {
                return null;
            }

            //Always allow users to manage their account
            if (IsAnyAction(
                "ManageAccount/ManageAccount",
                "ChangeEmail/ChangeEmail",
                "ChangeEmail/ChangeEmailPending",
                "ChangeEmail/VerifyChangeEmail",
                "ChangeEmail/ChangeEmailFailed",
                "ChangeEmail/CompleteChangeEmailAsync",
                "ChangeDetails/ChangeDetails",
                "ChangePassword/ChangePassword",
                "CloseAccount/CloseAccount"))
            {
                return null;
            }

            //Always allow user home or remove registration page 
            if (IsAnyAction(
                "Organisation/ManageOrganisations",
                "Organisation/RemoveOrganisation",
                "Organisation/RemoveOrganisationPost",
                "Organisation/ManageOrganisation",
                "Organisation/ChangeOrganisationScope",
                "Organisation/ActivateOrganisation",
                "Organisation/ReportForOrganisation",
                "Organisation/DeclareScope",
                "Organisation/ScopeDeclared"))
            {
                return null;
            }

            // if the user doesn't have a selected an organisation then go to the ManageOrgs page
            UserOrganisation userOrg = currentUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == ReportingOrganisationId);
            if (userOrg == null)
            {
                CustomLogger.Warning(
                    $"Cannot find UserOrganisation for user {currentUser.UserId} and organisation {ReportingOrganisationId}");

                return RedirectToAction("ManageOrganisationsGet", "ManageOrganisations");
            }

            if (userOrg.Organisation.SectorType == SectorTypes.Private)
            {
                if (userOrg.PINConfirmedDate.EqualsI(null, DateTime.MinValue))
                {
                    //If pin never sent then go to resend point
                    if (userOrg.PINSentDate.EqualsI(null, DateTime.MinValue))
                    {
                        if (IsAnyAction("Register/PINSent", "Register/RequestPIN"))
                        {
                            return null;
                        }
                        
                        if (FeatureFlagHelper.IsFeatureEnabled(FeatureFlag.PrivateManualRegistration) )
                        {
                            return RedirectToAction("ManageOrganisationsGet", "ManageOrganisations");
                        }

                        return RedirectToAction("PINSent", "Register");
                    }

                    //If PIN sent and expired then prompt to request a new pin
                    if (userOrg.PINSentDate.Value.AddDays(Global.PinInPostExpiryDays) < VirtualDateTime.Now)
                    {
                        if (IsAnyAction("Register/PINSent", "Register/RequestPIN"))
                        {
                            return null;
                        }

                        return View("CustomError", new ErrorViewModel(1106));
                    }

                    //If PIN resends are allowed and currently on PIN send page then allow it to continue
                    TimeSpan remainingTime = userOrg.PINSentDate.Value.AddDays(Global.PinInPostMinRepostDays) - VirtualDateTime.Now;
                    if (remainingTime <= TimeSpan.Zero && IsAnyAction("Register/PINSent", "Register/RequestPIN"))
                    {
                        return null;
                    }

                    //If PIN Not expired redirect to ActivateService where they can either enter the same pin or request a new one 
                    if (IsAnyAction("Register/RequestPIN"))
                    {
                        return View("CustomError", new ErrorViewModel(1120, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)}));
                    }

                    if (IsAnyAction("Register/ActivateService"))
                    {
                        return null;
                    }

                    return RedirectToAction("ActivateService", "Register");
                }
            }

            //Ensure user has completed the registration process
            //If user is fully registered then start submit process
            if (this is RegisterController)
            {
                if (IsAnyAction("Register/RequestReceived"))
                {
                    return null;
                }

                if (IsAnyAction("Register/ServiceActivated") && WasAnyAction("Register/ActivateService", "Register/ConfirmOrganisation"))
                {
                    return null;
                }

                return View("CustomError", new ErrorViewModel(1109));
            }

            //Ensure pending manual registrations always redirected back to home
            if (userOrg.PINConfirmedDate == null)
            {
                CustomLogger.Warning(
                    $"UserOrganisation for user {userOrg.UserId} and organisation {userOrg.OrganisationId} PIN is not confirmed");
                return RedirectToAction("ManageOrganisationsGet", "ManageOrganisations");
            }

            return null;
        }

        #endregion


        #region Public fields

        public string ActionName => ControllerContext.RouteData.Values["action"].ToString();

        public string ControllerName => ControllerContext.RouteData.Values["controller"].ToString();

        #endregion

        #region Exception handling methods

        [NonAction]
        public void AddModelError(int errorCode, string propertyName = null, object parameters = null)
        {
            ModelState.AddModelError(errorCode, propertyName, parameters);
        }

        protected ActionResult SessionExpiredView()
        {
            // create the session expired error model
            var errorModel = new ErrorViewModel(1134);

            // return the custom error view
            return View("CustomError", errorModel);
        }

        #endregion


        public string UserHostAddress => HttpContext.GetUserHostAddress();
        public Uri RequestUrl => HttpContext.GetUri();
        public Uri UrlReferrer => HttpContext.GetUrlReferrer();

        public List<string> PageHistory
        {
            get
            {
                string pageHistory = Session["PageHistory"]?.ToString();
                return string.IsNullOrWhiteSpace(pageHistory)
                    ? new List<string>()
                    : JsonConvert.DeserializeObject<List<string>>(pageHistory);
            }
            set
            {
                if (value == null || !value.Any())
                {
                    Session.Remove("PageHistory");
                }
                else
                {
                    Session["PageHistory"] = JsonConvert.SerializeObject(value);
                }
            }
        }

    }
}
