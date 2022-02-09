using System;
using System.Collections.Generic;
using GovUkDesignSystem.GovUkDesignSystemComponents.SubComponents;

namespace GovUkDesignSystem.GovUkDesignSystemComponents
{
    public class AccordionViewModel
    {

        /// <summary>
        ///     The accordion sections.
        /// </summary>
        public List<AccordionSectionViewModel> Sections;

    }

    public class AccordionSectionViewModel : IHtmlText
    {

        /// <summary>
        ///     The heading of the accordion section.
        /// </summary>
        public string Heading;

        /// <summary>
        ///     HTML to use for the content of the accordion section.
        ///     <br/>If `html` is provided, the `text` argument will be ignored.
        /// </summary>
        public Func<object, object> Html { get; set; }

        /// <summary>
        ///     Text to use for the content of the accordion section.
        ///     <br/>If `html` is provided, the `text` argument will be ignored.
        /// </summary>
        public string Text { get; }

    }
}
