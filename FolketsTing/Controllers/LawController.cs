using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using FolketsTing.Views;
using FT.DB;
using FT.Model;

namespace FolketsTing.Controllers
{
	public class LawController : Controller
	{
		private readonly ITagRepository _tagRep;
		private readonly ILawRepository _lawRep;
		private readonly IHitRepository _hitRep;
		public LawController()
		{
			_tagRep = new TagRepository();
			_lawRep = new LawRepository();
			_hitRep = new HitRepository();
		}

		public ActionResult ChartTest()
		{
			return View("ChartTest");
		}


		[OutputCache(Duration = 60, VaryByParam = "*", VaryByCustom = "userName")]
		public ActionResult Index()
		{
			var includelaws = 3;
			var lastweek = DateTime.Now.AddDays(-7);

			var vm = new LawIndexViewModel()
			{
				Proposed = _lawRep.RecentProposed(includelaws),
				Voted = _lawRep.RecentPassed(includelaws),
				First = _lawRep.RecentInDeliberation(LawStage.First, includelaws),
				Second = _lawRep.RecentInDeliberation(LawStage.Second, includelaws),
				Third = _lawRep.RecentInDeliberation(LawStage.Third, includelaws),
				Debated = _lawRep.TopDebated(lastweek, includelaws),
				Pop = _lawRep.Popular(lastweek, includelaws),//laws.OrderByDescending(l => l.WeekVisits).Take(10),
				Breadcrumb = new List<Breadcrumb>() { 
					Breadcrumb.Home,
					Breadcrumb.LawIndex,
				},
				MetaDescription = "Love på Folkets Ting",
				RecentlyProposedFeedLink = this.Url.Action("Feed", "Law", new { feedtype = "senest-fremsatte" }),
				RecentlyVotedFeedLink = this.Url.Action("Feed", "Law", new { feedtype = "senest-vedtagne" })
			};

			return View("Index", vm);
		}

		[OutputCache(Duration = 60, VaryByParam = "*", VaryByCustom = "userName")]
		public ActionResult All()
		{
			var laws = new LawsViewModel()
			{
				// get 'em all
				Laws = _lawRep.RecentProposed(int.MaxValue),
				MetaDescription = "Se alle love på Folkets Ting",
				Breadcrumb = new List<Breadcrumb>() { 
					Breadcrumb.Home,
					Breadcrumb.LawIndex,
					new Breadcrumb("Alle love", "Law", "Alle"),
				}
			};
			return View("All", laws);
		}

		//[OutputCache(Duration = 60, VaryByParam = "none", VaryByCustom = "userName")]
		public ActionResult Details(string lawname, int lawid)
		{
			var law = _lawRep.SingleLaw(lawid).Or404();

			var val = new LawDetailsViewModel()
			{
				Law = law,
				Breadcrumb = new List<Breadcrumb>() { 
					Breadcrumb.Home,
					Breadcrumb.LawIndex,
					law.Breadcrumb(),
				},
				MetaDescription = law.ShortName + " på Folkets Ting"
			};

			var tagvm = new TagControlViewModel(_tagRep, lawid, ContentType.Law)
			{
				ElementName = lawname,
			};

			val.TagVM = tagvm;

			if(this.User() != null)
				val.TagVM.UserTags = _tagRep.UserTags(lawid, this.User().UserId, ContentType.Law);

			var finalvote = law.LawVotes.Where(_ => _.IsFinal == true).FirstOrDefault();
			if (finalvote != null)
			{
				val.PieChartData = new PieChartViewModel()
				{
					PartyVotes = ViewConstants.GetPartyVotes(finalvote.LawVoteId).ToList(),
					LawVoteId = finalvote.LawVoteId
				};
				val.FinalVoteDate = finalvote.Date.Value;
				val.Vote = finalvote;
			}

			if (_lawRep.IsInStage(LawStage.First, lawid))
			{
				val.FirstNegot = _lawRep.SingleDeliberation(lawid, LawStage.First);
				val.FirstCommCount = _lawRep.DeliberationCommentCount(val.FirstNegot.DeliberationId);
			}
			if (_lawRep.IsInStage(LawStage.Second, lawid))
			{
				val.SecondNegot = _lawRep.SingleDeliberation(lawid, LawStage.Second);
				val.SecondCommCount = _lawRep.DeliberationCommentCount(val.SecondNegot.DeliberationId);
			}
			if (_lawRep.IsInStage(LawStage.Third, lawid))
			{
				val.ThirdNegot = _lawRep.SingleDeliberation(lawid, LawStage.Third);
				val.ThirdCommCount = _lawRep.DeliberationCommentCount(val.ThirdNegot.DeliberationId);
			}

			val.IsProposed = _lawRep.IsProposed(lawid);
			val.IsPassed = _lawRep.IsPassed(lawid);
			val.IsAfterSecond = _lawRep.IsAfterSecond(lawid);

			if (val.IsProposed)
				val.ProposedCommCount = _lawRep.CommentCount(LawStage.First, lawid);

			if (val.IsAfterSecond)
				val.AfterSecCommCount = _lawRep.CommentCount(LawStage.Second, lawid);

			if (val.IsPassed)
				val.PassedCommCount = _lawRep.CommentCount(LawStage.Third, lawid);

			val.MetaDescription = law.Summary;
			val.CountLink = law.CountLink(this);

			val.Views = _hitRep.ViewCount(lawid, ContentType.Law);
			val.LatestActivity = _lawRep.LatestActivity(lawid);

			return View("Details", val);
		}

		[OutputCache(Duration = 60, VaryByParam = "*", VaryByCustom = "userName")]
		public ActionResult Deliberation(string lawname, int lawid, int deliberationnr)
		{
			LawStage stage;
			switch (deliberationnr)
			{
				case 1: stage = LawStage.First; break;
				case 2: stage = LawStage.Second; break;
				case 3: stage = LawStage.Third; break;
				default: throw new ArgumentException();
			}

			var law = _lawRep.SingleLaw(lawid).Or404();
			var del = law.Deliberations.Single(_ => _.Number == stage);

			var res = new DeliberationViewModel()
			{
				Deliberation = del,
				Law = law,
				Breadcrumb = new List<Breadcrumb>() { 
					Breadcrumb.Home,
					Breadcrumb.LawIndex,
					law.Breadcrumb(),
					new Breadcrumb(
						string.Format("{0}. behandling", deliberationnr), 
						"Law", 
						"Deliberation", 
						new { lawname = lawname, lawid = lawid, deliberationnr = deliberationnr}),
				},
				MetaDescription = law.ShortName + " på Folkets Ting",
			};

			return View("Deliberation", res);
		}

		[OutputCache(Duration = 60, VaryByParam = "*", VaryByCustom = "userName")]
		public ActionResult LawText(string lawname, int lawid, string stage)
		{
			var law = _lawRep.SingleLaw(lawid).Or404();

			LawStage lawstage;
			string Stagestring;
			switch (stage.ToLower())
			{
				case "fremsat": lawstage = LawStage.First; Stagestring = "som fremsat"; break;
				case "vedtaget": lawstage = LawStage.Third; Stagestring = "som vedtaget"; break;
				case "aftersec": lawstage = LawStage.Second; Stagestring = "efter 2. behandling"; break;
				default: throw new ArgumentException("Unknown stage: " + stage);
			}
			return View("LawText", new LawTextViewModel()
			{
				Law = law,
				Stage = lawstage,
				StageString = Stagestring,
				Breadcrumb = new List<Breadcrumb>() { 
					Breadcrumb.Home,
					Breadcrumb.LawIndex,
					law.Breadcrumb(),
					new Breadcrumb(
						Stagestring, 
						"Law", 
						"LawText", 
						new{lawname = lawname, lawid = lawid, stage = stage}),
				},
				MetaDescription = law.ShortName + " på Folkets Ting",
			});
		}
		
		[OutputCache(Duration = 60, VaryByParam = "*")]
		public ActionResult Feed(string feedtype)
		{
			List<SyndicationItem> items = new List<SyndicationItem>();
			DateTime? lastupdated;
			string title;

			IQueryable<Law> laws;
			switch (feedtype)
			{
				case ("senest-vedtagne"):
					title = "Senest vedtagne love";
					laws = _lawRep.RecentPassed(10);
					lastupdated = laws.First().Passed;
					break;
				default:
					title = "Senest fremsatte love";
					laws = _lawRep.RecentProposed(10);
					lastupdated = laws.First().Proposed;
					break;
			}

			SyndicationFeed feed =
				new SyndicationFeed(
						title, "Activitets feed på Folkets Ting",
						new Uri(Request.Url.ToString()),
						Request.Url.AbsoluteUri,
						lastupdated.Value);

			foreach (var feeditem in laws)
			{
				var link = (this.Request.Url.GetLeftPart(UriPartial.Authority) + feeditem.DetailsLink(this));
				SyndicationItem item =
					new SyndicationItem(feeditem.ShortName,
									feeditem.Summary,
									new Uri(link),
									feeditem.LawId + "-" + feeditem.FtId,
									feeditem.Proposed.Value);
				items.Add(item);
			}
			feed.Items = items;

			return new RssActionResult() { Feed = feed };
		}

		[OutputCache(Duration = 60, VaryByParam = "*", VaryByCustom = "userName")]
		public ActionResult Votes(string lawname, int lawid)
		{
			var law = _lawRep.SingleLaw(lawid).Or404();

			var res = new LawVoteViewModel()
			{
				Law = law,
				Breadcrumb = new List<Breadcrumb>() { 
					Breadcrumb.Home,
					Breadcrumb.LawIndex,
					law.Breadcrumb(),
					new Breadcrumb(
						"afstemning", 
						"Law", 
						"Votes", 
						new { lawname = lawname, lawid = lawid } ),
				},
				MetaDescription = law.ShortName + " på Folkets Ting",
			};

			return View("Votes", res);
		}

		public ActionResult Count(string lawname, int lawid)
		{
			_hitRep.SaveHit(lawid, ContentType.Law, Request.UserHostAddress);

			return new EmptyResult();
		}
	}

	public class DeliberationViewModel : BaseViewModel
	{
		public Law Law { get; set; }
		public Deliberation Deliberation { get; set; }
	}

	public class LawViewModel : BaseViewModel
	{
		public Law Law { get; set; }
		public int Comments { get; set; }
	}

	public class LawIndexViewModel : BaseViewModel
	{
		public IEnumerable<Law> First { get; set; }
		public IEnumerable<Law> Second { get; set; }
		public IEnumerable<Law> Third { get; set; }
		public IEnumerable<Law> Pop { get; set; }
		public IEnumerable<Law> Debated { get; set; }
		public IEnumerable<Law> Proposed { get; set; }
		public IEnumerable<Law> Voted { get; set; }
		public string RecentlyProposedFeedLink { get; set; }
		public string RecentlyVotedFeedLink { get; set; }
	}

	public class PieChartViewModel
	{
		public int LawVoteId { get; set; }
		public IEnumerable<PartyVote> PartyVotes { get; set; }
	}

	public class LawDetailsViewModel : BaseViewModel
	{
		public Law Law { get; set; }
		public Deliberation FirstNegot { get; set; }
		public Deliberation SecondNegot { get; set; }
		public Deliberation ThirdNegot { get; set; }
		public PieChartViewModel PieChartData { get; set; }
		public DateTime? FinalVoteDate { get; set; }
		public LawVote Vote { get; set; }

		public TagControlViewModel TagVM { get; set; }
		
		public bool IsProposed { get; set; }
		public bool IsPassed { get; set; }
		public bool IsAfterSecond { get; set; }

		public int? ProposedCommCount { get; set; }
		public int? FirstCommCount { get; set; }
		public int? SecondCommCount { get; set; }
		public int? ThirdCommCount { get; set; }
		public int? PassedCommCount { get; set; }
		public int? AfterSecCommCount  { get; set; }

		public int Views { get; set; }
		public DateTime LatestActivity { get; set; }
	}

	public class LawTextViewModel : BaseViewModel
	{
		public Law Law { get; set; }
		public LawStage Stage { get; set; }
		public string StageString { get; set; }
	}

	public class LawVoteViewModel : BaseViewModel
	{
		public Law Law { get; set; }
	}

	public class LawsViewModel : BaseViewModel
	{
		public IEnumerable<Law> Laws { get; set; }
	}

	public static class Extensions
	{
		public static string CountLink(this Law l, Controller c)
		{
			return c.Url.Action("Count", "Law", new
			{
				lawname = l.ShortName.ToUrlFriendly(),
				lawid = l.LawId
			}
			);
		}

		public static string DetailsLink(this Law l, Controller c)
		{

			return c.Url.Action("Details", "Law", new
			{
				lawname = l.ShortName.ToUrlFriendly(),
				lawid = l.LawId
			}
			);
		}

		public static string LinkTo(this Speech s, Controller c)
		{
			return c.Url.Action("Deliberation", "Law",
				new
			{
				lawname = s.Deliberation.Law.ShortName.ToUrlFriendly(),
				lawid = s.Deliberation.Law.LawId,
				deliberationnr = s.Deliberation.Number
			}
			) + "#par-" + s.SpeechParas.OrderBy(sp => sp.Number).First().SpeechParaId
			;
		}

		public static string LinkTo(this Section s, Controller c)
		{
			return c.Url.Action("Lawtext", "Law",
				new
				{
					lawname = s.Paragraph.Law.ShortName.ToUrlFriendly(),
					lawid = s.Paragraph.Law.LawId,
					stage = GetStageString((int)s.Paragraph.Stage)
				}
			);
		}

		public static string LinkTo(this LawChange s, Controller c)
		{
			return c.Url.Action("Lawtext", "Law",
				new
				{
					lawname = s.Paragraph.Law.ShortName.ToUrlFriendly(),
					lawid = s.Paragraph.Law.LawId,
					stage = GetStageString((int)s.Paragraph.Stage)
				}
			);
		}

		public static string LinkTo(this P20Question s, Controller c)
		{
			return c.Url.Action("Details", "P20Question",
				new
				{
					questiontext = s.Title.ToUrlFriendly(),
					qid = s.P20QuestionId
				}
			);
		}

		private static string GetStageString(int number)
		{
			switch (number)
			{
				case 0: return "som fremsat";
				case 1: return "som vedtaget";
				case 2: return "efter 2. behandling";
				default: throw new ArgumentException("unkown stage: " + number);
			}
		}

		public static int Views(this Law l)
		{
			return (new HitRepository()).ViewCount(l.LawId, ContentType.Law);
		}

		public static int CommentCount(this LawChange thec)
		{
			using (DBDataContext db = new DBDataContext())
			{
				return db.Comments.Count(c =>
						c.ItemId == thec.LawChangeId &&
						c.CommentType == CommentType.Change);
			}
		}

		public static int CommentCount(this Section thes)
		{
			using (DBDataContext db = new DBDataContext())
			{
				return db.Comments.Count(c =>
					c.ItemId == thes.SectionId &&
						c.CommentType == CommentType.Section);
			}
		}

		public static int CommentCount(this SpeechPara thesp)
		{
			using (DBDataContext db = new DBDataContext())
			{
				return db.Comments.Count(c =>
						c.ItemId == thesp.SpeechParaId &&
							c.CommentType == CommentType.Speech);
			}
		}

		public static int CommentCount(this Law thel)
		{
			return (new LawRepository()).CommentCount(thel.LawId);
		}

		public static Breadcrumb Breadcrumb(this Law l)
		{
			return new Breadcrumb(
						l.ShortName,
						"Law",
						"Details",
						new { lawname = l.ShortName.ToUrlFriendly(), lawid = l.LawId });
		}
	}
}
