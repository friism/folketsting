using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using FolketsTing.Controllers;
using FT.DB;
using Microsoft.Web.Mvc;

namespace FolketsTing.Views
{
	public static class ViewConstants
	{
		public static string ReCaptchaKey { 
			get { return ConfigurationManager.AppSettings["ReCaptchaKey"]; } }

		public static string DateFormat { get { return "d. MMMM yyyy"; } }

		private static System.Web.Script.Serialization.JavaScriptSerializer serializer =
			new System.Web.Script.Serialization.JavaScriptSerializer();
		
		const int SECOND = 1;
		const int MINUTE = 60 * SECOND;
		const int HOUR = 60 * MINUTE;
		const int DAY = 24 * HOUR;
		const int MONTH = 30 * DAY;

		public static string ToProperLink(this string link)
		{
			string pre = "http://";
			if (!link.StartsWith(pre))
				return pre + link;
			else
				return link;
		}

		/// <summary>
		/// Lifted from Stackoverflow post: 
		/// http://stackoverflow.com/questions/11/how-can-i-calculate-relative-time-in-c
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static string GetTimeSpanString(DateTime dt)
		{
			var ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
			double delta = ts.TotalSeconds;

			if (delta < 1 * MINUTE)
			{
				return ts.Seconds == 1 ? "et sekund siden" : ts.Seconds + " sekunder siden";
			}
			if (delta < 2 * MINUTE)
			{
				return "et minut siden";
			}
			if (delta < 45 * MINUTE)
			{
				return ts.Minutes + " minutter siden";
			}
			if (delta < 90 * MINUTE)
			{
				return "en time siden";
			}
			if (delta < 24 * HOUR)
			{
				return ts.Hours + " timer siden";
			}
			if (delta < 48 * HOUR)
			{
				return "en dag siden";
			}
			if (delta < 30 * DAY)
			{
				return ts.Days + " dage siden";
			}
			if (delta < 12 * MONTH)
			{
				int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
				return months <= 1 ? "en måned siden" : months + " måneder siden";
			}
			else
			{
				int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
				return years <= 1 ? "et år siden" : years + " år siden";
			}
		}

		public static string GetTagClass(int tagcount, int totalcount)
		{
			var result = (tagcount * 100) / (totalcount / 20);
			if (result <= 1)
				return "tag1";
			if (result <= 2)
				return "tag2";
			if (result <= 4)
				return "tag3";
			if (result <= 6)
				return "tag4";
			if (result <= 9)
				return "tag5";
			if (result <= 15)
				return "tag6";
			return result <= 20 ? "tag7" : "";

		}

		public static Func<double, double> radius = c => Math.Sqrt(c * Math.PI);

		public static string GetVoteChartScale(IEnumerable<PartyVote> votes, int votetype)
		{
			double largest = Enumerable.Range(0, 4).
				Select(i => radius(votes.Where(v => v.Vote == i).Sum(v => v.Count))).Max();
			double oftype = radius(votes.Where(v => v.Vote == votetype).Sum(v => v.Count));
			return serializer.Serialize(Math.Max(oftype / largest, .15));
		}

		public static string GetPeopleVoteChartScale(LawVote vote, int votetype)
		{
			double largest = Enumerable.Range(0, 2).Select(i => radius(vote.UserLawVotes.
				Where(v => v.Vote == i).Count())).Max();
			double oftype = radius(vote.UserLawVotes.Where(v => v.Vote == votetype).Count());
			//double r = Math.Sqrt((oftype / largest) * Math.PI);
			if (largest == 0 || oftype == 0)
				return "0";
			else
				return serializer.Serialize(Math.Max(oftype / largest, .15));
		}

		public static string GGetVoteChartJSArray(IEnumerable<PartyVote> votes, byte votetype)
		{
			var parties =
				votes.Where(_ => _.Vote == votetype).
				Select(_ => new { party = _.Party, count = _.Count }).
				OrderBy(_ => _.party);

			if (parties.Where(_ => _.count > 0).Any())
			{
				return "[" + parties.Where(_ => _.count > 0).Select(_ =>
					string.Format("{{c:[{{v:'{0}'}},{{v:{1}}}]}}",
						_.party, _.count)
					).Aggregate((a, b) => a + "," + b)
					+ "]";
			}
			else
			{
				return "[]";
			}
		}

		public static string GGetVoteChartJSColourArray(IEnumerable<PartyVote> votes, byte votetype)
		{
			var parties =
				votes.Where(_ => _.Vote == votetype).
				Select(_ => new { party = _.Party, count = _.Count }).
				OrderBy(_ => _.party);

			return
				string.Join(",",
					parties.Where(_ => _.count > 0).Select(p =>
						string.Format("'{0}'", GetColour(p.party))).ToArray());
		}

		public static IQueryable<PartyVote> GetPartyVotes(int lawvoteid)
		{
			var db = new DBDataContext();
			return from plw in db.PoliticianLawVotes
				   join p in db.Politicians on plw.PoliticianId equals p.PoliticianId
				   where plw.LawVoteId == lawvoteid //&& plw.Vote == votetype
				   group plw by new { p.Party.Initials, plw.Vote } into ps
				   orderby ps.Key.Initials
				   select new PartyVote()
				   {
					   Party = ps.Key.Initials,
					   Vote = ps.Key.Vote.Value,
					   Count = ps.Count()
				   };
		}

		public static string PresentText(this string text, int maxLength)
		{
			string result = text.Truncate(maxLength, true);
			//result = result.Encode();
			return result;
		}

		public static string Truncate(this string str, int maxlength, bool addDots)
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

		/// <summary>
		/// This is lifted from a Stackoverflow post
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string ToUrlFriendly(this string s)
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

		public static string GetCanonicalParty(string p)
		{
			switch (p)
			{
				case null: return "Chairman";
				case "S": return "S";
				case "EL": return "EL";
				case "RV": return "RV";
				case "NY":
				case "LA": return "LA";
				case "SF": return "SF";
				case "DF": return "DF";
				case "V": return "V";
				case "KF": return "KF";

				case "IA":
				case "FF":
				case "SIU":
				case "SP":
				case "TF":
				case "UFG":
				default: return null;
			}
		}

		private static string GetColour(string p)
		{
			switch (p)
			{
				case "S": return "#9E0023";
				case "EL": return "#E70018";
				case "RV": return "#1D2C49";
				case "NY":
				case "LA": return "#F78A18";
				case "SF": return "#C60418";
				case "DF": return "#212021";//"#CE1829";
				case "V": return "#003063";
				case "KF": return "#005542";

				case "IA":
				case "FF":
				case "SIU":
				case "SP":
				case "TF":
				case "UFG":
				default: return "#000000";
			}
		}

		public static Dictionary<string, IEnumerable<Politician>> GetPartyForVote(LawVote vote, int votetype)
		{
			var parties = vote.PoliticianLawVotes.Where(_ => _.Vote == votetype).
				Select(_ => _.Politician.Party.Initials).Distinct().ToList().OrderBy(p => p);

			var res = new Dictionary<string, IEnumerable<Politician>>();

			foreach (var p in parties)
			{
				res.Add(p, (from plv in vote.PoliticianLawVotes
							where plv.Vote == votetype && plv.Politician.Party.Initials == p
							select plv.Politician).ToList().OrderBy(_ => _.FullName()));
			}
			return res;
		}

		public static string GetCommentCountString(int? count)
		{
			string res = "";
			if (count == 0)
				res = "0 kommentarer";
			else if (count == 1)
				res = "1 kommentar";
			else if (count > 1)
				res = string.Format("{0} kommentarer", count);
			else if (count == -1) // draft
				res = "(kladde)";
			return res;
		}

		public static string GetViewCountString(int? count)
		{
			string res = "";
			if (count == 0)
				res = "0 visninger";
			else if (count == 1)
				res = "1 visning";
			else if (count > 1)
				res = string.Format("{0} visninger", count);
			return res;
		}

		public static string GetTimesString(int? count)
		{
			string res = "";
			if (count == 1)
				res = "1 gang";
			else
				res = string.Format("{0} gange", count);
			return res;
		}

		public static MvcHtmlString LinkTo<T>(this HtmlHelper help, T item)
		{
			switch (typeof(T).Name)
			{
				case "Politician":
					{
						Politician p = item as Politician;
						return help.ActionLink<PoliticianController>(
							_ => _.Details(p.FullName().ToUrlFriendly(), p.PoliticianId), p.FullName());
					}
				case "User":
					{
						User u = item as User;
						return help.ActionLink<UserController>(
							_ => _.Details(u.Username), u.Username);
					}
				case "P20Question":
					{
						P20Question q = item as P20Question;
						return help.ActionLink<P20QuestionController>(
							_ => _.Details(q.Question.ToUrlFriendly(), q.P20QuestionId), q.Title);
					}
				default: throw new ArgumentException(string.Format("unknow type: {0}", typeof(T)));
			}
		}

		public static MvcHtmlString LinkTo<T>(this HtmlHelper help, T item, string linktext)
		{
			switch (typeof(T).Name)
			{
				case "Politician":
					{
						Politician p = item as Politician;
						return help.ActionLink<PoliticianController>(
							_ => _.Details(p.FullName().ToUrlFriendly(), p.PoliticianId), linktext);
					}
				case "User":
					{
						User u = item as User;
						return help.ActionLink<UserController>(
							_ => _.Details(u.Username), linktext);
					}
				case "P20Question":
					{
						P20Question q = item as P20Question;
						return help.ActionLink<P20QuestionController>(
							_ => _.Details(q.Question.ToUrlFriendly(), q.P20QuestionId), linktext);
					}
				case "Law":
					{
						Law l = item as Law;
						return help.ActionLink<LawController>(
							_ => _.Details(l.ShortName.ToUrlFriendly(), l.LawId), linktext);
					}
				default: throw new ArgumentException(string.Format("unknow type: {0}", typeof(T)));
			}
		}

		public static string GetHighlighted(IEnumerable<string> strings, IEnumerable<string> highlights, int wantedlength)
		{
			if (highlights != null)
			{
				foreach (var s in strings)
				{
					string lowers = s.ToLower();
					var lowerhighlights = highlights.Select(_ => _.ToLower());
					if (!string.IsNullOrEmpty(s) && lowerhighlights.Any(_ => lowers.Contains(_)))
					{
						var lowerhighlight = lowerhighlights.First(_ => lowers.Contains(_));
						int index = lowers.IndexOf(lowerhighlight);
						int start = Math.Max(0, index - wantedlength / 2);
						int length = Math.Min(wantedlength, s.Length - start);
						string substring = s.Substring(start, length);

						string res = Regex.Replace(substring, highlights.Aggregate((a, b) => a + "|" + b), @"<span class=""highlight"">$0</span>", RegexOptions.IgnoreCase);
						if (start > 0)
							res = "..." + res;
						if (start + length < s.Length)
							res = res + "...";
						return res;
					}
				}
			}
			// none found or highlights null
			if (strings.First() != null)
				return strings.First().Substring(0, Math.Min(strings.First().Length, wantedlength));
			else
				return ""; // bah
		}
	}

	public class PartyVote
	{
		public string Party { get; set; }
		public int Count { get; set; }
		public byte Vote { get; set; }
	}
}
