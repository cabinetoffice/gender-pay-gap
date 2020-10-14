using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
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
        
        [HttpGet("manage-account")]
        public IActionResult ManageAccountGet()
        {
            // Get the current user
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            // Set properties on view model
            var viewModel = new ManageAccountViewModel
            {
                User = currentUser,
                IsUserBeingImpersonated = LoginHelper.IsUserBeingImpersonated(User)
            };

            // Return the Manage Account page
            return View("ManageAccount", viewModel);
        }

    }
}
