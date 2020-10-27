using System;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.ExternalServices.CompaniesHouse;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Services;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
    [Route("admin")]
    public class AdminOrganisationNameController : Controller
    {
        private readonly IDataRepository dataRepository;
        private readonly ICompaniesHouseAPI companiesHouseApi;
        private readonly AuditLogger auditLogger;

        public AdminOrganisationNameController(
            IDataRepository dataRepository,
            ICompaniesHouseAPI companiesHouseApi,
            AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.companiesHouseApi = companiesHouseApi;
            this.auditLogger = auditLogger;
        }

        [HttpGet("organisation/{id}/name")]
        public IActionResult ViewNameHistory(long id)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);

            return View("ViewOrganisationName", organisation);
        }

        [HttpGet("organisation/{id}/name/change")]
        public IActionResult ChangeNameGet(long id)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);

            if (!string.IsNullOrWhiteSpace(organisation.CompanyNumber))
            {
                try
                {
                    CompaniesHouseCompany organisationFromCompaniesHouse =
                        companiesHouseApi.GetCompanyAsync(organisation.CompanyNumber).Result;

                    string nameFromCompaniesHouse = organisationFromCompaniesHouse.CompanyName;

                    if (!string.Equals(organisation.OrganisationName, nameFromCompaniesHouse, StringComparison.Ordinal))
                    {
                        return OfferNewCompaniesHouseName(organisation, nameFromCompaniesHouse);
                    }

                }
                catch (Exception ex)
                {
                    // Use Manual Change page instead
                    CustomLogger.Warning("Error from Companies House API", ex);
                }
            }

            // In all other cases...
            // * Organisation doesn't have a Companies House number
            // * CoHo API returns an error
            // * CoHo name matches Organisation name
            // ... send to the Manual Change page
            return SendToManualChangePage(organisation);
        }

        private IActionResult OfferNewCompaniesHouseName(Organisation organisation, string nameFromCompaniesHouse)
        {
            var viewModel = new ChangeOrganisationNameViewModel {
                Organisation = organisation,
                Action = ManuallyChangeOrganisationNameViewModelActions.OfferNewCompaniesHouseName,
                Name = nameFromCompaniesHouse
            };

            return View("OfferNewCompaniesHouseName", viewModel);
        }

        private IActionResult SendToManualChangePage(Organisation organisation)
        {
            var viewModel = new ChangeOrganisationNameViewModel
            {
                Organisation = organisation,
                Action = ManuallyChangeOrganisationNameViewModelActions.ManualChange,
                Name = organisation.OrganisationName
            };

            return View("ManuallyChangeOrganisationName", viewModel);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("organisation/{id}/name/change")]
        public IActionResult ChangeNamePost(long id, ChangeOrganisationNameViewModel viewModel)
        {
            // We might need to change the value of Action before we go to the view
            // Apparently this is necessary
            // https://stackoverflow.com/questions/4837744/hiddenfor-not-getting-correct-value-from-view-model
            ModelState.Clear();

            Organisation organisation = dataRepository.Get<Organisation>(id);

            switch (viewModel.Action) {
                case ManuallyChangeOrganisationNameViewModelActions.OfferNewCompaniesHouseName:
                    return OfferNewCompaniesHouseAction(viewModel, organisation);

                case ManuallyChangeOrganisationNameViewModelActions.ManualChange:
                    return ManualChangeAction(viewModel, organisation);

                case ManuallyChangeOrganisationNameViewModelActions.CheckChangesManual:
                case ManuallyChangeOrganisationNameViewModelActions.CheckChangesCoHo:
                    return CheckChangesAction(viewModel, organisation);

                default:
                    throw new ArgumentException("Unknown action in AdminOrganisationNameController.ChangeNamePost");
            }
        }

        private IActionResult OfferNewCompaniesHouseAction(ChangeOrganisationNameViewModel viewModel, Organisation organisation)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.AcceptCompaniesHouseName);

            if (viewModel.HasAnyErrors())
            {
                viewModel.Organisation = organisation;
                viewModel.Action = ManuallyChangeOrganisationNameViewModelActions.OfferNewCompaniesHouseName;
                return View("OfferNewCompaniesHouseName", viewModel);
            }

            if (viewModel.AcceptCompaniesHouseName == AcceptCompaniesHouseName.Reject)
            {
                return SendToManualChangePage(organisation);
            }

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (viewModel.HasAnyErrors())
            {
                viewModel.Organisation = organisation;
                viewModel.Action = ManuallyChangeOrganisationNameViewModelActions.OfferNewCompaniesHouseName;
                return View("OfferNewCompaniesHouseName", viewModel);
            }

            viewModel.Organisation = organisation;
            viewModel.Action = ManuallyChangeOrganisationNameViewModelActions.CheckChangesCoHo;
            return View("ConfirmNameChange", viewModel);
        }

        private IActionResult ManualChangeAction(ChangeOrganisationNameViewModel viewModel, Organisation organisation)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.Name);
            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (viewModel.HasAnyErrors())
            {
                viewModel.Organisation = organisation;
                viewModel.Action = ManuallyChangeOrganisationNameViewModelActions.ManualChange;
                return View("ManuallyChangeOrganisationName", viewModel);
            }

            viewModel.Organisation = organisation;
            viewModel.Action = ManuallyChangeOrganisationNameViewModelActions.CheckChangesManual;
            return View("ConfirmNameChange", viewModel);
        }

        private IActionResult CheckChangesAction(ChangeOrganisationNameViewModel viewModel, Organisation organisation)
        {
            if (viewModel.Action == ManuallyChangeOrganisationNameViewModelActions.CheckChangesManual)
            {
                OptOrganisationOutOfCompaniesHouseUpdates(organisation);
            }

            SaveChangesAndAuditAction(viewModel, organisation);

            return View("SuccessfullyChangedOrganisationName", organisation);
        }

        private void SaveChangesAndAuditAction(ChangeOrganisationNameViewModel viewModel, Organisation organisation)
        {
            string oldName = organisation.OrganisationName;

            OrganisationName newOrganisationName = CreateOrganisationNameFromViewModel(viewModel);
            AddNewNameToOrganisation(newOrganisationName, organisation);

            dataRepository.SaveChanges();

            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminChangeOrganisationName,
                organisation,
                new
                {
                    Action = viewModel.Action,
                    OldName = oldName,
                    NewName = newOrganisationName.Name,
                    NewNameId = newOrganisationName.OrganisationNameId,
                    Reason = viewModel.Reason
                },
                User);
        }

        private OrganisationName CreateOrganisationNameFromViewModel(ChangeOrganisationNameViewModel viewModel)
        {
            var organisationName = new OrganisationName {
                Name = viewModel.Name,
                Created = VirtualDateTime.Now,
                Source = "Service Desk",
            };

            return organisationName;
        }

        private void AddNewNameToOrganisation(OrganisationName organisationName, Organisation organisation)
        {
            organisationName.OrganisationId = organisation.OrganisationId;
            organisation.OrganisationNames.Add(organisationName);
            organisation.OrganisationName = organisationName.Name;

            dataRepository.Insert(organisationName);
        }

        private void OptOrganisationOutOfCompaniesHouseUpdates(Organisation organisation)
        {
            organisation.OptedOutFromCompaniesHouseUpdate = true;
            dataRepository.SaveChanges();
        }

        [HttpGet("organisation/{organisationId}/name/{nameId}/delete")]
        public IActionResult DeleteNameGet(long organisationId, long nameId)
        {
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            OrganisationName name = organisation.OrganisationNames.FirstOrDefault(oa => oa.OrganisationNameId == nameId);
            if (name == null)
            {
                throw new Exception($"Name ID {nameId} is not a valid name for this Organisation");
            }

            var viewModel = new DeleteOrganisationNameViewModel
            {
                Organisation = organisation,
                OrganisationNameToBeDeleted = name
            };

            return View("DeleteNameCheck", viewModel);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("organisation/{organisationId}/name/{nameId}/delete")]
        public IActionResult DeleteNamePost(long organisationId, long nameId, DeleteOrganisationNameViewModel viewModel)
        {
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            OrganisationName name = organisation.OrganisationNames.FirstOrDefault(oa => oa.OrganisationNameId == nameId);
            if (name == null)
            {
                throw new Exception($"Name ID {nameId} is not a valid name for this Organisation");
            }

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (viewModel.HasAnyErrors())
            {
                viewModel.Organisation = organisation;
                viewModel.OrganisationNameToBeDeleted = name;
                return View("DeleteNameCheck", viewModel);
            }

            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminDeleteOrganisationPreviousName,
                organisation,
                new
                {
                    DeletedName = name.Name,
                    DeletedNameId = name.OrganisationNameId,
                    DeletedNameSource = name.Source,
                    DeletedNameCreatedDate = name.Created.ToString("d MMM yyyy"),
                    Reason = viewModel.Reason
                },
                User);

            dataRepository.Delete(name);
            dataRepository.SaveChanges();

            return View("SuccessfullyDeletedOrganisationName", organisation);
        }

    }
}
