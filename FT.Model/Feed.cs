using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FT.DB;
using System.Web.Mvc;
using System.Text.RegularExpressions;

namespace FT.Model
{
	public class FeedEvent
	{
		public string ActionUrl { get; set; }
		public string ActionText { get; set; }
		public string SubjectUrl { get; set; }
		public string SubjectText { get; set; }

		public string BodyText { get; set; }
		public int? Comments { get; set; }
		public int? Views { get; set; }
		public DateTime date { get; set; }
		public string Binder { get; set; }

		public FeedEvent(P20Question q, int? commentcount, EventType et)
		{
			this.date = et == EventType.Question ? q.AskDate.Value : q.AnswerDate.Value;
			this.BodyText = q.Question.PresentText(200);

			this.ActionUrl = q.DetailsLink();
			this.ActionText = GetQuestionText(q, et);
			this.Comments = commentcount;
		}

		public FeedEvent(PoliticianLawVote vote)
		{
			var thelaw = vote.LawVote.Law;
			var thevote = vote.LawVote;

			this.date = thevote.Date.Value;
			this.BodyText = thelaw.Summary.PresentText(200);

			this.ActionUrl = thelaw.DetailsLink();
			this.ActionText = GetVoteText(vote) + " til " + thelaw.ShortName.PresentText(35);
		}

		public FeedEvent(ProposedLaw prop)
		{
			var thelaw = prop.Law;

			this.date = prop.Law.Proposed.Value;
			this.BodyText = thelaw.Summary.PresentText(200);

			this.ActionUrl = thelaw.DetailsLink();
			this.ActionText = "Foreslog " + thelaw.ShortName.PresentText(35);
		}

		public FeedEvent(Speaker speaker)
		{
			var thelaw = speaker.Law;

			this.date = speaker.Law.Proposed.Value;
			this.BodyText = thelaw.Summary.PresentText(200);

			this.ActionUrl = thelaw.DetailsLink();
			this.ActionText = "Blev ordfører for " + thelaw.ShortName.PresentText(35);
		}

		public FeedEvent(CommitteeTrip trip)
		{
			this.date = trip.StartDate;
			this.BodyText = trip.Purpose.PresentText(200);

			this.ActionUrl = trip.LinkTo();
			this.ActionText = string.Format("Rejste med {0} til {1}", trip.Committee.Name, trip.Place ?? "");
		}

		public FeedEvent(Speech speech, int? commentcount)
		{
			this.date = speech.Deliberation.Date.Value;
			this.BodyText = speech.SpeechParas.OrderBy(_ => _.Number).Take(3).
						Select(_ => _.ParText).Aggregate((a, b) => a + " " + b).PresentText(200);

			this.ActionText = "Debaterede ved " + speech.Deliberation.Number.UrlValue() + ". behandling";
			this.ActionUrl = speech.LinkTo();

			this.SubjectText = speech.Deliberation.Law.ShortName.PresentText(35);
			this.SubjectUrl = speech.Deliberation.Law.DetailsLink();

			this.Comments = commentcount;
			this.Binder = "af";
			//this.Comments = speech.SpeechParas.Sum(_ => _.SpeechParaComments.Count);
		}

		private string GetVoteText(PoliticianLawVote plv)
		{
			switch (plv.Vote)
			{
				case 0: return "Stemte Ja";
				case 1: return "Stemte Nej";
				case 2: return "Afstod fra at stemme";
				default: throw new ArgumentException("Unknown votetype: " + plv.Vote);
			}
		}

		private string GetQuestionText(P20Question q, EventType et)
		{
			return et == EventType.Question ?
				string.Format("Stillede {0} et spørgsmål om {1}", q.AskeeTitle, q.Title)
				:
				string.Format("Besvarede {0}s spørgsmål om {1}",
				q.Type == QuestionType.Politician ?
					q.AskerPol.Firstname + " " + q.AskerPol.Lastname :
					q.AskerUser.Username,
				q.Title)
				;
		}
	}

	public static class Feedhelper
	{
		public static string DetailsLink(this P20Question q)
		{
			return string.Format("/p20spoergsmaal/{0}/{1}",
				q.Title.ToUrlFriendly(),
				q.P20QuestionId);
		}

		public static string DetailsLink(this Law l)
		{
			return string.Format("/love/{0}/{1}",
				l.ShortName.ToUrlFriendly(),
				l.LawId);
		}

		public static string LinkTo(this Speech s)
		{
			return string.Format("/love/{0}/{1}/behandling/{2}#{3}",
				s.Deliberation.Law.ShortName.ToUrlFriendly(),
				s.Deliberation.Law.LawId,
				s.Deliberation.Number.UrlValue(),
				"par-" + s.SpeechParas.OrderBy(sp => sp.Number).First().SpeechParaId);
		}

		public static string LinkTo(this CommitteeTrip t)
		{
			return string.Format("/rejser/{0}", t.CommitteeTripId);
		}

		/// <summary>
		/// This is lifted from a Stackoverflow post
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		internal static string ToUrlFriendly(this string s)
		{
			// make it all lower case
			string title = s.ToLower();
			// replace ae, oe aa
			title = title.Replace("æ", "ae").Replace("å", "aa").Replace("ø", "oe");
			// remove entities
			title = Regex.Replace(title, @"&\w+;", "");
			// remove anything that is not letters, numbers, dash, or space
			title = Regex.Replace(title, @"[^a-z0-9\-\s]", "");
			// replace spaces
			title = title.Replace(' ', '-');
			// collapse dashes
			title = Regex.Replace(title, @"-{2,}", "-");
			// trim excessive dashes at the beginning
			title = title.TrimStart(new[] { '-' });
			// if it's too long, clip it
			if (title.Length > 80)
				title = title.Substring(0, 79);
			// remove trailing dashes
			title = title.TrimEnd(new[] { '-' });
			return title;
		}

		public static string PresentText(this string text, int maxLength)
		{
			return text.Truncate(maxLength, true);
		}

		internal static string Truncate(this string str, int maxlength, bool addDots)
		{
			if (str == null || str.Length <= maxlength)
			{
				return str;
			}

			if (addDots && maxlength >= 3)
			{
				string shortened = str.Substring(0, maxlength - 3);
				if (!shortened.EndsWith("..."))
				{
					shortened += "...";
				}
				return shortened;
			}
			else
			{
				return str.Substring(0, maxlength);
			}
		}
	}
}
