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

            #region Google Analytics Tracking

            ///Change category andtrack category to data-track-category
            ReplaceTrack("category", "action", "label", "options");

            void ReplaceTrack(params string[] fieldNames)
            {
                Parallel.ForEach(
                    fieldNames,
                    fieldName => {
                        foreach (string fn in new[] {$"track-{fieldName}", fieldName})
                        {
                            int i = output.Attributes.IndexOfName(fn);
                            if (i > -1)
                            {
                                if (output.Attributes.ContainsName($"data-track-{fieldName}"))
                                {
                                    throw new ArgumentException($"data-track-{fieldName}", "Duplicate tracking field");
                                }

                                var newAttribute = new TagHelperAttribute(
                                    $"data-track-{fieldName}",
                                    output.Attributes[i].Value,
                                    output.Attributes[i].ValueStyle);
                                output.Attributes.RemoveAt(i);
                                output.Attributes.Insert(i, newAttribute);
                            }
                        }
                    });
            }

            //Check if link should be tracked
            //TODO check what these fields mean
            bool isTracked = output.Attributes.ContainsName("data-track-category")
                             || output.Attributes.ContainsName("data-track-action")
                             || output.Attributes.ContainsName("data-track-label")
                             || output.Attributes.ContainsName("data-track-options");

            //Add the track relationship
            if (isTracked)
            {
                rels.Add("track");
            }

            //Ensure we have at least a category when tracking
            if (!output.Attributes.ContainsName("data-track-category") && rels.Contains("track"))
            {
                throw new ArgumentNullException("data-track-category", "You must specify a category for tracking");
            }

            #endregion

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
