using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Route("Register")]
    public partial class RegisterController : BaseController
    {

        private readonly EmailSendingService emailSendingService;
        private readonly AuditLogger auditLogger;

        public RegisterController(
            IHttpCache cache,
            IHttpSession session,
            IDataRepository dataRepository,
            IWebTracker webTracker,
            EmailSendingService emailSendingService,
            AuditLogger auditLogger)
            : base(
            cache,
            session,
            dataRepository,
            webTracker)
        {
            this.emailSendingService = emailSendingService;
            this.auditLogger = auditLogger;
        }

        /*
         * TODO - Delete this action once PITP is enabled
         */
        [Authorize]
        [HttpGet("~/pin-reset/{id}")]
        public IActionResult ResetPin(string id)
        {
            if (!FeatureFlagHelper.IsFeatureEnabled(FeatureFlag.PrivateManualRegistration))
            {
                return RedirectToAction("PINSent", "Register");
            }

            //Ensure user has completed the registration process
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            // Decrypt org id
            if (!id.DecryptToId(out long organisationId))
            {
                return new HttpBadRequestResult($"Cannot decrypt organisation id {id}");
            }

            // Check the user has permission for this organisation
            UserOrganisation userOrg = currentUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
            {
                return new HttpForbiddenResult($"User {currentUser?.EmailAddress} is not registered for organisation id {organisationId}");
            }

            userOrg.Organisation.UserOrganisations.Remove(userOrg);
            DataRepository.Delete(userOrg);
            DataRepository.SaveChanges();

            return RedirectToAction("ManageOrganisationsGet", "ManageOrganisations");
        }
    }
}
