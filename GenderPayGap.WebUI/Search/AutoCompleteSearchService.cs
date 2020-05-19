using System;
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
        
        public AutoCompleteOrganisationName TopName { get; set; }
        // The top name is pulled out and effectively duplicated here for performance benefits
    }

    internal class AutoCompleteOrganisationName
    {
        public string Name { get; set; }
        public List<double> Ranks { get; set; }
    }


    public class AutoCompleteSearchService
    {

        public List<SuggestOrganisationResult> Search(string query)
        {
            query = query.Trim().ToLower();

            bool queryContainsPunctuation = WordSplittingRegex.ContainsPunctuationCharacters(query);

            List<string> searchTerms = SearchHelper.ExtractSearchTermsFromQuery(query, queryContainsPunctuation);

            List<SearchCachedOrganisation> allOrganisations = SearchRepository.CachedOrganisations;

            List<SearchCachedOrganisation> matchingOrganisations = GetMatchingOrganisations(allOrganisations, searchTerms, queryContainsPunctuation);

            List<AutoCompleteOrganisation> organisationsWithRankings = CalculateRankings(matchingOrganisations, searchTerms, query, queryContainsPunctuation);

            List<AutoCompleteOrganisation> top10Organisations = TakeTop10RankingOrganisations(organisationsWithRankings);

            List<SuggestOrganisationResult> results = ConvertOrganisationsToSearchResults(top10Organisations);

            return results;
        }

        
        private static List<SearchCachedOrganisation> GetMatchingOrganisations(List<SearchCachedOrganisation> allOrganisations,
            List<string> searchTerms,
            bool queryContainsPunctuation)
        {
            return allOrganisations
                .Where(org => org.Status == OrganisationStatuses.Active || org.Status == OrganisationStatuses.Retired)
                .Where(org => SearchHelper.CurrentOrPreviousOrganisationNameMatchesSearchTerms(org, searchTerms, queryContainsPunctuation))
                .ToList();
        }
        
        private static List<AutoCompleteOrganisation> CalculateRankings(List<SearchCachedOrganisation> matchingOrganisations,
            List<string> searchTerms,
            string query,
            bool queryContainsPunctuation)
        {
            return matchingOrganisations
                .Select(organisation => CalculateRankForOrganisation(organisation, searchTerms, query, queryContainsPunctuation))
                .ToList();
        }

        private static AutoCompleteOrganisation CalculateRankForOrganisation(SearchCachedOrganisation organisation,
            List<string> searchTerms,
            string query,
            bool queryContainsPunctuation)
        {
            var autoCompleteOrganisation = new AutoCompleteOrganisation
            {
                EncryptedId = organisation.EncryptedId,
                Names = new List<AutoCompleteOrganisationName>()
            };

            for (var nameIndex = 0; nameIndex < organisation.OrganisationNames.Count; nameIndex++)
            {
                SearchReadyValue name = organisation.OrganisationNames[nameIndex];
                autoCompleteOrganisation.Names.Add(CalculateRankForName(name, searchTerms, query, queryContainsPunctuation, nameIndex));
            }

            autoCompleteOrganisation.TopName = autoCompleteOrganisation.Names
                .RankHelperOrderByListOfDoubles(name => name.Ranks)
                .First();

            var ranks = RankValueHelper.ApplyCompanySizeMultiplierToRanks(autoCompleteOrganisation.TopName.Ranks, organisation.MinEmployees);
            autoCompleteOrganisation.TopName.Ranks = ranks;
            
            return autoCompleteOrganisation;
        }
        
        private static AutoCompleteOrganisationName CalculateRankForName(SearchReadyValue name,
            List<string> searchTerms,
            string query,
            bool queryContainsPunctuation,
            int nameIndex)
        {
            var rankValues = new List<double>();

            rankValues.Add(RankValueHelper.CalculateRankValueForPrefixMatch(name, query));

            rankValues.Add(RankValueHelper.CalculateRankValueForAcronymMatch(name, query));

            for (int searchTermIndex = 0; searchTermIndex < searchTerms.Count; searchTermIndex++)
            {
                string searchTerm = searchTerms[searchTermIndex];

                List<string> words = queryContainsPunctuation ? name.LowercaseWordsWithPunctuation : name.LowercaseWords;
                for (int wordIndex = 0; wordIndex < words.Count; wordIndex++)
                {
                    string word = words[wordIndex];

                    rankValues.Add(RankValueHelper.CalculateRankValueForWordMatch(word, query, searchTerm, searchTermIndex, wordIndex, nameIndex));
                }
            }

            return new AutoCompleteOrganisationName
            {
                Name = name.OriginalValue,
                Ranks = rankValues.OrderByDescending(r => r).Take(5).ToList()
            };
        }

        private List<AutoCompleteOrganisation> TakeTop10RankingOrganisations(List<AutoCompleteOrganisation> organisationsWithRankings)
        {
            return organisationsWithRankings
                .RankHelperOrderByListOfDoubles(org => org.TopName.Ranks)
                .ThenBy(org => org.Names[0].Name)
                .Take(10)
                .ToList();
        }

        private static List<SuggestOrganisationResult> ConvertOrganisationsToSearchResults(List<AutoCompleteOrganisation> orderedOrganisations)
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

                    var result = new SuggestOrganisationResult
                    {
                        Id = org.EncryptedId,
                        Text = currentName.Name,
                        PreviousName = previousName
                    };

                    return result;
                })
                .ToList();
        }

    }
}
