using System;
using System.Linq;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.ErrorHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public class OrganisationController : BaseController
    {

        public OrganisationController(
            IHttpSession session,
            IDataRepository dataRepository,
            IWebTracker webTracker) : base(
            session,
            dataRepository,
            webTracker) { }

        [Authorize]
        [HttpGet("~/activate-organisation/{id}")]
        public IActionResult ActivateOrganisation(string id)
        {
            // Ensure user has completed the registration process
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            // Decrypt org id
            if (!id.DecryptToId(out long organisationId))
            {
                return new HttpBadRequestResult($"Cannot decrypt employe id {id}");
            }

            // Check the user has permission for this organisation
            UserOrganisation userOrg = currentUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
            {
                return new HttpForbiddenResult($"User {currentUser?.EmailAddress} is not registered for employer id {organisationId}");
            }

            // TODO - Delete this once PITP is enabled
            if (userOrg.HasExpiredPin())
            {
                userOrg.Organisation.UserOrganisations.Remove(userOrg);
                DataRepository.Delete(userOrg);
                DataRepository.SaveChanges();

                throw new PinExpiredException();
            }

            // Ensure this organisation needs activation on the users account
            if (userOrg.HasBeenActivated())
            {
                throw new Exception(
                    $"Attempt to activate employer {userOrg.OrganisationId}:'{userOrg.Organisation.OrganisationName}' for {currentUser.EmailAddress} by '{(OriginalUser == null ? currentUser.EmailAddress : OriginalUser.EmailAddress)}' which has already been activated");
            }

            // begin ActivateService journey
            ReportingOrganisationId = organisationId;
            return RedirectToAction("ActivateService", "Register");
        }
        
    }
}
