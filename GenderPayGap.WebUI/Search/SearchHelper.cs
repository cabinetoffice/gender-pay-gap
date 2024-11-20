using System.Collections.Generic;
using System.Linq;
using GenderPayGap.WebUI.Search.CachedObjects;

namespace GenderPayGap.WebUI.Search
{
    public static class SearchHelper
    {

        public static List<string> ExtractSearchTermsFromQuery(string query, bool queryContainsPunctuation)
        {
            return WordSplittingRegex.SplitValueIntoWords(query, queryContainsPunctuation);
        }

        public static bool CurrentOrPreviousOrganisationNameMatchesSearchTerms(
            SearchCachedOrganisation organisation,
            List<string> searchTerms,
            bool queryContainsPunctuation)
        {
            return AnyNameMatchesSearchTerms(organisation.OrganisationNames, searchTerms, queryContainsPunctuation);
        }

        public static bool AnyNameMatchesSearchTerms(
            List<SearchReadyValue> names,
            List<string> searchTerms,
            bool queryContainsPunctuation)
        {
            return names.Any(name => name.Matches(searchTerms, queryContainsPunctuation));
        }

    }

}
