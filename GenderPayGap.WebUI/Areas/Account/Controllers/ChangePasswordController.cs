using System.Threading.Tasks;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Areas.Account.Abstractions;
using GenderPayGap.WebUI.Areas.Account.Resources;
using GenderPayGap.WebUI.Areas.Account.ViewModels;
using GenderPayGap.WebUI.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Areas.Account.Controllers
{

    [Area("Account")]
    [Route("manage-account")]
    public class ChangePasswordController : BaseController
    {

        public ChangePasswordController(
            IChangePasswordViewService changePasswordService,
            IHttpCache cache,
            IHttpSession session,
            IDataRepository dataRepo,
            IWebTracker webTracker) :
            base(cache, session, dataRepo, webTracker)
        {
            ChangePasswordService = changePasswordService;
        }

        public IChangePasswordViewService ChangePasswordService { get; }

        [HttpGet("change-password")]
        public IActionResult ChangePassword()
        {
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            // prevent impersonation
            if (IsImpersonatingUser)
            {
                this.RedirectToAction<ManageAccountController>(nameof(ManageAccountController.ManageAccount));
            }

            return View(new ChangePasswordViewModel());
        }

        [HttpPost("change-password")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordViewModel formData)
        {
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            // return to page if there are errors
            if (ModelState.IsValid == false)
            {
                return View(nameof(ChangePassword), formData);
            }

            // execute change password process
            ModelStateDictionary errors = await ChangePasswordService.ChangePasswordAsync(
                currentUser,
                formData.CurrentPassword,
                formData.NewPassword);

            if (errors.ErrorCount > 0)
            {
                ModelState.Merge(errors);
                return View(nameof(ChangePassword), formData);
            }

            // set success alert flag
            TempData.Add(nameof(AccountResources.ChangePasswordSuccessAlert), true);

            // go to manage account page
            return this.RedirectToAction<ManageAccountController>(nameof(ManageAccountController.ManageAccount));
        }

    }

}
