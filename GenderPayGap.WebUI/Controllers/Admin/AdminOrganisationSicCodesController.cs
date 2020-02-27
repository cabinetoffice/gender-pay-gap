using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.API;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models.CompaniesHouse;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminOrganisationSicCodesController : Controller
    {
        private readonly IDataRepository dataRepository;
        private readonly ICompaniesHouseAPI companiesHouseApi;
        private readonly AuditLogger auditLogger;

        public AdminOrganisationSicCodesController(
            IDataRepository dataRepository,
            ICompaniesHouseAPI companiesHouseApi,
            AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.companiesHouseApi = companiesHouseApi;
            this.auditLogger = auditLogger;
        }

        [HttpGet("organisation/{id}/sic-codes")]
        public IActionResult ViewSicCodesHistory(long id)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);

            return View("ViewOrganisationSicCodes", organisation);
        }

        [HttpGet("organisation/{id}/sic-codes/change")]
        public IActionResult ChangeSicCodesGet(long id)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);

            if (!string.IsNullOrWhiteSpace(organisation.CompanyNumber))
            {
                try
                {
                    CompaniesHouseCompany organisationFromCompaniesHouse =
                        companiesHouseApi.GetCompanyAsync(organisation.CompanyNumber).Result;

                    List<string> sicCodeIdsFromCompaniesHouse = organisationFromCompaniesHouse.SicCodes;
                    List<string> sicCodesFromDatabase = organisation.GetSicCodes().Select(osc => osc.SicCodeId.ToString()).ToList();

                    if (!sicCodesFromDatabase.ToHashSet().SetEquals(sicCodeIdsFromCompaniesHouse))
                    {
                        return OfferNewCompaniesHouseSicCodes(organisation, sicCodeIdsFromCompaniesHouse);
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
            // * CoHo SIC codes match Organisation SIC codes
            // ... send to the Manual Change page
            return SendToManualChangePage(organisation);
        }

        private IActionResult OfferNewCompaniesHouseSicCodes(Organisation organisation, List<string> sicCodeIdStringsFromCompaniesHouse)
        {
            var viewModel = new ChangeOrganisationSicCodesViewModel();
            PopulateViewModelFromCompaniesHouseSicCodes(viewModel, sicCodeIdStringsFromCompaniesHouse, organisation);

            return View("OfferNewCompaniesHouseSicCodes", viewModel);
        }

        private void PopulateViewModelFromCompaniesHouseSicCodes(
            ChangeOrganisationSicCodesViewModel viewModel,
            List<string> sicCodeIdStringsFromCompaniesHouse, 
            Organisation organisation)
        {
            var sicCodesFromCompaniesHouse = new Dictionary<string, SicCode>();
            var sicCodeIdsFromCompaniesHouse = new List<int>();
            List<int> sicCodeIdsFromDatabase = organisation.GetSicCodeIds().ToList();

            foreach (string sicCodeIdString in sicCodeIdStringsFromCompaniesHouse.Distinct())
            {
                SicCode sicCode = null;
                if (int.TryParse(sicCodeIdString, out int sicCodeId))
                {
                    sicCode = dataRepository.Get<SicCode>(sicCodeId);
                    if (sicCode != null)
                    {
                        sicCodeIdsFromCompaniesHouse.Add(sicCodeId);
                    }
                }

                sicCodesFromCompaniesHouse.Add(sicCodeIdString, sicCode);
            }

            List<int> sicCodeIdsToAdd = sicCodeIdsFromCompaniesHouse.Except(sicCodeIdsFromDatabase).ToList();
            List<int> sicCodeIdsToRemove = sicCodeIdsFromDatabase.Except(sicCodeIdsFromCompaniesHouse).ToList();

            viewModel.Organisation = organisation;
            viewModel.SicCodesFromCoHo = sicCodesFromCompaniesHouse;
            viewModel.SicCodeIdsFromCoHo = sicCodeIdStringsFromCompaniesHouse;
            viewModel.SicCodeIdsToAdd = sicCodeIdsToAdd;
            viewModel.SicCodeIdsToRemove = sicCodeIdsToRemove;
        }

        private IActionResult SendToManualChangePage(Organisation organisation)
        {
            var viewModel = new ChangeOrganisationSicCodesViewModel {
                Organisation = organisation
            };
            PopulateViewModelWithSicCodeData(viewModel, organisation);

            return View("ManuallyChangeOrganisationSicCodes", viewModel);
        }

        private void PopulateViewModelWithSicCodeData(ChangeOrganisationSicCodesViewModel viewModel, Organisation organisation)
        {
            List<int> sicCodeIdsToAdd = viewModel.SicCodeIdsToAdd ?? new List<int>();
            viewModel.SicCodesToAdd = dataRepository
                .GetAll<SicCode>()
                .Where(sc => sicCodeIdsToAdd.Contains(sc.SicCodeId))
                .ToList();

            List<int> sicCodeIdsToRemove = viewModel.SicCodeIdsToRemove ?? new List<int>();
            viewModel.SicCodesToRemove = dataRepository
                .GetAll<SicCode>()
                .Where(sc => sicCodeIdsToRemove.Contains(sc.SicCodeId))
                .ToList();

            var sicCodeIdsToKeep = organisation
                .GetSicCodes()
                .Select(sc => sc.SicCodeId)
                .Except(sicCodeIdsToRemove)
                .ToList();

            viewModel.SicCodesToKeep = dataRepository
                .GetAll<SicCode>()
                .Where(sc => sicCodeIdsToKeep.Contains(sc.SicCodeId))
                .ToList();
        }

        [ValidateAntiForgeryToken]
        [HttpPost("organisation/{id}/sic-codes/change")]
        public IActionResult ChangeSicCodesPost(long id, ChangeOrganisationSicCodesViewModel viewModel)
        {
            // We might need to change the value of Action before we go to the view
            // Apparently this is necessary
            // https://stackoverflow.com/questions/4837744/hiddenfor-not-getting-correct-value-from-view-model
            ModelState.Clear();

            Organisation organisation = dataRepository.Get<Organisation>(id);
            viewModel.Organisation = organisation;

            switch (viewModel.Action)
            {
                case ManuallyChangeOrganisationSicCodesActions.OfferCompaniesHouseSicCodesAnswer:
                    return OfferCompaniesHouseAnswerAction(viewModel, organisation);

                case ManuallyChangeOrganisationSicCodesActions.ManualChangeDoNotAddSicCode:
                    DoNotAddNewSicCode(viewModel, organisation);
                    PopulateViewModelWithSicCodeData(viewModel, organisation);
                    return View("ManuallyChangeOrganisationSicCodes", viewModel);

                case ManuallyChangeOrganisationSicCodesActions.ManualChangeAddSicCode:
                    return AddNewSicCode(viewModel, organisation);

                case ManuallyChangeOrganisationSicCodesActions.ManualChangeRemoveSicCode:
                    RemoveNewSicCode(viewModel, organisation);
                    PopulateViewModelWithSicCodeData(viewModel, organisation);
                    return View("ManuallyChangeOrganisationSicCodes", viewModel);

                case ManuallyChangeOrganisationSicCodesActions.ManualChangeKeepSicCode:
                    KeepNewSicCode(viewModel, organisation);
                    PopulateViewModelWithSicCodeData(viewModel, organisation);
                    return View("ManuallyChangeOrganisationSicCodes", viewModel);

                case ManuallyChangeOrganisationSicCodesActions.ManualChangeContinue:
                    return CheckChangesAction(viewModel, organisation);

                case ManuallyChangeOrganisationSicCodesActions.MakeMoreManualChanges:
                    PopulateViewModelWithSicCodeData(viewModel, organisation);
                    return View("ManuallyChangeOrganisationSicCodes", viewModel);

                case ManuallyChangeOrganisationSicCodesActions.ConfirmManual:
                case ManuallyChangeOrganisationSicCodesActions.ConfirmCoho:
                    return ConfirmChangesAction(viewModel, organisation);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IActionResult OfferCompaniesHouseAnswerAction(ChangeOrganisationSicCodesViewModel viewModel, Organisation organisation)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.AcceptCompaniesHouseSicCodes);

            if (viewModel.HasAnyErrors())
            {
                PopulateViewModelFromCompaniesHouseSicCodes(viewModel, viewModel.SicCodeIdsFromCoHo, organisation);
                return View("OfferNewCompaniesHouseSicCodes", viewModel);
            }

            if (viewModel.AcceptCompaniesHouseSicCodes == AcceptCompaniesHouseSicCodes.Reject)
            {
                return SendToManualChangePage(organisation);
            }

            PopulateViewModelWithSicCodeData(viewModel, organisation);
            viewModel.ConfirmationType = ChangeOrganisationSicCodesConfirmationType.CoHo;
            return View("ConfirmSicCodeChanges", viewModel);
        }

        private IActionResult AddNewSicCode(ChangeOrganisationSicCodesViewModel viewModel, Organisation organisation)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.SicCodeIdToChange);

            if (viewModel.HasAnyErrors())
            {
                PopulateViewModelWithSicCodeData(viewModel, organisation);
                return View("ManuallyChangeOrganisationSicCodes", viewModel);
            }

            SicCode newSicCode = dataRepository.Get<SicCode>(viewModel.SicCodeIdToChange.Value);
            if (newSicCode == null)
            {
                viewModel.AddErrorFor<ChangeOrganisationSicCodesViewModel, int?>(
                    m => m.SicCodeIdToChange,
                    "This SIC code is not valid (it is not in our database of SIC codes)");
            }

            if (viewModel.HasAnyErrors())
            {
                PopulateViewModelWithSicCodeData(viewModel, organisation);
                return View("ManuallyChangeOrganisationSicCodes", viewModel);
            }

            if (viewModel.SicCodeIdsToAdd == null)
            {
                viewModel.SicCodeIdsToAdd = new List<int>();
            }

            viewModel.SicCodeIdsToAdd.Add(newSicCode.SicCodeId);
            viewModel.SicCodeIdToChange = null;

            PopulateViewModelWithSicCodeData(viewModel, organisation);
            return View("ManuallyChangeOrganisationSicCodes", viewModel);
        }

        private void RemoveNewSicCode(ChangeOrganisationSicCodesViewModel viewModel, Organisation organisation)
        {
            int newSicCodeToRemove = viewModel.SicCodeIdToChange.Value;

            if (!organisation.GetSicCodeIds().Contains(newSicCodeToRemove))
            {
                throw new ArgumentException("Cannot remove SIC code as it is not present on organisation "
                                            + $" OrganisationId({organisation.OrganisationId}) SIC code({newSicCodeToRemove})");
            }

            if (viewModel.SicCodeIdsToRemove == null)
            {
                viewModel.SicCodeIdsToRemove = new List<int>();
            }

            viewModel.SicCodeIdsToRemove.Add(newSicCodeToRemove);
            viewModel.SicCodeIdToChange = null;
        }

        private void KeepNewSicCode(ChangeOrganisationSicCodesViewModel viewModel, Organisation organisation)
        {
            int newSicCodeToKeep = viewModel.SicCodeIdToChange.Value;

            if (!organisation.GetSicCodeIds().Contains(newSicCodeToKeep))
            {
                throw new ArgumentException("Cannot keep SIC code as it is not present on organisation "
                                            + $" OrganisationId({organisation.OrganisationId}) SIC code({newSicCodeToKeep})");
            }

            viewModel.SicCodeIdsToRemove.Remove(newSicCodeToKeep);
            viewModel.SicCodeIdToChange = null;
        }

        private void DoNotAddNewSicCode(ChangeOrganisationSicCodesViewModel viewModel, Organisation organisation)
        {
            int newSicCodeToNotAdd = viewModel.SicCodeIdToChange.Value;

            viewModel.SicCodeIdsToAdd.Remove(newSicCodeToNotAdd);
            viewModel.SicCodeIdToChange = null;
        }

        private IActionResult CheckChangesAction(ChangeOrganisationSicCodesViewModel viewModel, Organisation organisation)
        {
            PopulateViewModelWithSicCodeData(viewModel, organisation);

            viewModel.ConfirmationType = ChangeOrganisationSicCodesConfirmationType.Manual;

            return View("ConfirmSicCodeChanges", viewModel);
        }

        private IActionResult ConfirmChangesAction(ChangeOrganisationSicCodesViewModel viewModel, Organisation organisation)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (viewModel.HasAnyErrors())
            {
                PopulateViewModelWithSicCodeData(viewModel, organisation);
                viewModel.ConfirmationType = ChangeOrganisationSicCodesConfirmationType.Manual;
                return View("ConfirmSicCodeChanges", viewModel);
            }

            if (viewModel.Action == ManuallyChangeOrganisationSicCodesActions.ConfirmManual)
            {
                OptOrganisationOutOfCompaniesHouseUpdates(organisation);
            }

            SaveChangesAndAuditAction(viewModel, organisation);

            return View("SuccessfullyChangedOrganisationSicCodes", organisation);
        }

        private void SaveChangesAndAuditAction(ChangeOrganisationSicCodesViewModel viewModel, Organisation organisation)
        {
            string oldSicCodes = organisation.GetSicCodeIdsString();

            RemoveSicCodes(viewModel, organisation);
            AddSicCodes(viewModel, organisation);

            dataRepository.SaveChangesAsync().Wait();

            string newSicCodes = organisation.GetSicCodeIdsString();

            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminChangeOrganisationSicCode,
                organisation,
                new
                {
                    Action = viewModel.Action,
                    OldSicCodes = oldSicCodes,
                    NewSicCodes = newSicCodes,
                    Reason = viewModel.Reason
                },
                currentUser);
        }

        private void OptOrganisationOutOfCompaniesHouseUpdates(Organisation organisation)
        {
            organisation.OptedOutFromCompaniesHouseUpdate = true;
            dataRepository.SaveChangesAsync().Wait();
        }

        private void RemoveSicCodes(ChangeOrganisationSicCodesViewModel viewModel, Organisation organisation)
        {
            if (viewModel.SicCodeIdsToRemove != null)
            {
                foreach (int sicCodeId in viewModel.SicCodeIdsToRemove)
                {
                    OrganisationSicCode organisationSicCodeToRemove = organisation.GetSicCodes().Where(osc => osc.SicCodeId == sicCodeId).First();
                    organisationSicCodeToRemove.Retired = VirtualDateTime.Now;
                }
            }
        }

        private void AddSicCodes(ChangeOrganisationSicCodesViewModel viewModel, Organisation organisation)
        {
            if (viewModel.SicCodeIdsToAdd != null)
            {
                foreach (int sicCodeId in viewModel.SicCodeIdsToAdd)
                {
                    // This line validates that the SIC code ID is in our database
                    SicCode sicCode = dataRepository.Get<SicCode>(sicCodeId);

                    var newSicCode = new OrganisationSicCode {
                        OrganisationId = organisation.OrganisationId,
                        SicCodeId = sicCode.SicCodeId,
                        Source = "Service Desk",
                        Created = VirtualDateTime.Now,
                    };

                    organisation.OrganisationSicCodes.Add(newSicCode);

                    dataRepository.Insert(newSicCode);
                }
            }
        }

    }
}
