﻿using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Models.AddOrganisation;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Search.CachedObjects;

namespace GenderPayGap.WebUI.Search
{
    internal class RankedAddOrganisationSearchOrganisation
    {
        public long OrganisationId { get; set; }
        public string CompanyNumber { get; set; }

        public SearchReadyValue OrganisationName { get; set; }
        public List<SearchReadyValue> OrganisationNames { get; set; } // All names (current and previous)

        public List<RankedName> Names { get; set; }
        public RankedName TopName { get; set; }
    }

    public class AddOrganisationSearchService
    {
        private const int MaximumNumberOfSearchResults = 100;
        private readonly IDataRepository dataRepository;

        public AddOrganisationSearchService(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }


        public AddOrganisationSearchResults SearchPublic(string query)
        {
            bool queryContainsPunctuation = WordSplittingRegex.ContainsPunctuationCharacters(query);
            List<string> searchTerms = SearchHelper.ExtractSearchTermsFromQuery(query, queryContainsPunctuation);

            // Get matching organisations from our database
            List<SearchCachedOrganisation> allOrganisations = SearchRepository.CachedOrganisations;
            List<RankedAddOrganisationSearchOrganisation> matchingOrganisations = GetMatchingOrganisations(allOrganisations, searchTerms, query, queryContainsPunctuation, SectorTypes.Public);

            List<RankedAddOrganisationSearchOrganisation> organisationsWithRankings = CalculateOrganisationRankings(matchingOrganisations, searchTerms, query, queryContainsPunctuation);
            List<RankedAddOrganisationSearchOrganisation> rankedOrganisations = OrderOrganisationsByRank(organisationsWithRankings);

            AddOrganisationSearchResults results = ConvertOrganisationsToSearchResults(rankedOrganisations);

            return results;
        }

        private static List<RankedAddOrganisationSearchOrganisation> GetMatchingOrganisations(List<SearchCachedOrganisation> allOrganisations,
            List<string> searchTerms,
            string query,
            bool queryContainsPunctuation,
            SectorTypes sector)
        {
            return allOrganisations
                .Where(organisation => organisation.Sector == sector)
                .Where(organisation => organisation.Status == OrganisationStatuses.Active)
                .Where(
                    organisation =>
                    {
                        bool nameMatches = SearchHelper
                            .CurrentOrPreviousOrganisationNameMatchesSearchTerms(organisation, searchTerms, queryContainsPunctuation);
                        bool employerRefMatches = organisation.EmployerReference == query;
                        bool companyNumberMatches = organisation.CompanyNumber == query;
                        return nameMatches || employerRefMatches || companyNumberMatches;
                    })
                .Select(organisation => new RankedAddOrganisationSearchOrganisation
                {
                    OrganisationId = organisation.OrganisationId,
                    CompanyNumber = organisation.CompanyNumber,
                    OrganisationName = organisation.OrganisationName,
                    OrganisationNames = organisation.OrganisationNames
                })
                .ToList();
        }

        private static List<RankedAddOrganisationSearchOrganisation> CalculateOrganisationRankings(List<RankedAddOrganisationSearchOrganisation> rankReadyOrganisations,
            List<string> searchTerms,
            string query,
            bool queryContainsPunctuation)
        {
            foreach (RankedAddOrganisationSearchOrganisation organisation in rankReadyOrganisations)
            {
                CalculateRankForOrganisation(organisation, searchTerms, query, queryContainsPunctuation);
            }
            return rankReadyOrganisations;
        }

        private static void CalculateRankForOrganisation(RankedAddOrganisationSearchOrganisation rankedAdminSearchOrganisation,
            List<string> searchTerms,
            string query,
            bool queryContainsPunctuation)
        {
            List<RankedName> rankedNames = RankValueHelper.GetRankedNames(rankedAdminSearchOrganisation.OrganisationNames, searchTerms, query, queryContainsPunctuation);

            rankedAdminSearchOrganisation.Names = rankedNames;

            rankedAdminSearchOrganisation.TopName = rankedAdminSearchOrganisation.Names
                .RankHelperOrderByListOfDoubles(name => name.Ranks)
                .First();
        }

        private static List<RankedAddOrganisationSearchOrganisation> OrderOrganisationsByRank(List<RankedAddOrganisationSearchOrganisation> organisationsWithRankings)
        {
            return organisationsWithRankings
                .RankHelperOrderByListOfDoubles(org => org.TopName.Ranks)
                .ThenBy(org => org.Names[0].Name)
                .ToList();
        }

        private AddOrganisationSearchResults ConvertOrganisationsToSearchResults(List<RankedAddOrganisationSearchOrganisation> orderedOrganisations)
        {
            bool tooManyResults = orderedOrganisations.Count > MaximumNumberOfSearchResults;

            List<RankedAddOrganisationSearchOrganisation> limitedNumberOfOrganisations = orderedOrganisations
                .Take(MaximumNumberOfSearchResults)
                .ToList();

            List<long> organisationIds = limitedNumberOfOrganisations
                .Select(org => org.OrganisationId)
                .ToList();

            List<Organisation> foundOrganisations = dataRepository
                .GetAll<Organisation>()
                .Where(org => organisationIds.Contains(org.OrganisationId))
                .ToList();

            List<AddOrganisationSearchResult> searchResults = limitedNumberOfOrganisations
                .Select(
                    org =>
                    {
                        var searchResult = new AddOrganisationSearchResult();
                        searchResult.OrganisationName = org.OrganisationName.OriginalValue;

                        Organisation foundOrganisation = foundOrganisations.FirstOrDefault(o => o.OrganisationId == org.OrganisationId);
                        if (foundOrganisation != null)
                        {
                            searchResult.EncryptedOrganisationId = foundOrganisation.GetEncryptedId();
                            searchResult.OrganisationAddress = foundOrganisation.GetLatestAddress()?.GetAddressString();
                            searchResult.CompanyNumber = foundOrganisation.CompanyNumber;

                            foreach (OrganisationReference reference in foundOrganisation.OrganisationReferences)
                            {
                                searchResult.Identifiers.Add(new AddOrganisationSearchResultOrganisationIdentifier
                                {
                                    IdentifierType = reference.ReferenceName,
                                    Identifier = reference.ReferenceValue
                                });
                            }
                        }
                        else
                        {
                            searchResult.CompanyNumber = org.CompanyNumber;
                        }

                        if (!string.IsNullOrWhiteSpace(searchResult.CompanyNumber))
                        {
                            searchResult.Identifiers.Add(new AddOrganisationSearchResultOrganisationIdentifier
                            {
                                IdentifierType = "Company number",
                                Identifier = searchResult.CompanyNumber
                            });
                        }

                        return searchResult;
                    })
                .ToList();

            return new AddOrganisationSearchResults
            {
                TooManyResults = tooManyResults,
                SearchResults = searchResults
            };
        }

    }
}
