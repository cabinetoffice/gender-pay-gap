using GenderPayGap.Core;
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
    public class ReportConfirmationController : Controller
    {

        private readonly IDataRepository dataRepository;

        public ReportConfirmationController(
            IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }
        
        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report/confirmation")]
        public IActionResult ReportConfirmation(string encryptedOrganisationId, int reportingYear, string confirmationId)
        {
            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear, organisationId, dataRepository);

            Return foundReturn = LoadReturnFromOrganisationIdReportingYearAndConfirmationId(organisationId, reportingYear, confirmationId);

            return View("ReportConfirmation", foundReturn);
        }

        [HttpPost("/submission-complete")]
        [ValidateAntiForgeryToken]
        public IActionResult ReportCompleteFinishAndSignOut()
        {
            string nextPageUrl =
                // Take the user to the "done" URL (the gov.uk survey page)
                Global.DoneUrl
                ?? // Or, if we don't have a "done URL", take them to the homepage
                Url.Action("Index", "Homepage", null, "https");

            // Global.Done url is not local
            //disable:DoNotUseRedirectWithReturnUrls
            IActionResult suggestedResult = Redirect(nextPageUrl);

            return LoginHelper.Logout(HttpContext, suggestedResult);
        }
        
        private Return LoadReturnFromOrganisationIdReportingYearAndConfirmationId(
            long organisationId,
            int reportingYear,
            string confirmationId)
        {
            long returnId = Encryption.DecryptId(confirmationId);

            var foundReturn = dataRepository.Get<Return>(returnId);

            if (foundReturn == null ||
                foundReturn.OrganisationId != organisationId ||
                foundReturn.AccountingDate.Year != reportingYear)
            {
                throw new PageNotFoundException();
            }

            return foundReturn;
        }

    }
}
