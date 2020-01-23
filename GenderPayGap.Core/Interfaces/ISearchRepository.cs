using System.Collections.Generic;
using System.Threading.Tasks;
using GenderPayGap.Core.Classes;
using Microsoft.Azure.Search.Models;

namespace GenderPayGap.Core.Interfaces
{
    public interface ISearchRepository<T>
    {

        Task RefreshIndexDataAsync(IEnumerable<T> allRecords);
        Task AddOrUpdateIndexDataAsync(IEnumerable<T> records);
        Task<int> RemoveFromIndexAsync(IEnumerable<T> records);
        Task<T> GetAsync(string key, string selectFields = null);
        Task<IList<T>> ListAsync(string selectFields = null);
        Task CreateIndexIfNotExistsAsync(string indexName);
        Task<long> GetDocumentCountAsync();

        Task<IEnumerable<KeyValuePair<string, T>>> SuggestAsync(string searchText,
            string searchFields = null,
            string selectFields = null,
            bool fuzzy = true,
            int maxRecords = 10);

        Task<PagedResult<T>> SearchAsync(string searchText,
            int currentPage,
            SearchType searchType,
            int pageSize = 20,
            string searchFields = null,
            string selectFields = null,
            string orderBy = null,
            Dictionary<string, Dictionary<object, long>> facets = null,
            string filter = null,
            string highlights = null,
            SearchMode searchMode = SearchMode.Any);

    }
}
