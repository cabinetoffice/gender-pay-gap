using System.Collections.Generic;

namespace GenderPayGap.WebUI.Search
{
    public static class SearchHelper
    {

        public static List<string> ExtractSearchTermsFromQuery(string query, bool queryContainsPunctuation)
        {
            return WordSplittingRegex.SplitValueIntoWords(query, queryContainsPunctuation);
        }

    }
}
