using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.ComponentModel.DataAnnotations;
using FT.DB;
using xVal.ServerSide;

namespace FT.Model
{
	public interface IP20QuestionRepository
	{
		IQueryable<P20Question> Recent(int limit);
		P20Question GetQuestion(int qid);
		IQueryable<P20Question> LatestAnsweredByParliament(int p);
		IQueryable<P20Question> LatestByParliament(int p);
		IQueryable<P20Question> LatestByPeople(int p);
		//IQueryable<P20Question> Popular(DateTime date,  int p);
		int QuestionCommentCount(int qid);
		int BackgroundCommentCount(int qid);
		DateTime LatestActivity(int lawid);
		IEnumerable<string> Ministrys();
		P20Question CreateQuestion(P20Question q);
		P20Question GetQuestionByAnswerPara(int apid);
		void ValidateQuestion(P20Question q);
		int LatestWithTitle(string title, DateTime date);

		IQueryable<P20Question> Debated(DateTime date, int includeqa);

		IQueryable<P20Question> PopularByPeople(DateTime date, int includeqa);
	}

	public class P20QuestionRepository : Repository, IP20QuestionRepository
	{
		public void ValidateQuestion(P20Question q)
		{
			var errors = DataAnnotationsValidationRunner.GetErrors(q);
			if (errors.Any())
				throw new RulesException(errors);
		}

		public P20Question GetQuestionByAnswerPara(int apid)
		{

			P20Question p20q = DB.AnswerParas.Single(q => q.AnswerParaId == apid).P20Question;
			return p20q;
		}

		public P20Question CreateQuestion(P20Question q)
		{
			// this means double work, too bad
			ValidateQuestion(q);

			DB.P20Questions.InsertOnSubmit(q);
			DB.SubmitChanges();
			// should now have id and stuff
			return q;
		}

		public int LatestWithTitle(string title, DateTime date)
		{
			// Find the latest politician to have held this title
			return (from q in DB.P20Questions
					where q.AskeeTitle.ToLower() == title.ToLower() &&
						q.AskDate < date &&
						q.AskeeId != null
					orderby q.AskDate descending
					select q.AskeeId
					).First().Value;
		}

		public IEnumerable<string> Ministrys()
		{
			return DB.P20Questions.Select(_ => _.AskeeTitle).Distinct().ToList().OrderBy(_ => _);
		}

		public DateTime LatestActivity(int qid)
		{
			DateTime? foo =
				(
					from c in DB.Comments
					join ap in DB.AnswerParas on c.ItemId equals ap.AnswerParaId
					where ap.QuestionId == qid && c.CommentType == CommentType.Answer
					select c.Date
				).Concat(
					from c in DB.Comments
					where c.ItemId == qid &&
						(c.CommentType == CommentType.Question || c.CommentType == CommentType.QuestionBackground)
					select c.Date
				).OrderByDescending(_ => _).FirstOrDefault();
			if (foo.HasValue)
				return foo.Value;
			else
			{
				P20Question q = DB.P20Questions.Single(_ => _.P20QuestionId == qid);
				return q.AnswerDate.HasValue ? q.AnswerDate.Value : q.AskDate.Value;
			}
		}

		public int QuestionCommentCount(int qid)
		{
			return CommentCount(qid, CommentType.Question);
		}

		public int BackgroundCommentCount(int qid)
		{
			return CommentCount(qid, CommentType.QuestionBackground);
		}

		private int CommentCount(int qid, CommentType type)
		{
			return DB.Comments.Count(_ =>
				_.ItemId == qid &&
				_.CommentType == type
				);
		}

		public IQueryable<P20Question> Recent(int limit)
		{
			return DB.P20Questions.OrderByDescending(q => q.AskDate).Take(limit);
		}

		public P20Question GetQuestion(int qid)
		{
			return DB.P20Questions.Single(q => q.P20QuestionId == qid);
		}

		public IQueryable<P20Question> LatestAnsweredByParliament(int limit)
		{
			return (from q in DB.P20Questions
					where q.AnswerDate != null && 
                        q.Document != null && 
                        q.Document.ScribdId != null
                    //&& q.AnswerParas.Any()
					orderby q.AnswerDate descending
					select q).Take(limit);
		}

		public IQueryable<P20Question> LatestByParliament(int limit)
		{
			return (from q in DB.P20Questions
                    where q.Type == QuestionType.Politician
					orderby q.AskDate descending
					select q).Take(limit);
		}

		public IQueryable<P20Question> LatestByPeople(int limit)
		{
			return (from q in DB.P20Questions
                    where q.Type == QuestionType.User
					orderby q.AskDate descending
					select q).Take(limit);
		}

		public IQueryable<P20Question> Debated(DateTime date, int limit)
		{
			return (from q in DB.P20Questions
					orderby
						(from c in DB.Comments
								where
								c.Date > date &&
								c.ItemId == q.P20QuestionId &&
								c.CommentType == CommentType.QuestionBackground ||
								c.CommentType == CommentType.Question
								select c.CommentId
							 ).Count()
							 +
							 q.AnswerParas.Sum(ap => DB.Comments.Where(
										c => c.Date > date &&
										c.ItemId == ap.AnswerParaId &&
										c.CommentType == CommentType.Answer).Count())
					descending
					select q).Take(limit);
		}


		public IQueryable<P20Question> PopularByPeople(DateTime date, int limit)
		{
			return (from q in DB.P20Questions
                    where q.Type == QuestionType.User
					orderby (from h in DB.Hits
							 where h.ContentType == ContentType.P20Question &&
								 h.ContentId == q.P20QuestionId &&
								 h.Date > date

							 group h by new { h.IP, h.Date.Date, h.Date.Hour, h.Date.Minute } into v2
							 select v2.Key).Count() descending
					select q).Take(limit);
		}
	}


}