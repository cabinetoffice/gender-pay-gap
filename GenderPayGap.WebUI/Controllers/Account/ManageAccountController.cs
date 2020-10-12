using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Account
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    public class ManageAccountController : Controller
    {

        private readonly IDataRepository dataRepository;

        public ManageAccountController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        // The user asks to close their account
        // We direct them to a page that asks for confirmation
        [HttpGet("manage-account-new")]
        public IActionResult ManageAccountGet()
        {
            // Admin impersonating a user shouldn't be able to view the manage account page
            if (LoginHelper.IsUserBeingImpersonated(User))
            {
                throw new PageNotFoundException();
            }
            
            // Get the current user
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            // Set properties on view model
            var viewModel = new ManageAccountViewModel
            {
                User = currentUser
            };

            // Return the Manage Account page
            return View("ManageAccountNew", viewModel);
        }

    }
}
