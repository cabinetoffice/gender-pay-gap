using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;

namespace GenderPayGap.BusinessLogic
{
    public interface ISearchBusinessLogic
    {

        ISearchRepository<EmployerSearchModel> SearchRepository { get; set; }

        IEnumerable<Organisation> LookupSearchableOrganisations(IList<Organisation> organisations);

        Task UpdateSearchIndexAsync(params Organisation[] organisations);

    }

    public class SearchBusinessLogic : ISearchBusinessLogic
    {

        public SearchBusinessLogic(ISearchRepository<EmployerSearchModel> searchRepository)
        {
            SearchRepository = searchRepository;
        }

        public ISearchRepository<EmployerSearchModel> SearchRepository { get; set; }

        //Returns a list of organisaations to include in search indexes
        public IEnumerable<Organisation> LookupSearchableOrganisations(IList<Organisation> organisations)
        {
            return organisations.Where(
                o => o.Status == OrganisationStatuses.Active
                     && (o.Returns.Any(r => r.Status == ReturnStatuses.Submitted)
                         || o.OrganisationScopes.Any(
                             sc => sc.Status == ScopeRowStatuses.Active
                                   && (sc.ScopeStatus == ScopeStatuses.InScope || sc.ScopeStatus == ScopeStatuses.PresumedInScope))));
        }

        //Add or remove an organisation from the search indexes based on status and scope
        public async Task UpdateSearchIndexAsync(params Organisation[] organisations)
        {
            //Ensure we have a at least one saved organisation
            if (organisations == null || !organisations.Any(o => o.OrganisationId > 0))
            {
                throw new ArgumentNullException(nameof(organisations), "Missing organisations");
            }

            //Get the organisations to include or exclude from search
            List<Organisation> includes = LookupSearchableOrganisations(organisations).ToList();
            List<Organisation> excludes = organisations.Except(includes).ToList();

            //Batch update the included organisations
            if (includes.Any())
            {
                await SearchRepository.AddOrUpdateIndexDataAsync(includes.Select(o => o.ToEmployerSearchResult()));
            }

            //Batch remove the excluded organisations
            if (excludes.Any())
            {
                await SearchRepository.RemoveFromIndexAsync(excludes.Select(o => o.ToEmployerSearchResult(true)));
            }
        }

    }
}
