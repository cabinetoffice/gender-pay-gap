using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
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

            int firstReportingYear = Global.FirstReportingYear;
            int currentReportingYear = SectorTypes.Public.GetAccountingStartDate().Year;
            int numberOfYears = currentReportingYear - firstReportingYear + 1;
            List<int> reportingYears = Enumerable.Range(firstReportingYear, numberOfYears).Reverse().ToList();

            var viewModel = new AdminViewOrganisationViewModel
            {
                Organisation = organisation,
                ReportingYears = reportingYears
            };

            return View("../Admin/ViewOrganisation", viewModel);
        }

    }
}
