using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FT.Model;
using FT.Search;
using FT.Search.Helpers;
using SolrNet;
using SolrNet.Exceptions;

namespace FolketsTing.Controllers
{
	public class SearchController : Controller
	{
		private readonly ISearchRepository _searchRep;

		public SearchController()
		{
			_searchRep = new SearchRepository();
		}

		public ActionResult Index()
		{
			return View();
		}

		private string GetSpellCheckingResult(ISolrQueryResults<Searchable> searchables)
		{
			return string.Join(" ", searchables.SpellChecking
										.Select(c => c.Suggestions.FirstOrDefault())
										.Where(c => !string.IsNullOrEmpty(c))
										.ToArray());
		}

		public ActionResult ExperimentalSearch(SearchParameters parameters)
		{
			throw new HttpException(404, "Search disabled");

			try
			{
				ISolrReadOnlyOperations<Searchable> solr = 
					Startup.Container.GetInstance<ISolrReadOnlyOperations<Searchable>>();
				var queryOptions = parameters.ToQueryOptions();
				var matchingSearchables = solr.Query(parameters.BuildQuery(), queryOptions);
				var results = new SearchableView
				{
					Searchables = matchingSearchables,
					Search = parameters,
					TotalCount = matchingSearchables.NumFound,
					Highlights = matchingSearchables.Highlights,
					CollapseResults = matchingSearchables.Collapsing,
					SuperSearchables = SuperSearchable.Parse(matchingSearchables),
					Facets = matchingSearchables.FacetFields,
					DidYouMean = GetSpellCheckingResult(matchingSearchables),
					QueryTime = matchingSearchables.Header.QTime,

					PaginationInfo = new PaginationInfo
					{
						PageUrl = Url.SetParameter("page", "!0"),
						CurrentPage = parameters.PageIndex,
						PageSize = parameters.PageSize,
						TotalItemCount = matchingSearchables.NumFound,
					}
				};
				var view = new ExperimentalSearchViewModel()
				{
					Results = results,
				};

				return View(view);
			}
			catch (InvalidFieldException)
			{
				return View(new SearchableView
				{
					QueryError = true,
				});
			}
		}

		/*
		[OutputCache(Duration = 600, VaryByParam = "query")]
		[AcceptVerbs(HttpVerbs.Get)]
		public ActionResult Search(string query)
		{
			query = !string.IsNullOrEmpty(query) ? query.Trim() : null;
			var vm = new SearchResultViewModel 
			{  
				Breadcrumb = new List<Breadcrumb>() { 
					Breadcrumb.Home,
					Breadcrumb.Search
				},
				MetaDescription = "Søgning på Folkets Ting",
				QueryText = query, 
				AdvQuery = new Query {},
				Results = !string.IsNullOrEmpty(query) ? _searchRep.Find(query, 30) : null
			};
			return View("Search", vm);
		}
		 * */

		//[AcceptVerbs(HttpVerbs.Get)]
		//public ActionResult AdvancedSearch(Query advquery)
		//{
		//    string textquery = 
		//        !string.IsNullOrEmpty(advquery.Text) ? advquery.Text.Trim() : null;

		//    if(!string.IsNullOrEmpty(advquery.FromString))
		//        advquery.From = DateTime.Parse(
		//            advquery.FromString, CultureInfo.GetCultureInfo("da-DK"));
		//    if (!string.IsNullOrEmpty(advquery.ToString)) 
		//        advquery.To = DateTime.Parse(
		//            advquery.ToString, CultureInfo.GetCultureInfo("da-DK"));

		//    var vm = new SearchResultViewModel
		//    {
		//        Breadcrumb = new List<Breadcrumb>() { 
		//            Breadcrumb.Home,
		//            Breadcrumb.Search
		//        },
		//        MetaDescription = "Søgning på Folkets Ting",
		//        QueryText = textquery,
		//        AdvQuery = advquery,
		//        Results = _searchRep.AdvancedFind(advquery, 30),
		//    };
		//    return View("Search", vm);
		//}

	}

	public class CollapsedResult
	{
		public CollapseFieldResult CollapseFieldResult { get; set; }
		public IEnumerable<KeyValuePair<Searchable, IDictionary<string, ICollection<string>>>>
			Highlights { get; set; }
	}

	public class ExperimentalSearchViewModel : BaseViewModel
	{
		public SearchableView Results { get; set; }
		public IEnumerable<CollapsedResult> ResultsCollapsed { get; set; }
	}

	public class SearchResultViewModel : BaseViewModel
	{
		public string QueryText { get; set; }
		public Query AdvQuery { get; set; }
		//public IEnumerable<IEnumerable<SearchSubResult>> Results { get; set; }
	}

	public class SearchResultItemViewModel
	{
		public string Query { get; set; }
		public bool IsFirstOfMany { get; set; }
		//public SearchSubResult Item { get; set; }

		public SearchResultItemViewModel()
		{
			this.IsFirstOfMany = false;
		}
	}
}
