using System.Collections.Generic;
using SolrNet;
using SolrNet.DSL;
using System.Linq;
using SolrNet.Commands.Parameters;

namespace FT.Search {
    public class SearchParameters {
        
        /// <summary>
        /// All selectable facet fields
        /// </summary>
        private static readonly string[] AllFacetFields = new[] {
            "proposed_year", "ministry_name", "politician_name", 
            "fct_stage", "fct_doc_type", "party" };

        public const int DefaultPageSize = 20;
        public const int DefaultMaxHighlights = 5;

        public string FreeSearch { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public IDictionary<string, string> Facets { get; set; }
        public string Sort { get; set; }
        public int HighlightsMax { get; set; }

        public SearchParameters() {
            Facets = new Dictionary<string, string>();
            PageSize = DefaultPageSize;
            PageIndex = 1;
            HighlightsMax = DefaultMaxHighlights;
        }

        public int FirstItemIndex {
            get {
                return (PageIndex-1)*PageSize;
            }
        }

        public int LastItemIndex {
            get {
                return FirstItemIndex + PageSize;
            }
        }

        /// <summary>
        /// Builds the Solr query from the search parameters
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public ISolrQuery BuildQuery()
        {
            if (!string.IsNullOrEmpty(FreeSearch))
                return new SolrQuery(FreeSearch);
            return SolrQuery.All;
        }

        public ICollection<ISolrQuery> BuildFilterQueries()
        {
            var queriesFromFacets = from p in Facets
                                    select (ISolrQuery)Query.Field(p.Key).Is(p.Value);
            return queriesFromFacets.ToList();
        }

        /// <summary>
        /// Gets the selected facet fields
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<string> SelectedFacetFields()
        {
            return Facets.Select(f => f.Key);
        }


        public SortOrder[] SelectedSort()
        {
            return new[] { SortOrder.Parse(Sort) }.Where(o => o != null).ToArray();
        }

        public QueryOptions ToQueryOptions() 
        {

            QueryOptions queryOptions = new QueryOptions()
            {
                FilterQueries = BuildFilterQueries(),
                Rows = PageSize,
                Start = (PageIndex - 1) * PageSize,
                OrderBy = SelectedSort(),
                SpellCheck = new SpellCheckingParameters(),
                Collapse = new CollapseParameters { 
                    Field = "collapse_id", FacetMode = CollapseFacetMode.Before, 
                    Max = HighlightsMax, Type = CollapseType.Normal},
                Highlight = new HighlightingParameters { 
                    Fields = new[] { "content" },
                    Snippets = HighlightsMax, Fragsize = 120 },
                    Facet = new FacetParameters {
                    Queries = AllFacetFields.Except(SelectedFacetFields())
                          .Select(f => new SolrFacetFieldQuery(f) { MinCount = 1, Limit = 10 })
                          .Cast<ISolrFacetQuery>()
                          .ToList(),
                },
            };
            return queryOptions;
        }
    }
}