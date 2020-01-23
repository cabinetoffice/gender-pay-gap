using System.Collections.Generic;
using System.Threading.Tasks;
using GenderPayGap.WebUI.Models.Shared;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GenderPayGap.WebUI.Classes.TagHelpers
{
    [HtmlTargetElement("Pagination", TagStructure = TagStructure.NormalOrSelfClosing)]
    [RestrictChildren("Next", "Back")]
    public class PaginationTagHelper : TagHelper
    {

        private readonly IHtmlHelper _htmlHelper;

        public PaginationTagHelper(IHtmlHelper htmlHelper)
        {
            _htmlHelper = htmlHelper;
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            //Create a context for the accordion
            var accordionContext = new PaginationContext();
            context.Items.Add(typeof(PaginationContext), accordionContext);

            //Execute the sections to populate 
            await output.GetChildContentAsync();

            (_htmlHelper as IViewContextAware).Contextualize(ViewContext);

            IHtmlContent content = await _htmlHelper.PartialModelAsync(new Pagination(accordionContext.Pages.ToArray()));

            output.Content.SetHtmlContent(content);
            output.TagName = null;
        }

    }

    [HtmlTargetElement("Back", ParentTag = "Pagination", TagStructure = TagStructure.WithoutEndTag)]
    public class PaginationBackTagHelper : TagHelper
    {

        private readonly IUrlHelper _urlHelper;

        public PaginationBackTagHelper(IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public string Title { get; set; }
        public string Label { get; set; }

        public string Action { get; set; }
        public string Controller { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            TagHelperContent htmlContent = await output.GetChildContentAsync();
            var accordionContext = (PaginationContext) context.Items[typeof(PaginationContext)];

            if (string.IsNullOrWhiteSpace(Controller))
            {
                Controller = ViewContext?.RouteData?.Values["Controller"]?.ToString();
            }

            accordionContext.Pages.Add(new PaginationLink(_urlHelper.Action(Action, Controller), Title, Label, true));

            output.SuppressOutput();
        }

    }

    [HtmlTargetElement("Next", ParentTag = "Pagination", TagStructure = TagStructure.WithoutEndTag)]
    public class PaginationNextTagHelper : TagHelper
    {

        private readonly IUrlHelper _urlHelper;

        public PaginationNextTagHelper(IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public string Title { get; set; }
        public string Label { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            TagHelperContent htmlContent = await output.GetChildContentAsync();
            var accordionContext = (PaginationContext) context.Items[typeof(PaginationContext)];
            if (string.IsNullOrWhiteSpace(Controller))
            {
                Controller = ViewContext?.RouteData?.Values["Controller"]?.ToString();
            }

            accordionContext.Pages.Add(new PaginationLink(_urlHelper.Action(Action, Controller), Title, Label));
            output.SuppressOutput();
        }

    }


    public class PaginationContext
    {

        public List<PaginationLink> Pages = new List<PaginationLink>();

    }
}
