using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace GenderPayGap.Core.Classes
{
    public class AzureSearchRepository : ISearchRepository<EmployerSearchModel>
    {

        private const string suggestorName = "sgOrgName";
        private const string synonymMapName = "desc-synonymmap";
        private readonly TelemetryClient _telemetryClient;

        private readonly Lazy<Task<ISearchIndexClient>> _indexClient;

        private readonly Lazy<Task<ISearchServiceClient>> _serviceClient;

        public AzureSearchRepository(string serviceName,
            string adminApiKey = null,
            string queryApiKey = null,
            TelemetryClient telemetryClient = null)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentNullException(nameof(serviceName));
            }

            if (string.IsNullOrWhiteSpace(adminApiKey) && string.IsNullOrWhiteSpace(queryApiKey))
            {
                throw new ArgumentNullException($"You must provide '{nameof(adminApiKey)}' or '{nameof(queryApiKey)}'");
            }

            if (!string.IsNullOrWhiteSpace(adminApiKey) && !string.IsNullOrWhiteSpace(queryApiKey))
            {
                throw new ArgumentException($"Cannot specify both '{nameof(adminApiKey)}' and '{nameof(queryApiKey)}'");
            }

            _serviceClient = new Lazy<Task<ISearchServiceClient>>(
                async () => {
                    //Get the service client
                    var _serviceClient = new SearchServiceClient(serviceName, new SearchCredentials(adminApiKey));

                    //Ensure the index exists
                    await CreateIndexIfNotExistsAsync(_serviceClient, Global.SearchIndexName).ConfigureAwait(false);

                    return _serviceClient;
                });

            _indexClient = new Lazy<Task<ISearchIndexClient>>(
                async () => {
                    //Get the index client
                    if (!string.IsNullOrWhiteSpace(adminApiKey))
                    {
                        ISearchServiceClient serviceClient = await _serviceClient.Value;
                        return serviceClient.Indexes.GetClient(Global.SearchIndexName);
                    }

                    if (!string.IsNullOrWhiteSpace(queryApiKey))
                    {
                        return new SearchIndexClient(serviceName, Global.SearchIndexName, new SearchCredentials(queryApiKey));
                    }

                    throw new ArgumentNullException($"You must provide '{nameof(adminApiKey)}' or '{nameof(queryApiKey)}'");
                });
        }

        public async Task CreateIndexIfNotExistsAsync(string indexName)
        {
            ISearchServiceClient serviceClient = await _serviceClient.Value;
            await CreateIndexIfNotExistsAsync(serviceClient, indexName);
        }

        public async Task RefreshIndexDataAsync(IEnumerable<EmployerSearchModel> allRecords)
        {
            //Add (or update) the records to the index
            await AddOrUpdateIndexDataAsync(allRecords);

            //Get the old records which will need deleting
            IList<EmployerSearchModel> indexedRecords = await ListAsync(nameof(EmployerSearchModel.OrganisationId));
            IEnumerable<EmployerSearchModel> oldRecords = indexedRecords.Except(allRecords);

            //Delete the old records
            if (oldRecords.Any())
            {
                await RemoveFromIndexAsync(oldRecords);
            }
        }

        /// <summary>
        ///     Adds all new records to index
        /// </summary>
        /// <param name="newRecords">The new or existing records which should be indexed.</param>
        public async Task AddOrUpdateIndexDataAsync(IEnumerable<EmployerSearchModel> newRecords)
        {
            if (newRecords == null || !newRecords.Any())
            {
                throw new ArgumentNullException(nameof(newRecords), "You must supply at least one record to index");
            }

            //Remove all test organisations
            if (!string.IsNullOrWhiteSpace(Global.TestPrefix))
            {
                newRecords = newRecords.Where(e => !e.Name.StartsWithI(Global.TestPrefix));
            }

            //Ensure the records are ordered by name
            newRecords = newRecords.OrderBy(o => o.Name);

            //Set the records to add or update
            List<IndexAction<EmployerSearchModel>> actions = newRecords.Select(r => IndexAction.MergeOrUpload(r)).ToList();

            var batches = new ConcurrentBag<IndexBatch<EmployerSearchModel>>();
            while (actions.Any())
            {
                int batchSize = actions.Count > 1000 ? 1000 : actions.Count;
                IndexBatch<EmployerSearchModel> batch = IndexBatch.New(actions.Take(batchSize).ToList());
                batches.Add(batch);
                actions.RemoveRange(0, batchSize);
            }

            ISearchIndexClient indexClient = await _indexClient.Value;

            Parallel.ForEach(
                batches,
                batch => {
                    var retries = 0;
                    retry:
                    try
                    {
                        indexClient.Documents.Index(batch);
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

        /// <summary>
        ///     Removes old records from index
        /// </summary>
        /// <param name="oldRecords">The old records which should be deleted from the index.</param>
        public async Task<int> RemoveFromIndexAsync(IEnumerable<EmployerSearchModel> oldRecords)
        {
            if (oldRecords == null || !oldRecords.Any())
            {
                throw new ArgumentNullException(nameof(oldRecords), "You must supply at least one record to index");
            }

            //Set the records to add or update
            List<IndexAction<EmployerSearchModel>> actions = oldRecords.Select(r => IndexAction.Delete(r)).ToList();

            var batches = new ConcurrentBag<IndexBatch<EmployerSearchModel>>();

            while (actions.Any())
            {
                int batchSize = actions.Count > 1000 ? 1000 : actions.Count;
                IndexBatch<EmployerSearchModel> batch = IndexBatch.New(actions.Take(batchSize).ToList());
                batches.Add(batch);
                actions.RemoveRange(0, batchSize);
            }

            var deleteCount = 0;

            var exceptions = new ConcurrentBag<Exception>();
            ISearchIndexClient indexClient = await _indexClient.Value;

            await batches.WaitForAllAsync(
                async batch => {
                    var retries = 0;
                    retry:
                    try
                    {
                        await indexClient.Documents.IndexAsync(batch);
                        Interlocked.Add(ref deleteCount, batch.Actions.Count());
                    }
                    catch (IndexBatchException e)
                    {
                        if (retries < 30)
                        {
                            retries++;
                            Thread.Sleep(1000);
                            goto retry;
                        }

                        exceptions.Add(e);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                });
            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }

            return deleteCount;
        }

        public async Task<EmployerSearchModel> GetAsync(string key, string selectFields = null)
        {
            //Limit result fields
            List<string> selectedFields = string.IsNullOrWhiteSpace(selectFields) ? null : selectFields.SplitI().ToList();

            ISearchIndexClient indexClient = await _indexClient.Value;

            EmployerSearchModel result = await indexClient.Documents.GetAsync<EmployerSearchModel>(key, selectedFields);

            return result;
        }

        public async Task<IList<EmployerSearchModel>> ListAsync(string selectFields = null)
        {
            long totalPages = 0;
            var currentPage = 1;
            var resultsList = new List<EmployerSearchModel>();
            do
            {
                PagedResult<EmployerSearchModel> searchResults = await SearchAsync(
                    null,
                    currentPage,
                    SearchType.NotSet,
                    selectFields: selectFields);
                totalPages = searchResults.PageCount;
                resultsList.AddRange(searchResults.Results);
                currentPage++;
            } while (currentPage < totalPages);

            return resultsList;
        }

        public async Task<long> GetDocumentCountAsync()
        {
            ISearchServiceClient serviceClient = await _serviceClient.Value;

            if (!await serviceClient.Indexes.ExistsAsync(Global.SearchIndexName))
            {
                return 0;
            }

            PagedResult<EmployerSearchModel> searchResults = await SearchAsync(null, 1, SearchType.NotSet);
            return searchResults.ActualRecordTotal;
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
        public async Task<IEnumerable<KeyValuePair<string, EmployerSearchModel>>> SuggestAsync(string searchText,
            string searchFields = null,
            string selectFields = null,
            bool fuzzy = true,
            int maxRecords = 10)
        {
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

            ISearchIndexClient indexClient = await _indexClient.Value;

            DocumentSuggestResult<EmployerSearchModel> results =
                await indexClient.Documents.SuggestAsync<EmployerSearchModel>(searchText, suggestorName, sp);
            IEnumerable<KeyValuePair<string, EmployerSearchModel>> suggestions =
                results?.Results.Select(s => new KeyValuePair<string, EmployerSearchModel>(s.Text, s.Document));

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
            ISearchIndexClient indexClient = await _indexClient.Value;

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
            DocumentSearchResult<EmployerSearchModel>
                results = await indexClient.Documents.SearchAsync<EmployerSearchModel>(searchText, sp);

            //Return the total records
            long totalRecords = results.Count.Value;

            /* There are too many empty searches being executed (about 1200). This needs further investigation to see if/how they can be reduced */
            if (!string.IsNullOrEmpty(searchText))
            {
                var telemetryProperties = new Dictionary<string, string> {
                    {"TimeStamp", VirtualDateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")},
                    {"QueryTerms", searchText},
                    {"ResultCount", totalRecords.ToString()},
                    {"SearchType", searchType.ToString()},
                    {"SearchParameters", HttpUtility.UrlDecode(sp.ToString())}
                };

                _telemetryClient?.TrackEvent("Gpg_Search", telemetryProperties);

                await Global.SearchLog.WriteAsync(telemetryProperties);
            }

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
            var searchResults = new PagedResult<EmployerSearchModel> {
                Results = results.Results.Select(r => r.Document).ToList(),
                CurrentPage = currentPage,
                PageSize = pageSize,
                ActualRecordTotal = totalRecords,
                VirtualRecordTotal = totalRecords
            };

            return searchResults;
        }

        /// <summary>
        ///     Create the default index if it doesnt already exist
        /// </summary>
        /// <returns>The existing or new index</returns>
        private async Task CreateIndexIfNotExistsAsync(ISearchServiceClient serviceClient, string indexName)
        {
            if (await serviceClient.Indexes.ExistsAsync(indexName))
            {
                return;
            }

            var index = new Index {Name = indexName, Fields = FieldBuilder.BuildForType<EmployerSearchModel>()};

            index.Suggesters = new List<Suggester> {
                new Suggester(
                    suggestorName,
                    nameof(EmployerSearchModel.Name),
                    nameof(EmployerSearchModel.PreviousName),
                    nameof(EmployerSearchModel.Abbreviations))
            };

            var charFilterRemoveAmpersand = new MappingCharFilter("gpg_remove_Ampersand", new List<string> {"&=>"});
            var charFilterRemoveDot = new MappingCharFilter("gpg_remove_Dot", new List<string> {".=>"});
            var charFilterRemoveLtdInfoCaseInsensitive = new PatternReplaceCharFilter(
                "gpg_patternReplaceCharFilter_Ltd",
                "(?i)(limited|ltd|llp| uk|\\(uk\\)|-uk)[\\.]*",
                string.Empty); // case insensitive 'limited' 'ltd', 'llp', ' uk', '(uk)', '-uk' followed by zero or more dots (to cater for ltd. and some mis-punctuated limited..)
            var charFilterRemoveWhitespace = new PatternReplaceCharFilter(
                "gpg_patternReplaceCharFilter_removeWhitespace",
                "\\s",
                string.Empty);

            index.CharFilters = new List<CharFilter> {
                charFilterRemoveAmpersand, charFilterRemoveDot, charFilterRemoveLtdInfoCaseInsensitive, charFilterRemoveWhitespace
            };

            var edgeNGramTokenFilterFront = new EdgeNGramTokenFilterV2("gpg_edgeNGram_front", 3, 300, EdgeNGramTokenFilterSide.Front);
            var edgeNGramTokenFilterBack = new EdgeNGramTokenFilterV2("gpg_edgeNGram_back", 3, 300, EdgeNGramTokenFilterSide.Back);
            index.TokenFilters = new List<TokenFilter> {edgeNGramTokenFilterFront, edgeNGramTokenFilterBack};

            var standardTokenizer = new StandardTokenizerV2("gpg_standard_v2_tokenizer");
            var keywordTokenizer = new KeywordTokenizerV2("gpg_keyword_v2_tokenizer");

            index.Tokenizers = new List<Tokenizer> {standardTokenizer, keywordTokenizer};

            var suffixAnalyzer = new CustomAnalyzer(
                "gpg_suffix",
                standardTokenizer.Name,
                new List<TokenFilterName> {TokenFilterName.Lowercase, edgeNGramTokenFilterBack.Name},
                new List<CharFilterName> {charFilterRemoveAmpersand.Name, charFilterRemoveLtdInfoCaseInsensitive.Name});

            var completeTokenAnalyzer = new CustomAnalyzer(
                "gpg_prefix_completeToken",
                keywordTokenizer.Name,
                new List<TokenFilterName> {TokenFilterName.Lowercase, edgeNGramTokenFilterFront.Name},
                new List<CharFilterName> {
                    charFilterRemoveDot.Name,
                    charFilterRemoveAmpersand.Name,
                    charFilterRemoveLtdInfoCaseInsensitive.Name,
                    charFilterRemoveWhitespace.Name
                });

            index.Analyzers = new List<Analyzer> {suffixAnalyzer, completeTokenAnalyzer};

            index.Fields.First(f => f.Name == nameof(EmployerSearchModel.PartialNameForSuffixSearches)).Analyzer = suffixAnalyzer.Name;
            index.Fields.First(f => f.Name == nameof(EmployerSearchModel.PartialNameForSuffixSearches)).SynonymMaps =
                new[] {synonymMapName};

            index.Fields.First(f => f.Name == nameof(EmployerSearchModel.PartialNameForCompleteTokenSearches)).Analyzer =
                completeTokenAnalyzer.Name;
            index.Fields.First(f => f.Name == nameof(EmployerSearchModel.PartialNameForCompleteTokenSearches)).SynonymMaps =
                new[] {synonymMapName};

            index.Fields.First(f => f.Name == nameof(EmployerSearchModel.Name)).SynonymMaps = new[] {synonymMapName};
            index.Fields.First(f => f.Name == nameof(EmployerSearchModel.PreviousName)).SynonymMaps = new[] {synonymMapName};

            //Add the synonyms if they dont already exist
            if (!await serviceClient.SynonymMaps.ExistsAsync(synonymMapName))
            {
                serviceClient.SynonymMaps.CreateOrUpdate(
                    new SynonymMap {
                        Name = synonymMapName,
                        //Format = "solr", cannot set after upgrade from v5.03 to version 9.0.0
                        Synonyms = "coop, co-operative"
                    });
            }

            await serviceClient.Indexes.CreateAsync(index);
        }

    }
}
