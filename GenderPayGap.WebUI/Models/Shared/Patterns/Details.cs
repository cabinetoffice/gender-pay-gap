using GenderPayGap.WebUI.Classes.Attributes;
using Microsoft.AspNetCore.Html;

namespace GenderPayGap.WebUI.Models.Shared
{

    [Partial("Patterns/Details")]
    public class Details
    {

        public string Id { get; set; }
        public bool Expanded { get; set; }
        public string LinkText { get; set; }
        public string SummaryText { get; set; }
        public string SummaryPartial { get; set; }

        /// <summary>
        ///     Whether to enclose in panel with left border
        /// </summary>
        public bool IsPanel { get; set; } = true;

        public IHtmlContent HtmlContent { get; set; }

    }

}
