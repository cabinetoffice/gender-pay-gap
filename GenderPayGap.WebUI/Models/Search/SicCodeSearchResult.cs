using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;

namespace GenderPayGap.WebUI.Models.Search
{
    public class SicCodeSearchResult
    {

        public string SicCodeId { get; set; }

        public string SicCodeDescription { get; set; }

        public string SicCodeMatchingSynonyms { get; set; }

        public override string ToString()
        {
            return $"{SicCodeDescription} ({SicCodeId})";
        }

        public static List<SicCodeSearchResult> ConvertToScreenReadableListOfSuggestions(string searchText,
            IEnumerable<KeyValuePair<string, SicCodeSearchModel>> listOfSicCodeSuggestions)
        {
            var resultingListOfSicCodeSuggestions = new List<SicCodeSearchResult>();

            foreach (KeyValuePair<string, SicCodeSearchModel> sicCodeSuggestion in listOfSicCodeSuggestions)
            {
                if (resultingListOfSicCodeSuggestions.Any(x => x.SicCodeDescription == sicCodeSuggestion.Value.SicCodeDescription))
                {
                    continue;
                }

                string concatenatedMatchingSynonyms = SearchKeyWasNotPartOfTheSicCodeDescription(
                    sicCodeSuggestion.Key,
                    sicCodeSuggestion.Value.SicCodeDescription);

                resultingListOfSicCodeSuggestions.Add(
                    new SicCodeSearchResult {
                        SicCodeId = sicCodeSuggestion.Value.SicCodeId,
                        SicCodeDescription = sicCodeSuggestion.Value.SicCodeDescription,
                        SicCodeMatchingSynonyms = concatenatedMatchingSynonyms
                    });
            }

            return resultingListOfSicCodeSuggestions;
        }

        /// <summary>
        ///     Suggestions are presented in the form of "description (id)", however, they might have been selected because of the
        ///     synonym
        /// </summary>
        /// <param name="searchKey"></param>
        /// <param name="sicCodeDescription"></param>
        private static string SearchKeyWasNotPartOfTheSicCodeDescription(string searchKey, string sicCodeDescription)
        {
            return string.IsNullOrEmpty(searchKey)
                   || string.IsNullOrEmpty(sicCodeDescription)
                   || sicCodeDescription.ContainsI(searchKey) // key 'nurses' - description 'Other human health activities'
                ? string.Empty
                : searchKey;

            /*
            var result = string.Empty;

            if (string.IsNullOrEmpty(searchKey) || string.IsNullOrEmpty(sicCodeDescription))
                return result;

            if (sicCodeDescription.Contains(searchKey))
                return result;

            return searchKey; */
        }

    }
}
