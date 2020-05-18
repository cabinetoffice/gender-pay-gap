using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Index = Microsoft.Azure.Search.Models.Index;

namespace GenderPayGap.Core.Classes
{
    public abstract class ASearchRepository<T> : ISearchRepository<T> where T : class
    {

        protected readonly Lazy<Task<ISearchServiceClient>> _searchServiceClient;
        private readonly TelemetryClient _telemetryClient;
        protected Lazy<Task<ISearchIndexClient>> _searchIndexClient;
        protected string _suggesterName;

        protected ASearchRepository(ISearchServiceClient searchServiceClient)
        {
            _searchServiceClient = new Lazy<Task<ISearchServiceClient>>(
                async () => {
                    //Ensure the index exists
                    await CreateIndexIfNotExistsAsync(searchServiceClient, Global.SicCodesIndexName).ConfigureAwait(false);

                    return searchServiceClient;
                });
        }

        private async Task CreateIndexIfNotExistsAsync(ISearchServiceClient searchServiceClient, string sicCodesIndexName)
        {
            if (searchServiceClient.Indexes == null || await searchServiceClient.Indexes.ExistsAsync(sicCodesIndexName))
            {
                return;
            }

            var index = new Index {
                Name = sicCodesIndexName,
                Fields = FieldBuilder.BuildForType<SicCodeSearchModel>(),
                Suggesters = new List<Suggester> {
                    new Suggester(
                        _suggesterName,
                        nameof(SicCodeSearchModel.SicCodeDescription),
                        nameof(SicCodeSearchModel.SicCodeListOfSynonyms))
                }
            };

            await searchServiceClient.Indexes.CreateAsync(index);
        }

        protected async Task<IList<T>> ListWorkAsync(string selectFields = null)
        {
            long totalPages = 0;
            var currentPage = 1;
            var resultsList = new List<T>();

            do
            {
                PagedResult<T> searchResults = await SearchAsync(null, currentPage, SearchType.NotSet, selectFields: selectFields);
                totalPages = searchResults.PageCount;
                resultsList.AddRange(searchResults.Results);
                currentPage++;
            } while (currentPage < totalPages);

            return resultsList;
        }

        #region methods implemented by this abstract class

        public async Task AddOrUpdateIndexDataAsync(IEnumerable<T> newRecords)
        {
            if (newRecords == null || !newRecords.Any())
            {
                throw new ArgumentNullException(nameof(newRecords), "You must supply at least one record to index");
            }

            //Set the records to add or update
            List<IndexAction<T>> actions = newRecords.Select(IndexAction.MergeOrUpload).ToList();

            var batches = new ConcurrentBag<IndexBatch<T>>();
            while (actions.Any())
            {
                int batchSize = actions.Count > 1000 ? 1000 : actions.Count;
                IndexBatch<T> batch = IndexBatch.New(actions.Take(batchSize).ToList());
                batches.Add(batch);
                actions.RemoveRange(0, batchSize);
            }

            ISearchIndexClient searchIndexClient = await _searchIndexClient.Value;

            Parallel.ForEach(
                batches,
                batch => {
                    var retries = 0;
                    retry:
                    try
                    {
                        searchIndexClient.Documents.Index(batch);
                    }
                    catch (IndexBatchException)
                    {
                        if (retries < 30)
                        {
                            retries++;
                            Thread.Sleep(1000);
                            goto retry;
                        }

                        throw;
                    }
                });
        }

        public async Task RefreshIndexDataAsync(IEnumerable<T> listOfRecordsToAddOrUpdate)
        {
            //Add (or update) the records to the index
            await AddOrUpdateIndexDataAsync(listOfRecordsToAddOrUpdate);

            //Get the old records which will need deleting
            IList<T> currentListOfDocumentsInIndex = await ListAsync();
            IEnumerable<T> listOfDocumentsNotPartOfThisUpdate = currentListOfDocumentsInIndex.Except(listOfRecordsToAddOrUpdate);

            //Delete the old records
            if (listOfDocumentsNotPartOfThisUpdate.Any())
            {
                await RemoveFromIndexAsync(listOfDocumentsNotPartOfThisUpdate);
            }
        }

        /// <summary>
        ///     Returns a list of search suggestions based on input text
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="fuzzy">
        ///     Gets or sets a value indicating whether to use fuzzy matching for the suggestion
        ///     query. Default is true. when set to true, the query will find suggestions even
        ///     if there's a substituted or missing character in the search text. While this
        ///     provides a better experience in some scenarios it comes at a performance cost
        ///     as fuzzy suggestion searches are slower and consume more resources.
        /// </param>
        /// <param name="maxRecords">Maximum number of suggestions to return (default=10)</param>
        /// </param>
        public async Task<IEnumerable<KeyValuePair<string, T>>> SuggestAsync(string searchText,
            string searchFields = null,
            string selectFields = null,
            bool fuzzy = true,
            int maxRecords = 10)
        {
            if (string.IsNullOrEmpty(searchText?.Trim()))
            {
                return new List<KeyValuePair<string, T>>();
            }

            // Execute search based on query string
            var sp = new SuggestParameters {UseFuzzyMatching = fuzzy, Top = maxRecords};

            //Specify the fields to search
            if (!string.IsNullOrWhiteSpace(searchFields))
            {
                sp.SearchFields = searchFields.SplitI().ToList();
            }

            //Limit result fields
            if (!string.IsNullOrWhiteSpace(selectFields))
            {
                sp.Select = selectFields.SplitI().ToList();
            }

            ISearchIndexClient searchIndexClient = await _searchIndexClient.Value;

            DocumentSuggestResult<T> results = await searchIndexClient.Documents.SuggestAsync<T>(searchText, _suggesterName, sp);
            IEnumerable<KeyValuePair<string, T>>
                suggestions = results?.Results.Select(s => new KeyValuePair<string, T>(s.Text, s.Document));
            return suggestions;
        }

        /// <summary>
        ///     Executes an advanced search using pagination, sorting, filters, facets, and highlighting.
        /// </summary>
        /// <param name="searchText">The text used for the search. When empty all results are returned.</param>
        /// <param name="totalRecords">The returned total number of records in the results</param>
        /// <param name="currentPage">The current page of results to return</param>
        /// <param name="pageSize">The size of the result set to return (default=20). Maximum is 1000.</param>
        /// <param name="filter">
        ///     A set of comma or semicolon separated field names to searching.
        ///     Only fields marked with the 'IsSearchable' attribute can be included.
        ///     The default is empty and all searchable fields will be searched.
        ///     ///
        /// </param>
        /// <param name="selectFields"></param>
        /// A set of comma or semicolon separated field names to return values for.
        /// Default is empty and will return all field values
        /// <param name="orderBy">
        ///     A set of comma or semicolon separated sort terms.
        ///     Default is empty and will return results sorted by score relevance.
        ///     For example, OrganisationName, SicName DESC
        ///     Only fields marked with the 'IsSortable' attribute can be included.
        /// </param>
        /// <param name="facets">
        ///     Specifies the facets to query and returns the facet results
        ///     The default is empty and no facets will be applied.
        ///     Only fields marked with the 'IsFacetable' attribute can be included.
        ///     Call by specifing field names as keys in the dictionary.
        ///     The resulting dictionary for each field returns all possible values and their count for that field.
        ///     ///
        /// </param>
        /// <param name="filter">
        ///     A filter expression using OData syntax (see
        ///     https://docs.microsoft.com/en-us/rest/api/searchservice/odata-expression-syntax-for-azure-search)
        ///     The default is empty and no filter will be applied.
        ///     Only fields marked with the 'IsFilterable' attribute can be included.
        ///     String comparisons are case sensitive.
        ///     You can also use the operators '==','!=', '>=', '>', '<=', '<', '&&', '||' which will be automatically replaced with OData counterparts 'EQ','NE', 'GE', 'GT', 'LE', 'LT', 'AND', 'OR'.
        /// Special functions also include search.in(myfield, 'a, b, c')
        /// /// </param>
        /// <param name="highlights">
        ///     A set of comma or semicolon separated field names used for hit highlights.
        ///     Only fields marked with the 'IsSearchable' attribute can be included.
        ///     By default, Azure Search returns up to 5 highlights per field.
        ///     The limit is configurable per field by appending -
        ///     <max # of highlights>
        ///         following the field name.
        ///         For example, highlight=title-3,description-10 returns up to 3 highlighted hits from the title field and up to
        ///         10 hits from the description field. <max # of highlights> must be an integer between 1 and 1000 inclusive.
        /// </param>
        public async Task<PagedResult<T>> SearchAsync(string searchText,
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
            // Execute search based on query string
            var sp = new SearchParameters {
                SearchMode = searchMode,
                Top = pageSize,
                Skip = (currentPage - 1) * pageSize,
                IncludeTotalResultCount = true,
                QueryType = QueryType.Simple
            };

            //Specify the fields to search
            if (!string.IsNullOrWhiteSpace(searchFields))
            {
                sp.SearchFields = searchFields.SplitI().ToList();
            }

            //Limit result fields
            if (!string.IsNullOrWhiteSpace(selectFields))
            {
                sp.Select = selectFields.SplitI().ToList();
            }

            // Define the sort type or order by relevance score
            if (!string.IsNullOrWhiteSpace(orderBy) && !orderBy.EqualsI("Relevance", "Relevance desc", "Relevance asc"))
            {
                sp.OrderBy = orderBy.SplitI().ToList();
            }

            // Add filtering
            sp.Filter = string.IsNullOrWhiteSpace(filter) ? null : filter;

            //Add facets
            if (facets != null && facets.Count > 0)
            {
                sp.Facets = facets.Keys.ToList();
            }

            //Execute the search
            ISearchIndexClient searchIndexClient = await _searchIndexClient.Value;
            DocumentSearchResult<T> results = searchIndexClient.Documents.Search<T>(searchText, sp);

            //Return the total records
            long totalRecords = results.Count.Value;

            //Return the facet results
            if (sp.Facets != null && sp.Facets.Any())
            {
                foreach (string facetGroupKey in results.Facets.Keys)
                {
                    if (facets[facetGroupKey] == null)
                    {
                        facets[facetGroupKey] = new Dictionary<object, long>();
                    }

                    foreach (FacetResult facetResult in results.Facets[facetGroupKey])
                    {
                        facets[facetGroupKey][facetResult.Value] = facetResult.Count.Value;
                    }
                }
            }

            //Return the results
            var searchResults = new PagedResult<T> {
                Results = results.Results.Select(r => r.Document).ToList(),
                CurrentPage = currentPage,
                PageSize = pageSize,
                ActualRecordTotal = totalRecords,
                VirtualRecordTotal = totalRecords
            };

            return searchResults;
        }

        /// <summary>
        ///     Removes old records from index
        /// </summary>
        /// <param name="oldRecords">The old records which should be deleted from the index.</param>
        public async Task<int> RemoveFromIndexAsync(IEnumerable<T> oldRecords)
        {
            if (oldRecords == null || !oldRecords.Any())
            {
                throw new ArgumentNullException(nameof(oldRecords), "You must supply at least one record to index");
            }

            //Set the records to add or update
            List<IndexAction<T>> actions = oldRecords.Select(IndexAction.Delete).ToList();

            var batches = new ConcurrentBag<IndexBatch<T>>();

            while (actions.Any())
            {
                int batchSize = actions.Count > 1000 ? 1000 : actions.Count;
                IndexBatch<T> batch = IndexBatch.New(actions.Take(batchSize).ToList());
                batches.Add(batch);
                actions.RemoveRange(0, batchSize);
            }

            var deleteCount = 0;
            ISearchIndexClient searchIndexClient = await _searchIndexClient.Value;

            Parallel.ForEach(
                batches,
                batch => {
                    var retries = 0;
                    retry:
                    try
                    {
                        searchIndexClient.Documents.Index(batch);
                        Interlocked.Add(ref deleteCount, batch.Actions.Count());
                    }
                    catch (IndexBatchException)
                    {
                        if (retries < 30)
                        {
                            retries++;
                            Thread.Sleep(1000);
                            goto retry;
                        }

                        throw;
                    }
                });
            return deleteCount;
        }

        #endregion

        #region methods left to be implemented by subclasses

        public abstract Task<T> GetAsync(string key, string selectFields = null);

        public abstract Task<IList<T>> ListAsync(string selectFields = null);

        public async Task CreateIndexIfNotExistsAsync(string indexName)
        {
            ISearchServiceClient serviceClient = await _searchServiceClient.Value;
            await CreateIndexIfNotExistsAsync(serviceClient, indexName);
        }

        public abstract Task<long> GetDocumentCountAsync();

        #endregion

    }
}
