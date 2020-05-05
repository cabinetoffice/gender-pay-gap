using AutoMapper;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Areas.Account.Resources;
using GenderPayGap.WebUI.Areas.Account.ViewModels;
using GenderPayGap.WebUI.Classes;
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
            var model = Mapper.Map<ManageAccountViewModel>(currentUser);

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
            ViewBag.CloseAccountUrl = "";
            ViewBag.ChangeEmailUrl = "";
            ViewBag.ChangePasswordUrl = "";
            ViewBag.ChangeDetailsUrl = "";

            if (IsImpersonatingUser == false)
            {
                ViewBag.CloseAccountUrl = Url.Action<CloseAccountController>(nameof(CloseAccountController.CloseAccount));
                ViewBag.ChangeEmailUrl = Url.Action<ChangeEmailController>(nameof(ChangeEmailController.ChangeEmail));
                ViewBag.ChangePasswordUrl = Url.Action<ChangePasswordController>(nameof(ChangePasswordController.ChangePassword));
                ViewBag.ChangeDetailsUrl = Url.Action<ChangeDetailsController>(nameof(ChangeDetailsController.ChangeDetails));
            }

            // remove any change updates
            TempData.Remove(nameof(AccountResources.ChangeDetailsSuccessAlert));
            TempData.Remove(nameof(AccountResources.ChangePasswordSuccessAlert));

            return View(model);
        }

    }

}
