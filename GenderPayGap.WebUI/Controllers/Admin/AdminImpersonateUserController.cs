using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
    [Route("admin")]
    public class AdminImpersonateUserController : Controller
    {

        private readonly UserRepository userRepository;
        private readonly IDataRepository dataRepository;

        public AdminImpersonateUserController(UserRepository userRepository, IDataRepository dataRepository)
        {
            this.userRepository = userRepository;
            this.dataRepository = dataRepository;
        }

        [HttpGet("impersonate")]
        public IActionResult Impersonate(string emailAddress)
        {

            var viewModel = new AdminImpersonateUserViewModel {EmailAddress = emailAddress};

            return View("ImpersonateUser", viewModel);
        }

        [HttpPost("impersonate")]
        [ValidateAntiForgeryToken]
        public IActionResult ImpersonatePost(AdminImpersonateUserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("ImpersonateUser", viewModel);
            }

            // find the latest active user by email
            User impersonatedUser = userRepository.FindByEmail(viewModel.EmailAddress, UserStatuses.Active);
            if (impersonatedUser == null)
            {
                ModelState.AddModelError(nameof(viewModel.EmailAddress), "This user does not exist");
                return View("ImpersonateUser");
            }

            if (impersonatedUser.IsFullOrReadOnlyAdministrator())
            {
                ModelState.AddModelError(nameof(viewModel.EmailAddress), "Impersonating other administrators is not permitted");
                return View("ImpersonateUser");
            }

            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            LoginHelper.LoginWithImpersonation(
                HttpContext,
                impersonatedUser.UserId,
                LoginRoles.GpgEmployer,
                currentUser.UserId);

            return RedirectToAction("ManageOrganisationsGet", "ManageOrganisations");
        }

        [HttpPost("impersonate/{userId}")]
        [ValidateAntiForgeryToken]
        public IActionResult ImpersonateDirectPost(long userId)
        {
            User impersonatedUser = dataRepository.Get<User>(userId);
            if (impersonatedUser == null)
            {
                throw new Exception($"Trying to impersonate user ({userId}) but this user does not exist");
            }

            if (impersonatedUser.IsFullOrReadOnlyAdministrator())
            {
                throw new Exception($"Trying to impersonate user ({userId}) but this user is an administrator");
            }

            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            LoginHelper.LoginWithImpersonation(
                HttpContext,
                impersonatedUser.UserId,
                LoginRoles.GpgEmployer,
                currentUser.UserId);

            return RedirectToAction("ManageOrganisationsGet", "ManageOrganisations");
        }

    }
}
