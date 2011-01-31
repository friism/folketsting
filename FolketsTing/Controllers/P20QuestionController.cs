using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using FolketsTing.Views;
using FT.DB;
using FT.Model;
using xVal.ServerSide;

namespace FolketsTing.Controllers
{
	public class P20QuestionController : Controller
	{
		private readonly IP20QuestionRepository _p20Rep;
		private readonly IHitRepository _hitRep;
		private readonly ITagRepository _tagRep;

		public P20QuestionController()
		{
			_p20Rep = new P20QuestionRepository();
			_hitRep = new HitRepository();
			_tagRep = new TagRepository();
		}

		[OutputCache(Duration = 60, VaryByParam = "*", VaryByCustom = "userName")]
		public ActionResult Index()
		{
			var includeqa = 5;
			var lastweek = DateTime.Now.AddDays(-7);

			var vm = new P20QuestionIndexViewModel()
			{
				LatestAnsweredByParliament = _p20Rep.LatestAnsweredByParliament(includeqa),
				LatestByParliament = _p20Rep.LatestByParliament(includeqa),
				LatestByPeople = _p20Rep.LatestByPeople(includeqa),
				PopularByPeople = _p20Rep.PopularByPeople(lastweek, includeqa),
				Debated = _p20Rep.Debated(lastweek, includeqa),
				//Popular = _p20Rep.Popular(lastweek, includeqa),
				Breadcrumb = new List<Breadcrumb>() { 
					Breadcrumb.Home,
					Breadcrumb.P20Index},
				MetaDescription = "§20 spørgsmål på Folkets Ting",
			};
			return View("Index", vm);
		}

		[OutputCache(Duration = 600, VaryByParam = "*")]
		public ActionResult ActivityFeed()
		{
			var p20feed = _p20Rep.Recent(25);
			List<SyndicationItem> items = new List<SyndicationItem>();

			SyndicationFeed feed =
				new SyndicationFeed(
						"Senest stillede §20 Spørgsmål", 
						"§20 spørgsmål kan stilles af folketingspolitikere til regeringens ministre.",
						new Uri(Request.Url.ToString()),
						Request.Url.AbsoluteUri,
						Convert.ToDateTime(p20feed.First().AskDate));

			foreach (var feeditem in p20feed)
			{
				var link = (this.Request.Url.GetLeftPart(UriPartial.Authority) + feeditem.DetailsLink(this));
				SyndicationItem item =
					new SyndicationItem(feeditem.Title,
									feeditem.Question,
									new Uri(link),
									feeditem.FTId.ToString(),
									Convert.ToDateTime(feeditem.AskDate));
				items.Add(item);
			}
			feed.Items = items;

			return new RssActionResult() { Feed = feed };
		}

		[CaptchaValidator]
		[AcceptVerbs(HttpVerbs.Post)]
		[Authorize]
		public ActionResult New(
				[Bind(Prefix = "Question", Include = "Title,Question,Background,AskeeTitle")] 
					P20Question question,
				bool captchaValid
			)
		{
			if (!captchaValid)
			{
				ViewData.ModelState.AddModelError("captcha", "CAPTCHA invalid");
			}

			question.Type = QuestionType.User;
			question.AskerUserId = this.User().UserId;
			question.AskDate = DateTime.Now;

			if (string.IsNullOrEmpty(question.AskeeTitle))
			{
				ViewData.ModelState.AddModelError("Question.AskeeTitle", "No title selected");
			}
			else
			{
				// set the askee to be the most likely politician
				question.AskeeId = _p20Rep.LatestWithTitle(question.AskeeTitle, DateTime.Now);
			}

			// TODO, replace with "current session or something"
			question.SessionId = int.Parse(ConfigurationManager.AppSettings["CurrentSessionId"]);

			try
			{
				_p20Rep.ValidateQuestion(question);
			}
			catch (RulesException ex)
			{
				ex.AddModelStateErrors(ModelState, "question");
			}

			if (!ModelState.IsValid)
			{
				// try again
				return View("New", new NewP20QuestionViewModel()
				{
					Ministrys = _p20Rep.Ministrys(),
					MetaDescription = " Foreslå §20 spørgsmål på Folkets Ting",
					Breadcrumb = new List<Breadcrumb>()
					{
						Breadcrumb.Home,
						Breadcrumb.P20Index,
						new Breadcrumb("Foreslå §20 spørgsmål","P20Question", "New"),
					},
					Question = question,
					LatestQuestions = _p20Rep.LatestByPeople(10),
				});
			}
			else
			{
				// create the bugger
				question = _p20Rep.CreateQuestion(question);

				return RedirectToAction("Details", "P20Question", 
					new
						{
							questiontext = question.Question.ToUrlFriendly(),
							qid = question.P20QuestionId
						});
			}
		}

		[OutputCache(Duration = 60, VaryByParam = "*", VaryByCustom = "userName")]
		[AcceptVerbs(HttpVerbs.Get)]
		public ActionResult New()
		{
			var vm = new NewP20QuestionViewModel()
				{
					Ministrys = _p20Rep.Ministrys(),
					MetaDescription = "Foreslå §20 spørgsmål på Folkets Ting",
					Breadcrumb = new List<Breadcrumb>()
					{
						Breadcrumb.Home,
						Breadcrumb.P20Index,
						new Breadcrumb("Foreslå §20 spørgsmål","P20Question", "New"),
					},
					Question = new P20Question(),
					LatestQuestions = _p20Rep.LatestByPeople(10),
				};
			return View("New", vm);
		}

		[OutputCache(Duration = 60, VaryByParam = "*", VaryByCustom = "userName")]
		public ActionResult All()
		{
			var vm = new AllP20ViewModel()
			{
				Questions = _p20Rep.Recent(int.MaxValue),
				Breadcrumb = new List<Breadcrumb>() { 
					Breadcrumb.Home,
					Breadcrumb.P20Index,
					new Breadcrumb(
						"Alle §20 spørgsmål", 
						"P20Question", 
						"All"),
				},
				MetaDescription = "Alle §20 spørgsmål på Folkets Ting",
			};
			return View("All", vm);
		}

		public ActionResult Details(string questiontext, int qid)
		{
			P20Question q = _p20Rep.GetQuestion(qid).Or404();

			TagControlViewModel tagvm = new TagControlViewModel(_tagRep, qid, ContentType.P20Question)
			{
				ElementName = questiontext,
			};

			if (this.User() != null)
				tagvm.UserTags = _tagRep.UserTags(qid, this.User().UserId, ContentType.P20Question);

			return View("Details", new P20QuestionDetailViewModel()
			{
				Question = q,
				QuestionCommentCount = _p20Rep.QuestionCommentCount(qid),
				BackgroundCommentCount = _p20Rep.BackgroundCommentCount(qid),
				CountLink = q.CountLink(this),
				LatestActivity = _p20Rep.LatestActivity(qid),
				TagVM = tagvm,
				Breadcrumb = new List<Breadcrumb>() { 
					Breadcrumb.Home,
					Breadcrumb.P20Index,
					new Breadcrumb(
						q.Question.Truncate(100, true), 
						"P20Question", 
						"Details", 
						new{lawname = questiontext, lawid = qid}),
				},
				MetaDescription = q.Question.Truncate(100, true) + " på Folkets Ting",
			}
			);
		}

		public ActionResult Count(string questiontext, int qid)
		{
			_hitRep.SaveHit(qid, ContentType.P20Question, Request.UserHostAddress);
			return new EmptyResult();
		}
	}

	public static class P20Extensions
	{
		private static IHitRepository _hitRep = new HitRepository();
		public static int CommentCount(this AnswerPara p)
		{
			DBDataContext db = new DBDataContext();
			return db.Comments.Count(c => c.ItemId == p.AnswerParaId &&
						c.CommentType == CommentType.Answer);
		}

		public static string CountLink(this P20Question q, Controller c)
		{
			return c.Url.Action("Count", "P20Question", new
			{
				questiontext = q.Question.ToUrlFriendly(),
				qid = q.P20QuestionId
			}
			);
		}

		public static int Views(this P20Question q)
		{
			return _hitRep.ViewCount(q.P20QuestionId, ContentType.P20Question);
		}

		public static int CommentCount(this P20Question q)
		{
			DBDataContext db = new DBDataContext();
			return
				(
				from c in db.Comments
				join ap in db.AnswerParas on c.ItemId equals ap.AnswerParaId
				where ap.QuestionId == q.P20QuestionId && c.CommentType == CommentType.Answer
				select c.CommentId
				).Concat(
				from c in db.Comments
				where c.ItemId == q.P20QuestionId &&
					(c.CommentType == CommentType.Question || c.CommentType == CommentType.QuestionBackground)
				select c.CommentId
				).Count();
		}

		public static string DetailsLink(this P20Question q, Controller c)
		{
			return c.Url.Action("Details", "P20Question",
				new
				{
					questiontext = q.Title.ToUrlFriendly(),
					qid = q.P20QuestionId
				});
		}
	}

	public class P20QuestionIndexViewModel : BaseViewModel
	{
		public IQueryable<P20Question> LatestByParliament { get; set; }
		public IQueryable<P20Question> LatestAnsweredByParliament { get; set; }
		public IQueryable<P20Question> LatestByPeople { get; set; }
		public IQueryable<P20Question> PopularByPeople { get; set; }
		public IQueryable<P20Question> Debated { get; set; }
		
	}

	public class P20QuestionDetailViewModel : BaseViewModel
	{
		public P20Question Question { get; set; }
		public int QuestionCommentCount { get; set; }
		public int BackgroundCommentCount { get; set; }
		public DateTime LatestActivity { get; set; }

		public TagControlViewModel TagVM { get; set; }
	}

	public class NewP20QuestionViewModel : BaseViewModel
	{
		public IEnumerable<string> Ministrys { get; set; }
		public P20Question Question { get; set; }
		public IQueryable<P20Question> LatestQuestions { get; set; }
	}

	public class AllP20ViewModel : BaseViewModel
	{
		public IQueryable<P20Question> Questions { get; set; }
	}

	public class P20QListViewModel
	{
		public enum RenderMode { TitleOnly, QuestionText};
		
		public RenderMode Mode { get; set; }
		public IEnumerable<P20Question> Questions { get; set; }
	}
}
