using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FolketsTing.Views;
using FT.DB;
using FT.Model;

namespace FolketsTing.Controllers
{
	public class CommentController : Controller
	{
		private readonly ICommentRepository _comRep;
		private readonly ILawRepository _lawRep;
		private readonly IP20QuestionRepository _p20Rep;

		public CommentController()
		{
			_comRep = new CommentRepository();
			_lawRep = new LawRepository();
			_p20Rep = new P20QuestionRepository();
		}

		public JsonResult GetFeedObject(CommentFeedObject comment)
		{
			var responseObject = new FacebookFeedHandler().GetResponse(comment);
			if (responseObject != null)
				return Json(responseObject);
			else
				return null;
		}

		public ActionResult Comments(int itemid, CommentType commenttype, string returnurl)
		{
			HttpCachePolicyBase cache = HttpContext.Response.Cache;
			cache.SetCacheability(HttpCacheability.NoCache);
			cache.SetExpires(DateTime.Now.AddDays(-1));

			return View("Comments", new CommentsViewModel()
				{
					Comments = GetAllComments(itemid, commenttype),
					ElementId = itemid,
					Type = commenttype,
					CurrentUrl = returnurl,
				}
				);
		}

		private string GetStageString(int number)
		{
			switch (number)
			{
				case 0: return "fremsat";
				case 1: return "vedtaget";
				default: throw new ArgumentException("unkown stage: " + number);
			}
		}

		[AcceptVerbs(HttpVerbs.Post)]
		[Authorize]
		public ActionResult VoteNo(int lawvoteid)//, string lawname, int lawid)
		{
			return HandleLawVote(lawvoteid, 1);

		}

		[AcceptVerbs(HttpVerbs.Post)]
		[Authorize]
		public ActionResult VoteYes(int lawvoteid)//, string lawname, int lawid)
		{
			return HandleLawVote(lawvoteid, 0);
		}

		private RedirectToRouteResult HandleLawVote(int lawvoteid, int vote)
		{
			var db = new DBDataContext();
			var lv = db.LawVotes.Single(_ => _.LawVoteId == lawvoteid);

			var thevote = db.UserLawVotes.
				SingleOrDefault(_ => _.LawVoteId == lawvoteid && _.UserId == this.User().UserId);
			if (thevote != null)
			{
				// update
				thevote.Date = DateTime.Now;
				thevote.Vote = (byte)vote;
			}
			else
			{
				db.UserLawVotes.InsertOnSubmit(new UserLawVote()
				{
					Date = DateTime.Now,
					UserId = this.User().UserId,
					Vote = (byte)vote,
					LawVoteId = lawvoteid
				}
				);
			}
			db.SubmitChanges();

			return RedirectToAction("Details", "Law", new
			{
				lawname = lv.Law.ShortName.ToUrlFriendly(),
				lawid = lv.Law.LawId
			});
		}

		[AcceptVerbs(HttpVerbs.Post)]
		[Authorize]
		public ActionResult Vote(int commentid, int vote)
		{
			_comRep.RecordVote(commentid, (byte)vote, this.User().UserId);
			return Content(string.Empty);
		}

		[AcceptVerbs(HttpVerbs.Post)]
		[Authorize]
		public ActionResult CreateComment(int elementid,
			int? parentid,
			string comment,
			CommentType commenttype,
			bool facebook_publish)
		{
			if (string.IsNullOrEmpty(comment))
				return Content("false");
			
			_comRep.SaveComment(elementid, parentid != -1 ? parentid : null, comment, (CommentType)commenttype, this.User().UserId);

			if (facebook_publish)
			{
				CommentFeedObject cf = new CommentFeedObject()
				{
					CommentBody = comment,
				};
				switch (commenttype)
				{
					case CommentType.Speech:
						{
							var sp = _lawRep.SpeechPara(elementid);
							var pol = sp.Speech.Politician;
							cf.target_type = "tale";
							cf.target_name = pol.FullName();
							cf.target_link = sp.Speech.LinkTo(this);
							cf.pol_link = Url.Action("Details", "Politician", new { polname = pol.FullName().ToUrlFriendly(), polid = pol.PoliticianId });
							break;
						}
					case CommentType.Change:
						{
							LawChange lc = _lawRep.LawChange(elementid);
							cf.target_type = "lov";
							cf.target_name = lc.Paragraph.Law.ShortName;
							cf.target_link = lc.LinkTo(this);
							break;
						}
					case CommentType.Section:
						{
							Section sec = _lawRep.Section(elementid);
							cf.target_type = "lov";
							cf.target_name = sec.Paragraph.Law.ShortName;
							cf.target_link = sec.LinkTo(this);
							break;
						}
					case CommentType.Question: 
					case CommentType.QuestionBackground:
						{
							P20Question p20q = _p20Rep.GetQuestion(elementid);

							switch (p20q.Type)
							{
								case QuestionType.Politician:
									Politician pol = p20q.AskerPol;
									cf.pol_link = Url.Action("Details", "Politician", new { polname = pol.FullName().ToUrlFriendly(), polid = pol.PoliticianId });
									cf.pol = pol;
									break;
								case QuestionType.User:
									User user = p20q.AskerUser;
									cf.user = user;
									cf.user_link = Url.Action("Details", "User", new { uname = user.Username });
									break;
							}
							cf.target_type = "§20 spørgsmål";
							cf.target_title = p20q.Title;
							cf.target_link = p20q.LinkTo(this);
							break;
						}
					case CommentType.Answer:
						{
							P20Question p20q = _p20Rep.GetQuestionByAnswerPara(elementid);
							Politician pol = p20q.AskeePol;
							cf.target_type = "§20 svar";
							cf.pol = pol;
							cf.target_title = p20q.Title;
							cf.target_link = p20q.LinkTo(this);
							cf.pol_link = Url.Action("Details", "Politician", new { polname = pol.FullName().ToUrlFriendly(), polid = pol.PoliticianId });
							break;
						}
					default:
						// TODO, do something real
						return Content("false");
						//throw new ArgumentException(string.Format("Unknow commenttype: {0}", commenttype));
				}
				var feedobject = GetFeedObject(cf);
				if (feedobject != null)
					return feedobject;
				else
					return Content("false");
			}
			else
				return Content("false");
		}


		private IEnumerable<CommentViewModel> GetAllComments(int itemid, CommentType type)
		{
			var db = new DBDataContext();

			var topcomments = from c in db.Comments
							  where c.ParentId == null &&
								c.ItemId == itemid &&
								c.CommentType == type
							  orderby c.Date
							  select c;

			Func<Comment, int, CommentViewModel> tovm =
				(Comment c, int depth) => new CommentViewModel()
				{
					Depth = depth,
					Date = c.Date,
					Text = c.CommentText,
					Id = c.CommentId,
					User = c.User,
					VoteCount = c.CommentVotes.Count,
					Score = c.CommentVotes.Sum(_ => _.Vote == 1 ? 1 : -1)
				};

			Func<Comment, int> getid = spc => spc.CommentId;

			Func<DBDataContext, int, IEnumerable<Comment>> getter =
				(DBDataContext thebase, int id) => from c in thebase.Comments
														 where c.ParentId == id
														 orderby c.Date
														 select c;

			return topcomments.ToList().SelectMany(tc => GetSubComments(tc, 0, db, tovm, getter, getid));
		}

		/// <summary>
		/// Suck on my higher order functions
		/// </summary>
		/// <typeparam name="T">L2S type</typeparam>
		/// <param name="curC">Currentcomment</param>
		/// <param name="curDepth"></param>
		/// <param name="db"></param>
		/// <param name="tovm">How to create viewcomment</param>
		/// <param name="getcomments">How to get the underlying comments</param>
		/// <param name="getid">How to get the id from T</param>
		/// <returns></returns>
		private IEnumerable<CommentViewModel> GetSubComments<T>(
			T curC,
			int curDepth,
			DBDataContext db,
			Func<T, int, CommentViewModel> tovm,
			Func<DBDataContext, int, IEnumerable<T>> getcomments,
			Func<T, int> getid)
		{
			yield return tovm(curC, curDepth);
			curDepth++;
			var comments = getcomments(db, getid(curC));
			foreach (var c2 in comments.SelectMany(c =>
				GetSubComments(c, curDepth, db, tovm, getcomments, getid)))
			{
				yield return c2;
			}
		}
	}

	public class TempComment
	{
		public Comment comment { get; set; }
		public int depth { get; set; }
	}

	public class CommentViewModel
	{
		public string Text { get; set; }
		public int Depth { get; set; }
		public DateTime? Date { get; set; }
		public User User { get; set; }
		public int? Score { get; set; }
		public int VoteCount { get; set; }
		public int Id { get; set; }
	}

	public class CommentsViewModel
	{
		public IEnumerable<CommentViewModel> Comments { get; set; }
		public int ElementId { get; set; }
		//public string IdPrefix { get; set; }
		//public string ActionName { get; set; }
		public CommentType Type { get; set; }
		// do not delete me, rqwurl doesn't work for comments because they ajaxed in
		public string CurrentUrl { get; set; }
	}
}
