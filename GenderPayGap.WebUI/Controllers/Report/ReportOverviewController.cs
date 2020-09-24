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
            PopulateViewModel(viewModel, organisationId, reportingYear);
            SetReportStatuses(viewModel, organisationId, reportingYear);

            return View("ReportOverview", viewModel);
        }

        private void PopulateViewModel(ReportOverviewViewModel viewModel, long organisationId, int reportingYear)
        {
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);

            viewModel.Organisation = organisation;
            viewModel.ReportingYear = reportingYear;

            Return submittedReturn = organisation.GetReturn(reportingYear);
            bool isEditingSubmittedReturn = submittedReturn != null;
            viewModel.IsEditingSubmittedReturn = isEditingSubmittedReturn;
        }

        private void SetReportStatuses(ReportOverviewViewModel viewModel, long organisationId, int reportingYear)
        {
            DraftReturn draftReturn = draftReturnService.GetDraftReturn(organisationId, reportingYear);
            if (draftReturn != null)
            {
                viewModel.DraftReturnExists = true;
                SetReportStatusesFromDraftReturn(viewModel, draftReturn);
                return;
            }

            Return submittedReturn = viewModel.Organisation.GetReturn(reportingYear);
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
            
            // LinkStatus is the only status that isn't definitely Complete
            // All the other fields are non-nullable in the database
            viewModel.LinkStatus = GetStatusFromStrings(submittedReturn.CompanyLinkToGPGInfo);

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
                && viewModel.OrganisationSizeStatus == TaskListStatus.Completed)
                // The "website link" page is optional, so we don't check viewModel.LinkStatus
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
