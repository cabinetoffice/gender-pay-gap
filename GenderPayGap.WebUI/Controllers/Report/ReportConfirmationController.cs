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
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            Return foundReturn = LoadReturnFromOrganisationIdReportingYearAndConfirmationId(organisationId, reportingYear, confirmationId);

            return View("ReportConfirmation", foundReturn);
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

    }
}
