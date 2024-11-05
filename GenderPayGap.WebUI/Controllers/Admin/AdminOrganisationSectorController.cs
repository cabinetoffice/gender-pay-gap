using System;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = LoginRoles.GpgAdmin + "," + LoginRoles.GpgAdminReadOnly)]
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
            var viewModel = new AdminSectorHistoryViewModel();
            
            var organisation = dataRepository.Get<Organisation>(id);
            var sectorHistory = dataRepository.GetAll<AuditLog>()
                .Where(al => al.Action == AuditedAction.AdminChangedOrganisationSector)
                .Where(al => al.Organisation.OrganisationId == id)
                .OrderByDescending(al => al.CreatedDate)
                .ToList();

            viewModel.Organisation = organisation;
            viewModel.SectorHistory = sectorHistory;

            return View("ViewOrganisationSector", viewModel);
        }
        
        [HttpGet("organisation/{id}/sector/change")]
        [Authorize(Roles = LoginRoles.GpgAdmin)]
        public IActionResult ChangeSectorGet(long id)
        {
            var viewModel = new AdminChangeSectorViewModel();

            UpdateAdminChangeSectorViewModelFromOrganisation(viewModel, id);
            viewModel.NewSector = viewModel.Organisation.SectorType == SectorTypes.Private ? NewSectorTypes.Private : NewSectorTypes.Public;

            return View("ChangeSector", viewModel);
        }
        
        [HttpPost("organisation/{id}/sector/change")]
        [Authorize(Roles = LoginRoles.GpgAdmin)]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeSectorPost(long id, AdminChangeSectorViewModel viewModel)
        {
            switch (viewModel.Action)
            {
                case ChangeOrganisationSectorViewModelActions.OfferNewSectorAndReason:
                    return OfferNewSectorAndReason(id, viewModel);
            
                case ChangeOrganisationSectorViewModelActions.ConfirmSectorChange:
                    return ConfirmSectorChange(id, viewModel);
                default:
                    throw new ArgumentException("Unknown action in AdminOrganisationSectorController.ChangeSectorPost");
            }
        }

        private IActionResult OfferNewSectorAndReason(long id, AdminChangeSectorViewModel viewModel)
        {
            UpdateAdminChangeSectorViewModelFromOrganisation(viewModel, id);
                    
            // Check if new sector is same as original organisation sector
            var newSector = viewModel.NewSector == NewSectorTypes.Private ? SectorTypes.Private : SectorTypes.Public;
            if (newSector == viewModel.Organisation.SectorType)
            {
                ModelState.AddModelError(nameof(viewModel.NewSector), "The organisation is already assigned to this sector.");
            }
                    
            if (!ModelState.IsValid)
            {
                return View("ChangeSector", viewModel);
            }
            
            return View("ConfirmSectorChange", viewModel);
        }

        private IActionResult ConfirmSectorChange(long id, AdminChangeSectorViewModel viewModel)
        {
            ChangeSector(viewModel, id);
            return RedirectToAction("ViewSectorHistory", "AdminOrganisationSector", new {id});
        }

        [HttpGet("organisation/{id}/change-public-sector-classification")]
        [Authorize(Roles = LoginRoles.GpgAdmin)]
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
        [Authorize(Roles = LoginRoles.GpgAdmin)]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePublicSectorClassificationPost(long id, AdminChangePublicSectorClassificationViewModel viewModel)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);
            viewModel.OrganisationId = organisation.OrganisationId;
            viewModel.OrganisationName = organisation.OrganisationName;
            viewModel.PublicSectorTypes = dataRepository.GetAll<PublicSectorType>().ToList();

            if (!ModelState.IsValid)
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

            AddNewOrganisationPublicSectorType(organisation, viewModel.SelectedPublicSectorTypeId.Value);

            dataRepository.SaveChanges();

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
        }
        
        private void ChangeSector(AdminChangeSectorViewModel viewModel, long organisationId)
        {
            var organisation = dataRepository.Get<Organisation>(organisationId);

            SectorTypes previousSector = organisation.SectorType;
            SectorTypes newSector = viewModel.NewSector.Value == NewSectorTypes.Private ? SectorTypes.Private : SectorTypes.Public;

            // Update the sector
            organisation.SectorType = newSector;

            // Remove SIC codes when company changes between sectors
            organisation.OrganisationSicCodes.Clear();
            
            // Change snapshot date for all organisation scopes to match new sector
            foreach (OrganisationScope scope in organisation.OrganisationScopes)
            {
                scope.SnapshotDate = organisation.SectorType.GetAccountingStartDate(scope.SnapshotDate.Year);
            }
            
            // Change accounting date for all returns to match new sector
            foreach (Return returnItem in organisation.Returns)
            {
                returnItem.AccountingDate = organisation.SectorType.GetAccountingStartDate(returnItem.AccountingDate.Year);
            }

            dataRepository.SaveChanges();

            // Audit log
            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminChangedOrganisationSector,
                organisation,
                new AdminChangeSectorAuditLogDetails {OldSector = previousSector, NewSector = newSector, Reason = viewModel.Reason},
                User);

        }

    }
}
