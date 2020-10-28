using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Models.Organisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{

    public partial class OrganisationController : BaseController
    {

        [Authorize]
        [HttpGet("~/manage-organisations/{id}")]
        public IActionResult ManageOrganisation(string id)
        {
            // Check for feature flag and redirect if enabled
            if (FeatureFlagHelper.IsFeatureEnabled(FeatureFlag.NewManageOrganisationsJourney))
            {
                return RedirectToAction("ManageOrganisationGet", "ManageOrganisations", new {encryptedOrganisationId = id});
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
            if (userOrg == null || userOrg.PINConfirmedDate == null)
            {
                return new HttpForbiddenResult($"User {currentUser?.EmailAddress} is not registered for organisation id {organisationId}");
            }

            // clear the stash
            this.ClearStash();

            //Get the current snapshot date
            DateTime currentSnapshotDate = userOrg.Organisation.SectorType.GetAccountingStartDate();

            //Make sure we have an explicit scope for last and year for organisations new to this year
            if (userOrg.PINConfirmedDate != null && userOrg.Organisation.Created >= currentSnapshotDate)
            {
                ScopeStatuses scopeStatus =
                    ScopeBusinessLogic.GetLatestScopeStatusForSnapshotYear(organisationId, currentSnapshotDate.Year - 1);
                if (!scopeStatus.IsAny(ScopeStatuses.InScope, ScopeStatuses.OutOfScope))
                {
                    return RedirectToAction(nameof(DeclareScope), "Organisation", new {id});
                }
            }

            // get any associated users for the current org
            List<UserOrganisation> associatedUserOrgs = userOrg.GetAssociatedUsers().ToList();

            // build the view model
            List<int> yearsWithDraftReturns =
                DataRepository.GetAll<DraftReturn>()
                    .Where(d => d.OrganisationId == organisationId)
                    .Select(d => d.SnapshotYear)
                    .ToList();

            var model = new ManageOrganisationModel {
                CurrentUserOrg = userOrg,
                AssociatedUserOrgs = associatedUserOrgs,
                EncCurrentOrgId = Encryption.EncryptQuerystring(organisationId.ToString()),
                ReportingYearsWithDraftReturns = yearsWithDraftReturns
            };

            return View(model);
        }

        [Authorize]
        [HttpGet("~/manage-organisations")]
        public IActionResult ManageOrganisations()
        {
            // Check for feature flag and redirect if not enabled
            if (FeatureFlagHelper.IsFeatureEnabled(FeatureFlag.NewManageOrganisationsJourney))
            {
                return RedirectToAction("ManageOrganisationsGet", "ManageOrganisations");
            }
            
            //Clear all the stashes
            this.ClearAllStashes();

            //Reset the current reporting organisation
            ReportingOrganisation = null;

            //Ensure user has completed the registration process
            IActionResult checkResult = CheckUserRegisteredOk(out User currentUser);
            if (checkResult != null && IsImpersonatingUser == false)
            {
                return checkResult;
            }

            // check if the user has accepted the privacy statement (unless admin or impersonating)
            if (!IsImpersonatingUser && !base.CurrentUser.IsAdministrator())
            {
                DateTime? hasReadPrivacy = currentUser.AcceptedPrivacyStatement;
                if (hasReadPrivacy == null || hasReadPrivacy.Value < Global.PrivacyChangedDate)
                {
                    return RedirectToAction(nameof(PrivacyPolicyController.PrivacyPolicyGet), "PrivacyPolicy");
                }
            }
            
            //create the new view model 
            IOrderedEnumerable<UserOrganisation> model = currentUser.UserOrganisations.OrderBy(uo => uo.Organisation.OrganisationName);
            return View("ManageOrganisations", model);
        }

    }

}
