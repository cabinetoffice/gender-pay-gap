using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.ManageOrganisations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.ManageOrganisations
{
    [Route("account/organisations")]
    public class ManageOrganisationsController : Controller
    {

        private readonly IDataRepository dataRepository;

        public ManageOrganisationsController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet]
        [Authorize(Roles = LoginRoles.GpgEmployer + "," + LoginRoles.GpgAdmin)]
        public IActionResult ManageOrganisationsGet()
        {
            if (User.IsInRole(LoginRoles.GpgAdmin))
            {
                return RedirectToAction("AdminHomePage", "AdminHomepage");
            }

            User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(user);
            ControllerHelper.RedirectIfUserNeedsToReadPrivacyPolicy(User, user, Url);

            var viewModel = new ManageOrganisationsViewModel
            {
                UserOrganisations = user.UserOrganisations.OrderBy(uo => uo.Organisation.OrganisationName)
            };

            return View("ManageOrganisations", viewModel);

        }

        [HttpGet("{encryptedOrganisationId}")]
        [Authorize(Roles = LoginRoles.GpgEmployer)]
        public IActionResult ManageOrganisationGet(string encryptedOrganisationId)
        {
            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(user);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);

            var organisation = dataRepository.Get<Organisation>(organisationId);
            if (OrganisationIsNewThisYearAndHasNotProvidedScopeForLastYear(organisation))
            {
                return RedirectToAction("DeclareScopeGet", "Scope", new { encryptedOrganisationId = encryptedOrganisationId }); 
            }

            // build the view model
            List<DraftReturn> allDraftReturns =
                dataRepository.GetAll<DraftReturn>()
                    .Where(d => d.OrganisationId == organisationId)
                    .ToList();
            var viewModel = new ManageOrganisationViewModel(organisation, user, allDraftReturns);

            return View("ManageOrganisation", viewModel);
        }

        [HttpGet("{encryptedOrganisationId}/AllReports")]
        [Authorize(Roles = LoginRoles.GpgEmployer)]
        public IActionResult AllOrganisationReportsGet(string encryptedOrganisationId, int? page = 1)
        {
            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(user);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            
            var organisation = dataRepository.Get<Organisation>(organisationId);
            if (OrganisationIsNewThisYearAndHasNotProvidedScopeForLastYear(organisation))
            {
                return RedirectToAction("DeclareScopeGet", "Scope", new { encryptedOrganisationId = encryptedOrganisationId });
            }
            
            // build the view model
            List<DraftReturn> allDraftReturns =
                dataRepository.GetAll<DraftReturn>()
                    .Where(d => d.OrganisationId == organisationId)
                    .ToList();

            var totalEntries = organisation.GetRecentReports(Global.ShowReportYearCount).Count() + 1; // Years we report for + the year they joined
            var maxEntriesPerPage = 10;
            var totalPages = (int)Math.Ceiling((double)totalEntries / maxEntriesPerPage);

            if (page < 1)
            {
                page = 1;
            }

            if (page > totalPages)
            {
                page = totalPages;
            }
            
            var viewModel = new AllOrganisationReportsViewModel(organisation, user, allDraftReturns, page, totalPages, maxEntriesPerPage);
            return View("AllOrganisationReports", viewModel);
        }

        private static bool OrganisationIsNewThisYearAndHasNotProvidedScopeForLastYear(Organisation organisation)
        {
            DateTime currentYearSnapshotDate = organisation.SectorType.GetAccountingStartDate();
            bool organisationCreatedInCurrentReportingYear = organisation.Created >= currentYearSnapshotDate;

            if (organisationCreatedInCurrentReportingYear)
            {
                int previousReportingYear = currentYearSnapshotDate.AddYears(-1).Year;
                OrganisationScope scope = organisation.GetScopeForYear(previousReportingYear);

                if (scope.IsScopePresumed())
                {
                    return true;
                }
            }

            return false;
        }

    }
}
