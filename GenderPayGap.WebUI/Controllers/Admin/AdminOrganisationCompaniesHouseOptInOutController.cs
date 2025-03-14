﻿using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.ExternalServices.CompaniesHouse;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
    [Route("admin")]
    public class AdminOrganisationCompaniesHouseOptInOutController : Controller
    {
        private readonly AuditLogger auditLogger;
        private readonly CompaniesHouseAPI companiesHouseApi;
        private readonly IDataRepository dataRepository;
        private readonly UpdateFromCompaniesHouseService updateFromCompaniesHouseService;

        public AdminOrganisationCompaniesHouseOptInOutController(IDataRepository dataRepository,
            AuditLogger auditLogger,
            CompaniesHouseAPI companiesHouseApi,
            UpdateFromCompaniesHouseService updateFromCompaniesHouseService)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
            this.companiesHouseApi = companiesHouseApi;
            this.updateFromCompaniesHouseService = updateFromCompaniesHouseService;
        }


        [HttpGet("organisation/{id}/coho-sync/opt-in")]
        public IActionResult OptIn(long id)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);

            var model = new AdminChangeCompaniesHouseOptInOutViewModel();
            model.Organisation = organisation;
            PopulateViewModelWithCompanyFromCompaniesHouse(model, organisation);

            return View("OptIn", model);
        }

        [HttpPost("organisation/{id}/coho-sync/opt-in")]
        [ValidateAntiForgeryToken]
        public IActionResult OptIn(long id, AdminChangeCompaniesHouseOptInOutViewModel viewModel)
        {
            var organisation = dataRepository.Get<Organisation>(id);

            if (!ModelState.IsValid)
            {
                viewModel.Organisation = organisation;
                PopulateViewModelWithCompanyFromCompaniesHouse(viewModel, organisation);

                // If there are any errors, return the user back to the same page to correct the mistakes
                return View("OptIn", viewModel);
            }

            updateFromCompaniesHouseService.UpdateOrganisationDetails(organisation.OrganisationId);

            organisation.OptedOutFromCompaniesHouseUpdate = false;
            dataRepository.SaveChanges();

            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminChangeCompaniesHouseOpting,
                organisation,
                new {
                    Opt = "In",
                    Reason = viewModel.Reason
                },
                User);

            return RedirectToAction("ViewOrganisation", "AdminViewOrganisation", new {id = organisation.OrganisationId});
        }

        private void PopulateViewModelWithCompanyFromCompaniesHouse(AdminChangeCompaniesHouseOptInOutViewModel viewModel, Organisation organisation)
        {
            CompaniesHouseCompany companiesHouseCompany;
            try
            {
                companiesHouseCompany = companiesHouseApi.GetCompany(organisation.CompanyNumber);
            }
            catch (Exception)
            {
                throw new Exception("This organisation doesn't have a companies house record.");
            }

            viewModel.CompaniesHouseCompany = companiesHouseCompany;
        }


        [HttpGet("organisation/{id}/coho-sync/opt-out")]
        public IActionResult OptOut(long id)
        {
            var organisation = dataRepository.Get<Organisation>(id);

            var model = new AdminChangeCompaniesHouseOptInOutViewModel();
            model.Organisation = organisation;

            return View("OptOut", model);
        }

        [HttpPost("organisation/{id}/coho-sync/opt-out")]
        [ValidateAntiForgeryToken]
        public IActionResult OptOut(long id, AdminChangeCompaniesHouseOptInOutViewModel viewModel)
        {
            var organisation = dataRepository.Get<Organisation>(id);

            if (!ModelState.IsValid)
            {
                viewModel.Organisation = organisation;

                // If there are any errors, return the user back to the same page to correct the mistakes
                return View("OptOut", viewModel);
            }

            organisation.OptedOutFromCompaniesHouseUpdate = true;
            dataRepository.SaveChanges();

            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminChangeCompaniesHouseOpting,
                organisation,
                new {
                    Opt = "Out",
                    Reason = viewModel.Reason
                },
                User);

            return RedirectToAction("ViewOrganisation", "AdminViewOrganisation", new {id = organisation.OrganisationId});
        }

    }
}
