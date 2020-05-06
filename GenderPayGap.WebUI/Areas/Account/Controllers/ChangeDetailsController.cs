using System.Threading.Tasks;
using AutoMapper;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Areas.Account.Abstractions;
using GenderPayGap.WebUI.Areas.Account.Resources;
using GenderPayGap.WebUI.Areas.Account.ViewModels;
using GenderPayGap.WebUI.Classes;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Areas.Account.Controllers
{

    [Area("Account")]
    [Route("manage-account")]
    public class ChangeDetailsController : BaseController
    {

        public ChangeDetailsController(
            IChangeDetailsViewService changeDetailsService,
            IHttpCache cache,
            IHttpSession session,
            IDataRepository dataRepo,
            IWebTracker webTracker) :
            base(cache, session, dataRepo, webTracker)
        {
            ChangeDetailsService = changeDetailsService;
        }

        public IChangeDetailsViewService ChangeDetailsService { get; }

        [HttpGet("change-details")]
        public IActionResult ChangeDetails()
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

            // map the user to the edit view model
            var model = Mapper.Map<ChangeDetailsViewModel>(currentUser);

            return View(model);
        }

        [HttpPost("change-details")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeDetails([FromForm] ChangeDetailsViewModel formData)
        {
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            // Validate fields
            if (ModelState.IsValid == false)
            {
                return View(nameof(ChangeDetails), formData);
            }

            // Execute change details
            bool success = ChangeDetailsService.ChangeDetails(formData, currentUser);

            // set success alert flag
            if (success)
            {
                TempData.Add(nameof(AccountResources.ChangeDetailsSuccessAlert), true);
            }

            // go to manage account page
            return this.RedirectToAction<ManageAccountController>(nameof(ManageAccountController.ManageAccount));
        }

    }

}
