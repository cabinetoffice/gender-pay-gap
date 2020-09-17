using System;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
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
    public class AdminOrganisationSectorController : Controller
    {
        private readonly IDataRepository dataRepository;
        private readonly AuditLogger auditLogger;

        public AdminOrganisationSectorController(
            IDataRepository dataRepository,
            AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
        }
        
          [HttpGet("organisation/{id}/sector")]
        public IActionResult ViewSectorHistory(long id)
        {
            var organisation = dataRepository.Get<Organisation>(id);

            return View("ViewOrganisationSector", organisation);
        }
        
        [HttpGet("organisation/{id}/sector/change")]
        public IActionResult ChangeSectorGet(long id)
        {
            var viewModel = new AdminChangeSectorViewModel();

            UpdateAdminChangeSectorViewModelFromOrganisation(viewModel, id);

            return View("ChangeSector", viewModel);
        }
        
        [HttpPost("organisation/{id}/sector/change")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeSectorPost(long id, AdminChangeSectorViewModel viewModel)
        {
            switch (viewModel.Action)
            {
                case ChangeOrganisationSectorViewModelActions.OfferNewSectorAndReason:
                    UpdateAdminChangeSectorViewModelFromOrganisation(viewModel, id);
                    ValidateAdminChangeSectorViewModel(viewModel);
                    if (viewModel.HasAnyErrors())
                    {
                        return View("ChangeSector", viewModel);
                    }
            
                    return View("ConfirmSectorChange", viewModel);
            
                case ChangeOrganisationSectorViewModelActions.ConfirmSectorChange:
                    ChangeSector(viewModel, id);
                    return RedirectToAction("ViewSectorHistory", "AdminOrganisationSector", new {id});
                default:
                    throw new ArgumentException("Unknown action in AdminOrganisationSectorController.ChangeSectorPost");
            }
        }

        [HttpGet("organisation/{id}/change-public-sector-classification")]
        public IActionResult ChangePublicSectorClassificationGet(long id)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);

            var viewModel = new AdminChangePublicSectorClassificationViewModel
            {
                OrganisationId = organisation.OrganisationId,
                OrganisationName = organisation.OrganisationName,
                PublicSectorTypes = dataRepository.GetAll<PublicSectorType>().ToList(),
                SelectedPublicSectorTypeId = organisation.LatestPublicSectorType?.PublicSectorTypeId
            };

            return View("ChangePublicSectorClassification", viewModel);
        }

        [HttpPost("organisation/{id}/change-public-sector-classification")]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePublicSectorClassificationPost(long id, AdminChangePublicSectorClassificationViewModel viewModel)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);
            viewModel.OrganisationId = organisation.OrganisationId;
            viewModel.OrganisationName = organisation.OrganisationName;
            viewModel.PublicSectorTypes = dataRepository.GetAll<PublicSectorType>().ToList();

            viewModel.ParseAndValidateParameters(Request, m=> m.Reason);

            if (!viewModel.SelectedPublicSectorTypeId.HasValue)
            {
                viewModel.AddErrorFor(
                    m => m.SelectedPublicSectorTypeId,
                    "Please select a public sector classification");
            }

            if (viewModel.HasAnyErrors())
            {
                return View("ChangePublicSectorClassification", viewModel);
            }

            var newPublicSectorType = dataRepository.GetAll<PublicSectorType>()
                .FirstOrDefault(p => p.PublicSectorTypeId == viewModel.SelectedPublicSectorTypeId.Value);
            if (newPublicSectorType == null)
            {
                throw new ArgumentException($"User selected an invalid PublicSectorType ({viewModel.SelectedPublicSectorTypeId})");
            }

            AuditChange(viewModel, organisation, newPublicSectorType);

            RetireExistingOrganisationPublicSectorTypesForOrganisation(organisation);

            AddNewOrganisationPublicSectorType(organisation, viewModel.SelectedPublicSectorTypeId.Value);

            dataRepository.SaveChangesAsync().Wait();

            return RedirectToAction("ViewOrganisation", "AdminViewOrganisation", new {id = organisation.OrganisationId});
        }

        private void AuditChange(
            AdminChangePublicSectorClassificationViewModel viewModel,
            Organisation organisation,
            PublicSectorType newPublicSectorType)
        {
            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminChangeOrganisationPublicSectorClassification,
                organisation,
                new
                {
                    OldClassification = organisation.LatestPublicSectorType?.PublicSectorType?.Description,
                    NewClassification = newPublicSectorType.Description,
                    viewModel.Reason
                },
                User);
        }

        private void RetireExistingOrganisationPublicSectorTypesForOrganisation(Organisation organisation)
        {
            var organisationPublicSectorTypes = dataRepository.GetAll<OrganisationPublicSectorType>()
                .Where(opst => opst.OrganisationId == organisation.OrganisationId)
                .ToList();

            foreach (OrganisationPublicSectorType organisationPublicSectorType in organisationPublicSectorTypes)
            {
                organisationPublicSectorType.Retired = VirtualDateTime.Now;
            }
        }

        private void AddNewOrganisationPublicSectorType(Organisation organisation, int publicSectorTypeId)
        {
            var newOrganisationPublicSectorType = new OrganisationPublicSectorType
            {
                OrganisationId = organisation.OrganisationId,
                PublicSectorTypeId = publicSectorTypeId,
                Source = "Service Desk"
            };

            dataRepository.Insert(newOrganisationPublicSectorType);

            organisation.LatestPublicSectorType = newOrganisationPublicSectorType;
        }
        
        private void UpdateAdminChangeSectorViewModelFromOrganisation(AdminChangeSectorViewModel viewModel, long organisationId)
        {
            viewModel.Organisation = dataRepository.Get<Organisation>(organisationId);

            viewModel.InactiveUserOrganisations = dataRepository.GetAll<InactiveUserOrganisation>()
                .Where(m => m.OrganisationId == organisationId).ToList();
        }
        
        private void ValidateAdminChangeSectorViewModel(AdminChangeSectorViewModel viewModel)
        {
            if (!viewModel.NewSector.HasValue)
            {
                viewModel.AddErrorFor(m => m.NewSector, "Please select a new sector");
            }

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);
        }
        
        private void ChangeSector(AdminChangeSectorViewModel viewModel, long organisationId)
        {
            var organisation = dataRepository.Get<Organisation>(organisationId);
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            SectorTypes previousSector = organisation.SectorType;
            SectorTypes newSector = viewModel.NewSector ?? SectorTypes.Unknown;

            // Update the sector
            organisation.SectorType = newSector;

            // Remove SIC codes when company changes between sectors
            organisation.OrganisationSicCodes.Clear();

            dataRepository.SaveChangesAsync().Wait();

            // Audit log
            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminChangedOrganisationSector,
                organisation,
                new {PreviousStatus = previousSector, NewStatus = newSector, viewModel.Reason},
                User);

        }

    }
}
