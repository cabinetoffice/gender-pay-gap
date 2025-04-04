﻿using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.AddOrganisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.AddOrganisation
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("add-employer")]
    public class AddOrganisationManualSicCodesController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AddOrganisationManualSicCodesController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }


        [HttpGet("manual/sic-codes")]
        public IActionResult ManualSicCodes(AddOrganisationManualViewModel viewModel)
        {
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);

            if (viewModel.Validate == true)
            {
                return ProceedToNextPage(viewModel);
            }

            viewModel.SicSections = GetSicSectionsExceptPublicSector();
            if (viewModel.SicCodes == null)
            {
                viewModel.SicCodes = new List<int>();
            }

            return View("ManualSicCodes", viewModel);
        }

        private List<SicSection> GetSicSectionsExceptPublicSector()
        {
            return dataRepository.GetAll<SicSection>()
                .Where(ss => ss.SicSectionId != "X") // Remove "Public Sector" SIC section
                .ToList();
        }

        private IActionResult ProceedToNextPage(AddOrganisationManualViewModel viewModel)
        {
            viewModel.Validate = null; // Required to prevent the next page immediately trying to validate the (empty) address
            viewModel.Editing = null; // To make the url look a bit nicer (the Review page implies we're editing so there's no need for "Editing" in the url)
            return RedirectToAction("ManualConfirmGet", "AddOrganisationManualConfirm", viewModel);
        }

    }
}
