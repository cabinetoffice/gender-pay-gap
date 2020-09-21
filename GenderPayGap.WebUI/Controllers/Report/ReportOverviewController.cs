using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Report;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Views.Components.TaskList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Report
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("account/organisations")]
    public class ReportOverviewController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly DraftReturnService draftReturnService;

        public ReportOverviewController(
            IDataRepository dataRepository,
            DraftReturnService draftReturnService)
        {
            this.dataRepository = dataRepository;
            this.draftReturnService = draftReturnService;
        }


        [HttpGet("{encryptedOrganisationId}/reporting-year-{reportingYear}/report")]
        public IActionResult ReportOverview(string encryptedOrganisationId, int reportingYear)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewReportingJourney);

            long organisationId = ControllerHelper.DecryptOrganisationIdOrThrow404(encryptedOrganisationId);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRange(reportingYear);

            var viewModel = new ReportOverviewViewModel();
            PopulateViewModelWithOrganisationAndReportingYear(viewModel, organisationId, reportingYear);
            SetReportStatuses(viewModel, organisationId, reportingYear);

            return View("ReportOverview", viewModel);
        }

        private void PopulateViewModelWithOrganisationAndReportingYear(ReportOverviewViewModel viewModel, long organisationId, int reportingYear)
        {
            viewModel.Organisation = dataRepository.Get<Organisation>(organisationId);
            viewModel.ReportingYear = reportingYear;
        }

        private void SetReportStatuses(ReportOverviewViewModel viewModel, long organisationId, int reportingYear)
        {
            DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
            if (draftReturn != null)
            {
                SetReportStatusesFromDraftReturn(viewModel, draftReturn);
                return;
            }

            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            Return submittedReturn = organisation.GetReturn(reportingYear);
            if (submittedReturn != null)
            {
                SetReportStatusesFromSubmittedReturn(viewModel, submittedReturn);
                return;
            }

            SetInitialReportStatuses(viewModel);
        }

        private void SetReportStatusesFromDraftReturn(ReportOverviewViewModel viewModel, DraftReturn draftReturn)
        {
            viewModel.HourlyPayStatus = GetStatusFromNullableValues(
                draftReturn.DiffMeanHourlyPayPercent,
                draftReturn.DiffMedianHourlyPercent);

            if (draftReturn.MaleMedianBonusPayPercent == 0 &&
                draftReturn.FemaleMedianBonusPayPercent == 0)
            {
                // If the number of men & women that receive bonuses is BOTH 0, then:
                // - these two fields are complete
                // - we don't need them to fill in the other two fields
                // so, the overall bonus task is complete
                viewModel.BonusPayStatus = TaskListStatus.Completed;
            }
            else
            {
                viewModel.BonusPayStatus = GetStatusFromNullableValues(
                    draftReturn.FemaleMedianBonusPayPercent,
                    draftReturn.MaleMedianBonusPayPercent,
                    draftReturn.DiffMeanBonusPercent,
                    draftReturn.DiffMedianBonusPercent);
            }

            viewModel.EmployessByPayQuartileStatus = GetStatusFromNullableValues(
                draftReturn.FemaleUpperQuartilePayBand,
                draftReturn.MaleUpperQuartilePayBand,
                draftReturn.FemaleUpperPayBand,
                draftReturn.MaleUpperPayBand,
                draftReturn.FemaleMiddlePayBand,
                draftReturn.MaleMiddlePayBand,
                draftReturn.FemaleLowerPayBand,
                draftReturn.MaleLowerPayBand);

            viewModel.PersonResponsibleStatus = GetStatusFromStrings(
                draftReturn.FirstName,
                draftReturn.LastName,
                draftReturn.JobTitle);

            viewModel.OrganisationSizeStatus = draftReturn.OrganisationSize.HasValue
                ? TaskListStatus.Completed
                : TaskListStatus.NotStarted;

            viewModel.LinkStatus = GetStatusFromStrings(draftReturn.CompanyLinkToGPGInfo);

            viewModel.ReviewAndSubmitStatus = CalculateReviewStatus(viewModel);
        }

        private void SetReportStatusesFromSubmittedReturn(ReportOverviewViewModel viewModel, Return submittedReturn)
        {
            viewModel.HourlyPayStatus = TaskListStatus.Completed;
            viewModel.BonusPayStatus = TaskListStatus.Completed;
            viewModel.EmployessByPayQuartileStatus = TaskListStatus.Completed;
            viewModel.PersonResponsibleStatus = TaskListStatus.Completed;
            viewModel.OrganisationSizeStatus = TaskListStatus.Completed;
            viewModel.LinkStatus = TaskListStatus.Completed;
            viewModel.ReviewAndSubmitStatus = TaskListStatus.Completed;
        }

        private void SetInitialReportStatuses(ReportOverviewViewModel viewModel)
        {
            viewModel.HourlyPayStatus = TaskListStatus.NotStarted;
            viewModel.BonusPayStatus = TaskListStatus.NotStarted;
            viewModel.EmployessByPayQuartileStatus = TaskListStatus.NotStarted;
            viewModel.PersonResponsibleStatus = TaskListStatus.NotStarted;
            viewModel.OrganisationSizeStatus = TaskListStatus.NotStarted;
            viewModel.LinkStatus = TaskListStatus.NotStarted;
            viewModel.ReviewAndSubmitStatus = TaskListStatus.CannotStartYet;
        }

        private TaskListStatus GetStatusFromNullableValues(params decimal?[] nullableValues)
        {
            if (nullableValues.All(v => v.HasValue))
            {
                return TaskListStatus.Completed;
            }
            else if (nullableValues.Any(v => v.HasValue))
            {
                return TaskListStatus.InProgress;
            }
            else
            {
                return TaskListStatus.NotStarted;
            }
        }

        private TaskListStatus GetStatusFromStrings(params string[] stringValues)
        {
            if (stringValues.All(v => !string.IsNullOrWhiteSpace(v)))
            {
                return TaskListStatus.Completed;
            }
            else if (stringValues.Any(v => !string.IsNullOrWhiteSpace(v)))
            {
                return TaskListStatus.InProgress;
            }
            else
            {
                return TaskListStatus.NotStarted;
            }
        }

        private TaskListStatus CalculateReviewStatus(ReportOverviewViewModel viewModel)
        {
            if (viewModel.HourlyPayStatus == TaskListStatus.Completed
                && viewModel.BonusPayStatus == TaskListStatus.Completed
                && viewModel.EmployessByPayQuartileStatus == TaskListStatus.Completed
                && viewModel.PersonResponsibleStatus == TaskListStatus.Completed
                && viewModel.OrganisationSizeStatus == TaskListStatus.Completed
                && viewModel.LinkStatus == TaskListStatus.Completed)
            {
                return TaskListStatus.NotStarted;
            }
            else
            {
                return TaskListStatus.CannotStartYet;
            }
        }

    }
}
