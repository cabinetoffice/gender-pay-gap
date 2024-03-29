﻿using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes.Presentation;
using GenderPayGap.WebUI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public class ViewReportsController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly ICompareViewService compareViewService;

        public ViewReportsController(
            IDataRepository dataRepository,
            ICompareViewService compareViewService)
        {
            this.dataRepository = dataRepository;
            this.compareViewService = compareViewService;
        }

        [HttpGet("employers/{organisationId}")]
        public IActionResult Employer(long organisationId)
        {
            compareViewService.LoadComparedEmployersFromCookie();
            
            Organisation organisation = ControllerHelper.LoadOrganisationOrThrow404(organisationId, dataRepository);
            ControllerHelper.Throw404IfOrganisationIsNotSearchable(organisation);

            return View("Employer", organisation);
        }

    }
}
