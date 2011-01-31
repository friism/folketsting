using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;
using FolketsTing.Views;
using FT.DB;
using FT.Model;

namespace FolketsTing.Controllers
{
	public class PoliticianController : Controller
	{
		const int monthstoshow = 24;
		const int includeposts = 25;

		private readonly IPoliticianRepository _polRep;

		public PoliticianController()
		{
			_polRep = new PoliticianRepository();
		}

		[OutputCache(Duration = 60, VaryByParam = "*", VaryByCustom = "userName")]
		public ActionResult Index()
		{
			var lastweek = DateTime.Now.AddDays(-7);

			var vm = new PoliticianIndexViewModel()
			{
				MostVisited = _polRep.MostVisited(lastweek, 10),
				MostDebated = _polRep.MostDebated(lastweek, 10),
				MostActive = _polRep.MostActive(lastweek, 10),
				Breadcrumb = new List<Breadcrumb>() { 
					Breadcrumb.Home,
					Breadcrumb.PolIndex,
				},
				MetaDescription = "Politikere på Folkets Ting"
			};
			return View("Index", vm);
		}

		[OutputCache(Duration = 60, VaryByParam = "*", VaryByCustom = "userName")]
		public ActionResult All()
		{
			var db = new DBDataContext();
			var vm = new PoliticiansViewModel() { Politicians = db.Politicians.OrderBy(p => p.Firstname) };
			vm.Breadcrumb = new List<Breadcrumb>() { 
					Breadcrumb.Home,
					Breadcrumb.PolIndex,
					new Breadcrumb("Alle politikere", "Politician", "All")};

			vm.MetaDescription = "Alle politikere på Folkets Ting";
			return View("All", vm);
		}

		[OutputCache(Duration = 60, VaryByParam = "*", VaryByCustom = "userName")]
		public ActionResult Details(string polname, int polid)
		{
			var pol = _polRep.GetPoliticianById(polid).Or404();
			var topdebated = _polRep.DebatedFeed(polid, includeposts).ToList();
			var events = _polRep.LatestFeed(polid, includeposts).ToList();
			var activity = _polRep.ActivityStats(polid, monthstoshow);
			var terms = _polRep.LatestSpeechesBlob(polid, 500, 50);

			var res = new PolViewModel()
			{
				Politician = pol,
				MetaDescription = pol.FullName() + (pol.FullName().EndsWith("s") ? "'" : "s") + " side på Folkets Ting",
				Events = events,
				ActivityRows =
					//"",
				"[" + activity.Select(_ =>
					string.Format("{{c:[{{v:{0}}},{{v:{1}}},{{v:{2}}},{{v:{3}}}]}}",
						string.Format("new Date({0},{1},{2})", _.Date.Year, _.Date.Month, _.Date.Day),
						_.Votes,
						_.Speeches,
						_.Questions
						)
					).Aggregate((a, b) => a + "," + b)
				+ "]",
				ActivityCols =
					//"",
				(new JavaScriptSerializer()).Serialize(new object[] { 
					new {id = "month", type = "date", label = "måned" }, 
					new {id = "votes", type = "number", label = "stemmer (måned)" }, 
					new {id = "speeches", type = "number", label= "taler (måned)" }, 
					new {id = "questions", type = "number", label= "§20 spørgsmål (måned)" }, 
				}),
				WordCols =
				(new JavaScriptSerializer()).Serialize(new object[] { 
					new {id = "text", type = "string"}, 
					new {id = "count", type = "number"}, 
				}),
				WordRows = "[" + 
					terms.Shuffle().Select(t =>
						string.Format("{{c:[{{v:\"{0}\"}},{{v:{1}}}]}}", t.Item1, t.Item2)
						).Aggregate((a,b) => a + "," + b)
					 + "]",
				
				Breadcrumb = new List<Breadcrumb>() { 
					Breadcrumb.Home,
					Breadcrumb.PolIndex,
						new Breadcrumb(
						pol.FullName(), 
						"Politician", 
						"Details", 
						new{polname = pol.FullName().ToUrlFriendly(), polid = pol.PoliticianId})
				},
				CountLink = pol.CountLink(this),
				ActivityFeedLink = pol.PolActivityFeedLink(this)
			};

			res.TopDebated = topdebated;
			return View("Details", res);
		}

		[OutputCache(Duration = 600, VaryByParam = "*")]
		public ActionResult ActivityFeed(string polname, int polid)
		{
			var polfeed = _polRep.LatestFeed(polid, includeposts);
			List<SyndicationItem> items = new List<SyndicationItem>();
			var pol = _polRep.GetPoliticianById(polid);
			var polLink = this.Request.Url.GetLeftPart(UriPartial.Authority) + pol.PolLink(this);

			SyndicationFeed feed =
				new SyndicationFeed(
						String.Format("{0} Feed", pol.FullName()), "Activity Feed på Folkets Ting",
						new Uri(Request.Url.ToString()),
						Request.Url.AbsoluteUri,
						polfeed.Last().date);

			foreach (var feeditem in polfeed)
			{
				var link = (this.Request.Url.GetLeftPart(UriPartial.Authority) + feeditem.ActionUrl);
				SyndicationItem item =
					new SyndicationItem(feeditem.ActionText,
									feeditem.BodyText,
									new Uri(link),
									feeditem.ActionUrl + feeditem.ActionText,
									feeditem.date);
				items.Add(item);
			}
			feed.Items = items;

			return new RssActionResult() { Feed = feed };
		}

		public ActionResult Count(string polname, int polid)
		{
			IHitRepository rep = new HitRepository();
			rep.SaveHit(polid, ContentType.Politician, Request.UserHostAddress);

			return new EmptyResult();
		}
	}

	public class PolViewModel : BaseViewModel
	{
		public Politician Politician { get; set; }
		public IEnumerable<FeedEvent> Events { get; set; }
		public IEnumerable<FeedEvent> TopDebated { get; set; }
		public string ActivityCols { get; set; }
		public string ActivityRows { get; set; }
		public string WordCols { get; set; }
		public string WordRows { get; set; }
		public string ActivityFeedLink { get; set; }
	}

	public class PoliticiansViewModel : BaseViewModel
	{
		public IEnumerable<Politician> Politicians { get; set; }
	}

	public class PoliticianIndexViewModel : BaseViewModel
	{
		public IEnumerable<Politician> MostDebated { get; set; }
		public IEnumerable<Politician> MostVisited { get; set; }
		public IEnumerable<Politician> MostActive { get; set; }
	}

	public class PolBoxViewModel
	{
		public Politician Pol { get; set; }
		public string Title { get; set; }
	}

	public class RssActionResult : ActionResult
	{
		public SyndicationFeed Feed { get; set; }

		public override void ExecuteResult(ControllerContext context)
		{
			context.HttpContext.Response.ContentType = "application/rss+xml";

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.NewLineOnAttributes = true;
			Rss20FeedFormatter rssFormatter = new Rss20FeedFormatter(Feed);
			using (XmlWriter writer = XmlWriter.Create(context.HttpContext.Response.Output, settings))
			{
				rssFormatter.WriteTo(writer);
			}
		}
	}

	public static class PoliticianExtensions
	{
		public static string FullName(this Politician p)
		{
			return (p.Firstname ?? "") + " " + (p.Lastname ?? "");
		}

		public static string FTMemberPage(this Politician p)
		{
			return string.Format("http://www.ft.dk/Folketinget/Medlemmer/findMedlem/{0}.aspx", p.Initials);
		}

		public static string CountLink(this Politician p, Controller c)
		{
			return c.Url.Action("Count", "Politician", new
			{
				polname = p.FullName().ToUrlFriendly(),
				polid = p.PoliticianId
			}
			);
		}

		public static string PolLink(this Politician p, Controller c)
		{
			return c.Url.Action("Details", "Politician", new
			{
				polname = p.FullName().ToUrlFriendly(),
				polid = p.PoliticianId
			}
			);
		}

		public static string PolActivityFeedLink(this Politician p, Controller c)
		{
			return c.Url.Action("ActivityFeed", "Politician", new { polname = p.FullName().ToUrlFriendly(), polid = p.PoliticianId });
		}

		public static int Views(this Politician p)
		{
			using (DBDataContext db = new DBDataContext())
			{
				// hopefully, this will discount two in same minute from same IP
				return (from v in db.Hits
						where v.ContentType == ContentType.Politician && v.ContentId == p.PoliticianId
						group v by new { v.IP, v.Date.Date, v.Date.Hour, v.Date.Minute } into v2
						select v2.Key).Count();
			}
		}

		public static int CommentCount(this Politician thep)
		{
			using (DBDataContext db = new DBDataContext())
			{
				return (from c in db.Comments
						join sp in db.SpeechParas on c.ItemId equals sp.SpeechParaId
						join s in db.Speeches on sp.SpeechId equals s.SpeechId
						where s.PoliticianId == thep.PoliticianId &&
							c.CommentType == CommentType.Speech
						select c.CommentId).Concat(
							from c in db.Comments
							join ap in db.AnswerParas on c.ItemId equals ap.AnswerParaId
							join q in db.P20Questions on ap.QuestionId equals q.P20QuestionId
							where q.AskeeId == thep.PoliticianId &&
								c.CommentType == CommentType.Answer
							select c.CommentId).Concat(
								from c in db.Comments
								join q in db.P20Questions on c.ItemId equals q.P20QuestionId
								where (c.CommentType == CommentType.Question ||
									c.CommentType == CommentType.QuestionBackground) &&
									q.AskerPolId == thep.PoliticianId
								select c.CommentId)
						.Count();
			}
		}
	}
}
