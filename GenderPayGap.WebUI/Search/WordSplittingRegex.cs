using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GenderPayGap.WebUI.Search
{
    public static class WordSplittingRegex
    {
        private static Regex regex;

        public static Regex Regex
        {
            get
            {
                if (regex != null)
                {
                    return regex;
                }

                // The "language=regexp" comment below tells Resharper to do regex syntax highlighting
                //language=regexp
                var regexPattern = "[a-zA-Z0-9'-]+";

                regex = new Regex(regexPattern, RegexOptions.Compiled);
                return regex;
            }
        }

        public static List<string> SplitValueIntoWords(string originalValue)
        {
            if (originalValue != null)
            {
                MatchCollection matches = Regex.Matches(originalValue);

                return matches
                    .Select(m => m.Value.ToLower())
                    .Select(RemovePunctuation)
                    .Where(word => word != "")
                    .ToList();
            }

            return new List<string>();
        }

        private static string RemovePunctuation(string input)
        {
            return input
                .Replace("'", "")
                .Replace("-", "");
        }

    }
}
