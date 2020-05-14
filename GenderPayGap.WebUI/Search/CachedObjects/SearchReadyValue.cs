using System;
using System.Collections.Generic;
using System.Linq;

namespace GenderPayGap.WebUI.Search.CachedObjects
{
    public class SearchReadyValue
    {

        public string OriginalValue { get; }

        public string LowercaseValue { get; }

        public List<string> LowercaseWords { get; }

        public string Acronym { get; }


        public SearchReadyValue(string originalValue)
        {
            OriginalValue = originalValue;
            LowercaseValue = originalValue.ToLower();
            LowercaseWords = WordSplittingRegex.SplitValueIntoWords(originalValue);
            Acronym = MakeAcronymFromWords(LowercaseWords);
        }

        private string MakeAcronymFromWords(List<string> words)
        {
            var firstLetterOfEachWord = words.Select(word => word.Substring(0, 1));
            string acronym = string.Join("", firstLetterOfEachWord);
            return acronym;
        }


        public bool Matches(List<string> searchTerms)
        {
            foreach (string searchTerm in searchTerms)
            {
                if (!(SearchTermIsPartOfAcronym(searchTerm) || SearchTermIsPartOfAWord(searchTerm)))
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

        private bool SearchTermIsPartOfAWord(string searchTerm)
        {
            foreach (string word in LowercaseWords)
            {
                if (word.Contains(searchTerm))
                {
                    return true;
                }
            }

            return false;
        }

    }
}
