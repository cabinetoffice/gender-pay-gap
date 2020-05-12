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

                // The "language=regexp" comments below tell Resharper to do regex syntax highlighting
                // "CG" means "character group" - i.e. [A-Z] in regex

                //language=regexp
                string wordCG = "[a-zA-Z0-9'-]";
                //language=regexp
                string nonWordCG = "[^a-zA-Z0-9'-]";

                //language=regexp
                string regexPattern =
                    "^" // Start of line
                    + "(?:" // Start of non-capturing group
                    +    $"{nonWordCG}*" // Start with non-word characters (0+ times - i.e. optional)
                    +    "(" // Capture this next bit - we only want to capture the word characters, not the non-word characters
                    +        $"{wordCG}+" // Some word characters (1+ i.e. there must be some word characters!)
                    +    ")" // End capture
                    + ")+"  // End of non-capturing group - repeat this 1+ times
                    + $"{nonWordCG}*" // End with non-word characters (0+ times - i.e. optional)
                    + "$"; // End of line

                regex = new Regex(regexPattern, RegexOptions.Compiled);
                return regex;
            }
        }

    }
}
