using System;
using System.Collections.Generic;
using GovUkDesignSystem.GovUkDesignSystemComponents.SubComponents;

namespace GenderPayGap.WebUI.Views.Components.StepByStep
{

    public class StepViewModel
    {

        /// <summary>
        ///     The title of the step.
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        ///     HTML / Text to use for the description of the step above the task links.
        /// </summary>
        public StepDescription Description { get; set; }

        /// <summary>
        ///     The step type to display in the step-by-step sidebar.
        /// </summary>
        public StepType StepType { get; set; }

        /// <summary>
        ///     The step number to display in the step-by-step sidebar. If the StepType is AND or OR then this should be null.
        /// </summary>
        public int? StepNumber { get; set; }

        /// <summary>
        ///     Check for current step. This is used to add class names to the current step in the view.
        /// </summary> 
        public bool IsCurrentStep { get; set; } = false;

        /// <summary>
        ///     A list of tasks on this step.
        /// </summary>
        public List<StepTask> StepTasks { get; set; } 
        
        /// <summary>
        ///    This value is used to calculate the scroll position in the step-by-step navigation.
        /// </summary>
        public int Position { get; set; }

    }

    public enum StepType
    {

        Number = 0,
        And = 1,
        Or = 2

    }

    public class StepTask
    {
        public string TaskText { get; set; }

        public string TaskUrl { get; set; }

        public bool IsCurrentTask { get; set; } = false;

        public bool OpenInNewTab { get; set; } = false;

    }
    
    public class StepDescription : IHtmlText
    {
        /// <summary>
        ///     HTML to use for the description of the step.
        ///     <br/>If `html` is provided, the `text` argument will be ignored.
        /// </summary>
        public Func<object, object> Html { get; set; }

        /// <summary>
        ///     Text to use for the description of the step.
        ///     <br/>If `html` is provided, the `text` argument will be ignored.
        /// </summary>
        public string Text { get; set; }
    }

}
