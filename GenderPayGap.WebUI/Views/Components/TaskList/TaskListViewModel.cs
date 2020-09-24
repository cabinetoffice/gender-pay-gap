using System;
using System.Collections.Generic;

namespace GenderPayGap.WebUI.Views.Components.TaskList
{
    public class TaskListViewModel
    {
        public List<TaskListSectionViewModel> Sections { get; set; }
    }

    public class TaskListSectionViewModel
    {
        public string SectionName { get; set; }
        public List<TaskListItemViewModel> Items { get; set; }
    }

    public class TaskListItemViewModel
    {
        public string Title { get; set; }
        public Func<object, object> BodyHtml { get; set; }
        public string Href { get; set; }
        public TaskListStatus Status { get; set; }
    }

    public enum TaskListStatus
    {
        Completed,
        InProgress,
        NotStarted,
        CannotStartYet
    }
}
