using System.Linq;
using GenderPayGap.WebUI.Classes.Attributes;
using Microsoft.AspNetCore.Html;

namespace GenderPayGap.WebUI.Models.Shared
{

    [Partial("Patterns/Accordion")]
    public class Accordion
    {

        public Accordion(params AccordionSection[] sections)
        {
            Sections = sections.Where(x => x != null).ToArray();
        }

        public AccordionSection[] Sections { get; set; }

    }

    public class AccordionSection
    {

        public AccordionSection(string title, string desc, string contentPartial, string id)
        {
            Title = title;
            Description = desc;
            Partial = contentPartial;
            Id = id;
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Partial { get; set; }
        public IHtmlContent HtmlContent { get; set; }

    }

}
