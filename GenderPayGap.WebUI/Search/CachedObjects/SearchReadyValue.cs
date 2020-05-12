using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GenderPayGap.WebUI.Search.CachedObjects
{
    public class SearchReadyValue
    {

        public string OriginalValue { get; }

        public List<string> LowercaseWords { get; }

        public string Acronym { get; }


        public SearchReadyValue(string originalValue)
        {
            OriginalValue = originalValue;
            LowercaseWords = SplitValueIntoWords(originalValue);
            Acronym = MakeAcronymFromWords(LowercaseWords);
        }

        private static List<string> SplitValueIntoWords(string originalValue)
        {
            Match match = WordSplittingRegex.Regex.Match(originalValue);
            if (match.Success)
            {
                return match.Groups
                    .Select(g => g.Value.ToLower())
                    .ToList();
            }
            return new List<string>();
        }

        private string MakeAcronymFromWords(List<string> words)
        {
            var firstLetterOfEachWord = words.Select(word => word.Substring(0, 1));
            string acronym = string.Join("", firstLetterOfEachWord);
            return acronym;
        }

    }
}
