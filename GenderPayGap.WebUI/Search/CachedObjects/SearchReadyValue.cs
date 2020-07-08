using System.Collections.Generic;
using System.Linq;

namespace GenderPayGap.WebUI.Search.CachedObjects
{
    public class SearchReadyValue
    {

        public string OriginalValue { get; }

        public string LowercaseValue { get; }

        public List<string> LowercaseWords { get; }
        public List<string> LowercaseWordsWithPunctuation { get; }

        public string Acronym { get; }


        public SearchReadyValue(string originalValue)
        {
            OriginalValue = originalValue;
            LowercaseValue = originalValue.ToLower();
            LowercaseWordsWithPunctuation = WordSplittingRegex.SplitValueIntoWords(originalValue, retainPunctuation: true);
            LowercaseWords = WordSplittingRegex.RemovePunctuationFromWords(LowercaseWordsWithPunctuation).ToList();
            Acronym = MakeAcronymFromWords(LowercaseWords);
        }

        private string MakeAcronymFromWords(List<string> words)
        {
            var firstLetterOfEachWord = words.Select(word => word.Substring(0, 1));
            string acronym = string.Join("", firstLetterOfEachWord);
            return acronym;
        }


        public bool Matches(List<string> searchTerms, bool queryContainsPunctuation = false, bool fuzzyMatch = true)
        {
            foreach (string searchTerm in searchTerms)
            {
                if (!(SearchTermIsPartOfAcronym(searchTerm) || SearchTermIsPartOfAWord(searchTerm, queryContainsPunctuation, fuzzyMatch)))
                {
                    return false;
                }
            }

            return true;
        }

        private bool SearchTermIsPartOfAcronym(string searchTerm)
        {
            return Acronym.Contains(searchTerm);
        }

        private bool SearchTermIsPartOfAWord(string searchTerm, bool queryContainsPunctuation, bool fuzzyMatch = true)
        {
            List<string> words = queryContainsPunctuation ? LowercaseWordsWithPunctuation : LowercaseWords;
            foreach (string word in words)
            {
                if (word.Contains(searchTerm) || (fuzzyMatch && IsFuzzyMatch(word, searchTerm)))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsFuzzyMatch(string word, string searchTerm)
        {
            if (word.Length <= 2)
            {
                // For 1-letter and 2-letter words, we need an exact match (no spelling mistakes)
                return false;
            }
            else
            {
                // For words of more than 3 letters, allow "length/3" spelling mistakes

                int numberOfSpellingMistakes = FuzzySearch.GetDamerauLevenshteinDistance(word, searchTerm);
                int allowableSpellingMistakes = word.Length / 3;

                return numberOfSpellingMistakes <= allowableSpellingMistakes;
            }
        }

    }
}
