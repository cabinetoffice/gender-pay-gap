using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public class ViewReportsController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly ComparisonBasketService comparisonBasketService;

        public ViewReportsController(
            IDataRepository dataRepository,
            ComparisonBasketService comparisonBasketService)
        {
            this.dataRepository = dataRepository;
            this.comparisonBasketService = comparisonBasketService;
        }

        [HttpGet("employers/{organisationId}")]
        public IActionResult Employer(long organisationId)
        {
            comparisonBasketService.LoadComparedEmployersFromCookie();
            comparisonBasketService.SaveComparedEmployersToCookieIfAnyAreObfuscated();
            
            Organisation organisation = ControllerHelper.LoadOrganisationOrThrow404(organisationId, dataRepository);
            ControllerHelper.Throw404IfOrganisationIsNotSearchable(organisation);

            return View("Employer", organisation);
        }

        [HttpGet("employers/{organisationId}/reporting-year-{reportingYear}")]
        public IActionResult ReportForYear(long organisationId, int reportingYear)
        {
            comparisonBasketService.LoadComparedEmployersFromCookie();
            comparisonBasketService.SaveComparedEmployersToCookieIfAnyAreObfuscated();
            
            Organisation organisation = ControllerHelper.LoadOrganisationOrThrow404(organisationId, dataRepository);
            ControllerHelper.Throw404IfOrganisationIsNotSearchable(organisation);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear, organisationId, dataRepository);
            
            Return returnForYear = ControllerHelper.LoadReturnForYearOrThrow404(organisation, reportingYear);

            return View("ReportForYear", returnForYear);
        }

    }
}
