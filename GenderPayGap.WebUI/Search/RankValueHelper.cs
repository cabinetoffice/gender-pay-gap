using GenderPayGap.WebUI.Search.CachedObjects;

namespace GenderPayGap.WebUI.Search
{
    public class RankedName
    {
        public string Name { get; set; }
        public List<double> Ranks { get; set; }
    }
    
    public static class RankValueHelper
    {
        
        public static List<RankedName> GetRankedNames(List<SearchReadyValue> organisationNames,
            List<string> searchTerms,
            string query,
            bool queryContainsPunctuation)
        {
            var names = new List<RankedName>();
            for (var nameIndex = 0; nameIndex < organisationNames.Count; nameIndex++)
            {
                SearchReadyValue name = organisationNames[nameIndex];
                names.Add(CalculateRankForName(name, searchTerms, query, queryContainsPunctuation, nameIndex));
            }

            return names;
        }
        
        private static RankedName CalculateRankForName(SearchReadyValue name,
            List<string> searchTerms,
            string query,
            bool queryContainsPunctuation,
            int nameIndex)
        {
            var rankValues = new List<double>();

            rankValues.Add(CalculateRankValueForPrefixMatch(name, query));

            rankValues.Add(CalculateRankValueForAcronymMatch(name, query));

            for (int searchTermIndex = 0; searchTermIndex < searchTerms.Count; searchTermIndex++)
            {
                string searchTerm = searchTerms[searchTermIndex];

                List<string> words = queryContainsPunctuation ? name.LowercaseWordsWithPunctuation : name.LowercaseWords;
                for (int wordIndex = 0; wordIndex < words.Count; wordIndex++)
                {
                    string word = words[wordIndex];

                    rankValues.Add(
                        CalculateRankValueForWordMatch(word, query, searchTerm, searchTermIndex, wordIndex, nameIndex));
                }
            }

            return new RankedName
            {
                Name = name.OriginalValue, Ranks = rankValues.OrderByDescending(r => r).Take(5).ToList()
            };
        }
        
        public static double CalculateRankValueForPrefixMatch(SearchReadyValue name, string query)
        {
            if (name.LowercaseValue.StartsWith(query))
            {
                // Prefix match
                double prefixAmount = (double) query.Length / (double) name.LowercaseValue.Length;
                double rankValue = (prefixAmount / 2) + 0.5;
                return rankValue;
            }

            return 0;
        }

        private static double CalculateRankValueForAcronymMatch(SearchReadyValue name, string query)
        {
            if (name.Acronym.StartsWith(query))
            {
                // Acronym Prefix match
                double prefixAmount = (double) query.Length / (double) name.Acronym.Length;
                double rankValue = prefixAmount;
                return rankValue;
            }

            return 0;
        }

        private static double CalculateRankValueForWordMatch(
            string word,
            string query,
            string searchTerm,
            int searchTermIndex,
            int wordIndex,
            int nameIndex)
        {
            if (word.StartsWith(searchTerm))
            {
                double prefixAmount = (double) query.Length / (double) word.Length;

                // In the Add Organisation search, we use this method on names that come from the Companies House API
                // In that case, the company name might not appear in SearchRepository.OrganisationNameWords (our corpus of words)
                // We don't want this ranking to fail, so let's say it appears 1 time
                // 1 is a fairly decent guess, because it's probably an unpopular word (if it's nowhere in our corpus of words)
                int appearancesOfWordInCorpus = SearchRepository.OrganisationNameWords.ContainsKey(word) ? SearchRepository.OrganisationNameWords[word] : 1;

                double popularityOfWord =
                    1 - ((double) appearancesOfWordInCorpus / (double) SearchRepository.MaxOrganisationNameWords);

                double startOfSearchness = ((double) 4) / (searchTermIndex + wordIndex + nameIndex + 4);

                return prefixAmount * popularityOfWord * startOfSearchness;
            }

            return 0;
        }
        
        public static List<double> ApplyCompanySizeMultiplierToRanks(List<double> ranks, int minEmployees)
        {
            double companySizeMultiplier = 1 + (0.2 * (minEmployees / 20000)); // Multiply by up to 1.2 for big companies
            for (int i = 0; i < Math.Min(ranks.Count, 5); i++)
            {
                ranks[i] *= companySizeMultiplier;
            }

            return ranks;
        }

        internal static IOrderedEnumerable<TSource> RankHelperOrderByListOfDoubles<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, List<double>> selector
            )
        {
            return source
                .OrderByDescending(item => GetNthOr0(selector(item), 0))
                .ThenByDescending(item => GetNthOr0(selector(item), 1))
                .ThenByDescending(item => GetNthOr0(selector(item), 2))
                .ThenByDescending(item => GetNthOr0(selector(item), 3))
                .ThenByDescending(item => GetNthOr0(selector(item), 4));
        }

        private static double GetNthOr0(List<double> list, int n)
        {
            if (list.Count - 1 >= n)
            {
                return list[n];
            }

            return 0;
        }

    }
}
