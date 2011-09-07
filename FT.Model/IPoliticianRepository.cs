using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using FT.DB;

namespace FT.Model
{
	public interface IPoliticianRepository
	{
		IQueryable<Politician> GetPoliticians();
		IQueryable<Politician> MostVisited(DateTime since, int limit);
		IQueryable<Politician> MostActive(DateTime fromDate, int limit);
		IQueryable<Politician> MostDebated(DateTime fromDate, int limit);
		Politician GetPoliticianById(int id);
		IEnumerable<ActivityStat> ActivityStats(int polid, int monthstoshow);
		IEnumerable<FeedEvent> LatestFeed(int polid, int includeposts);
		IEnumerable<FeedEvent> DebatedFeed(int polid, int includeposts);
		IEnumerable<Tuple<string,int>> LatestSpeechesBlob(int polid, int includeposts, int takewords);
	}

	public class PoliticianRepository : Repository, IPoliticianRepository
	{
		public PoliticianRepository()
		{
		}

		public Politician GetPoliticianById(int id)
		{
			return DB.Politicians.SingleOrDefault(l => l.PoliticianId == id);
		}

		public IQueryable<Politician> GetPoliticians()
		{
			var politicians = DB.Politicians;
			return politicians;
		}

		public IQueryable<Politician> MostVisited(DateTime since, int limit)
		{
			return GetPoliticians().OrderByDescending(
				p =>
					(from h in DB.Hits
					 where h.ContentType == ContentType.Politician &&
						 h.ContentId == p.PoliticianId &&
						 h.Date > since
					 group h by new { h.IP, h.Date.Date, h.Date.Hour, h.Date.Minute } into v2
					 select v2.Key).Count()
				).Take(limit);
		}

		public IQueryable<Politician> MostActive(DateTime fromDate, int limit)
		{
			var mostActive = (from p in GetPoliticians()
							  orderby
								((int?)p.Speeches.Where(t => t.Deliberation.Date > fromDate).Count() ?? 0)
								+
								((int?)p.PoliticianLawVotes.Where(v => v.LawVote.Date > fromDate).Count() ?? 0 / 10)
								+
								((int?)p.AskerP20Questions.Where(_ => _.AskDate > fromDate).Count() ?? 0)
								+
								((int?)p.AskeeP20Questions.Where(_ => _.AnswerDate > fromDate).Count() ?? 0)
							  descending
							  select p).Take(limit);
			return mostActive;
		}

		private static Func<DBDataContext, DateTime, int, IQueryable<Politician>> mostdebated =
			CompiledQuery.Compile((DBDataContext DB, DateTime fromDate, int limit) =>
				(
				from p in DB.Politicians
				orderby p.Speeches.
					Sum(t => t.SpeechParas.Sum(sp => DB.Comments.Where(
					c => c.Date > fromDate &&
					c.ItemId == sp.SpeechParaId &&
					c.CommentType == CommentType.Speech).Count()))
					+
					p.AskeeP20Questions.Sum(q => q.AnswerParas.Sum(ap =>
						DB.Comments.Where(
						 c => c.Date > fromDate &&
						 c.ItemId == ap.AnswerParaId &&
						 c.CommentType == CommentType.Answer).Count()))
					 +
					 p.AskerP20Questions.Sum(q => DB.Comments.Where(
						 c => c.Date > fromDate &&
							 c.ItemId == q.P20QuestionId &&
							 (c.CommentType == CommentType.Question ||
							 c.CommentType == CommentType.QuestionBackground)
							 ).Count())
					descending
				select p).Take(limit)
				);

		public IQueryable<Politician> MostDebated(DateTime fromDate, int limit)
		{
			var mostDebated = mostdebated(DB, fromDate, limit);
			return mostDebated;
		}

		public IEnumerable<ActivityStat> ActivityStats(int polid, int monthstoshow)
		{
			var voteact =
				(
				from plv in DB.PoliticianLawVotes.Where(_ => _.PoliticianId == polid)
				where plv.LawVote.Date > DateTime.Now.AddMonths(-monthstoshow) &&
					plv.Vote != 3 // i.e "not abstain"
				group plv by new
				{
					year = plv.LawVote.Date.Value.Year,
					month = plv.LawVote.Date.Value.Month
				} into d
				select new
				{
					date = new DateTime(d.Key.year, d.Key.month, 1),
					count = d.Count()
				}).ToList();

			var taleact =
				(
				from pt in DB.Speeches.Where(_ => _.PoliticianId == polid)
				where pt.SpeechTime.Value > DateTime.Now.AddMonths(-monthstoshow)
				group pt by new
				{
					year = pt.SpeechTime.Value.Year,
					month = pt.SpeechTime.Value.Month
				} into d
				select new
				{
					date = new DateTime(d.Key.year, d.Key.month, 1),
					count = d.Count()
				}).ToList();

			// there's a lovely bug in here, for when government changes we need another grouping
			var questionact =
				(
					from q in DB.P20Questions
					where ((q.Type == QuestionType.Politician && q.AskerPolId == polid) || q.AskeeId == polid ) &&
						q.AskDate > DateTime.Now.AddMonths(-monthstoshow)
					group q by new
					{
						year = q.AskDate.Value.Year,
						month = q.AskDate.Value.Month
					} into d
					select new {
						date = new DateTime(d.Key.year, d.Key.month, 1),
						count = d.Count(),
					}).ToList();

			var now = DateTime.Now;
			var months = from i in Enumerable.Range(0, monthstoshow)
						 select new DateTime(now.AddMonths(-i).Year, now.AddMonths(-i).Month, 1);

			var res2 = months.Select(_ => new
			{
				date = _,
				votes = voteact.SingleOrDefault(v => v.date == _),
				speeches = taleact.SingleOrDefault(t => t.date == _),
				questions = questionact.SingleOrDefault(q => q.date == _),
			});

			var res3 = res2.Select(r => new ActivityStat()
			{
				Date = r.date,
				Votes = r.votes != null ? r.votes.count : 0,
				Speeches = r.speeches != null ? r.speeches.count : 0,
				Questions = r.questions != null ? r.questions.count : 0,
			});

			return res3;
		}

		public IEnumerable<FeedEvent> LatestFeed(int polid, int includeposts)
		{
			var prelimvotes = Votes(polid, includeposts, _ => _.Date);
			var delibs = Deliberations(polid, includeposts, Ordering.Date);
			var prelimquestions = Questions(polid, includeposts, _ => _.Date);
			var prelimanswers = Answers(polid, includeposts, _ => _.Date);
			var prelimlawprops = LawSuggestions(polid, includeposts, _ => _.Date);
			var prelimspeakerships = Speakerships(polid, includeposts, _ => _.Date);
			var prelimtrips = Trips(polid, includeposts, _ => _.Date);

			var preevents =
				prelimvotes
				.Concat(prelimquestions)
				.Concat(delibs)
				.Concat(prelimanswers)
				.Concat(prelimlawprops)
				.Concat(prelimspeakerships)
				.Concat(prelimtrips)
				.Where(x => x.Date < DateTime.Now)
				.OrderByDescending(_ => _.Date).
				Take(includeposts);

			var events =
				preevents.Select(_ => GetEvent(_.Id, _.CommentCount, _.Type));

			return events;
		}


		public IEnumerable<FeedEvent> DebatedFeed(int polid, int includeposts)
		{
			var prelimquestions =
				Questions(polid, includeposts, _ => _.CommentCount).Where(_ => _.CommentCount > 0);
			var delibs2 =
				Deliberations(polid, includeposts, Ordering.Comments).Where(_ => _.CommentCount > 0);
			var prelimanswers =
				Answers(polid, includeposts, _ => _.CommentCount).Where(_ => _.CommentCount > 0);

			var preevents =
			prelimquestions
				.Concat(delibs2)
				.Concat(prelimanswers)
				.OrderByDescending(_ => _.CommentCount).Take(includeposts);

			var events =
			preevents.Select(_ => GetEvent(_.Id, _.CommentCount, _.Type));

			return events;
		}

		private FeedEvent GetEvent(int id, int? commentcount, EventType t/*, Controller con*/)
		{
			switch (t)
			{
				case EventType.Question:
					return new FeedEvent(DB.P20Questions.Single(q => q.P20QuestionId == id), commentcount, EventType.Question);
				case EventType.Debate:
					return new FeedEvent(DB.Speeches.Single(s => s.SpeechId == id), commentcount);
				case EventType.Vote:
					return new FeedEvent(DB.PoliticianLawVotes.Single(v => v.PoliticianLawVoteId == id));
				case EventType.Answer:
					return new FeedEvent(DB.P20Questions.Single(q => q.P20QuestionId == id), commentcount, EventType.Answer);
				case EventType.ProposedLaw:
					return new FeedEvent(DB.ProposedLaws.Single(p => p.ProposedLawId == id));
				case EventType.Speaker:
					return new FeedEvent(DB.Speakers.Single(q => q.SpeakerId == id));
				case EventType.Trip:
					return new FeedEvent(DB.CommitteeTrips.Single(x => x.CommitteeTripId== id));
				default: throw new ArgumentException();
			}
		}

		private IEnumerable<ProtoEvent> Questions<T>(int polid, int includeposts, Func<ProtoEvent, T> order)
		{
			return (
				from q in DB.P20Questions
				where q.Type == QuestionType.Politician &&
					q.AskerPolId == polid
				select new ProtoEvent()
				{
					Date = q.AskDate,
					Id = q.P20QuestionId,
					Type = EventType.Question,
					CommentCount =
						(from c in DB.Comments
						 join ap in DB.AnswerParas on c.ItemId equals ap.AnswerParaId
						 where ap.QuestionId == q.P20QuestionId && c.CommentType == CommentType.Answer
						 select c.CommentId
						).Concat(
						from c in DB.Comments
						where c.ItemId == q.P20QuestionId &&
							(c.CommentType == CommentType.Question || c.CommentType == CommentType.QuestionBackground)
						select c.CommentId
						).Count(),
				}
				).OrderByDescending(order).Take(includeposts);
		}

		private IEnumerable<ProtoEvent> Answers<T>(int polid, int includeposts, Func<ProtoEvent, T> order)
		{
			return (
				from q in DB.P20Questions
				where
					q.AskeeId == polid &&
					q.AnswerDate != null
				select new ProtoEvent()
				{
					Date = q.AnswerDate,
					Id = q.P20QuestionId,
					Type = EventType.Answer,
					CommentCount =
						(from c in DB.Comments
						 join ap in DB.AnswerParas on c.ItemId equals ap.AnswerParaId
						 where ap.QuestionId == q.P20QuestionId && c.CommentType == CommentType.Answer
						 select c.CommentId
						).Concat(
						from c in DB.Comments
						where c.ItemId == q.P20QuestionId &&
							(c.CommentType == CommentType.Question || c.CommentType == CommentType.QuestionBackground)
						select c.CommentId
						).Count(),
				}
				).OrderByDescending(order).Take(includeposts);
		}

		private IEnumerable<ProtoEvent> Speakerships<T>(int polid, int includeposts, Func<ProtoEvent, T> order)
		{
			return
				(from s in DB.Speakers
				 join l in DB.Laws on s.Law equals l
				 where s.PoliticianId == polid
				 select new ProtoEvent()
				 {
					 Date = l.Proposed,
					 Id = s.SpeakerId,
					 Type = EventType.Speaker,
					 CommentCount = null
				 }
				).OrderByDescending(order).Take(includeposts);
		}

		private IEnumerable<ProtoEvent> Trips<T>(int polid, int includeposts, Func<ProtoEvent, T> order)
		{
			return
				(from t in DB.CommitteeTrips
				 join p in DB.CommitteeTripParticipants on t.CommitteeTripId equals p.CommitteeTripId
				 where p.Politician.PoliticianId == polid
					&& t.Place != null && t.Place != "" && t.CommitteeTripDestinations.Any()
				 select new ProtoEvent()
				 {
					 Date = t.StartDate,
					 Id = t.CommitteeTripId,
					 Type = EventType.Trip,
					 CommentCount = null
				 }
				).OrderByDescending(order).Take(includeposts);
		}

		private IEnumerable<ProtoEvent> LawSuggestions<T>(int polid, int includeposts, Func<ProtoEvent, T> order)
		{
			return
				(from pl in DB.ProposedLaws
				 join l in DB.Laws on pl.Law equals l
				 where pl.PoliticianId == polid
				 select new ProtoEvent()
				 {
					 Date = l.Proposed,
					 Id = pl.ProposedLawId,
					 Type = EventType.ProposedLaw,
					 CommentCount = null
				 }
				).OrderByDescending(order).Take(includeposts);
		}

		private IEnumerable<ProtoEvent> Votes<T>(int polid, int includeposts, Func<ProtoEvent, T> order)
		{
			return
				(from plw in DB.PoliticianLawVotes
				 join lw in DB.LawVotes on plw.LawVoteId equals lw.LawVoteId
				 join l in DB.Laws on lw.LawId equals l.LawId
				 where plw.Vote != 3 &&
					plw.PoliticianId == polid &&
					lw.IsFinal.Value
				 select
				  new ProtoEvent()
				  {
					  Date = lw.Date,
					  Id = plw.PoliticianLawVoteId,
					  Type = EventType.Vote,
					  CommentCount = null
				  }
					).OrderByDescending(order).Take(includeposts);
		}

		private IEnumerable<ProtoEvent> Deliberations(int polid, int includeposts, Ordering order/*Func<ProtoEvent, T> order*/)
		{
			if (order == Ordering.Date)
			{
				return
					(
						from d in DB.Deliberations
						let firstspeech =
							d.Speeches.Where(_ => _.PoliticianId == polid).
							   OrderBy(_ => _.SpeechNr).First()
						let commentcount = d.Speeches.Where(
								_ => _.PoliticianId == polid).Sum(
									_ => _.SpeechParas.Sum(
										__ => DB.Comments.Count(
											c => c.CommentType == CommentType.Speech && c.ItemId == __.SpeechParaId)))
						where d.Speeches.Any(ss => ss.PoliticianId == polid)
						orderby firstspeech.Deliberation.Date
						select new { firstspeech, commentcount }
					).Take(includeposts).
					Select(_ => new ProtoEvent()
					{
						Date = _.firstspeech.Deliberation.Date,
						Id = _.firstspeech.SpeechId,
						Type = EventType.Debate,
						CommentCount = _.commentcount
					});
			}
			else if (order == Ordering.Comments)
			{
				return
					(
						from d in DB.Deliberations
						let firstspeech =
							d.Speeches.Where(_ => _.PoliticianId == polid).
							   OrderBy(_ => _.SpeechNr).First()
						let commentcount = d.Speeches.Where(
								_ => _.PoliticianId == polid).Sum(
									_ => _.SpeechParas.Sum(
										__ => DB.Comments.Count(
											c => c.CommentType == CommentType.Speech && c.ItemId == __.SpeechParaId)))
						where d.Speeches.Any(ss => ss.PoliticianId == polid)
						orderby commentcount
						select new { firstspeech, commentcount }
					).Take(includeposts).
					Select(_ => new ProtoEvent()
						{
							Date = _.firstspeech.Deliberation.Date,
							Id = _.firstspeech.SpeechId,
							Type = EventType.Debate,
							CommentCount = _.commentcount
						});
			}
			else
			{
				throw new ArgumentException("Unknown ordering");
			}
		}

		public IEnumerable<Tuple<string, int>> LatestSpeechesBlob(
			int polid, int includeposts, int takewords)
		{
			var stopwords = (new UtilRepository()).GetDKStopWords();
			
			var words = string.Join(" ", DB.Speeches.Where(s => s.PoliticianId == polid).
				OrderByDescending(_ => _.Deliberation.Date).ThenByDescending(_ => _.SpeechTime).
				Take(includeposts).
				SelectMany(s => s.SpeechParas.Select(sp => sp.ParText))).Split(
					new char[] { ' ', '.', ',' }, StringSplitOptions.RemoveEmptyEntries).
				Where(_ => !stopwords.Contains(_.ToLower()));
			
			var groups = (from w in words
						 group w by w into g
						 select new { word = g.Key, count = g.Count() }).
						 OrderByDescending(_ => _.count).Take(takewords);
						 
			return groups.Select(_ => new Tuple<string, int>(_.word, _.count));
		}
	}

	public class ProtoEvent
	{
		public int Id { get; set; }
		public EventType Type { get; set; }
		public int? CommentCount { get; set; }
		public DateTime? Date { get; set; }
	}

	public class ActivityStat
	{
		public DateTime Date { get; set; }
		public int Votes { get; set; }
		public int Speeches { get; set; }
		public int Questions { get; set; }
	}

	public enum EventType { Vote, Debate, Question, Answer, Speaker, ProposedLaw, Trip };
	public enum Ordering { Comments, Date };
}
