using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Models;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminUserContactPreferencesController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly AuditLogger auditLogger;

        public AdminUserContactPreferencesController(IDataRepository dataRepository, AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
        }

        [HttpGet("user/{id}/change-contact-preferences")]
        public IActionResult ChangeContactPreferencesGet(long id)
        {
            User user = dataRepository.Get<User>(id);

            var viewModel = new AdminChangeUserContactPreferencesViewModel
            {
                UserId = user.UserId,
                FullName = user.Fullname,
                AllowContact = user.AllowContact,
                SendUpdates = user.SendUpdates
            };

            return View("ChangeContactPreferences", viewModel);
        }

        [HttpPost("user/{id}/change-contact-preferences")]
        public IActionResult ChangeContactPreferencesPost(long id, AdminChangeUserContactPreferencesViewModel viewModel)
        {
            User user = dataRepository.Get<User>(id);

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (viewModel.HasAnyErrors())
            {
                viewModel.UserId = user.UserId;
                viewModel.FullName = user.Fullname;

                return View("ChangeContactPreferences", viewModel);
            }

            auditLogger.AuditChangeToUser(
                this,
                AuditedAction.AdminChangeUserContactPreferences,
                user,
                new
                {
                    AllowContact_Old = user.AllowContact ? "Yes" : "No",
                    AllowContact_New = viewModel.AllowContact ? "Yes" : "No",
                    SendUpdates_Old = user.SendUpdates ? "Yes" : "No",
                    SendUpdates_New = viewModel.SendUpdates ? "Yes" : "No",
                    Reason = viewModel.Reason
                });

            user.AllowContact = viewModel.AllowContact;
            user.SendUpdates = viewModel.SendUpdates;

            dataRepository.SaveChangesAsync().Wait();

            return RedirectToAction("ViewUser", "AdminViewUser", new {id = user.UserId});
        }

    }
}
