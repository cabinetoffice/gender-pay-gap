using System.Threading.Tasks;
using GenderPayGap.WebUI.Models.Shared;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GenderPayGap.WebUI.Classes.TagHelpers
{
    [HtmlTargetElement("Expand", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ExpandDetailsTagHelper : TagHelper
    {

        private readonly IHtmlHelper _htmlHelper;

        public ExpandDetailsTagHelper(IHtmlHelper htmlHelper)
        {
            _htmlHelper = htmlHelper;
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public string Id { get; set; }
        public string Summary { get; set; }
        public bool IsPanel { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            (_htmlHelper as IViewContextAware).Contextualize(ViewContext);

            TagHelperContent htmlContent = await output.GetChildContentAsync();
            IHtmlContent content = await _htmlHelper.PartialModelAsync(
                new Details {Id = Id, LinkText = Summary, IsPanel = IsPanel, HtmlContent = htmlContent});

            output.Content.SetHtmlContent(content);
            output.TagName = null;
        }

    }
}
