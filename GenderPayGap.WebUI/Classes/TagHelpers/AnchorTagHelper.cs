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

            var isExternal = false;
            if (output.Attributes.ContainsName("href"))
            {
                string href = output.Attributes["href"].Value.ToStringOrEmpty().TrimI(" \\/.");
                if (!string.IsNullOrWhiteSpace(href) && !href.StartsWithI("mailto:", "javascript:", "ftp:", "file:", "#"))
                {
                    //Check if the link is external
                    isExternal = ViewContext.HttpContext.GetIsExternalUrl(href);

                    //If external then 
                    if (isExternal)
                    {
                        //Indicates that the referenced document is not part of the same site as the current document
                        rels.Add("external");

                        //Prevent google from following the link
                        rels.Add("nofollow");

                        //Never send the referrer or allow opening of tabs
                        rels.AddRange("noreferrer", "noopener");
                    }
                }
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

            //Make sure we open in new tab unless otherwise stated
            if (isExternal && !output.Attributes.ContainsName("target"))
            {
                output.Attributes.SetAttribute("target", "_blank");
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
