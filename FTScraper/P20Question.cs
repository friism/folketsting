using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FT.DB;
using HtmlAgilityPack;

namespace FT.Scraper
{
	public class P20QuestionScraper
	{
		public static void GetQChecked(int ftid, string title,
			IEnumerable<string> commiteestrings, bool answered, string url,
			Session samling, bool record = true)
		{
			try
			{
				GetQ(ftid, title, commiteestrings, answered, url, samling, record);
			}
			catch (Exception e)
			{
				Console.WriteLine("Problem with: {0}", url);
				//throw new Exception("Problem with question " + ftid + " year " + samling.Year
				//    + " nr " + samling.Number, e);
			}
		}
		
		public static void GetQ(int ftid, string title, IEnumerable<string> commiteestrings,
			bool answered, string url, Session samling, bool record = true)
		{
			var db = new DBDataContext();
			// check to see if we have this one and if it's answered
			var question = db.P20Questions.SingleOrDefault(
				_ => _.SessionId == samling.SessionId && _.FTId == ftid);

			if (question != null && question.AnswerDate.HasValue)
			{
				// we're done here
				return;
			}

			if (ftid == 2370)
			{
				// question is borked, ignore
				return;
			}

			// ok, follow the link
			HtmlDocument doc = Scrape2009.GetDoc(Scrape2009.fastdomain + url);

			if (question == null)
			{
				// create a new one
				// get asker, askee short title and background
				var shortitle = doc.DocumentNode.SelectSingleNode("//div[@id='menuSkip']/h1").
					InnerText.Split(new string[] { ftid.ToString() }, StringSplitOptions.None)[1].Trim().Trim('.');

				var pasker = doc.SelectHtmlNodes("//div[@id='menuSkip']/p").SingleOrDefault(
					_ => _.InnerText.Trim().ToLower().StartsWith("af "));
				if (pasker == null)
				{
					// we have to this due to this one with no asker: http://www.ft.dk/samling/20091/spoergsmaal/S445/index.htm
					return;
				}
				//var polurls = pasker.SelectNodes("a").OfType<HtmlNode>()
				//    .Where(x => x.Attributes["href"] != null)
				//    .Select(n => n.Attributes["href"].Value).Distinct();

				var politicianAnchors = pasker.SelectNodes("a").OfType<HtmlNode>();

				var askerPoliticianNameAndParty = politicianAnchors
					.Where(x => x.InnerText.Contains("("))
					.First().InnerText;
				var askerName = askerPoliticianNameAndParty.Split('(')[0].Trim();
				var askerParty = askerPoliticianNameAndParty.Split('(')[1].Replace(")", "").Trim();

				//var paskerurl = polurls.First();
					//pasker.SelectNodes("a").OfType<HtmlNode>().First().Attributes["href"].Value;
				var asker =
					Scrape2009.GetPoliticianByNameAndParty(askerName, askerParty, db)
					.PoliticianId;

				// get the relevant minister
				var minregex = new Regex(@"Til[ \t]*(?'tit'[\w\s-]*)<br>");
				var match = minregex.Matches(pasker.InnerHtml);
				if (match.Count < 1)
				{
					// might be an incomplete question, just return
					return;
				}
				string ministertitle = match[0].Groups["tit"].Value.Trim();
				//var paskeeurl = polurls.Skip(1).First();
					//pasker.SelectNodes("a").OfType<HtmlNode>().Skip(1).First().Attributes["href"].Value;
				//var askee = Scrape2009.GetPoliticianByUrl(paskeeurl, db);

				var askeeePoliticianNameAndParty = politicianAnchors
					.Skip(1).Last().InnerText;
				var askeeName = askeeePoliticianNameAndParty.Split('(')[0].Trim();

				int? askee = null;
				if (!askeeePoliticianNameAndParty.Contains("("))
				{
					// sometimes the party is not listed with name
					askee = Scrape2009.GetPoliticianByName(askeeName, db).PoliticianId;
				}
				else
				{
					var askeeParty = askeeePoliticianNameAndParty.Split('(')[1].Replace(")", "").Trim();
					askee = Scrape2009.GetPoliticianByNameAndParty(askeeName,
						askeeParty, db).PoliticianId;
				}

				var pbackground = doc.SelectHtmlNodes("//div[@id='menuSkip']/p").SingleOrDefault(
					_ => _.InnerText.Trim().ToLower().StartsWith("skriftlig begrundelse"));

				string backgroundtext = null;
				if (pbackground != null)
				{
					pbackground.InnerHtml.
						 Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries).
						 Skip(1).Aggregate((a, b) => a + " " + b.Trim());
				}

				var datereg = new Regex(@"<br>(?'day'\d\d)-(?'mon'\d\d)-(?'yea'\d\d\d\d)<br>");
				var datematch = datereg.Matches(pasker.InnerHtml);
				var askdate = new DateTime(
					int.Parse(datematch[0].Groups["yea"].Value.Trim()),
					int.Parse(datematch[0].Groups["mon"].Value.Trim()),
					int.Parse(datematch[0].Groups["day"].Value.Trim())
					);

				question = new P20Question
				{
					AskeeId = askee,
					AskerPolId = asker,
					Title = shortitle,
					Background = backgroundtext,
					Type = QuestionType.Politician,
					FTId = ftid,
					Question = title,
					AskeeTitle = ministertitle,
					SessionId = samling.SessionId,
					AskDate = askdate,
				};
				db.P20Questions.InsertOnSubmit(question);
				if (record)
				{
					db.SubmitChanges();
				}

				var committees = db.Committees.Where(_ => commiteestrings.Contains(_.Name));
				
				db.ItemCommittees.InsertAllOnSubmit(committees.ToList().Select(_ =>
					new ItemCommittee
					{
						CommitteeId = _.CommitteeId,
						ItemId = question.P20QuestionId,
						ItemType = 1
					}
					));
				if (record)
				{
					db.SubmitChanges();
				}
			}

			if (question != null && (!question.AnswerDate.HasValue || !answered))
			{
				// ok, try to get the answer, it should be there since the question looks answered
				// first, the date
				// have to do last due to this one
				// http://www.ft.dk/samling/20091/spoergsmaal/S2566/index.htm

				var dateps = doc.SelectHtmlNodes("//p[@style='padding-left:10px;']");
				if (dateps.Any())
				{
					var datep = dateps.Last();
					//doc.SelectHtmlNodes("//div[@class='lovlist' or class='line clearfix']/*/p").Single();

					var receivedregex =
						new Regex(@"Modtaget: (?'day'\d\d)-(?'mon'\d\d)-(?'yea'\d\d\d\d)<br>");
					var rdatematch = receivedregex.Matches(datep.InnerHtml);
					var answer = new DateTime(
						int.Parse(rdatematch[0].Groups["yea"].Value.Trim()),
						int.Parse(rdatematch[0].Groups["mon"].Value.Trim()),
						int.Parse(rdatematch[0].Groups["day"].Value.Trim())
						);

					var tablewithanswerlink = doc.SelectHtmlNodes("//table[@class='lovTable']").Last();

					Func<string, bool> answerrowfinder = _ =>
							_.StartsWith("Svar:") ||
							_.StartsWith("Svar :") ||
							_.StartsWith("Svar (endeligt):") ||
							_.StartsWith("Endeligt svar") ||
							_.StartsWith("Svar på") ||
							_.StartsWith("Supplerende svar på") ||
							_.StartsWith("UDKASTspg") ||
							_.ToLower().Contains("besvarelse") ||
							_.ToLower().Contains("svar på") ||
							_.StartsWith("S ");

					var rowwithcrapanswerlink = tablewithanswerlink.SelectHtmlNodes("tbody/tr").
						SingleOrDefault(_ => answerrowfinder(_.InnerText));
					if (rowwithcrapanswerlink == null)
					{
						// apparently not quite ready yet
					}
					else
					{
						var craplinkurl = rowwithcrapanswerlink.SelectHtmlNodes("td/ul/li/a").
							Single().Attributes["href"].Value;

						var crapdoc = Scrape2009.GetDoc(craplinkurl);
						// we do last due to this one
						// http://www.ft.dk/samling/20091/spoergsmaal/s2695/svar/737831/index.htm#dok
						var answerrow = crapdoc.SelectHtmlNodes("//table[@class='lovTable']/tbody/tr").
							LastOrDefault(_ => answerrowfinder(_.InnerText.Replace("  ", " ")));
						if (answerrow == null)
						{
							// due to weirdness here: http://www.ft.dk/samling/20091/spoergsmaal/s536/svar/669456/index.htm#dok
							answerrow = crapdoc.SelectHtmlNodes("//table[@class='lovTable']/tbody/tr").First();
						}
						var answerlink = answerrow.SelectHtmlNodes("td/a").
							Single(_ =>
								_.InnerText.Trim().StartsWith("Html-version")).Attributes["href"].Value;
						var docanswerlink = answerrow.SelectHtmlNodes("td/div/div/ul/li/a").
							First().Attributes["href"].Value;

						if (docanswerlink.Contains("founded"))
						{
							throw new ArgumentException("no such pdf for " + ftid);
						}

						var answerdocid = Util.DownloadDocument(docanswerlink, question);
						if (answerdocid != null)
						{
							question.AnswerDocumentId = answerdocid;
							question.AnswerDate = answer;
						}
						else
						{
							// apparently something went wrong when downloading doc, disregard
						}
					}
				}
				else
				{
					// hmm, looks like it's not actually answered for reals
				}

			}
			if (record)
			{
				//Console.WriteLine("submitting {0}", question.Title);
				db.SubmitChanges();
			}
		}

		private static void ScrapeStringAnswer(string answerlink)
		{
			var answerdoc = Scrape2009.GetDoc(Scrape2009.domain + answerlink);

			// basically loop over spans until svar is found, then record
			var spans = answerdoc.SelectHtmlNodes("//*[name()='span' or name()='div']").
				Where(_ => _.Attributes["class"] != null && _.Attributes["class"].Value.StartsWith("pos "));

			Func<string, bool> spandecider = foo => foo.ToLower().StartsWith("Svar:".ToLower()) ||
						foo.ToLower().StartsWith("Svar :".ToLower()) ||
						foo.ToLower().StartsWith("Svar (endeligt):".ToLower()) ||
						foo.ToLower().StartsWith("Endeligt svar".ToLower()) ||
						foo.ToLower().StartsWith("svar på") ||
				// shoot me, please
						foo.ToLower().StartsWith("Svar S 323".ToLower()) ||
						(foo.ToLower() == "svar" && foo.Length == 4);
			bool ispastsvar = false;
			bool ispastend = false;

			StringBuilder sb = new StringBuilder();
			foreach (var span in spans)
			{
				if (span.InnerText.ToLower().Contains("venlig hilsen"))
				{
					ispastend = true;
					break;
				}

				if (ispastsvar)
				{
					sb.Append(" " + span.InnerText);
				}

				if (spandecider(span.InnerText))
				{
					ispastsvar = true;
				}
			}
			string answerstring = sb.ToString().Trim();
		}
	}
}
