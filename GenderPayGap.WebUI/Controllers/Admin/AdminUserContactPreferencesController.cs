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

        private readonly AuditLogger auditLogger;

        private readonly IDataRepository dataRepository;

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

            // We don't currently have a way of saying that an admin user made a change TO a normal user 
            //auditLogger.AuditAction(
            //    this,
            //    AuditedAction.AdminChangeUserContactPreferences,
            //    null,
            //    new
            //    {
            //        AllowContact_Old = user.AllowContact,
            //        AllowContact_New = viewModel.AllowContact,
            //        SendUpdates_Old = user.SendUpdates,
            //        SendUpdates_New = viewModel.SendUpdates,
            //        Reason = viewModel.Reason
            //    });

            user.AllowContact = viewModel.AllowContact;
            user.SendUpdates = viewModel.SendUpdates;

            dataRepository.SaveChangesAsync().Wait();

            return RedirectToAction("ViewUser", "AdminViewUser", new {id = user.UserId});
        }

    }
}
