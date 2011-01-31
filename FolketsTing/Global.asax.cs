using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Threading;
using System.Globalization;
using FT.Search;
using FT.Search.Binders;
using SolrNet;

namespace FolketsTing
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			//routes.IgnoreRoute("elmah.axd");
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });

			routes.MapRoute(
				"Front",                                              // Route name
				"",                           // URL with parameters
				new { controller = "Home", action = "Index" }  // Parameter defaults
			);

			routes.MapRoute(
				"About",                                              // Route name
				"igang",                           // URL with parameters
				new { controller = "Home", action = "Howto" }  // Parameter defaults
			);

			routes.MapRoute(
				"Howto",                                              // Route name
				"om",                           // URL with parameters
				new { controller = "Home", action = "About" }  // Parameter defaults
			);

			routes.MapRoute(
				"StopWords",                                              // Route name
				"util/stopwords",                           // URL with parameters
				new { controller = "Util", action = "StopWords" }  // Parameter defaults
			);

			routes.MapRoute(
				"Terms",                                              // Route name
				"betingelser",                           // URL with parameters
				new { controller = "Home", action = "Terms" }  // Parameter defaults
			);

			routes.MapRoute(
				"LiveDebate",                                              // Route name
				"live",                           // URL with parameters
				new { controller = "Home", action = "LiveDebate" }  // Parameter defaults
			);

			routes.MapRoute(
				"Contact",                                              // Route name
				"kontakt",                           // URL with parameters
				new { controller = "Home", action = "Contact" }  // Parameter defaults
			);

			routes.MapRoute(
				"LawIndex",                                              // Route name
				"love",                           // URL with parameters
				new { controller = "Law", action = "Index" }  // Parameter defaults
			);

			routes.MapRoute(
				"LawAll",                                              // Route name
				"love/alle",                           // URL with parameters
				new { controller = "Law", action = "All" }  // Parameter defaults
			);

			routes.MapRoute(
				"LawFeeds",                                              // Route name
				"love/feed/{feedtype}",                           // URL with parameters
				new { controller = "Law", action = "Feed" }  // Parameter defaults
			);

			routes.MapRoute(
				"LawDetails",                                              // Route name
				"love/{lawname}/{lawid}",                           // URL with parameters
				new { controller = "Law", action = "Details" }  // Parameter defaults
			);

			routes.MapRoute(
				"LawCount",                                              // Route name
				"love/{lawname}/{lawid}/count",                           // URL with parameters
				new { controller = "Law", action = "Count" }  // Parameter defaults
			);

			routes.MapRoute(
				"LawDeliberation",                                              // Route name
				"love/{lawname}/{lawid}/behandling/{deliberationnr}",                           // URL with parameters
				new { controller = "Law", action = "Deliberation" }  // Parameter defaults
			);

			routes.MapRoute(
				"LawText",                                              // Route name
				"love/{lawname}/{lawid}/lovtekst/{stage}",                           // URL with parameters
				new { controller = "Law", action = "LawText" }  // Parameter defaults
			);

			routes.MapRoute(
				"LawVotes",                                              // Route name
				"love/{lawname}/{lawid}/afstemninger",                           // URL with parameters
				new { controller = "Law", action = "Votes" }  // Parameter defaults
			);

			routes.MapRoute(
				"PolAll",                                              // Route name
				"politikere/alle",                           // URL with parameters
				new { controller = "Politician", action = "All" }  // Parameter defaults
			);

			routes.MapRoute(
				"PolIndex",                                              // Route name
				"politikere",                           // URL with parameters
				new { controller = "Politician", action = "Index" }  // Parameter defaults
			);

			routes.MapRoute(
				"PolDetails",                                              // Route name
				"politikere/{polname}/{polid}",                           // URL with parameters
				new { controller = "Politician", action = "Details" }  // Parameter defaults
			);

			routes.MapRoute(
				"PolActivityFeed",                                              // Route name
				"politikere/{polname}/{polid}/activityfeed",                           // URL with parameters
				new { controller = "Politician", action = "ActivityFeed" }  // Parameter defaults
			);

			routes.MapRoute(
				"PolCount",                                              // Route name
				"politikere/{polname}/{polid}/count",                           // URL with parameters
				new { controller = "Politician", action = "Count" }  // Parameter defaults
			);

			routes.MapRoute(
				"GetComments",                                              // Route name
				"Comments/Comments/{itemid}/{commenttype}",    // URL with parameters
				new { controller = "Comment", action = "Comments" }  // Parameter defaults
			);

			routes.MapRoute(
				"ParagraphComments",                                              // Route name
				"Comments/Paragraph/{parid}",                           // URL with parameters
				new { controller = "Comment", action = "ParagraphComments" }  // Parameter defaults
			);

			routes.MapRoute(
				"SectionComments",                                              // Route name
				"Comments/Section/{secid}",                           // URL with parameters
				new { controller = "Comment", action = "SectionComments" }  // Parameter defaults
			);

			routes.MapRoute(
				"LawChangeComments",                                              // Route name
				"Comments/LawChange/{chaid}",                           // URL with parameters
				new { controller = "Comment", action = "LawChangeComments" }  // Parameter defaults
			);

			routes.MapRoute(
				"AccountLogonNU",                                              // Route name
				"konto/logon/",                           // URL with parameters
				new { controller = "Account", action = "LogOn" }  // Parameter defaults
			);

			//routes.MapRoute(
			//    "AccountRegisterNU",                                              // Route name
			//    "konto/registrer/",                           // URL with parameters
			//    new { controller = "Account", action = "Register" }  // Parameter defaults
			//);

			routes.MapRoute(
				"UserDetails",                                              // Route name
				"brugere/{uname}",                           // URL with parameters
				new { controller = "User", action = "Details" }  // Parameter defaults
			);

			routes.MapRoute(
				"FindTags",
				"tags/find/",
				new { controller = "Tag", action = "Find" }
			);

			routes.MapRoute(
				"TagDetails",
				"tags/{tag}",
				new { controller = "Tag", action = "Details" }
			);

			routes.MapRoute(
				"TagIndex",
				"tags",
				new { controller = "Tag", action = "Index" }
			);

			routes.MapRoute(
				"P20Index",                                              // Route name
				"p20spoergsmaal",                           // URL with parameters
				new { controller = "P20Question", action = "Index" }  // Parameter defaults
			);
			routes.MapRoute(
				"P20LatestAsked",                                              // Route name
				"p20spoergsmaal/feed",                           // URL with parameters
				new { controller = "P20Question", action = "activityfeed" }  // Parameter defaults
			);

			routes.MapRoute(
				"P20Details",
				"p20spoergsmaal/{questiontext}/{qid}",
				new { controller = "P20Question", action = "Details" }
			);

			routes.MapRoute(
				"NewP20",
				"p20spoergsmaal/foreslaa",
				new { controller = "P20Question", action = "New" }
			);

			routes.MapRoute(
				"P20All",                                              // Route name
				"p20spoergsmaal/alle",                           // URL with parameters
				new { controller = "P20Question", action = "All" }  // Parameter defaults
			);

			routes.MapRoute(
				"P20Count",                                              // Route name
				"p20spoergsmaal/{questiontext}/{qid}/count",                           // URL with parameters
				new { controller = "P20Question", action = "Count" }  // Parameter defaults
			);

			routes.MapRoute(
				"GetImage",                                              // Route name
				"filer/billeder/{imagename}/{imageid}",                           // URL with parameters
				new { controller = "File", action = "GetImage" }  // Parameter defaults
			);
			
			routes.MapRoute(
				"GetScaledImage",                                              // Route name
				"filer/billeder/skaleret/{imagename}/{imageid}",                           // URL with parameters
				new { controller = "File", action = "GetScaledImage" }  // Parameter defaults
			);
			
			routes.MapRoute(
				"BasicSearch",                                              // Route name
				"soegning",                           // URL with parameters
				new { controller = "Search", action = "Search" }  // Parameter defaults
			);

			routes.MapRoute(
				"AdvancedSearch",                                              // Route name
				"soegning/avanceret",                           // URL with parameters
				new { controller = "Search", action = "AdvancedSearch" }  // Parameter defaults
			);

			routes.MapRoute(
				"ExperimentalSearch",                                              // Route name
				"soegning/experimental",                           // URL with parameters
				new { controller = "Search", action = "ExperimentalSearch" }  // Parameter defaults
			);

			routes.MapRoute(
				"NewApiUser",                                              // Route name
				"api/nykey",                           // URL with parameters
				new { controller = "ApiRegistration", action = "New" }  // Parameter defaults
			);

			routes.MapRoute(
				"ApiExample",                                              // Route name
				"api/eksempel",                           // URL with parameters
				new { controller = "ApiRegistration", action = "Example" }  // Parameter defaults
			);

			routes.MapRoute(
				"ApiUserCreated",                                              // Route name
				"api/keyoprettet",                           // URL with parameters
				new { controller = "ApiRegistration", action = "Created" }  // Parameter defaults
			);

			routes.MapRoute(
				 "TravelTest",                                              // Route name
				 "test/traveltest",                           // URL with parameters
				 new { controller = "Test", action = "TravelTest" }  // Parameter defaults
			 );

			routes.MapRoute(
				"Default",                                              // Route name
				"{controller}/{action}/{id}",                           // URL with parameters
				new { controller = "Home", action = "Index", id = "" }  // Parameter defaults
			);
		}

		protected void Application_Start()
		{
			RegisterRoutes(RouteTable.Routes);
			//RouteDebug.RouteDebugger.RewriteRoutesForTesting(RouteTable.Routes);
			Startup.Init<Searchable>("http://localhost:8983/solr");

			ModelBinders.Binders[typeof(SearchParameters)] = new SearchParametersBinder();
		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{
			Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("da-dk");
		}

		public override string GetVaryByCustomString(HttpContext context, string arg)
		{
			if (arg == "userName")
			{
				return context.User.Identity.Name;
			}
			return string.Empty;
		}
	}
}