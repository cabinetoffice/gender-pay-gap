using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.WebUI.Search.CachedObjects;

namespace GenderPayGap.WebUI.Search
{
    public static class RankValueHelper
    {

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
        public static double CalculateRankValueForAcronymMatch(SearchReadyValue name, string query)
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
        public static double CalculateRankValueForWordMatch(
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

                double popularityOfWord =
                    1 - ((double) SearchRepository.OrganisationNameWords[word] / (double) SearchRepository.MaxOrganisationNameWords);

                double startOfSearchness = ((double) 4) / (searchTermIndex + wordIndex + nameIndex + 4);

                return prefixAmount * popularityOfWord * startOfSearchness;
            }

            return 0;
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
