using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Report
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("account/organisations")]
    public class NewReportConfirmationController : Controller
    {

        private readonly IDataRepository dataRepository;

        public NewReportConfirmationController(
            IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }
        
        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/new-confirmation")]
        public IActionResult NewReportConfirmation(string encryptedOrganisationId, int reportingYear, string confirmationId)
        {
            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            Return foundReturn = LoadReturnFromOrganisationIdReportingYearAndConfirmationId(organisationId, reportingYear, confirmationId);

            return View("~/Views/ReportConfirmation/NewReportConfirmation.cshtml", foundReturn);
        }

        private Return LoadReturnFromOrganisationIdReportingYearAndConfirmationId(
            long organisationId,
            int reportingYear,
            string confirmationId)
        {
            string returnIdString = Encryption.DecryptQuerystring(confirmationId);
            long returnId = long.Parse(returnIdString);

            Return foundReturn = dataRepository.Get<Return>(returnId);

            if (foundReturn.OrganisationId != organisationId ||
                foundReturn.AccountingDate.Year != reportingYear)
            {
                throw new PageNotFoundException();
            }

            return foundReturn;
        }
        
        [HttpPost("/new-report-complete-finish-and-sign-out")]
        [ValidateAntiForgeryToken]
        public IActionResult NewReportCompleteFinishAndSignOut()
        {
            string nextPageUrl =
                // Take the user to the "done" URL (the gov.uk survey page)
                Global.DoneUrl
                ?? // Or, if we don't have a "done URL", take them to the homepage
                Url.Action("Index", "Viewing", null, "https");

            // Global.Done url is not local
            //disable:DoNotUseRedirectWithReturnUrls
            IActionResult suggestedResult = Redirect(nextPageUrl);

            return LoginHelper.Logout(HttpContext, suggestedResult);
        }

    }
}
