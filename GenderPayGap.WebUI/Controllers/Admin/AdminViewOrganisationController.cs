using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Authorize(Roles = LoginRoles.GpgAdmin + "," + LoginRoles.GpgAdminReadOnly)]
    [Route("admin")]
    public class AdminViewOrganisationController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AdminViewOrganisationController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet("organisation/{id}")]
        public IActionResult ViewOrganisation(long id)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);

            List<int> reportingYears = ReportingYearsHelper.GetReportingYears(organisation.SectorType).OrderByDescending(year => year).ToList();

            var viewModel = new AdminViewOrganisationViewModel
            {
                Organisation = organisation,
                ReportingYears = reportingYears
            };

            return View("../Admin/ViewOrganisation", viewModel);
        }

    }
}
