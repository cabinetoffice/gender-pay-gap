using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Areas.Account.Resources;
using GenderPayGap.WebUI.Areas.Account.ViewModels;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Areas.Account.Controllers
{

    [Area("Account")]
    [Route("manage-account")]
    public class ManageAccountController : BaseController
    {

        public ManageAccountController(IHttpCache cache,
            IHttpSession session,
            IDataRepository dataRepo,
            IWebTracker webTracker) :
            base(cache, session, dataRepo, webTracker) { }

        [HttpGet]
        public IActionResult ManageAccount()
        {
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null && IsImpersonatingUser == false)
            {
                return checkResult;
            }

            // map the user to the view model
            var model = new ManageAccountViewModel
            {
                FirstName = currentUser.Firstname,
                LastName = currentUser.Lastname,
                JobTitle = currentUser.JobTitle,
                EmailAddress = currentUser.EmailAddress,
                ContactPhoneNumber = currentUser.ContactPhoneNumber,
                SendUpdates = currentUser.SendUpdates,
                AllowContact = currentUser.AllowContact,
                IsUserBeingImpersonated = LoginHelper.IsUserBeingImpersonated(User)
            };

            // check if we have any successful changes
            if (TempData.ContainsKey(nameof(AccountResources.ChangeDetailsSuccessAlert)))
            {
                ViewBag.ChangeSuccessMessage = AccountResources.ChangeDetailsSuccessAlert;
            }
            else if (TempData.ContainsKey(nameof(AccountResources.ChangePasswordSuccessAlert)))
            {
                ViewBag.ChangeSuccessMessage = AccountResources.ChangePasswordSuccessAlert;
            }

            // generate flow urls
            ViewBag.ChangeEmailUrl = "";
            ViewBag.ChangePasswordUrl = "";
            ViewBag.ChangeDetailsUrl = "";

            if (IsImpersonatingUser == false)
            {
                ViewBag.ChangeEmailUrl = Url.Action("ChangeEmailGet", "ChangeEmail");
            }

            // remove any change updates
            TempData.Remove(nameof(AccountResources.ChangeDetailsSuccessAlert));
            TempData.Remove(nameof(AccountResources.ChangePasswordSuccessAlert));

            return View(model);
        }

    }

}
