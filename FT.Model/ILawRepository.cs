using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using FT.DB;
using System.Data.Linq;

namespace FT.Model
{
	public interface ILawRepository
	{
		IQueryable<Law> TopDebated(DateTime date, int limit);
		IQueryable<Law> RecentProposed(int limit);
		IQueryable<Law> RecentPassed(int limit);
		IQueryable<Law> RecentInDeliberation(LawStage deliberation, int limit);
		IQueryable<Law> Popular(DateTime date, int limit);
		Law SingleLaw(int lawid);
		int DeliberationCommentCount(int delibid);
		Deliberation SingleDeliberation(int lawid, LawStage delibnr);
		bool IsProposed(int lawid);
		bool IsPassed(int lawid);
		bool IsAfterSecond(int lawid);
		int CommentCount(LawStage thestage, int lawid);
		int CommentCount(int lawid);
		bool IsInStage(LawStage stage, int lawid);
		DateTime LatestActivity(int lawid);
		SpeechPara SpeechPara(int elementid);
		LawChange LawChange(int elementid);
		Section Section(int elementid);
	}

	public class LawRepository : Repository, ILawRepository
	{
		private IQueryable<Law> ValidLaws()
		{
			return DB.Laws;
		}

		public Section Section(int elementid)
		{
			return DB.Sections.Single(_ => _.SectionId == elementid);
		}

		public LawChange LawChange(int elementid)
		{
			return DB.LawChanges.Single(_ => _.LawChangeId == elementid);
		}

		public SpeechPara SpeechPara(int elementid)
		{
			return DB.SpeechParas.Single(_ => _.SpeechParaId == elementid);
		}

		public Law SingleLaw(int lawid)
		{
			return DB.Laws.SingleOrDefault(l => l.LawId == lawid);
		}

		private static Func<DBDataContext, DateTime, int, IQueryable<Law>> topdebated =
			CompiledQuery.Compile((DBDataContext datab, DateTime date, int limit) =>
			(from l in datab.Laws
			 orderby
				(from c in datab.Comments
				join sp in datab.SpeechParas on c.ItemId equals sp.SpeechParaId
				join s in datab.Speeches on sp.SpeechId equals s.SpeechId
				join d in datab.Deliberations on s.DeliberationId equals d.DeliberationId
				where d.LawId == l.LawId && c.CommentType == CommentType.Speech
				select c.CommentId).Count()
				+
				(from c in datab.Comments
				join lc in datab.LawChanges on c.ItemId equals lc.LawChangeId
				join p in datab.Paragraphs on lc.ParagraphId equals p.ParagraphId
				where p.LawId == l.LawId && c.CommentType == CommentType.Change
				select c.CommentId).Count()
				+
				(from c in datab.Comments
				join s in datab.Sections on c.ItemId equals s.SectionId
				join p in datab.Paragraphs on s.ParagraphId equals p.ParagraphId
				where p.LawId == l.LawId && c.CommentType == CommentType.Section
				select c.CommentId).Count()
			 descending
			 select l).Take(limit)
			);


		public IQueryable<Law> TopDebated(DateTime date, int limit)
		{
			return topdebated(DB, date, limit);
		}

		public IQueryable<Law> Popular(DateTime date, int limit)
		{
			return (from l in ValidLaws()
					orderby (from h in DB.Hits
							 where h.ContentType == ContentType.Law &&
								 h.ContentId == l.LawId &&
								 h.Date > date
							 group h by new { h.IP, h.Date.Date, h.Date.Hour, h.Date.Minute } into v2
							 select v2.Key).Count() descending
					select l).Take(limit);
		}

		public IQueryable<Law> RecentProposed(int limit)
		{
			return ValidLaws().OrderByDescending(l => l.Proposed).Take(limit);
		}

		public IQueryable<Law> RecentPassed(int limit)
		{
			return ValidLaws().OrderByDescending(l => l.Passed).Take(limit);
		}

		public IQueryable<Law> RecentInDeliberation(LawStage deliberation, int limit)
		{
			return (from l in ValidLaws()
					where l.Deliberations.Any(b => b.Number == deliberation && b.Speeches.Any())
					orderby l.Deliberations.Single(b => b.Number == deliberation).Date descending
					select l).Take(limit);
		}


		public int DeliberationCommentCount(int delibid)
		{
			var comments = (from s in DB.Speeches
							join sp in DB.SpeechParas on s.SpeechId equals sp.SpeechId
							join c in DB.Comments on sp.SpeechParaId equals c.ItemId
							where s.DeliberationId == delibid && c.CommentType == CommentType.Speech
							select c.CommentId).Count();

			if (comments == 0)
			{
				// this might be because the deliberation is still a draft
				if (DB.Speeches.Any(s => s.DeliberationId == delibid && s.IsTemp == true))
				{
					// let this signify a draft deliberation
					comments = -1;
				}
			}

			return comments;
		}

		public Deliberation SingleDeliberation(int lawid, LawStage delibnr)
		{
			return DB.Deliberations.SingleOrDefault(_ => _.Number == delibnr && _.LawId == lawid);
		}

		public bool IsProposed(int lawid)
		{
			return DB.Paragraphs.Where(p => p.Stage == LawStage.First && p.LawId == lawid).Any();
		}

		public bool IsAfterSecond(int lawid)
		{
			return DB.Paragraphs.Where(p => p.Stage == LawStage.Second && p.LawId == lawid).Any();
		}

		public bool IsPassed(int lawid)
		{
			return DB.Paragraphs.Where(p => p.Stage == LawStage.Third && p.LawId == lawid).Any();
		}

		private static Func<DBDataContext, LawStage, int, int> changecommentcount =
			CompiledQuery.Compile((DBDataContext datab, LawStage stage, int thelawid) =>
				(from p in datab.Paragraphs
				 join lc in datab.LawChanges on p.ParagraphId equals lc.ParagraphId
				 join c in datab.Comments on lc.LawChangeId equals c.ItemId
				 where p.Stage == stage && p.LawId == thelawid && c.CommentType == CommentType.Change
				 select c.CommentId).Count());

		private static Func<DBDataContext, LawStage, int, int> sectioncommentcount =
			CompiledQuery.Compile((DBDataContext datab, LawStage stage, int thelawid) =>
				(from p in datab.Paragraphs
				 join s in datab.Sections on p.ParagraphId equals s.ParagraphId
				 join c in datab.Comments on s.SectionId equals c.ItemId
				 where p.Stage == stage && p.LawId == thelawid && c.CommentType == CommentType.Section
				 select c.CommentId).Count());

		private static Func<DBDataContext, int, int> lawcommentcount =
				CompiledQuery.Compile((DBDataContext datab, int thelawid) =>
					(
						(from c in datab.Comments
						join sp in datab.SpeechParas on c.ItemId equals sp.SpeechParaId
						join s in datab.Speeches on sp.SpeechId equals s.SpeechId
						join d in datab.Deliberations on s.DeliberationId equals d.DeliberationId
						where d.LawId == thelawid && c.CommentType == CommentType.Speech
						select c.CommentId).Count()
						+
						(from c in datab.Comments
						join lc in datab.LawChanges on c.ItemId equals lc.LawChangeId
						join p in datab.Paragraphs on lc.ParagraphId equals p.ParagraphId
						where p.LawId == thelawid && c.CommentType == CommentType.Change
						select c.CommentId).Count()
						+
						(from c in datab.Comments
						join s in datab.Sections on c.ItemId equals s.SectionId
						join p in datab.Paragraphs on s.ParagraphId equals p.ParagraphId
						where p.LawId == thelawid && c.CommentType == CommentType.Section
						select c.CommentId).Count()
					));

		public int CommentCount(LawStage thestage, int lawid)
		{
			return changecommentcount(DB, thestage, lawid) + sectioncommentcount(DB, thestage, lawid);
		}

		public int CommentCount(int lawid)
		{
			return lawcommentcount(DB, lawid);
		}

		public bool IsInStage(LawStage stage, int lawid)
		{
			return (from s in DB.Speeches
					join d in DB.Deliberations on s.DeliberationId equals d.DeliberationId
					where d.Number == stage && d.LawId == lawid
					select s.SpeechId).Any();
		}

		public DateTime LatestActivity(int lawid)
		{
			DateTime? foo = (from uv in DB.UserLawVotes
							 join lv in DB.LawVotes on uv.LawVoteId equals lv.LawVoteId
							 where lv.LawId == lawid
							 select uv.Date).
						Concat(
						from c in DB.Comments
						join sp in DB.SpeechParas on c.ItemId equals sp.SpeechParaId
						join s in DB.Speeches on sp.SpeechParaId equals s.SpeechId
						join d in DB.Deliberations on s.DeliberationId equals d.DeliberationId
						where d.LawId == lawid && c.CommentType == CommentType.Speech
						select c.Date
						).Concat(
						from c in DB.Comments
						join lc in DB.LawChanges on c.ItemId equals lc.LawChangeId
						join p in DB.Paragraphs on lc.ParagraphId equals p.ParagraphId
						where p.LawId == lawid && c.CommentType == CommentType.Change
						select c.Date
						).Concat(
						from c in DB.Comments
						join s in DB.Sections on c.ItemId equals s.SectionId
						join p in DB.Paragraphs on s.ParagraphId equals p.ParagraphId
						where p.LawId == lawid && c.CommentType == CommentType.Section
						select c.Date
						).OrderByDescending(_ => _).FirstOrDefault();

			if (foo.HasValue)
				return foo.Value;
			else
			{
				var thel = SingleLaw(lawid);
				return (from d in DB.Deliberations
						where d.LawId == lawid
						select d.Date).ToList().Concat(new List<DateTime?>() { thel.Proposed, thel.Passed }).
					OrderByDescending(_ => _).First().Value;
			}
		}
	}
}
