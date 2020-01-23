using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;
using Microsoft.Azure.Search.Models;

namespace GenderPayGap.Tests.Common.Mocks
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

    public class MockSearchRepository : ISearchRepository<EmployerSearchModel>
    {

        private List<EmployerSearchModel> _documents = new List<EmployerSearchModel>();

        public MockSearchRepository(List<EmployerSearchModel> documents = null)
        {
            if (documents != null)
            {
                _documents = documents;
            }
        }


        public async Task CreateIndexIfNotExistsAsync(string indexName) { }

        public async Task<EmployerSearchModel> GetAsync(string key, string selectFields = null)
        {
            return _documents.FirstOrDefault(d => d.OrganisationId == key);
        }

        public async Task<long> GetDocumentCountAsync()
        {
            return _documents.Count;
        }

        public async Task AddOrUpdateIndexDataAsync(IEnumerable<EmployerSearchModel> records)
        {
            _documents.AddOrUpdate(records.ToList());
        }

        public async Task<int> RemoveFromIndexAsync(IEnumerable<EmployerSearchModel> records)
        {
            var c = 0;
            foreach (EmployerSearchModel record in records)
            {
                int i = _documents.FindIndex(d => d.OrganisationId == record.OrganisationId);
                if (i > -1)
                {
                    _documents.RemoveAt(i);
                    c++;
                }
            }

            return c;
        }

        public async Task<PagedResult<EmployerSearchModel>> SearchAsync(string searchText,
            int currentPage,
            SearchType searchType,
            int pageSize = 20,
            string searchFields = null,
            string selectFields = null,
            string orderBy = null,
            Dictionary<string, Dictionary<object, long>> facets = null,
            string filter = null,
            string highlights = null,
            SearchMode searchMode = SearchMode.Any)
        {
            var result = new PagedResult<EmployerSearchModel>();
            result.Results = new List<EmployerSearchModel>(_documents);
            //result.ActualRecordTotal = _documents.Count;
            //result.VirtualRecordTotal = _documents.Count;

            int totalRecords = _documents.Count;

            //Return the results
            var searchResults = new PagedResult<EmployerSearchModel> {
                Results = result.Results,
                CurrentPage = currentPage,
                PageSize = pageSize,
                ActualRecordTotal = totalRecords,
                VirtualRecordTotal = totalRecords
            };

            return searchResults;
        }

        public Task<IEnumerable<KeyValuePair<string, EmployerSearchModel>>> SuggestAsync(string searchText,
            string searchFields = null,
            string selectFields = null,
            bool fuzzy = true,
            int maxRecords = 10)
        {
            return Task.FromResult<IEnumerable<KeyValuePair<string, EmployerSearchModel>>>(null);
        }

        public async Task RefreshIndexDataAsync(IEnumerable<EmployerSearchModel> listOfRecordsToAddOrUpdate)
        {
            _documents = listOfRecordsToAddOrUpdate.ToList();
        }

        public async Task<IList<EmployerSearchModel>> ListAsync(string selectFields = null)
        {
            return _documents;
        }

    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

}
