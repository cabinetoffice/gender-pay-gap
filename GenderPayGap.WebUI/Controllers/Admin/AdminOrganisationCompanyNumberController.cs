using System;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.ExternalServices.CompaniesHouse;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Services;
using GovUkDesignSystem;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
    [Route("admin")]
    public class AdminOrganisationCompanyNumberController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly ICompaniesHouseAPI companiesHouseApi;
        private readonly AuditLogger auditLogger;

        public AdminOrganisationCompanyNumberController(
            IDataRepository dataRepository,
            ICompaniesHouseAPI companiesHouseApi,
            AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.companiesHouseApi = companiesHouseApi;
            this.auditLogger = auditLogger;
        }

        [HttpGet("organisation/{id}/change-company-number")]
        public IActionResult ChangeCompanyNumberGet(long id)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);

            var viewModel = new AdminOrganisationCompanyNumberViewModel
            {
                Organisation = organisation
            };

            if (string.IsNullOrEmpty(organisation.CompanyNumber))
            {
                // CompanyNumber is empty - send them straight to ChangeCompanyNumber
                return View("ChangeOrganisationCompanyNumber", viewModel);
            }
            // CompanyNumber is not empty - ask the user if they want to change or remove the CompanyNumber
            return View("OfferChangeOrRemoveOrganisationCompanyNumber", viewModel);
        }

        [HttpPost("organisation/{id}/change-company-number")]
        public IActionResult ChangeCompanyNumberPost(long id, AdminOrganisationCompanyNumberViewModel viewModel)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);
            
            switch (viewModel.CurrentPage)
            {
                case AdminOrganisationCompanyNumberViewModelCurrentPage.OfferChangeOrRemove:
                    return OfferChangeOrRemove(organisation, viewModel);
                case AdminOrganisationCompanyNumberViewModelCurrentPage.Remove:
                    return Remove(organisation, viewModel);
                case AdminOrganisationCompanyNumberViewModelCurrentPage.Change:
                    return Change(organisation, viewModel);
                case AdminOrganisationCompanyNumberViewModelCurrentPage.BackToChange:
                    return BackToChange(organisation, viewModel);
                case AdminOrganisationCompanyNumberViewModelCurrentPage.ConfirmNew:
                    return ConfirmNew(organisation, viewModel);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IActionResult OfferChangeOrRemove(Organisation organisation, AdminOrganisationCompanyNumberViewModel viewModel)
        {
            viewModel.Organisation = organisation;

            viewModel.ParseAndValidateParameters(Request, m => m.ChangeOrRemove);

            if (viewModel.HasAnyErrors())
            {
                return View("OfferChangeOrRemoveOrganisationCompanyNumber", viewModel);
            }

            // If removing CompanyNumber
            if (viewModel.ChangeOrRemove == AdminOrganisationCompanyNumberChangeOrRemove.Remove)
            {
                return View("RemoveOrganisationCompanyNumber", viewModel);
            }

            // If changing CompanyNumber
            return View("ChangeOrganisationCompanyNumber", viewModel);
        }

        private IActionResult Remove(Organisation organisation, AdminOrganisationCompanyNumberViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (viewModel.HasAnyErrors())
            {
                viewModel.Organisation = organisation;
                return View("RemoveOrganisationCompanyNumber", viewModel);
            }

            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminChangeOrganisationCompanyNumber,
                organisation,
                new
                {
                    OldCompanyNumber = organisation.CompanyNumber,
                    NewCompanyNumber = "(removed)",
                    Reason = viewModel.Reason
                }
            );

            organisation.CompanyNumber = null;

            dataRepository.SaveChangesAsync().Wait();

            return RedirectToAction("ViewOrganisation", "AdminViewOrganisation", new {id = organisation.OrganisationId});
        }

        private IActionResult Change(Organisation organisation, AdminOrganisationCompanyNumberViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.NewCompanyNumber);
            viewModel.Organisation = organisation;

            if (viewModel.HasAnyErrors())
            {
                return View("ChangeOrganisationCompanyNumber", viewModel);
            }

            string formattedCompanyNumber = viewModel.NewCompanyNumber.Trim().ToUpper();

            if (formattedCompanyNumber == organisation.CompanyNumber?.Trim().ToUpper())
            {
                viewModel.AddErrorFor(m => m.NewCompanyNumber,
                    "Company number must be different to the current company number");
                return View("ChangeOrganisationCompanyNumber", viewModel);
            }

            // Check that another stored organisation doesn't have the same company number
            Organisation organisationWithSameNewCompanyNumber =
                dataRepository.GetAll<Organisation>()
                    .FirstOrDefault(o => o.CompanyNumber == formattedCompanyNumber);

            if (organisationWithSameNewCompanyNumber != null)
            {
                viewModel.AddErrorFor(
                    m => m.NewCompanyNumber,
                    $"Another organisation ({organisationWithSameNewCompanyNumber.OrganisationName}) has this company number");
                return View("ChangeOrganisationCompanyNumber", viewModel);
            }

            CompaniesHouseCompany companiesHouseCompany;
            try
            {
                companiesHouseCompany = companiesHouseApi.GetCompanyAsync(formattedCompanyNumber).Result;
            }
            catch (Exception)
            {
                viewModel.AddErrorFor(
                    m => m.NewCompanyNumber,
                    "Companies House API gave an error (maybe the API is down, or maybe the company number is invalid)");
                return View("ChangeOrganisationCompanyNumber", viewModel);
            }

            if (companiesHouseCompany == null)
            {
                viewModel.AddErrorFor(
                    m => m.NewCompanyNumber,
                    "Companies House didn't recognise this company number");
                return View("ChangeOrganisationCompanyNumber", viewModel);
            }

            viewModel.CompaniesHouseCompany = companiesHouseCompany;
            viewModel.NewCompanyNumber = companiesHouseCompany.CompanyNumber; // Set the company number in the right format
            return View("ConfirmNewOrganisationCompanyNumber", viewModel);
        }

        private IActionResult BackToChange(Organisation organisation, AdminOrganisationCompanyNumberViewModel viewModel)
        {
            viewModel.Organisation = organisation;
            return View("ChangeOrganisationCompanyNumber", viewModel);
        }

        private IActionResult ConfirmNew(Organisation organisation, AdminOrganisationCompanyNumberViewModel viewModel)
        {
            viewModel.Organisation = organisation;

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (viewModel.HasAnyErrors())
            {
                return View("ChangeOrganisationCompanyNumber", viewModel);
            }

            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminChangeOrganisationCompanyNumber,
                organisation,
                new
                {
                    OldCompanyNumber = organisation.CompanyNumber,
                    viewModel.NewCompanyNumber,
                    viewModel.Reason
                },
                User
            );

            organisation.CompanyNumber = viewModel.NewCompanyNumber;

            dataRepository.SaveChangesAsync().Wait();

            return RedirectToAction("ViewOrganisation", "AdminViewOrganisation", new { id = organisation.OrganisationId });
        }

    }
}
