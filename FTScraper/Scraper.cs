using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using HtmlAgilityPack;
using System.Transactions;

namespace FT.Scraper
{
	public class Scraper
	{
		static bool getlawtexts = true;

		static bool getafstemninger = true;
		static bool getbehandlinger = true;
		static bool gettaler = false;

		static bool onlygetlawname = false;
		//static bool createupdatelaws = false;

		private static List<string> tobeconsidered =
			null;
		//    new List<string>() 
		//{
		//    "ligningsloven, lov om vægtafgift af motorkøretøjer",
		//    //"Udlændingeloven og retsafgiftsloven",
		//};

		static Encoding e = Encoding.GetEncoding("ISO-8859-1");

		static object pollock = new object();

		static void Main(string[] args)
		{
			//TripScraper.ExportCommittees();
			//return;

			Scrape2009 newScraper = new Scrape2009();
			newScraper.DoScrape();
			return;

			//Scraper s = new Scraper();
			//s.DoScrape();
			//s.HelpBadPols();
			//s.UpdatePols();
			//s.GetPolPics();


			//Paragraf20 p = new Paragraf20();
			//p.DoScrape();

			//if (tobeconsidered == null)
			//{
			//}
			Console.WriteLine("Press the any key...");
			Console.ReadKey();
		}


		// some pols don't have url, get one for them
		//private void HelpBadPols()
		//{
		//    ScrapeDBDataContext db = new ScrapeDBDataContext();
		//    foreach (var p in db.Politicians.Where(_ => _.BioUrl == null))
		//    {
		//        p.BioUrl = string.Format("/BAGGRUND/Biografier/{0}.htm",
		//            p.Name.Replace(' ','_').Replace("æ","ae").Replace("å","aa").Replace("ø","oe"));
		//    }
		//    db.SubmitChanges();
		//}

		//private void UpdatePols()
		//{
		//    ScrapeDBDataContext db = new ScrapeDBDataContext();
		//    foreach (var p in db.Politicians.Where(_ => _.BioUrl != null && _.BioUrl != "" /*&& _.BioUrl2 == null*/))
		//    {
		//        try
		//        {
		//            var doc = GetDoc("http://ft.dk" + p.BioUrl);
		//            var script = doc.DocumentNode.SelectNodes("//script").OfType<HtmlNode>().
		//                SingleOrDefault(_ => _.OuterHtml.Contains("document.referrer.indexOf('/system/krumme/')"));
		//            if (script != null)
		//            {
		//                string prelink = "top.location.href='";
		//                int start = script.InnerText.IndexOf(prelink) + prelink.Length;
		//                int end = script.InnerText.IndexOf((char)39, start);
		//                string link = script.InnerText.Substring(start, end - start);
		//                p.BioUrl2 = link;
		//                if (!string.IsNullOrEmpty(p.BioUrl2))
		//                {
		//                    var doc2 = GetDoc("http://ft.dk" + p.BioUrl2);
		//                    p.BioText = doc2.DocumentNode.OuterHtml;
		//                }
		//            }
		//        }
		//        catch(Exception e)
		//        {
		//            // swallow, some pols may have bad urls created by HelpBadPols()
		//        }
		//    }
		//    db.SubmitChanges();
		//}

		//private void UpdatePols2()
		//{
		//    ScrapeDBDataContext db = new ScrapeDBDataContext();
		//    foreach (var p in db.Politicians.Where(_ => _.BioUrl2 != null && _.BioUrl2 != "" /*&& _.BioText == null*/))
		//    {
		//        var doc = GetDoc("http://ft.dk" + p.BioUrl2);
		//        p.BioText = doc.DocumentNode.OuterHtml;
		//    }
		//    db.SubmitChanges();
		//}

		//private void GetPolPics()
		//{
		//    // http://www.ft.dk/billeder/DFBENB.jpg
		//    ScrapeDBDataContext db = new ScrapeDBDataContext();
		//    foreach (var p in db.Politicians.Where(_ => _.Initials != null && _.Initials != "" && _.ImageId == null))
		//    {
		//        FT.DB.Image i = Util.GetImage(string.Format("http://www.ft.dk/billeder/{0}.jpg", p.Initials));
		//        if (i != null)
		//        {
		//            db.Images.InsertOnSubmit(i);
		//            db.SubmitChanges();
		//            p.ImageId = i.ImageId;
		//            db.SubmitChanges();
		//        }
		//    }

		//}

		//private void DoScrape()
		//{
		//    ScrapeDBDataContext db = new ScrapeDBDataContext();

		//    foreach (var samling in db.Sessions.Where(s => s.IsDone == false))
		//    {
		//        WebClient wc = GetWC();
		//        // hent oversigt for denne samling
		//        //string url = string.Format("http://ft.dk/samling/{0}{1}/menu/00000002.htm", samling.year, samling.number);

		//        // lov-oversigt har vist altid denne url
		//        string lovurl =
		//            string.Format("http://ft.dk/samling/{0}{1}/MENU/00000121.htm",
		//            samling.Year, samling.Number);
		//        var links = GetLinkNodes(lovurl);
		//        //links.ForAll(node => GetLaws(node.Attributes["href"].Value));
		//        links.AsParallel().ForAll(
		//            node =>
		//                GetLaws(
		//                    node.Attributes["href"].Value,
		//                    samling,
		//                    GetMinistryId(node.InnerText.Trim())));
		//        //foreach (var node in links.AsParallel())
		//        //{
		//        //    GetLaws(node.Attributes["href"].Value, samling);
		//        //}
		//    }
		//}

		//private static int GetMinistryId(string minname)
		//{
		//    ScrapeDBDataContext db = new ScrapeDBDataContext();

		//    Ministry min = db.Ministries.SingleOrDefault(_ => _.Name == minname);
		//    if (min == null)
		//    {
		//        min = new Ministry() { Name = minname };
		//        db.Ministries.InsertOnSubmit(min);
		//    }
		//    db.SubmitChanges();
		//    return min.MinistryId;
		//}

		//// takes care of the outer law-page
		//// this is the one tat says: "by minister, not passed etc."
		//private static void GetLaws(string urlend, Session samling, int ministryid)
		//{
		//    string url = "http://ft.dk" + urlend;
		//    var links = GetLinkNodes(url);
		//    foreach (var node in links)// not a good place for parallel, usually max three entries
		//    {
		//        GetLaws2(node.Attributes["href"].Value, samling, ministryid);
		//    }
		//}

		//// iterates the list of "not passed" etc
		//private static void GetLaws2(string urlend, Session samling, int ministryid)
		//{
		//    HtmlDocument doc = GetDoc("http://ft.dk" + urlend);
		//    var rows = doc.DocumentNode.SelectNodes("//table/tr").OfType<HtmlNode>();
		//    rows.AsParallel().ForAll(lawRow => GetSingleLaw(lawRow, samling, ministryid));
		//    //foreach (var lawRow in rows.AsParallel())
		//    //{
		//    //    GetSingleLaw(lawRow, samling);
		//    //}
		//}

		//private static void GetSingleLaw(HtmlNode lawRow, Session samling, int ministryid)
		//{
		//    string name = lawRow.ChildNodes[4].InnerText;
		//    if (tobeconsidered != null)
		//    {
		//        if (!tobeconsidered.Any(_ => name.ToLower().Contains(_.ToLower())))
		//            return;
		//    }
		//    string urlend = lawRow.ChildNodes[3].ChildNodes[0].Attributes["href"].Value;
		//    bool record = false;
		//    HtmlDocument lawDoc = null;
		//    string ftid = urlend.Split('/')[4].TrimStart('L');
		//    IEnumerable<HtmlNode> links = Enumerable.Empty<HtmlNode>();
		//    if (!onlygetlawname)
		//    {
		//        lawDoc =
		//            GetDoc("http://ft.dk" + urlend);
		//        var linksorig = lawDoc.DocumentNode.SelectNodes("//table/tr/td/a");
		//        if (linksorig == null)
		//            return;
		//        links = linksorig.OfType<HtmlNode>();
		//        //examine the links to see if any point to speeches

		//        foreach (HtmlNode l in links)
		//        {
		//            string text = l.InnerText.Trim().ToLower();
		//            if (text.StartsWith("1.beh".ToLower()))
		//            {
		//                record = true;
		//            }
		//            else if (text.StartsWith("2.beh".ToLower()))
		//            {
		//                record = true;
		//            }
		//            else if (text.StartsWith("3.beh".ToLower()))
		//            {
		//                record = true;
		//            }
		//            else if (text.StartsWith("Lovf som fremsat".ToLower()))
		//            {
		//                record = true;
		//            }
		//            //else if (text.StartsWith("Fremme af"))
		//            //{
		//            //    record = true;
		//            //}
		//            //else
		//            //{
		//            //    throw new Exception();
		//            //}
		//        }
		//    }
		//    if (record || onlygetlawname)
		//    {
		//        ScrapeDBDataContext db = new ScrapeDBDataContext();

		//        // decide whether this law is already in the database
		//        Law lov = db.Laws.SingleOrDefault(
		//            l => l.FtId == ftid && l.SessionId == samling.SessionId);
		//        if (lov == null && !onlygetlawname)
		//        {
		//            // no lov, create 
		//            lov = new Law
		//            {
		//                OverviewText = lawDoc.DocumentNode.OuterHtml,
		//                SessionId = samling.SessionId,
		//                Name = name,
		//                FtId = ftid,
		//                MinistryId = ministryid
		//            };
		//            db.Laws.InsertOnSubmit(lov);
		//            Console.WriteLine("Created law {0}", lov.Name);
		//        }
		//        else
		//        {
		//            // due to my stupidity
		//            if (lov != null)
		//            {
		//                // already there, update
		//                lov.Name = name;
		//                if (!onlygetlawname)
		//                {
		//                    lov.OverviewText = lawDoc.DocumentNode.OuterHtml;
		//                    lov.MinistryId = ministryid;
		//                }
		//                Console.WriteLine("Updated law {0}", lov.Name);
		//            }
		//        }
		//        db.SubmitChanges();

		//        foreach (HtmlNode l in links)
		//        {
		//            string text = l.InnerText.Trim();
		//            if (text.StartsWith("1.beh"))
		//            {
		//                if (getbehandlinger)
		//                    GetBehandling(1, l, lov);
		//            }
		//            else if (text.StartsWith("2.beh"))
		//            {
		//                if (getbehandlinger)
		//                    GetBehandling(2, l, lov);
		//            }
		//            else if (text.StartsWith("3.beh"))
		//            {
		//                if (getbehandlinger)
		//                    GetBehandling(3, l, lov);
		//            }

		//            db.SubmitChanges();
		//        }

		//        if (getafstemninger)
		//        {
		//            // so the url to afstemninger is not in the html, but in some javascript
		//            if (
		//                lawDoc.DocumentNode.OuterHtml.Contains("LinieRef [29] = \"/") &&
		//                lawDoc.DocumentNode.OuterHtml.Contains("LinieText [29] =\"Afstemnin")
		//                )
		//            {
		//                string l = ExtractLinkFromJS(lawDoc, 29);
		//                GetAfstemnings(l, lov);
		//            }
		//        }

		//        if (getlawtexts)
		//        {
		//            foreach (HtmlNode l in links)
		//            {
		//                string text = l.InnerText.Trim();
		//                if (text.StartsWith("Lovf som vedt") &&
		//                    string.IsNullOrEmpty(lov.PassedText))
		//                {
		//                    Getlawtext(l, lov,
		//                        (Law thelaw, string s) => thelaw.PassedText = s,
		//                        (Law thelaw, DateTime d) => thelaw.PassedDate = d, 3, false);
		//                }
		//                else if (text.StartsWith("Lovf som fremsat") &&
		//                    string.IsNullOrEmpty(lov.ProposedLawText))
		//                {
		//                    Getlawtext(l, lov,
		//                        (Law thelaw, string s) => thelaw.ProposedLawText = s,
		//                        (Law thelaw, DateTime d) => thelaw.ProposedLawDate = d, 3, false);
		//                }
		//            }

		//            // the link for text after second vote is bad
		//            if (lawDoc.DocumentNode.OuterHtml.Contains("Lovf optrykt efter 2") &&
		//                string.IsNullOrEmpty(lov.AfterSecondLawText))
		//            {
		//                string theurl = ExtractLinkFromJS(lawDoc, 9);
		//                if (theurl != null)
		//                {
		//                    theurl = "/" + theurl;

		//                    string thetext = ExtractLinkTextFromJS(lawDoc, 9);
		//                    HtmlNode l = HtmlNode.CreateNode(string.Format("<a href=\"{0}\">{1}</a>", theurl, thetext));
		//                    Getlawtext(l, lov,
		//                        (Law thelaw, string s) => thelaw.AfterSecondLawText = s,
		//                        (Law thelaw, DateTime d) => thelaw.AfterSecondLawDate = d, 4, true);
		//                }
		//            }
		//            db.SubmitChanges();
		//        }

		//        if (!onlygetlawname)
		//        {
		//            string retsinfourl = "";
		//            var link = lawDoc.DocumentNode.SelectSingleNode("//html/body/p/font/a");
		//            if (link != null)
		//                retsinfourl = link.Attributes["href"].Value;
		//        }
		//        // todo, do afstemning

		//        // todo the below is non-functional
		//        //string resume = "";
		//        //var ps = lawDoc.DocumentNode.SelectNodes("/html/body/p/font").OfType<HtmlNode>();
		//        //// /html/body/p/font
		//        //foreach (var p in ps)
		//        //{
		//        //    if (p.PreviousSibling != null
		//        //        && p.PreviousSibling.FirstChild != null
		//        //        && p.PreviousSibling.FirstChild.InnerText != null
		//        //        && p.PreviousSibling.FirstChild.InnerText.StartsWith("Resume:"))
		//        //    {
		//        //        resume = p.InnerText;
		//        //    }
		//        //}
		//    }
		//}

		//private static string ExtractLinkFromJS(HtmlDocument lawDoc, int linenum)
		//{
		//    string cut = string.Format("LinieRef [{0}] = \"/", linenum);
		//    if(lawDoc.DocumentNode.OuterHtml.Contains(cut))
		//    {
		//    int begin = lawDoc.DocumentNode.OuterHtml.IndexOf(cut);
		//    int end = lawDoc.DocumentNode.OuterHtml.IndexOf('"', begin + cut.Length + 2);
		//    string l =
		//        lawDoc.DocumentNode.OuterHtml.
		//            Substring(begin + cut.Length, end - begin - cut.Length).
		//            Trim().TrimEnd(new char[] { '"' });
		//    return l;
		//    }
		//    else return null;
		//}

		//private static string ExtractLinkTextFromJS(HtmlDocument lawDoc, int linenum)
		//{
		//    string cut = string.Format("LinieText [{0}] =\"", linenum);
		//    int begin = lawDoc.DocumentNode.OuterHtml.IndexOf(cut);
		//    int end = lawDoc.DocumentNode.OuterHtml.IndexOf('"', begin + cut.Length + 2);
		//    string l =
		//        lawDoc.DocumentNode.OuterHtml.
		//            Substring(begin + cut.Length, end - begin - cut.Length).
		//            Trim().TrimEnd(new char[] { '"' });
		//    return l;
		//}

		//private static void Getlawtext(HtmlNode link, Law law,
		//    Action<Law, string> stringsetter, Action<Law, DateTime> datesetter, int dateoffset, bool isfucked)
		//{
		//    string linktxt = link.InnerText.Trim();
		//    DateTime date = GetDateFromLinkString(linktxt, dateoffset);

		//    var finaldoc = Scraper.GetDoc("http://ft.dk" + link.Attributes["href"].Value.Trim());
		//    var finalhtml = finaldoc.DocumentNode.SelectSingleNode("//body/font").InnerHtml.Trim();

		//    stringsetter(law, finalhtml);
		//    datesetter(law, date);
		//    //db.SubmitChanges();
		//}

		//private static void GetBehandling(int nr, HtmlNode link, Law lov)
		//{
		//    string urlend = link.Attributes["href"].Value;
		//    string linktitle = link.InnerText.Trim();
		//    var behDoc = GetDoc("http://ft.dk" + urlend);
		//    var links = behDoc.DocumentNode.SelectNodes("//table/tr/td/a").OfType<HtmlNode>();

		//    int year, month, date;
		//    date = int.Parse(linktitle.Split(new char[] { ' ' })[1].Split(new char[] { '/' })[0]);
		//    month = int.Parse(linktitle.Split(new char[] { ' ' })[1].Split(new char[] { '/' })[1]);
		//    year = int.Parse(linktitle.Split(new char[] { ' ' })[2]);

		//    // warning terrible bug if we go before 2000
		//    DateTime behandlingdate = new DateTime(2000 + year, month, date);

		//    ScrapeDBDataContext db = new ScrapeDBDataContext();
		//    // check to see wether we already have this behandling
		//    Deliberation thisBehandling =
		//        db.Deliberations.SingleOrDefault(
		//            b => b.LawId == lov.LawId && b.Number == nr);

		//    if (thisBehandling == null)
		//    {
		//        thisBehandling =
		//            new Deliberation
		//            {
		//                DeliberationText = behDoc.DocumentNode.OuterHtml,
		//                LawId = lov.LawId,
		//                Number = nr,
		//                Date = behandlingdate
		//            };
		//        db.Deliberations.InsertOnSubmit(thisBehandling);
		//        Console.WriteLine("    Created behandling: {0}", nr);
		//    }
		//    else
		//    {
		//        thisBehandling.DeliberationText = behDoc.DocumentNode.OuterHtml;
		//        thisBehandling.Date = behandlingdate;
		//        Console.WriteLine("    Updated behandling: {0}", nr);
		//    }
		//    db.SubmitChanges();

		//    // expand here
		//    if ((!thisBehandling.Speeches.Any() || thisBehandling.Speeches.Any(s => s.IsTemp == true) && gettaler))
		//    {
		//        foreach (var l in links)
		//        {
		//            string numberstring = l.InnerText.Trim().Split(new char[] { ' ' })[0];
		//            int i;
		//            bool s = int.TryParse(numberstring, out i);
		//            if (s && i > 0)
		//            {
		//                GetTale(l, i, thisBehandling);
		//            }
		//        }
		//    }
		//}

		//private static void GetTale(HtmlNode link, int i, Deliberation beh)
		//{
		//    string urlend = link.Attributes["href"].Value;
		//    var taleDoc = GetDoc("http://ft.dk" + urlend);

		//    bool istemp = false;
		//    bool wasupgraded = false;

		//    if (taleDoc.DocumentNode.InnerText.Contains("1. udgave (med forbehold for fejl og udeladelser)")
		//        ||
		//        taleDoc.DocumentNode.InnerText.Contains("(Talen er under udarbejdelse)"))
		//    {
		//        // keep note
		//        istemp = true;
		//        Console.WriteLine("        marked as temporary");
		//        // abort and wait for full version
		//        //return;
		//    }

		//    string name;
		//    string polLink;
		//    Util.GetPolleFromTale(taleDoc, out name, out polLink);

		//    string urlstring = link.Attributes["href"].Value;
		//    int last_slash = urlstring.LastIndexOf('/');

		//    string urlid = urlstring.Substring(last_slash + 1, urlstring.Length - last_slash - 4);

		//    ScrapeDBDataContext db = new ScrapeDBDataContext();
		//    // check to see whether we already have this one
		//    Speech t = db.Speeches.SingleOrDefault(
		//        _ => _.SpeechNr == i && _.DeliberationId == beh.DeliberationId);

		//    if (t == null)
		//    {
		//        // create new one
		//        t = new Speech
		//        {
		//            DeliberationId = beh.DeliberationId,
		//            SpeechText = taleDoc.DocumentNode.OuterHtml,//sb.ToString(),
		//            SpeechNr = i,
		//            UrlString = urlid,
		//            IsTemp = istemp,
		//        };

		//        Politician pol = CheckForPolitian(name, polLink, db);
		//        t.PoliticianId = pol.PoliticianId;
		//        db.Speeches.InsertOnSubmit(t);


		//        Console.WriteLine("        Created tale");
		//    }
		//    else
		//    {
		//        if (t.IsTemp == true && istemp == false)
		//            wasupgraded = true;

		//        t.SpeechText = taleDoc.DocumentNode.OuterHtml;
		//        t.UrlString = urlid;
		//        t.IsTemp = istemp;
		//        Console.WriteLine("        Updated tale");
		//    }

		//    db.SubmitChanges();

		//    if (wasupgraded)
		//    {
		//        // hose the old first-draft ones, shouldn't have comments
		//        db.SpeechParas.DeleteAllOnSubmit(t.SpeechParas);
		//        db.SubmitChanges();
		//    }

		//    // now chop it up
		//    // this is a legacy two-step process lifted from the scrubber
		//    var ps = taleDoc.DocumentNode.SelectNodes("//p");
		//    bool go = false;

		//    StringBuilder thetext = new StringBuilder();
		//    if (ps != null && ps.Count > 0)
		//    {
		//        foreach (var p in ps)
		//        {
		//            // paragraphs after the one with the speaker are the ones we want
		//            if (!go && p.Attributes["class"] != null &&
		//                (p.Attributes["class"].Value == "TalerTitel" ||
		//                p.Attributes["class"].Value == "TalerTitelMedTaleType"))
		//            {
		//                go = true;
		//                continue;
		//            }

		//            if (go && p.Attributes["class"] != null &&
		//                (p.Attributes["class"].Value != "Tid"))
		//            {
		//                thetext.Append("<p>" + p.InnerText + "</p>");
		//                if (p.NextSibling != null &&
		//                    p.NextSibling.NextSibling != null &&
		//                    p.NextSibling.NextSibling.Name == "hr")
		//                {
		//                    // we've reached the end
		//                    break;
		//                }
		//            }
		//        }

		//        // also get time
		//        var ptimes = taleDoc.DocumentNode.SelectNodes("//p[@class=\"Tid\"]");
		//        if (ptimes != null && ptimes.Count > 0)
		//        {
		//            var ptime = ptimes.OfType<HtmlNode>().First().
		//                InnerText.Trim().Split(new char[] { ' ' })[1].Trim();
		//            int hour = int.Parse(ptime.Split(new char[] { ':' })[0].Trim());
		//            int min = int.Parse(ptime.Split(new char[] { ':' })[1].Trim());

		//            DateTime bdate = t.Deliberation.Date.Value;
		//            DateTime tdate = new DateTime(bdate.Year, bdate.Month, bdate.Day, hour, min, 0);

		//            t.SpeechTime = tdate;
		//        }
		//        t.FormText = thetext.ToString();
		//    }

		//    if (!string.IsNullOrEmpty(t.FormText))
		//    {
		//        var doc = GetDocFromText(t.FormText);

		//        var pars = doc.DocumentNode.SelectNodes("//p");

		//        if (pars != null && ps.Count > 0)
		//        {
		//            for (int j = 0; j < pars.Count; j++)
		//            {
		//                var par = new SpeechPara()
		//                {
		//                    Speech = t,
		//                    ParText = pars[j].InnerText,
		//                    Number = j
		//                };

		//                db.SpeechParas.InsertOnSubmit(par);
		//            }

		//            // lifted from klatcher
		//            StringBuilder b = new StringBuilder();
		//            foreach (var p in t.SpeechParas.OrderBy(_ => _.Number).Select(_ => _.ParText))
		//            {
		//                b.Append(" ");
		//                b.Append(p);
		//            }
		//            t.SpeechTextFT = b.ToString();
		//        }
		//        else
		//        {
		//            Console.WriteLine("alarm");
		//        }
		//    }

		//    db.SubmitChanges();
		//}

		//private static Politician CheckForPolitian(string name, string polLink, ScrapeDBDataContext db)
		//{
		//    Politician pol;
		//    lock (pollock)
		//    {
		//        using (TransactionScope ts = new TransactionScope())
		//        {
		//            pol = db.Politicians.SingleOrDefault(p => p.Name == name);
		//            if (pol != null)
		//            {
		//            }
		//            else
		//            {
		//                pol = new Politician { Name = name, BioUrl = polLink };
		//                db.Politicians.InsertOnSubmit(pol);
		//                db.SubmitChanges();
		//                Console.WriteLine("           Created politker {0}", name);
		//            }
		//            ts.Complete();
		//        }
		//    }
		//    return pol;
		//}

		//private static void GetAfstemnings(string link, Law lov)
		//{
		//    ScrapeDBDataContext db = new ScrapeDBDataContext();
		//    var links = GetLinkNodes("http://ft.dk/" + link);
		//    foreach (var l in links)
		//    {
		//        var afsdoc = GetDoc("http://ft.dk/" + l.Attributes["href"].Value);
		//        var afsname = l.InnerText.Trim();

		//        LawVote a = db.LawVotes.SingleOrDefault(lw => lw.Name == afsname && lw.LawId == lov.LawId);
		//        if (a == null)
		//        {
		//            a = new LawVote
		//            {
		//                VoteText = afsdoc.DocumentNode.OuterHtml,
		//                LawId = lov.LawId,
		//                Name = l.InnerText.Trim()
		//            };
		//            db.LawVotes.InsertOnSubmit(a);
		//        }
		//        else
		//        {
		//            a.VoteText = afsdoc.DocumentNode.OuterHtml;
		//        }
		//    }
		//    db.SubmitChanges();
		//}

		//public static IEnumerable<HtmlNode> GetLinkNodes(string url)
		//{
		//    HtmlDocument doc = GetDoc(url);
		//    var links = doc.DocumentNode.SelectNodes("//table/tr/td/a").OfType<HtmlNode>();
		//    return links;
		//}
		//public static HtmlDocument GetDoc(string url)
		//{
		//    return GetDoc(url, null);
		//}

		public static HtmlDocument GetDoc(string url, Encoding enc)
		{
			WebClient wc = GetWC(enc);

			string s = Util.ExtractString(wc.DownloadData(url), enc);
			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(s);
			return doc;
		}

		public static WebClient GetWC()
		{
			return GetWC(null);
		}

		public static WebClient GetWC(Encoding enc)
		{
			WebClient wc = new CGWebClient();
			wc.Encoding = enc ?? e;//Encoding.UTF8;
			wc.Headers.Add("Accept-Encoding", "gzip");
			//wc.Headers.Add("Accept-Encoding", "deflate");
			return wc;
		}

		//private static DateTime GetDateFromLinkString(string linktxt, int offset)
		//{
		//    int day = int.Parse(linktxt.Split(new char[] { ' ' })[offset].Split(new char[] { '/' })[0]);
		//    int month = int.Parse(linktxt.Split(new char[] { ' ' })[offset].Split(new char[] { '/' })[1]);
		//    // TODO: warn bad year 2000 bug
		//    int year = 2000 + int.Parse(linktxt.Split(new char[] { ' ' })[offset + 1]);

		//    var date = new DateTime(year, month, day);
		//    return date;
		//}

		//private static HtmlDocument GetDocFromText(string html)
		//{
		//    HtmlDocument doc = new HtmlDocument();
		//    doc.LoadHtml(html);
		//    return doc;
		//}

	}

	public class CGWebClient : WebClient
	{
		private System.Net.CookieContainer cookieContainer;
		private string userAgent;
		private int timeout;

		public System.Net.CookieContainer CookieContainer
		{
			get { return cookieContainer; }
			set { cookieContainer = value; }
		}

		public string UserAgent
		{
			get { return userAgent; }
			set { userAgent = value; }
		}

		public int Timeout
		{
			get { return timeout; }
			set { timeout = value; }
		}

		public CGWebClient()
		{
			timeout = -1;
			cookieContainer = new CookieContainer();
		}

		protected override WebRequest GetWebRequest(Uri address)
		{
			WebRequest request = base.GetWebRequest(address);

			if (request.GetType() == typeof(HttpWebRequest))
			{
				((HttpWebRequest)request).CookieContainer = cookieContainer;
				((HttpWebRequest)request).UserAgent = userAgent;
				((HttpWebRequest)request).Timeout = timeout;
			}

			return request;
		}
	}  
}
