using GenderPayGap.WebUI.Views.Components.TaskList;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Report
{
    public class ReportOverviewViewModel
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Database.Organisation Organisation { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public int ReportingYear { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public bool DraftReturnExists { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public bool IsEditingSubmittedReturn { get; set; }

        public TaskListStatus HourlyPayStatus { get; set; }
        public TaskListStatus BonusPayStatus { get; set; }
        public TaskListStatus EmployessByPayQuartileStatus { get; set; }
        public TaskListStatus PersonResponsibleStatus { get; set; }
        public TaskListStatus OrganisationSizeStatus { get; set; }
        public TaskListStatus LinkStatus { get; set; }
        public TaskListStatus ReviewAndSubmitStatus { get; set; }

    }
}
