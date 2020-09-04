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
        
        [HttpPost("organisation/{id}/status/change")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public void ChangeSectorPost(long id, AdminChangeSectorViewModel viewModel)
        {
            //TODO: Cleverness
            // switch (viewModel.Action)
            // {
            //     case ChangeOrganisationSectorViewModelActions.OfferNewStatusAndReason:
            //         UpdateAdminChangeStatusViewModelFromOrganisation(viewModel, id);
            //         ValidateAdminChangeStatusViewModel(viewModel);
            //         if (viewModel.HasAnyErrors())
            //         {
            //             return View("ChangeStatus", viewModel);
            //         }
            //
            //         return View("ConfirmStatusChange", viewModel);
            //
            //     case ChangeOrganisationStatusViewModelActions.ConfirmStatusChange:
            //         ChangeStatus(viewModel, id);
            //         return RedirectToAction("ViewStatusHistory", "AdminOrganisationStatus", new {id});
            //     default:
            //         throw new ArgumentException("Unknown action in AdminOrganisationStatusController.ChangeStatusPost");
            // }
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

    }
}
