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


        public bool Matches(List<string> searchTerms, bool queryContainsPunctuation = false)
        {
            foreach (string searchTerm in searchTerms)
            {
                if (!(SearchTermIsPartOfAcronym(searchTerm) || SearchTermIsPartOfAWord(searchTerm, queryContainsPunctuation)))
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

        private bool SearchTermIsPartOfAWord(string searchTerm, bool queryContainsPunctuation)
        {
            List<string> words = queryContainsPunctuation ? LowercaseWordsWithPunctuation : LowercaseWords;
            foreach (string word in words)
            {
                if (word.Contains(searchTerm) 
                    || FuzzySearch.GetDamerauLevenshteinDistance(word, searchTerm) < 3
                )
                {
                    return true;
                }
            }

            return false;
        }

    }
}
