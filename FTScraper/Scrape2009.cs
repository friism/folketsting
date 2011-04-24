using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FT.DB;
using System.Net;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using FolketsTing.Controllers;
using SD = System.Drawing;
using FT.Model;
using System.IO;
using System.Drawing.Imaging;
using System.Data.Linq;
using System.Globalization;


namespace FT.Scraper
{
	public class Scrape2009
	{
		public static string domain = "http://www.ft.dk/"; //"http://ft.dk/";
		public static string fastdomain = "http://www.ft.dk/dokumenter/tingdok.aspx?";

		public static object dblock = new object();

		public void DoScrape()
		{
			var db = new DBDataContext();
			GetCategories();
			TripScraper.GetTrips();
			TripScraper.GeoCode();
			foreach (var samling in db.Sessions.Where(s => s.IsDone == false))
			{
				DoSession(samling);
				CategorizeLaws(samling.Year, samling.Number);
			}

			FetchPolPics();
		}

		private static void AssortedCalls()
		{
			//polcache = db.Politicians.ToList();
			//var craplaw = db.Laws.Single(_ => _.LawId == 731);
			//var foolaw = GetNewLaw("Lov om ændring af lov om naturbeskyttelse, lov om miljøbeskyttelse og forskellige andre love.");
			//FetchCrapPoliticians();
			//BulkShrinkPolPics();
			//DeleteLargePics();
			//return;

			//return;

			//FetchPoliticians();

			//TripScraper.ExportPols();
			//TripScraper.GetTrips();
			//FetchPolPics();
		}

		private void DoSession(Session samling)
		{
			DoLaws(samling);
			Dop20s(samling);
		}

		private void Dop20s(Session samling)
		{
			// there are two types. Bizarely, the oral ones are easiest to scrape
			// this gets the ones with written answers
			string url =
				string.Format(
				"http://www.ft.dk/Dokumenter/Vis_efter_type/Spoergsmaal/" +
					"Spoergsmaal.aspx?questionSearchtype=2&startDate=&endDate=&" +
					"session={0}{1}&committee=-1&minister=&statusAnswer=-1&" +
					"inquirer=-1&sortColumn=&sortOrder=&startRecord=&totalNumberOfRecords=&" +
					"numberOfRecords={2}&pageNr=1",
				samling.Year, samling.Number, 100000);

			var doc = GetDoc(url);
			var rows = doc.DocumentNode.SelectHtmlNodes(
				"//div[@id='page']/*/table/tbody/tr").Distinct(new QuestionRowCompater());

			var parsedrows = rows.Select(r => ParseRow(r));

			var groupedrows = from r in parsedrows
							  group r by r.Item1 into g
							  select new Tuple<int, string, IEnumerable<string>, bool, string>
							  (g.Key, g.First().Item2,
								g.Select(_ => _.Item3), g.First().Item4, g.First().Item5);

			groupedrows.AsParallel().WithDegreeOfParallelism(20).ForAll(
				_ => P20QuestionScraper.GetQChecked(_.Item1, _.Item2, _.Item3, _.Item4, _.Item5,
					samling) //"L 67"  "L 14" "L 24 A" "L 103""L 78" "L 2"
				);
		}

		private static Tuple<int, string, string, bool, string> ParseRow(HtmlNode row)
		{
			var links = row.SelectNodes("td/a").OfType<HtmlNode>().ToArray();
			int ftid = int.Parse(links[0].InnerText.Trim().Split(' ')[1]);
			string title = links[1].InnerText.Trim();
			string committeestring = links[2].InnerText.Trim();
			bool answered = !links[3].InnerText.Contains("Ikke");
			string url = links[0].Attributes["href"].Value;
			return new Tuple<int, string, string, bool, string>(
				ftid, title, committeestring, answered, url);
		}

		private static void DoLaws(Session samling)
		{
			var rows = GetLawRows(samling.Year, samling.Number);
			rows.AsParallel().WithDegreeOfParallelism(20).ForAll(
				_ => GetLawChecked(_, samling,
					//"L 127", 2000, 1
					 null, null, null
					)
				);
		}//"L 67"  "L 14" "L 24 A" "L 103""L 78" "L 2"

		public static IEnumerable<HtmlNode> GetLawRows(int year, int number)
		{
			// get a view of all the laws
			string lawurl = domain +
				"Dokumenter/Vis_efter_type/Lovforslag.aspx?session={0}{1}&caseStatus=-1&ministerArea=-1&committee=&proposedBy=-1&startDate=&endDate=&dateRelatedActivity=&sortColumn=&sortOrder=&startRecord=0&totalNumberOfRecords=&numberOfRecords={2}&pageNr=1";
			string url = string.Format(lawurl, year, number, 10000);
			HtmlDocument doc = GetDoc(url);

			var rows = doc.DocumentNode.SelectHtmlNodes(
				"//div[@id='page']/*/table/tbody/tr");
			return rows;
		}

		public static void GetLawChecked(HtmlNode row, Session samling, string singlecode, 
			int? singleyear, int? singlenr)
		{
			var links = row.SelectNodes("td/a").OfType<HtmlNode>().ToArray();
			string code = links[0].InnerText.Trim();
			string name = links[1].InnerText.Trim();
			// we skip propersers since it's better on the actual law
			string ministry = links[3].InnerText.Trim();
			string committee = links[4].InnerText.Trim();
			string state = links[5].InnerText.Trim();

			// actual link to law is inside any one of these
			string url = links[0].Attributes["href"].Value;
			try
			{

				GetLaw(code, name, ministry, committee, state, url, 
					samling, singlecode, singleyear, singlenr, true);
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Problem with law {0} {1} {2} {3}",
					code, name, samling.Year, samling.Number), e);
			}
		}

		public static void GetLaw(string code, string name, string ministry, string committee,
			string state, string url, Session samling, string singlecode, 
			int? singleyear, int? singlenr, bool record)
		{

			if (!string.IsNullOrEmpty(singlecode) && !(code == singlecode &&
				samling.Year == singleyear && samling.Number == singlenr))
			{
				Console.WriteLine("WARNING, in singlemode");
				return; // this is used to test single laws
			}

			DBDataContext db = new DBDataContext();
			// check to see if we have this law already
			var law = db.Laws.SingleOrDefault(l =>
				l.FtId == code && l.SessionId == samling.SessionId);

			Func<Law, bool> pursuitdecider = thelaw => thelaw != null &&
				thelaw.Deliberations.Any(_ => _.Number == LawStage.First &&
					_.Speeches.Any() && !_.Speeches.Any(s => s.IsTemp.Value)) &&
				thelaw.Deliberations.Any(_ => _.Number == LawStage.Second &&
					_.Speeches.Any() && !_.Speeches.Any(s => s.IsTemp.Value)) &&
				thelaw.Deliberations.Any(_ => _.Number == LawStage.Third &&
					_.Speeches.Any() && !_.Speeches.Any(s => s.IsTemp.Value)) &&
				thelaw.Paragraphs.Any(_ => _.Stage == LawStage.First) &&
				//thelaw.Paragraphs.Any(_ => _.Stage == LawStage.Second) &&
				thelaw.Paragraphs.Any(_ => _.Stage == LawStage.Third) &&
				thelaw.LawVotes.Any(_ => _.IsFinal.Value);

			// maybe it's completely done, in that case do nothing
			if (pursuitdecider(law))
			{
				// looks complete
				Console.WriteLine("skipping...");
				return;
			}

			//fetch the actual document
			//try
			//{
			HtmlDocument doc = GetDoc(fastdomain + url);
			if (law == null)
			{
				var h1 = doc.DocumentNode.SelectSingleNode("//div[@id='menuSkip']/h1").
					InnerText.Replace(code, "").Trim();

				var parenp = doc.DocumentNode.SelectHtmlNodes("//div[@id='menuSkip']/p").
					SingleOrDefault(_ => _.InnerText.StartsWith("("));
				string subtitle = null;
				if (parenp != null)
				{
					var parenhtml = parenp.InnerHtml;
					subtitle = parenhtml.Substring(0, parenhtml.IndexOf("<br>")).
						Trim(new char[] { '(', ')', '.' }).Trim();

				}
				// create new law
				law = GetNewLaw(h1);
				law.Subtitle = subtitle;
				law.FtId = code;
				law.MinistryId = GetMinistryId(ministry, db);
				law.CommitteeId = GetCommitteeId(committee, db);
				law.SessionId = samling.SessionId;
				if (record)
				{
					lock (dblock)
						db.Laws.InsertOnSubmit(law);
				}
			}

			if (string.IsNullOrEmpty(law.Summary))
			{
				// get the resume
				string resume = null;
				if (doc.DocumentNode.SelectNodes("//div[@id='menuSkip']/p").OfType<HtmlNode>().
					Where(_ => _.InnerText.Trim().StartsWith("Resumé")).Any())
				{
					var resumepar =
						doc.DocumentNode.SelectNodes("//div[@id='menuSkip']/p").OfType<HtmlNode>().
							Single(_ => _.InnerText.Trim().StartsWith("Resumé"));
					resume = resumepar.InnerText.Replace("Resumé", "").Trim();
					law.Summary = resume;
				}
			}

			if (!law.ProposedLaws.Any())
			{
				// the p with proposers is somewhere in there, it's the first one
				var proposerpar = doc.DocumentNode.SelectNodes("//div[@id='menuSkip']/p")[0];
				var pollinks = proposerpar.SelectHtmlNodes("a");
				//Regex mintitleregex = new Regex(@"Af (?'tit'\w*)<br>");
				Regex mintitleregex = new Regex(@".*Af (?'tit'[\w\s\-]*)<br>");

				var match = mintitleregex.Matches(proposerpar.InnerHtml);
				string ministername = null;
				if (match.Count > 0)
				{
					// this likely means it has been proposed by a minister
					ministername = match[0].Groups["tit"].Value.Trim();
				}
				else
				{
					// not proposed by minister, but by group of members
					Regex proposerregex = new Regex(
						//@".*Af (<a title="""" href=""(?'url'[\w\./]*)"">(?'nam'[\w\s\-()]*)</a>(, )?)+"
						@"<a title=\\""\\"" href=\\""(?'url'[\w\./]*)\\"">(?'nam'[\w\s\-\.]*)\s*\((?'par'[\w]*)\)</a>"
						);
					var matches = proposerregex.Matches(proposerpar.InnerHtml);
					foreach (var m in matches.OfType<Match>())
					{
						var polurl = m.Groups["url"].Value.Trim();
						var polname = m.Groups["nam"].Value.Trim();
						var partyname = m.Groups["par"].Value.Trim();
					}
				}

				// this bit of crud to to borkiness in this law: http://www.ft.dk/samling/20091/lovforslag/L36/index.htm
				if (pollinks.Select(p => p.Attributes["href"]).Distinct().Count() >= 1
					&& !string.IsNullOrEmpty(ministername))
				{
					// save the one minister
					int polid = -1;
					if (pollinks.First().Attributes["href"] != null)
					{
						polid = GetPoliticianByUrl(pollinks.First().Attributes["href"].Value, db).Value;
					}
					else
					{
						// where in a big doo-doo
						// http://www.ft.dk/samling/20091/lovforslag/L127/index.htm
						var lameministername = pollinks.First().InnerText.
							Replace("(", "").Replace(")", "").Trim();
						if (lameministername == "Lykke Friis")
						{
							polid =
								GetPoliticianByNameAndParty(lameministername, "V", db).
								PoliticianId;
						}
					}

					lock (dblock)
					{
						db.ProposedLaws.InsertOnSubmit(
						new ProposedLaw
						{
							Law = law,
							PoliticianId = polid,
							IsMinister = true,
							Title = ministername
						});
					}

				}
				else if (!string.IsNullOrEmpty(ministername) && pollinks.Count() > 1)
				{
					// something went very wrong
					throw new ArgumentException();
				}
				else
				{
					// just save the set of proposers
					lock (dblock)
					{
						db.ProposedLaws.InsertAllOnSubmit(
							pollinks.Select(_ =>
								new ProposedLaw
								{
									Law = law,
									IsMinister = false,
									PoliticianId = GetPoliticianByUrl(_.Attributes["href"].Value, db)
								}
							));
					}
				}
			}

			// fish out the relevant dates

			var datepar =
				doc.DocumentNode.SelectNodes("//div[@id='menuSkip']/p").OfType<HtmlNode>().
					SingleOrDefault(_ => _.InnerText.Trim().StartsWith("Sagsgang"));
			if (datepar == null)
			{
				// completely borked one, see here:
				// http://www.ft.dk/dokumenter/tingdok.aspx?/samling/20072/lovforslag/L31A/index.htm
				return;
			}

			var fremsatdate = GetDate(datepar.InnerHtml, "Fremsat");
			var firstbehdate = GetDate(datepar.InnerHtml, "1. behandlet / henvist til udvalg");
			//var secondbehdate = GetDate(datepar.InnerHtml, "2. behandlet/direkte til 3. behandling");
			var secondbehdate = GetDate(datepar.InnerHtml, "2. behandlet/henvist til udvalg");
			if (secondbehdate == null)
			{
				// sometimes they use different text
				secondbehdate = GetDate(datepar.InnerHtml, "2. behandlet/direkte til 3. behandling");
			}
			var thirdbehdate = GetDate(datepar.InnerHtml, "3. behandlet, vedtaget");
			law.Proposed = fremsatdate;
			law.FirstDeliberation = firstbehdate;
			law.SecondDeliberation = secondbehdate;
			law.Passed = thirdbehdate;

			if (!law.Speakers.Any())
			{
				// "ordførere", should always be there
				string speakerurl = fastdomain +
					string.Format("/samling/{0}{1}/lovforslag/{2}/ordforere.htm",
						samling.Year, samling.Number, code.Replace(" ", ""));
				var speakerdoc = GetDoc(speakerurl);
				var speakerlinks = speakerdoc.DocumentNode.SelectNodes("//ul/li/a").OfType<HtmlNode>().
						Where(_ => _.InnerText.Trim() == "Vis medlemsside");

				var speakerhrefs = speakerlinks.Select(_ => _.Attributes["href"].Value.Trim());
				List<Speaker> speakers = new List<Speaker>();
				foreach (var href in speakerhrefs)
				{
					Speaker s = new Speaker();
					s.Law = law;
					s.PoliticianId = GetPoliticianByUrl(href, db);
				}
				//= 
				//    speakerhrefs.Distinct().Select(_ =>
				//        new Speaker
				//        {
				//            Law = law,
				//            PoliticianId = GetPoliticianByUrl(_, db)
				//        }
				//    );
				lock (dblock)
					db.Speakers.InsertAllOnSubmit(speakers);
			}

			Func<LawStage, Law, bool> delibdecider = (n, l) =>
				(!l.Deliberations.Any(_ => _.Number == n) || !l.Deliberations.Single(_ => _.Number == n).Speeches.Any() ||
				l.Deliberations.Single(_ => _.Number == n).Speeches.Any(_ => _.IsTemp == true));

			// 1. deliberation, may not always be there			string deliburl = domain + "samling/20081/lovforslag/{0}/BEH{1}/forhandling.htm";
			if (firstbehdate.HasValue && delibdecider(LawStage.First, law))
			{
				GetDeliberation(code.Replace(" ", ""), LawStage.First, samling.Year, samling.Number, firstbehdate.Value, law, db);
			}
			if (secondbehdate.HasValue && delibdecider(LawStage.Second, law))
			{
				GetDeliberation(code.Replace(" ", ""), LawStage.Second, samling.Year, samling.Number, secondbehdate.Value, law, db);
			}
			if (thirdbehdate.HasValue && delibdecider(LawStage.Third, law))
			{
				GetDeliberation(code.Replace(" ", ""), LawStage.Third, samling.Year, samling.Number, thirdbehdate.Value, law, db);
			}

			bool ischange = name.Contains("ændring af");
			if (!law.Paragraphs.Any(p => p.Stage == LawStage.First))
				GetLawtext(code.Replace(" ", ""), LawStage.First, samling.Year, samling.Number, law, db);
			if (!law.Paragraphs.Any(p => p.Stage == LawStage.Second))
				GetLawtext(code.Replace(" ", ""), LawStage.Second, samling.Year, samling.Number, law, db);
			if (!law.Paragraphs.Any(p => p.Stage == LawStage.Third))
				GetLawtext(code.Replace(" ", ""), LawStage.Third, samling.Year, samling.Number, law, db);

			// and get vote, if any
			GetVotes(doc, code.Replace(" ", ""), samling.Year, samling.Number, law, db);

			lock (dblock)
			{
				Console.WriteLine(string.Format("Submitting {0}", law.ShortName));
				db.SubmitChanges();
			}
			//}
			//catch (Exception e)
			//{
			//    throw new Exception("Problem with law: " + code + " " + samling.Year + " " + samling.Number + " -- " + law.ShortName, e);
			//}
		}

		private static Law GetNewLaw(string name)
		{
			string ample1 = "Forslag til lov om ændring af ";
			string ample2 = "Forslag til ";
			string ample3 = "Forslag  til ";
			string ample4 = "Lov om ændring af ";
			string noamble = null;
			//string subt = null;

			bool changefalg = false;

			Law lov = new Law();

			if (name.StartsWith(ample1))
			{
				noamble = name.Substring(ample1.Length);
				noamble = char.ToUpper(noamble[0]) + noamble.Substring(1);
				changefalg = true;
			}
			else if (name.StartsWith(ample2))
			{
				noamble = name.Substring(ample2.Length);
				noamble = char.ToUpper(noamble[0]) + noamble.Substring(1);
			}
			else if (name.StartsWith(ample3))
			{
				noamble = name.Substring(ample3.Length);
				noamble = char.ToUpper(noamble[0]) + noamble.Substring(1);
			}
			else if (name.StartsWith(ample4))
			{
				noamble = name.Substring(ample4.Length);
				noamble = char.ToUpper(noamble[0]) + noamble.Substring(1);
				changefalg = true;
			}
			else
			{
				// very simple title
				lov.ShortName = name.Trim(new char[] { '.' });
				return lov;
			}

			//if (name.Contains("(") && string.IsNullOrEmpty(lov.Subtitle))
			//{
			//    subt = name.Split(new char[] { '(' })[1].Split(new char[] { ')' })[0];
			//    if(!string.IsNullOrEmpty(noamble) && noamble.Contains("("))
			//        noamble = noamble.Split(new char[] { '(' })[0];

			//}

			if (!string.IsNullOrEmpty(noamble))
			{
				noamble = noamble.Trim().TrimEnd(new char[] { '.' });
				if (changefalg)
					noamble = noamble + " (ændring af)";
				lov.ShortName = noamble;
			}
			//if (!string.IsNullOrEmpty(subt))
			//    lov.Subtitle = subt;

			//if (!string.IsNullOrEmpty(lov.ShortName) && lov.ShortName.Contains("("))
			//{
			//    int parentindex = lov.ShortName.IndexOf('(');
			//    string followstring = lov.ShortName.Split('(')[1];
			//    if (!followstring.StartsWith("ændring af"))
			//    {
			//        // exise the subtitle
			//        string newname = lov.ShortName.Split(new char[] { '(' })[0];
			//        if (lov.ShortName.Contains("ændring af"))
			//        {
			//            newname = newname + "(ændring af)";
			//        }
			//        lov.ShortName = newname;
			//    }
			//}

			return lov;
		}

		private static void GetVotes(HtmlDocument doc, string lawftid, int samyear, int samnr, Law law, DBDataContext db)
		{
			//string voteoverviewurl = null;
			//we need to dig out the proper url
			if (doc.DocumentNode.SelectHtmlNodes("//a[@title='Afstemning']") != null &&
				doc.DocumentNode.SelectHtmlNodes("//a[@title='Afstemning']").Any())
			{
				var afs = doc.DocumentNode.SelectHtmlNodes("//a[@title='Afstemning']");
				foreach (var afurl in afs)
				{
					string voteoverviewurl = fastdomain +
						//string.Format("dokumenter/tingdok.aspx?%2fsamling%2f{0}{1}%2flovforslag%2f{2}%2f32%2fafstemning.htm=&pageSize=50&pageNr=1", samyear, samnr, lawftid);
						afurl.Attributes["href"].Value;

					var overviewdoc = GetDoc(voteoverviewurl);
					//Scraper.GetDoc(domain + voteoverviewurl + "=&pageSize=50&pageNr=1");
					var voteurls = overviewdoc.SelectHtmlNodes("//a").
						Where(_ => _.InnerText.StartsWith("Afstemningsnummer:")).
						Select(_ => _.Attributes["href"].Value);

					foreach (var url in voteurls)
					{
						var votedoc = GetDoc(fastdomain + url);
						bool isfinal = votedoc.SelectHtmlNodes("//p").
							Any(_ => _.InnerText.Contains("Endelig vedtagelse"));
						var table = votedoc.DocumentNode.SelectSingleNode("//table[@class='lovFilter']");
						var rows = table.SelectHtmlNodes("./tr");

						var datestrong =
							votedoc.DocumentNode.SelectHtmlNodes("//div[@id='menuSkip']/p").
							Single(_ => _.InnerText.StartsWith("Afstemningsnummer")).SelectSingleNode("./strong").InnerText.Trim();
						//votedoc.DocumentNode.SelectSingleNode("//div[@class='menuSkip']/p/strong").InnerText.Trim();
						var firstcomma = datestrong.IndexOf(',');
						var lastcomma = datestrong.LastIndexOf(',');
						var datestring = datestrong.Substring(firstcomma + 1, lastcomma - firstcomma - 1).Trim();
						var dateparts = datestring.Split(new char[] { '.' });
						var date = new DateTime(int.Parse(dateparts[2]), int.Parse(dateparts[1]), int.Parse(dateparts[0]));

						var goodp = votedoc.DocumentNode.SelectHtmlNodes("//div[@id='menuSkip']/p").
							Single(_ => _.InnerText.StartsWith("Afstemningsnummer")).InnerHtml.Trim();
						var lastbrindex = goodp.LastIndexOf("<br>");
						var votename = goodp.Substring(lastbrindex + 4, goodp.Length - lastbrindex - 4).Trim();

						if (db.LawVotes.Any(_ => _.Name == votename &&
							_.Law.FtId == lawftid &&
							_.Law.Session.Year == samyear &&
							_.Law.Session.Number == samnr))
						{
							//alredy have this one
							continue;
						}

						//if(!string.IsNullOrEmpty(votename) && votename.Length > 200)
						//    throw new Exception("argh, too long");

						LawVote lv = new LawVote
						{
							IsFinal = isfinal,
							Law = law,
							Name = votename,
							Date = date,
						};
						lock (dblock)
							db.LawVotes.InsertOnSubmit(lv);

						// first row has header info
						foreach (var row in rows.Skip(1))
						{
							var cells = row.SelectHtmlNodes("./td").ToArray();
							var name = cells[0].InnerText.Trim();
							var party = cells[1].InnerText.Trim();
							Politician p = GetPoliticianByNameAndParty(name, party, db);
							var votestring = cells[2].InnerText.Trim();
							int vote;
							switch (votestring)
							{
								case "For": vote = 0; break;
								case "Imod": vote = 1; break;
								case "Hverken for eller imod": vote = 2; break;
								case "Fraværende": vote = 3; break;
								default: throw new ArgumentException(string.Format("What's this vote: {0}", votestring));
							}

							// Save this vote
							PoliticianLawVote v = new PoliticianLawVote
							{
								Vote = (byte)vote,
								LawVote = lv,
								Politician = p,
							};
							lock (dblock)
								db.PoliticianLawVotes.InsertOnSubmit(v);
						}
					}
				}

			}
			else
			{
				return;
			}
		}

		private static string GetLawtext(string lawftid, LawStage delibnr, int samyear, int samnr, Law law, DBDataContext db)
		{
			bool ischange = false;
			if (!string.IsNullOrEmpty(law.ShortName) && law.ShortName.Contains("(ændring af)"))
				ischange = true;
			// fremsat lawtext, should always be there
			string urlchunk;
			switch (delibnr)
			{
				case LawStage.First: urlchunk = "som_fremsat"; break;
				case LawStage.Second: urlchunk = "efter_2behandling"; break;
				case LawStage.Third: urlchunk = "som_vedtaget"; break;
				default: throw new ArgumentException();
			}
			string url = fastdomain + string.Format("samling/{0}{1}/lovforslag/{2}/{3}.htm", samyear, samnr, lawftid, urlchunk);
			var doc = GetDoc(url);
			if (!doc.DocumentNode.InnerText.Contains("Teksten kunne ikke findes"))
			{
				var fontelement = doc.DocumentNode.SelectSingleNode(
					"//div[@id='menuSkip' and @class='content']");
				string html = fontelement.InnerHtml.Trim();
				if (ischange)
					Law2009.DoChangeLawTexts(html, delibnr, law, db);
				else
					Law2009.DoNonChangeLawTexts(html, delibnr, law, db);
				return html;
			}
			else
			{
				return null;
			}
		}

		private static void GetDeliberation(string lawftid, LawStage delibnr,
			int samyear, int samnr, DateTime delibdate, Law lov, DBDataContext db)
		{
			// use www-domain for this one, for some reason
			string url = domain +
				string.Format("dokumenter/tingdok.aspx?/samling/{0}{1}/lovforslag/{2}/beh{3}/forhandling.htm&startItem=-1",
					samyear, samnr, lawftid, delibnr.UrlValue()
				);

			var delibdoc = GetDoc(url);
			if (delibdoc.DocumentNode.InnerText.Contains("Referatet kunne ikke findes.") ||
				delibdoc.DocumentNode.InnerText.Contains("Referatet af forhandlingerne er endnu ikke tilgængelige.") ||
				delibdoc.DocumentNode.InnerText.Contains("Dokumentet er ikke fundet"))
			{
				// not there yet
				return; //delibdoc.DocumentNode.SelectSingleNode("//div[@id='page']").InnerHtml;
			}
			else
			{
				var releasedcount = delibdoc.SelectHtmlNodes("//meta[@content='Released']").Count();
				var proofedcount = delibdoc.SelectHtmlNodes("//meta[@content='Proofed']").Count() +
					delibdoc.SelectHtmlNodes("//meta[@content='Typeset']").Count();

				if (releasedcount == 0 && proofedcount == 0)
				{
					// nothing to see here, move along
					return;
				}

				//if (proofedcount > 0 && releasedcount > 0)
				//{
				//    throw new ArgumentException("both 1. and 2. version texts");
				//}

				bool isdraft;

				if (releasedcount > 0 && !(releasedcount < proofedcount && releasedcount < 3))
					isdraft = true;
				else if (proofedcount > 0)
				{
					isdraft = false;
					if (releasedcount > 1)
					{
						Console.WriteLine("bad proof-status");
					}
				}
				else
				{
					if (DateTime.Now.Year - samyear > 1)
					{
						// assume its done
						isdraft = false;
					}
					else
					{
						//return;
						throw new ArgumentException("cannot determine draft status");
					}
				}

				// ok, looks like we have the real deal
				// check to see if we already have deliberation
				var d = db.Deliberations.SingleOrDefault(_ => _.Number == delibnr && _.LawId == lov.LawId);
				if (d != null)
				{
					if (isdraft && d.Speeches.Any(_ => _.IsTemp.Value))
					{
						// we've found one that is still draft, and we've already recorded draft stuff
						return;
					}
					if (!isdraft && d.Speeches.Any(_ => !_.IsTemp.Value))
					{
						// we're not in a draft situation and we have non-draft speeches
						return;
					}

					if (isdraft && d.Speeches.Any(_ => !_.IsTemp.Value))
					{
						throw new ArgumentException("hmm, it's draft, but we have non-draft speeches!");
					}

					//ok, we seems to be good
				}
				else
				{
					// create new deliberation
					d = new Deliberation
					{
						Date = delibdate,
						Number = delibnr,
						Law = lov
					};
					lock (dblock)
						db.Deliberations.InsertOnSubmit(d);
				}

				//these are the relevant ones
				var speechdivs = delibdoc.
					SelectHtmlNodes(
						"//div[@id='menuSkip']/div[@class='tableTitle' or @class='tableTitleComment' or @class='lovListViewAllElements' or @class='tableTitle clearfix']"
					);

				//if (speechdivs.Count() > 0) //DBDataContext
				//{
				//    // there are speeches and we don't know whether they are drafts, crap
				//    throw new ArgumentException("Don't know whether speeches are drafts or not");
				//}

				// divs are somewhat threaded, one speaker says something and others respond
				Speech currentspeakerspeech = null;
				var count = 0; // used to order them
				// itereate over relevant divs and create speeches
				foreach (var div in speechdivs)
				{
					if (div.Attributes["class"].Value == "lovListViewAllElements")
						currentspeakerspeech = null; // this ends a stream of comments
					else
					{
						// also fish out time and politician name
						string polstring = div.SelectSingleNode("div[@class='col2']/a").InnerText;
						string name = null;
						string party = null;
						string title = null;
						if (polstring != "Formanden" && polstring != "Kommentar") // kommentar-bit is due to here: http://www.ft.dk/dokumenter/tingdok.aspx?/samling/20081/lovforslag/l2/beh1/forhandling.htm&startItem=-1
						{
							if (polstring.ToLower().Contains("minister"))
							{
								// uh-oh
								title = polstring.Split('(')[0].Trim();
								name = polstring.Split('(')[1].Replace(")", "").Trim();
							}
							else
							{
								name = polstring.Split('(')[0].Trim();
								party = polstring.Split('(')[1].Replace(")", "").Trim();
							}
						}
						else
						{
							// it's the formand
						}

						string timestring =
							div.SelectSingleNode("div[@class='col3']").InnerText.Trim();
						int? hours = null;
						int? minutes = null;
						if (!string.IsNullOrEmpty(timestring))
						{
							var split = ' ';
							if (timestring.Contains(":"))
								split = ':';
							else if (timestring.Contains("."))
								split = '.';
							else
								throw new ArgumentException("unknown timesplitter");

							timestring = timestring.Replace("Kl.", "");
							hours = int.Parse(timestring.Split(split)[0].Trim());
							minutes = int.Parse(timestring.Split(split)[1].Trim());
						}
						else
						{
							// no time means we're in the first bogus speech, ignore
							continue;
						}

						// get politician
						var p = GetPoliticianByNameAndParty(name, party, db);

						if (!string.IsNullOrEmpty(title) && title.Length > 50)
							throw new Exception("argh, long title");

						var speech = new Speech
						{
							SpeechNr = count++,
							Politician = p,
							Deliberation = d,
							IsTemp = isdraft,
							PoliticianTitle = title

						};
						if (minutes.HasValue)
						{
							speech.SpeechTime = new DateTime(delibdate.Year, delibdate.Month,
								delibdate.Day, hours.Value, minutes.Value, 0);
						}

						if (div.Attributes["class"].Value == "tableTitleComment")
						{
							// it's likely a comment to speaker one, or introductory formand stuff    
							speech.ParentSpeech = currentspeakerspeech;

						}
						else if (div.Attributes["class"].Value == "tableTitle" ||
							div.Attributes["class"].Value == "tableTitle clearfix")
						{
							// it's one of the speaker ones
							speech.ParentSpeech = null;
						}
						else
						{
							throw new ArgumentException("unknow speech state");
						}

						lock (dblock)
							db.Speeches.InsertOnSubmit(speech);

						currentspeakerspeech = speech;

						lock (dblock)
							db.SpeechParas.InsertAllOnSubmit(GetPars(div.NextSibling, speech));
					}
				}
			}
		}

		private void FetchPolPics()
		{
			var db = new DBDataContext();
			var pols = db.Politicians.Where(_ => 
				_.ImageId == null || _.Homepage == null || _.Birthdate == null);

			pols.AsParallel().WithDegreeOfParallelism(20).ForAll(p => UpdatePol(p, db));
			db.SubmitChanges();
		}

		private void UpdatePol(Politician pol, DBDataContext db)
		{
			try
			{
				var doc = GetDoc(pol.FTMemberPage());
				if (pol.Homepage == null)
				{
					GetSetPolHomepage(pol, doc);
				}
				if (pol.ImageId == null)
				{
					GetDefaultPolPic(db, pol, doc);
				}
				if (pol.Birthdate == null)
				{
					GetPolBirthDate(pol, doc);
				}
				//GetLargePolPic(db, pol, doc);
			}
			catch (Exception e)
			{
				// prolly just means politician is not there
			}
		}

		private void GetPolBirthDate(Politician pol, HtmlDocument doc)
		{
			var firstp = doc.SelectHtmlNodes(
				"//div[@class='tabContainer clearfix']/div[@class='tabContent clearfix']/p").
				First();

			string regs = @" født \d{1,2}. \w{1,4}. \d{4}";
			Regex reg = new Regex(regs);
			var match = reg.Matches(firstp.InnerText);
			
			if (match.Count > 0)
			{
				string url = match[0].Groups[0].Value.Trim();
				var datepart = url.Replace("født","").Trim();
					//url.Split(new string[]{"født"}, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
				try
				{
					var bdate = DateTime.ParseExact(datepart, "dd. MMM. yyyy", CultureInfo.GetCultureInfo("da-DK"));
					pol.Birthdate = bdate;
				}
				catch (Exception ee)
				{
					try
					{
						var bdate = DateTime.ParseExact(datepart, "d. MMM. yyyy", CultureInfo.GetCultureInfo("da-DK"));
						pol.Birthdate = bdate;

					}
					catch (Exception e)
					{
						var bdate = DateTime.ParseExact(datepart, "dd. MMMM. yyyy", CultureInfo.GetCultureInfo("da-DK"));
						pol.Birthdate = bdate;
					}
				}
			}
			else
			{

			}
		}

		//private void FetchPolPics()
		//{
		//    var db = new DBDataContext();
		//    var pols = db.Politicians.Where(_ => _.ShrunkImageId == null || _.Homepage == null);

		//}

		private void GetDefaultPolPic(DBDataContext db, Politician pol, HtmlDocument doc)
		{
			var imgnode = doc.DocumentNode.SelectSingleNode("//div[@class='person clearfix']/img");
			if (imgnode != null)
			{
				var imgurl = imgnode.Attributes["src"].Value;
				try
				{
					var img = Util.GetNewImage(
						"http://www.ft.dk/Folketinget/Medlemmer/findMedlem/" + imgurl);
					if (img != null)
					{
						db.Images.InsertOnSubmit(img);
						db.SubmitChanges();
						pol.ImageId = img.ImageId;
						db.SubmitChanges();

					}
				}
				catch (Exception e)
				{
					// let it glide
				}
			}
		}

		private static void GetLargePolPic(DBDataContext db, Politician pol, HtmlDocument doc)
		{
			if (doc.DocumentNode.SelectSingleNode("//div[@class='download']//a") != null)
			{
				var piclink =
					doc.DocumentNode.SelectSingleNode(
					"//div[@class='download']//a").Attributes["href"].Value;
				//http://www.ft.dk/Folketinget/Medlemmer/findMedlem/~/media/biographies/HighRes/Highres_kvadrat/SophieHaestorpAndersen%20jpg.ashx

				try
				{
					var img = Util.GetNewImage(
						"http://www.ft.dk/Folketinget/Medlemmer/findMedlem/" + piclink);
					if (img != null)
					{
						db.Images.InsertOnSubmit(img);
						db.SubmitChanges();
						pol.ImageId = img.ImageId;
						db.SubmitChanges();

						ShrinkPolPic(pol.PoliticianId, db);

						// delete the big picture again, takes up too much space
						pol.ImageId = null;
						db.Images.DeleteOnSubmit(img);
						db.SubmitChanges();
					}
				}
				catch (Exception e)
				{
					Console.WriteLine("Curses, couldn't download pic for " + pol.Initials + " " + pol.PoliticianId);
				}
			}
			else
			{
				Console.WriteLine("problem with " + pol.Initials + " " + pol.PoliticianId);
			}
		}

		private static void GetSetPolHomepage(Politician pol, HtmlDocument doc)
		{
			var persondiv = doc.DocumentNode.SelectSingleNode("//div[@class='person clearfix']").InnerHtml;
			var lines = persondiv.Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
			var homepage = lines.SingleOrDefault(_ => _.Trim().StartsWith("Hjemmeside"));
			if (homepage != null)
			{
				string urltemplate = @"href=""(?'url'[^""]*)""";
				Regex reg = new Regex(urltemplate);
				var match = reg.Matches(homepage);
				if (match.Count > 0)
				{
					string url = match[0].Groups["url"].Value.Trim();
					pol.Homepage = url;
				}
			}
		}

		private void DeleteLargePics()
		{
			Console.WriteLine("preparing deletion");
			var db = new DBDataContext();
			var pols = db.Politicians.Where(_ => _.ImageId != null);
			foreach (var p in pols)
			{
				db.Images.DeleteOnSubmit(db.Images.Single(_ => _.ImageId == p.ImageId));
				p.ImageId = null;
				Console.WriteLine("deleted " + p.ImageId);
			}
			Console.WriteLine("here goes nothing...");
			db.SubmitChanges();
		}

		//private void FetchCrapPoliticians()
		//{
		//    var db = new DBDataContext();
		//    var crappols = 
		//        db.Politicians.
		//        //polcache.
		//        Where(_ => _.Firstname == null || _.Firstname == "");
		//    foreach (var pol in crappols)
		//    {
		//        FetchCrapPol(pol, db);
		//    }
		//    db.SubmitChanges();

		//}

		private static void FetchCrapPol(Politician pol, DBDataContext db)
		{
			var doc = GetDoc(pol.FTMemberPage());
			var h1 = doc.DocumentNode.SelectSingleNode("//h1").InnerText;
			string poltemplate = @"(?'nam'[^\)]*)\((?'par'[\w]*)\)";
			Regex reg = new Regex(poltemplate);
			var match = reg.Matches(h1);
			if (match.Count > 0)
			{
				string name = match[0].Groups["nam"].Value.Trim();
				string partyinitials = match[0].Groups["par"].Value.Trim();
				pol.Firstname = name.Split(new char[] { ' ' })[0].Trim();
				pol.Lastname = name.Replace(pol.Firstname + " ", "");
				pol.PartyId = db.Parties.Single(_ => _.Initials == partyinitials).PartyId;
				return;
			}
			else
			{
				if (!h1.Contains('('))
				{
					pol.Firstname = h1.Split(new char[] { ' ' })[0].Trim();
					pol.Lastname = h1.Replace(pol.Firstname + " ", "");
					return;
				}
				Console.WriteLine("problem, not adding pol");    
			}
			Console.WriteLine("problem, not adding pol");
		}

		private void FetchPoliticians()
		{
			string polurl = "http://www.ft.dk/Folketinget/Medlemmer/searchResults.aspx?letter=ALLE&pageSize=500&pageNr=1";
			HtmlDocument doc = GetDoc(polurl);
			var rows = doc.SelectHtmlNodes("//table[@class='telbogTable']/tr").Skip(1); // skip header row
			var db = new DBDataContext();

			//rows.AsParallel().WithDegreeOfParallelism(20).ForAll(
			//    r => FetchPolitician(db, r)
			//    );

			foreach (var row in rows)
			{

				FetchPolitician(db, row);
			}
		}

		private static void FetchPolitician(DBDataContext db, HtmlNode row)
		{
			var cells = row.SelectHtmlNodes("td").ToArray();
			var initials = GetFTIdFromUrl(cells[4].SelectHtmlNodes("ul/li/a").
						Single(_ => _.InnerText.Trim() == "Vis medlemsside").
						Attributes["href"].Value);

			if (!
				db.Politicians.
				Any(p => p.Initials == initials))
			{
				var pol = new Politician
				{
					Firstname = cells[0].InnerText.Trim(),
					Lastname = cells[1].InnerText.Trim(),
					PartyId = db.Parties.Single(_ => _.Name == cells[2].InnerText.Trim()).PartyId,
					Initials = initials,
				};

				var html = cells[4].InnerHtml.Trim();

				if (html.Contains("e-mail"))
				{
					pol.EmailAdd = cells[4].SelectHtmlNodes("ul/li/a").
						Single(_ => _.InnerText.Trim() == "Send e-mail").
						Attributes["href"].Value.Replace("mailto:", "");
				}

				if (html.Contains("Tlf"))
				{
					var tlfxi = html.IndexOf("Tlf: ");
					var bri = html.IndexOf("<br>");
					var tlfstring = html.Substring(tlfxi + 5, bri - tlfxi - 5);
					pol.Telephone = tlfstring.Trim();
				}
				if (html.Contains("Mobil"))
				{
					var tlfxi = html.IndexOf("Mobil: ");
					var bri = html.IndexOf("<br>", tlfxi);
					var tlfstring = html.Substring(tlfxi + 7, bri - tlfxi - 7);
					pol.Mobile = tlfstring.Trim();
				}


				db.Politicians.InsertOnSubmit(pol);
				db.SubmitChanges();

				//polcache = db.Politicians.ToList();
			}
		}

		private static Politician GetPoliticianByNameAndParty(string name, string party, DBDataContext db)
		{
			lock (dblock)
			{
				if (name == "Karina Lorentzen")
					name = "Karina Lorentzen Dehnhardt";

				Politician p = null;
				if (name != null)
				{
					p = db.Politicians.
						//polcache.
						SingleOrDefault(_ => _.Firstname + " " + _.Lastname == name &&
							_.Party.Initials == party);
					if (p == null)
					{
						p = db.Politicians.
							//polcache.
							SingleOrDefault(_ => _.Firstname + " " + _.Lastname == name);
					}
					if (p == null)
					{
						// this is to catch Inger "Beinov" Støjberg
						p = db.Politicians.
							//polcache.
							SingleOrDefault(
							_ => _.Firstname != null &&
								_.Firstname.StartsWith(name.Trim().Split(new char[] { ' ' })[0]) &&
								_.Lastname != null &&
								_.Lastname == name.Trim().Split(new char[] { ' ' })[1]);
					}
					if (p == null)
					{
						// try some voodoo
						// initials seem to be comprised of party and first two letters of first and lastname
						var lastspace = name.LastIndexOf(' ');
						string firsttwo = name.Substring(0, 2);
						string lasttwo = name.Substring(lastspace + 1, 2);
						int pid = GetPoliticianFromSuspectedInitials(party + firsttwo + lasttwo, name, party, db);
						p = db.Politicians.
							//polcache.
							Single(_ => _.PoliticianId == pid);
						//throw new ArgumentException("could not find politician " + name);
					}
				}
				return p;
			}
		}

		private static int GetMinistryId(string minname, DBDataContext db)
		{
			lock (dblock)
			{
				Ministry min = db.Ministries.SingleOrDefault(_ => _.Name == minname);
				if (min == null)
				{
					min = new Ministry() { Name = minname };
					db.Ministries.InsertOnSubmit(min);
					lock (dblock)
						db.SubmitChanges();
				}
				return min.MinistryId;
			}
		}

		private static string GetFTIdFromUrl(string url)
		{
			url = url.Trim();
			var slashi = url.LastIndexOf('/');
			var doti = url.LastIndexOf('.');
			var ftid = url.Substring(slashi + 1, doti - slashi - 1);
			return ftid.Trim();
		}

		public static int? GetPoliticianByUrl(string url, DBDataContext db)
		{
			lock (dblock)
			{
				var ftid = GetFTIdFromUrl(url);
				ftid = NormaliseInitials(ftid);
				var pol = db.Politicians.
					//polcache.
					SingleOrDefault(_ => _.Initials == ftid);
				if (pol == null)
				{
					// this shouldn't happen, somebody stop it
					pol = new Politician() { Initials = ftid };
					FetchCrapPol(pol, db);

					if (!string.IsNullOrEmpty(pol.Firstname) &&
						!string.IsNullOrEmpty(pol.Lastname) &&
						!string.IsNullOrEmpty(pol.Initials))
					{
						db.Politicians.InsertOnSubmit(pol);
						db.SubmitChanges();
					}
					else
					{
						// this is bad
						return null;
					}
				}
				return pol.PoliticianId;
			}
		}

		private static void ShrinkPolPic(int polid, DBDataContext db)
		{
			var pol = db.Politicians.Single(_ => _.PoliticianId == polid);
			if (pol.ImageId.HasValue)
			{
				FileRepository _fileRep = new FileRepository();

				FT.DB.Image i = _fileRep.Image(pol.ImageId.Value);
				SD.Image img = SD.Image.FromStream(new MemoryStream(i.Data.ToArray()));
				img = img.Resize(1000, 1000);

				MemoryStream s = new MemoryStream();
				img.Save(s, GetImageFormat(i.ContentType));
				Binary bin = new Binary(s.ToArray());

				FT.DB.Image ishrunk = new Image();
				ishrunk.Data = bin;
				ishrunk.ContentType = i.ContentType;
				db.Images.InsertOnSubmit(ishrunk);
				db.SubmitChanges();
				pol.ImageId = ishrunk.ImageId;
				db.SubmitChanges();
			}
		}

		private static ImageFormat GetImageFormat(string contenttype)
		{
			switch (contenttype)
			{
				case "image/jpeg": return ImageFormat.Jpeg;
				default: throw new ArgumentException(contenttype);
			}
		}

		private static int GetPoliticianFromSuspectedInitials(
			string initials, string name, string party, DBDataContext db)
		{
			initials = NormaliseInitials(initials);
			//else if (initials == "AnRa")
			//    initials = "VANFR";

			var pol =
				db.Politicians.
				//polcache.
				SingleOrDefault(_ => _.Initials == initials);
			if (pol == null)
			{
				// ok, not good, but see if he may be there
				try
				{
					//hm, new we are desperate, execute a search
					var docattempt =
						GetDoc(string.Format(
							"http://www.ft.dk/Search.aspx?q={0}&tab=1&pageSize=10&pageNr=1",
							name.Replace(' ', '+')));
					// doing first here is pretty daring
					var url = docattempt.SelectHtmlNodes("//div[@class='lefty pers']/h3/a").
						First().Attributes["href"].Value;

					var goodinitials = GetFTIdFromUrl(url);
					goodinitials = NormaliseInitials(goodinitials);
					pol = db.Politicians.
						//polcache.
						SingleOrDefault(_ => _.Initials == goodinitials);
					if (pol != null)
					{
						return pol.PoliticianId;
					}
					else
					{
						pol = new Politician { Initials = goodinitials };
						db.Politicians.InsertOnSubmit(pol);

						FetchCrapPol(pol, db);

						lock (dblock)
							db.SubmitChanges();

						//polcache = db.Politicians.ToList();
						return pol.PoliticianId;
					}
				}
				catch (Exception e)
				{
					try
					{
						var docattempt =
							GetDoc(string.Format(
								"http://www.ft.dk/Folketinget/Medlemmer/findMedlem/{0}.aspx",
								initials));

						// OK, looks like politician may actually be there
						pol = new Politician { Initials = initials };
						db.Politicians.InsertOnSubmit(pol);

						FetchCrapPol(pol, db);

						lock (dblock)
							db.SubmitChanges();

						//polcache = db.Politicians.ToList();

						return pol.PoliticianId;
					}
					catch (Exception e2)
					{
						throw new ArgumentException("I really can't find this guy " + initials + " -- " + name + " " + party, e2);
						// this is bad
						pol = new Politician
						{
							// do not use these initials, as they are made up and suspected
							//Initials = initials,
							Firstname = name.Trim().Split(' ').First(),
							Lastname = name.Trim().Split(' ').Skip(1).Aggregate((a, b) => a + " " + b),
							PartyId = db.Parties.Single(_ => _.Initials == party).PartyId,
						};
						db.Politicians.InsertOnSubmit(pol);
						lock (dblock)
							db.SubmitChanges();

						//polcache = db.Politicians.ToList();
						return pol.PoliticianId;
					}
				}
			}
			else
			{
				return pol.PoliticianId;
			}
		}

		private static string NormaliseInitials(string initials)
		{
			if (initials == "DFBeHo")
				initials = "DFBKHF";
			else if (initials == "SIUDoJa")
				initials = "NADOJA";
			else if (initials == "KFPeLø")
				initials = "KFPERL";
			else if (initials == "VMaRø")
				initials = "VMADR";
			else if (initials == "ChSa" ||
				initials.ToLower() == "KF_Charlotte%20Sahl-Madsen".ToLower() ||
				initials.ToLower() == "KF_Charlotte Sahl-Madsen".ToLower())
				initials = "kf_charlotte%20sahl-madsen";
			else if (initials == "DFJeDa")
				initials = "DF_JettePlesnerDali";
			else if (initials == "SNiNi")
				initials = "snien";
			return initials;
		}

		public static int GetCommitteeId(string comname, DBDataContext db)
		{
			//var db = new DBDataContext();
			lock (dblock)
			{
				Committee com = db.Committees.SingleOrDefault(_ => _.Name == comname);
				if (com == null)
				{
					com = new Committee() { Name = comname };
					db.Committees.InsertOnSubmit(com);

					db.SubmitChanges();
				}
				return com.CommitteeId;
			}
		}

		public static IEnumerable<SpeechPara> GetPars(HtmlNode speechnode, Speech speech)
		{
			var parnodes = speechnode.SelectHtmlNodes("p[@class='Tekst' or @class='TekstIndryk']");
			var counter = 0;
			foreach (var node in parnodes)
			{
				yield return new SpeechPara { Number = counter++, Speech = speech, ParText = node.InnerText };
			}
		}

		private void BulkShrinkPolPics()
		{
			DBDataContext db = new DBDataContext();
			foreach (var pol in db.Politicians.Where(_ => _.ImageId == null))
			{
				ShrinkPolPic(pol.PoliticianId, db);
			}
		}

		private static DateTime? GetDate(string html, string name)
		{
			string dateregtemplate = @"{0} (?'dat'[^<]*)";
			Regex reg = new Regex(string.Format(dateregtemplate, name));
			var match = reg.Matches(html);
			if (match.Count > 0)
			{
				string date = match[0].Groups["dat"].Value;
				var comps = date.Split('-');
				return new DateTime(int.Parse(comps[2]), int.Parse(comps[1]), int.Parse(comps[0]));
			}
			else
				return null;
		}

		public static HtmlDocument GetDoc(string url)
		{
			return GetDoc(url, 0);
		}

		private static HtmlDocument GetDoc(string url, int tries)
		{
			try
			{
				return GetDocRec(url);
			}
			catch (Exception e)
			{
				if (tries < 10)
				{
					return GetDoc(url, ++tries);
				}
				else
				{
					throw new Exception(string.Format("Failed to download {0}", url), e);
				}
			}
		}

		private static HtmlDocument GetDocRec(string url)
		{
			return Scraper.GetDoc(url, Encoding.UTF8);
		}

		private static void CategorizeLaws(int year, int number)
		{
			var db = new DBDataContext();
			var cats = db.Categories;

			foreach (var c in cats)
			{
				var doc = GetDoc(
					string.Format("http://www.ft.dk/Dokumenter/Vis_efter_emne/" +
					"{0}.aspx?session={1}{2}&caseType=100016&subject={0}&startDate=&endDate=" +
					"&sortColumn=&sortOrder=&startRecord=&numberOfRecords=100000" +
					"&totalNumberOfRecords=&pageNr=1", c.FTId, year, number)
					);

				var rows = doc.SelectHtmlNodes("//div[@id='page']/*/table/tbody/tr");

				foreach (var r in rows)
				{
					var ftid = r.SelectHtmlNodes("td/a").ElementAt(0).InnerText.Trim();
					var lawid = db.Laws.Single(l => l.FtId == ftid
								&& l.Session.Year == year && l.Session.Number == number).LawId;
					if (!db.ItemCategories.Any(ic => ic.ItemType == ItemType.Law
						&& ic.ItemId == lawid && ic.CategoryId == c.CategoryId))
					{
						db.ItemCategories.InsertOnSubmit(
							new ItemCategory
							{
								CategoryId = c.CategoryId,
								ItemId = lawid,
								ItemType = ItemType.Law,
							});
					}
				}
			}
			db.SubmitChanges();
		}

		private static void GetCategories()
		{
			var doc = GetDoc("http://www.ft.dk/Dokumenter/Vis_efter_emne.aspx");

			var links = doc.SelectHtmlNodes("//ul/li/a[@class='noArrow']").
				Where(_ => _.Attributes["href"].Value.StartsWith("Vis_efter_emne"));

			var cats = links.Select(l =>
				new Category
				{
					Name = l.InnerText.Trim(),
					FTId = (byte)int.Parse(
						l.Attributes["href"].Value.Split('.').First().Split('/').Skip(1).First().Trim())
				});
			var db = new DBDataContext();
			db.Categories.InsertAllOnSubmit(
				cats.Where(c => !db.Categories.Any(
					_ => _.FTId == c.FTId && _.Name == c.Name)));
			db.SubmitChanges();
		}
	}

	public static class Extensions
	{
		public static IEnumerable<HtmlNode> SelectHtmlNodes(this HtmlDocument doc, string xpath)
		{
			var nodes = doc.DocumentNode.SelectNodes(xpath);
			if (nodes != null)
				return nodes.OfType<HtmlNode>();
			else
				return Enumerable.Empty<HtmlNode>();
		}

		public static IEnumerable<HtmlNode> SelectHtmlNodes(this HtmlNode node, string xpath)
		{
			var nodes = node.SelectNodes(xpath);
			if (nodes != null)
				return node.SelectNodes(xpath).OfType<HtmlNode>();
			else
				return Enumerable.Empty<HtmlNode>();
		}
	}

	public class QuestionRowCompater : IEqualityComparer<HtmlNode>
	{
		public bool Equals(HtmlNode x, HtmlNode y)
		{
			var xlinks = x.SelectNodes("td/a").OfType<HtmlNode>().ToArray();
			var ylinks = y.SelectNodes("td/a").OfType<HtmlNode>().ToArray();
			int xftid = int.Parse(xlinks[0].InnerText.Trim().Split(' ')[1]);
			int yftid = int.Parse(ylinks[0].InnerText.Trim().Split(' ')[1]);

			return xftid == yftid;
		}

		public int GetHashCode(HtmlNode obj)
		{
			var xlinks = obj.SelectNodes("td/a").OfType<HtmlNode>().ToArray();
			int xftid = int.Parse(xlinks[0].InnerText.Trim().Split(' ')[1]);
			return xftid;
		}
	}
}
