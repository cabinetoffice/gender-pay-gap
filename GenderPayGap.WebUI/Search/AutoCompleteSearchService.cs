using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.WebUI.Models.Search;
using GenderPayGap.WebUI.Search.CachedObjects;

namespace GenderPayGap.WebUI.Search
{

    internal class AutoCompleteOrganisation
    {
        public string EncryptedId { get; set; }
        public List<AutoCompleteOrganisationName> Names { get; set; }
        public List<double> Ranks { get; set; }
    }

    internal class AutoCompleteOrganisationName
    {
        public string Name { get; set; }
        public List<double> Ranks { get; set; }
    }


    public class AutoCompleteSearchService
    {

        public List<SuggestEmployerResult> Search(string query)
        {
            query = query.Trim().ToLower();

            List<string> searchTerms = ExtractSearchTermsFromQuery(query);

            List<SearchCachedOrganisation> allOrganisations = SearchRepository.CachedOrganisations;

            List<SearchCachedOrganisation> matchingOrganisations = GetMatchingOrganisations(allOrganisations, searchTerms);

            List<AutoCompleteOrganisation> orderedOrganisations = CalculateRankings(matchingOrganisations, searchTerms, query);

            List<SuggestEmployerResult> results = ConvertOrganisationsToSearchResults(orderedOrganisations);

            return results;
        }

        private static List<string> ExtractSearchTermsFromQuery(string query)
        {
            return WordSplittingRegex.SplitValueIntoWords(query);
        }

        private static List<SearchCachedOrganisation> GetMatchingOrganisations(
            List<SearchCachedOrganisation> allOrganisations,
            List<string> searchTerms)
        {
            return allOrganisations
                .Where(org => org.Status == OrganisationStatuses.Active || org.Status == OrganisationStatuses.Retired)
                .Where(org => CurrentOrPreviousOrganisationNameMatchesSearchTerms(org, searchTerms))
                .ToList();
        }

        private static bool CurrentOrPreviousOrganisationNameMatchesSearchTerms(SearchCachedOrganisation organisation, List<string> searchTerms)
        {
            return organisation.OrganisationNames.Any(on => on.Matches(searchTerms));
        }

        private static List<AutoCompleteOrganisation> CalculateRankings(
            List<SearchCachedOrganisation> matchingOrganisations,
            List<string> searchTerms,
            string query)
        {
            return matchingOrganisations
                .Select(organisation => CalculateRankForOrganisation(organisation, searchTerms, query))
                .RankHelperOrderByListOfDoubles(name => name.Ranks)
                .ThenBy(mar => mar.Names[0].Name)
                .Take(10)
                .ToList();
        }

        private static AutoCompleteOrganisation CalculateRankForOrganisation(
            SearchCachedOrganisation organisation,
            List<string> searchTerms,
            string query)
        {
            var autoCompleteOrganisation = new AutoCompleteOrganisation
            {
                EncryptedId = organisation.EncryptedId,
                Names = new List<AutoCompleteOrganisationName>()
            };

            for (var nameIndex = 0; nameIndex < organisation.OrganisationNames.Count; nameIndex++)
            {
                SearchReadyValue name = organisation.OrganisationNames[nameIndex];
                autoCompleteOrganisation.Names.Add(CalculateRankForName(name, searchTerms, query, nameIndex));
            }

            autoCompleteOrganisation.Ranks = autoCompleteOrganisation.Names
                .RankHelperOrderByListOfDoubles(name => name.Ranks)
                .Select(n => n.Ranks)
                .First();

            double companySizeMultiplier = 1 + (0.2 * (organisation.MinEmployees / 20000)); // Multiply by up to 1.2 for big companies
            for (int i = 0; i < 5; i++)
            {
                autoCompleteOrganisation.Ranks[i] *= companySizeMultiplier;
            }

            return autoCompleteOrganisation;
        }

        private static AutoCompleteOrganisationName CalculateRankForName(SearchReadyValue name,
            List<string> searchTerms,
            string query,
            int nameIndex)
        {
            var rankValues = new List<double>();

            rankValues.Add(RankValueHelper.CalculateRankValueForPrefixMatch(name, query));

            rankValues.Add(RankValueHelper.CalculateRankValueForAcronymMatch(name, query));

            for (int searchTermIndex = 0; searchTermIndex < searchTerms.Count; searchTermIndex++)
            {
                string searchTerm = searchTerms[searchTermIndex];

                for (int wordIndex = 0; wordIndex < name.LowercaseWords.Count; wordIndex++)
                {
                    string word = name.LowercaseWords[wordIndex];

                    rankValues.Add(RankValueHelper.CalculateRankValueForWordMatch(word, query, searchTerm, searchTermIndex, wordIndex, nameIndex));
                }
            }

            return new AutoCompleteOrganisationName
            {
                Name = name.OriginalValue,
                Ranks = rankValues.OrderByDescending(r => r).Take(5).ToList()
            };
        }

        private static List<SuggestEmployerResult> ConvertOrganisationsToSearchResults(List<AutoCompleteOrganisation> orderedOrganisations)
        {
            return orderedOrganisations
                .Select(org =>
                {
                    AutoCompleteOrganisationName currentName = org.Names[0];

                    AutoCompleteOrganisationName highestRankedName = org.Names
                        .RankHelperOrderByListOfDoubles(name => name.Ranks)
                        .First();

                    string previousName = "";
                    if (highestRankedName.Name != currentName.Name)
                    {
                        previousName = highestRankedName.Name;
                    }

                    var result = new SuggestEmployerResult
                    {
                        Id = org.EncryptedId,
                        Text = currentName.Name,
                        PreviousName = previousName,
                        Rank = highestRankedName.Ranks
                    };

                    return result;
                })
                .ToList();
        }

    }
}
