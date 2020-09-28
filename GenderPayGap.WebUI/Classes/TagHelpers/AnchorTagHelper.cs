using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using GenderPayGap.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GenderPayGap.WebUI.Classes.TagHelpers
{
    [HtmlTargetElement("a", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class AnchorTagHelper : TagHelper
    {

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            //Load the relationship attribute
            var rels = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            if (output.Attributes.ContainsName("rel"))
            {
                rels.AddRange(output.Attributes["rel"].Value.ToStringOrEmpty().SplitI(";, "));
            }

            //Save the relationship attribute changes
            if (rels.Any())
            {
                output.Attributes.SetAttribute("rel", rels.ToDelimitedString(" "));
            }
            else if (output.Attributes.ContainsName("rel"))
            {
                output.Attributes.RemoveAll("rel");
            }
        }

    }
}
