using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using ScrapeDB;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Transactions;

namespace FT.Scraper
{
	class Paragraf20
	{
		static object pollock = new object();
		static Regex partynamereg =
				new Regex(@"Til (?'tit'.*) \((?'pol'.*)\((?'par'.*)\)\).*\((?'d'\d+)/(?'m'\d+) (?'y'\d+) \).*\((?'par2'.*)\)");
		Regex qtitleregex = new Regex(@".*:(?'tit'.*)");

		public void DoScrape()
		{
			ScrapeDBDataContext db = new ScrapeDBDataContext();
			foreach (var samling in db.Sessions.Where(s => s.IsDone == false))
			{
				// hent oversigt for denne samling
				string lovurl =
					string.Format("http://ft.dk/Samling/{0}{1}/MENU/par20_oversigt.htm",
					samling.Year, samling.Number);
				var links = Scraper.GetLinkNodes(lovurl);
				links.AsParallel().ForAll(
					node => GetQs(
							node.Attributes["href"].Value,
							samling)
				);

			}
		}

		private void GetQs(string urlend, Session samling)
		{
			HtmlDocument doc = Scraper.GetDoc("http://ft.dk" + urlend);
			var rows = doc.DocumentNode.SelectNodes("//table/tr").OfType<HtmlNode>();
			rows.AsParallel().ForAll(lawRow => GetSingleQ(lawRow, samling));

		}

		private void GetSingleQ(HtmlNode qRow, Session s)
		{
			string urlend = qRow.ChildNodes[3].ChildNodes[0].Attributes["href"].Value;

			var tseDoc = Scraper.GetDoc("http://ft.dk" + urlend);
			var trs = tseDoc.DocumentNode.SelectNodes("//tr/td[@valign='TOP' and @colspan='2']");
			HtmlNode askeenode;
			if (trs.Count == 1)
				askeenode = trs[0];
			else
				askeenode = trs[1];

			string qtitle = tseDoc.DocumentNode.SelectSingleNode("//head/title").InnerText.Trim().TrimEnd(new char[] {'.'});
			qtitle = qtitleregex.Matches(qtitle)[0].Groups["tit"].Value.Trim();
			if (qtitle.ToLower().StartsWith("om "))
			{
				// cut off "Om " and capitalize
				qtitle = char.ToUpper(qtitle[3]) + qtitle.Substring(4);
			}

			var tagnode = tseDoc.DocumentNode.SelectSingleNode("//head/meta[@name='FTW-UEMNE']");
			IEnumerable<string> tags = null;
			if (tagnode != null)
			{
				// crappy redundant code
				tags = tagnode.Attributes["content"].Value.Trim().Split(',').
					Select(_ => 
						_.Trim().ToLower().Replace(" ","").Replace("/","")).
					Where(_ => !string.IsNullOrEmpty(_) && _ != "");
			}

			string askeestring = askeenode.InnerText.Replace("\r", "").Replace("\n", "");

			var match = partynamereg.Matches(askeestring);
			if (match.Count == 0)
			{
				// string with no date in it, give up
				return;
			}

			var matches = match[0];
			var partyname = matches.Groups["par"].Value.Trim();
			var partyname2 = matches.Groups["par2"].Value.Trim();
			var polname = matches.Groups["pol"].Value.Trim();
			var title = matches.Groups["tit"].Value.Trim();
			var month = int.Parse(matches.Groups["m"].Value);
			var date = int.Parse(matches.Groups["d"].Value);
			var year = int.Parse(matches.Groups["y"].Value);

			string pol2name;
			string pol2link = null;
			if (askeenode.SelectSingleNode("a") != null)
			{
				pol2name = askeenode.SelectSingleNode("a").InnerText;
				pol2link = askeenode.SelectSingleNode("a").Attributes["href"].Value;
			}
			else
			{
				// there is no lik for the asker
				pol2name = (new Regex(@".* af: (?'nam2'.*) \(.*\)")).Matches(askeestring)[0].Groups["nam2"].Value.Trim();
			}


			// TODO, also get ft question id
			var idstring = tseDoc.DocumentNode.SelectSingleNode("//tr/th[@valign='TOP' and @colspan='2']/b").
				InnerText;
			var id = int.Parse((new Regex(@"(\d+)")).Match(idstring).Groups[0].Value);

			var question = tseDoc.DocumentNode.SelectNodes("//tr/td[@valign='TOP']").OfType<HtmlNode>().
				Where(_ => _.InnerText.Trim().ToLower() == "Spm i fuld tekst:".ToLower()).Single().
				ParentNode.SelectNodes("td")[1].InnerText.Trim();

			var reason = tseDoc.DocumentNode.SelectNodes("//tr/td[@valign='TOP']").OfType<HtmlNode>().
				Where(_ => _.InnerText.Trim().ToLower() == "Skr begrundelse:".ToLower()).Single().
				ParentNode.SelectNodes("td")[1].InnerText.Trim();

			// check too see whether we already have this one
			ScrapeDBDataContext db = new ScrapeDBDataContext();
			P20Question q = db.P20Questions.SingleOrDefault(
				_ => _.FTId == id && _.SessionId == s.SessionId);
			//&& _.Question == question

			var asker = CheckForPolitian(pol2name, partyname2, pol2link, db);
			var askee = CheckForPolitian(polname, partyname, null, db);

			if (q != null)
			{
				// already there, what to update?
			}
			else
			{
				q = new P20Question()
					{
						AskeeId = askee.PoliticianId,
						AskerPolId = asker.PoliticianId,
						Question = question,
						Background = reason,
						AskDate = new DateTime(year + 2000, month, date),
						FTId = id,
						SessionId = s.SessionId,
						Doc1 = tseDoc.DocumentNode.OuterHtml,
						AskeeTitle = title,
						Title = qtitle,
						Type = QuestionType.Politician,
					};
				db.P20Questions.InsertOnSubmit(q);
				db.SubmitChanges();
				if (tags != null)
				{
					db.Tags.InsertAllOnSubmit(
						from t in tags
						select new Tag()
						{
							ContentType = ContentType.P20Question,
							ContentId = q.P20QuestionId,
							UserId = 6,
							TagName = t,
							Date = q.AskDate.Value,
						}
						);
				}
			}
			db.SubmitChanges();


			// get the link node for the answer, in case it's provided
			var links = tseDoc.DocumentNode.SelectNodes("//tr/td[@align='LEFT' and @colspan='2']/a");
			if (links != null && links.Count > 0)
			{
				var link = links[0];
				var answerdoc = Scraper.GetDoc("http://ft.dk" + link.Attributes["href"].Value);
				if (q.AnswerDate == null)
				{
					q.Doc2 = answerdoc.DocumentNode.OuterHtml;
					// digg out the date
					if (answerdoc.DocumentNode.SelectNodes("//tr/td") == null)
					{
						// apparently not ready yet, give up
						return;
					}
					var answerdate = answerdoc.DocumentNode.SelectNodes("//tr/td").OfType<HtmlNode>().
						Where(_ => _.InnerText.Trim().ToLower() == "Svar dato:".ToLower()).Single().
						ParentNode.SelectNodes("td")[1].InnerText;

					// parse the date
					var thematches = (new Regex(@"(?'d'\d+)/(?'m'\d+) (?'y'\d+)")).Match(answerdate);
					var amonth = int.Parse(thematches.Groups["m"].Value);
					var adate = int.Parse(thematches.Groups["d"].Value);
					var ayear = int.Parse(thematches.Groups["y"].Value);

					q.AnswerDate = new DateTime(ayear + 2000, amonth, adate);
					db.SubmitChanges();
				}

				if (!q.AnswerParas.Any())
				{
					// get the next document over
					var links2 = answerdoc.DocumentNode.SelectNodes("//tr/td[@valign='TOP' and @colspan='2']/a");
					var thelinks = links2.OfType<HtmlNode>().Where(
						_ =>
							//_.InnerText.Trim().ToLower().StartsWith("Svar på spm.".ToLower()) && 
							_.InnerText.Trim().ToLower().EndsWith("(HTM)".ToLower())
						);
					HtmlNode thelink;
					if (thelinks.Count() > 1)
					{
						var morelinks = thelinks.Where(_ => _.InnerText.Trim().ToLower().StartsWith("Svar".ToLower()));
						if (morelinks.Any())
						{
							thelink = morelinks.First();
						}
						else
						{
							if (thelinks.Any(_ => _.InnerText.ToLower().Contains("besvarelse")))
								thelink = thelinks.Where(_ => _.InnerText.ToLower().Contains("besvarelse")).First();
							else
								thelink = thelinks.First();
						}
					}
					else if (thelinks.Count() == 0)
					{
						// give up
						return;
					}

					else
					{
						thelink = thelinks.Single();
					}



					// this page has a different encoding than the other nes
					var answer2doc = Scraper.GetDoc("http://ft.dk" + thelink.Attributes["href"].Value,
						Encoding.UTF8
						//Encoding.GetEncoding("iso-8859-1")
						);

					q.Doc3 = answer2doc.DocumentNode.OuterHtml;
					// get the relevant elements
					//  and @style='line-height: 14pt;'
					// /span[@lang='DA']

					//[@style='margin-right: -2.7pt;']
					var spans = answer2doc.DocumentNode.SelectNodes(
					"//p[@class='MsoNormal' or @class='MsoBodyText' or @class='MVTUBrdtekstfed' or @class='MVTUBrdtekst' or @class='Underpunkter' or @class='BMOverskrift1' or @class='BMBrdtekst' or @class='MsoNormalCxSpMiddle' or @class='MsoHeader' or @class='Normal-medluft'] | //h1 | //h2"
						).OfType<HtmlNode>().
						Where(_ => !string.IsNullOrEmpty(_.InnerText.Trim()) && !_.InnerText.Trim().Equals("&nbsp;")).ToArray();

					bool ispastsvar = false;
					int nr = 1;
					for (int i = 0; i < spans.Length; i++)
					{
						if (ispastsvar)
						{
							db.AnswerParas.InsertOnSubmit(
								new AnswerPara()
								{
									QuestionId = q.P20QuestionId,
									ParText = spans[i].InnerText.Trim(),
									Number = nr++,
								}
								);
						}

						string foo = spans[i].InnerText.Trim().ToLower();
						if (foo.StartsWith("Svar:".ToLower()) ||
							foo.StartsWith("Svar :".ToLower()) ||
							foo.StartsWith("Svar (endeligt):".ToLower()) ||
							foo.StartsWith("Endeligt svar".ToLower()) ||
							foo.StartsWith("svar på") ||
							// shoot me, please
							foo.StartsWith("Svar S 323".ToLower()) ||
							(foo == "svar" && foo.Length == 4))
							ispastsvar = true;
					}
					if (ispastsvar == false)
					{
						Console.WriteLine("crap, no svar found");
					}

					db.SubmitChanges();
				}
			}
		}

		private static Politician CheckForPolitian(string name, string party, string pollink, ScrapeDBDataContext db)
		{
			Politician pol;
			lock (pollock)
			{
				using (TransactionScope ts = new TransactionScope())
				{
					pol = db.Politicians.SingleOrDefault(p => p.Name == name);
					if (pol != null)
					{
					}
					else
					{
						pol = new Politician { Name = name, PartyString = party, };
						db.Politicians.InsertOnSubmit(pol);
						db.SubmitChanges();
						Console.WriteLine("           Created politker {0}", name);
					}
					ts.Complete();
				}
			}
			return pol;
		}

	}
}
