using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Route("privacy-policy")]
    public class PrivacyPolicyController : Controller
    {

        private readonly IDataRepository dataRepository;

        public PrivacyPolicyController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }


        [HttpGet]
        public IActionResult PrivacyPolicyGet()
        {
            return View("PrivacyPolicy", null);
        }

        [Authorize(Roles = LoginRoles.GpgEmployer)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult PrivacyPolicyPost()
        {
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);

            if (!LoginHelper.IsUserBeingImpersonated(User))
            {
                // Only update User.AcceptedPrivacyStatement if it's the real user
                // i.e. (don't update User.AcceptedPrivacyStatement if the user is being impersonated by an admin)

                User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
                
                user.AcceptedPrivacyStatement = VirtualDateTime.Now;

                dataRepository.SaveChangesAsync().Wait();
            }

            return RedirectToAction("ManageOrganisations", "Organisation");
        }

    }
}
