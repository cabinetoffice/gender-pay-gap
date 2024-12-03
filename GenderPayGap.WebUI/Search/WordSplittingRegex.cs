using System.Text.RegularExpressions;

namespace GenderPayGap.WebUI.Search
{
    public static class WordSplittingRegex
    {
        // The "language=regexp" comment below tells Resharper to do regex syntax highlighting
        //language=regexp
        private const string RegexPattern = "[a-zA-Z0-9'-]+";
        private static readonly string[] PunctuationCharacters = { "'", "-" };

        private static readonly Regex Regex = new Regex(RegexPattern, RegexOptions.Compiled);


        public static List<string> SplitValueIntoWords(string originalValue, bool retainPunctuation = false)
        {
            if (originalValue != null)
            {
                MatchCollection matches = Regex.Matches(originalValue);

                var words = matches
                    .Select(m => m.Value.ToLower());

                if (!retainPunctuation)
                {
                    words = RemovePunctuationFromWords(words);
                }

                return words.ToList();
            }

            return new List<string>();
        }

        public static IEnumerable<string> RemovePunctuationFromWords(IEnumerable<string> words)
        {
            return words
                .Select(RemovePunctuation)
                .Where(word => word != "");
        }

        public static string RemovePunctuation(string input)
        {
            string output = input;

            foreach (string punctuationCharacter in PunctuationCharacters)
            {
                output = output.Replace(punctuationCharacter, "");
            }

            return output;
        }

        public static bool ContainsPunctuationCharacters(string stringToTest)
        {
            return PunctuationCharacters.Any(punc => stringToTest.Contains(punc));
        }

    }
}
