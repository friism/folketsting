using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FT.DB;

namespace FT.Scraper.Tests
{
	[TestClass]
	public class P20ScrapeTest
	{
		[TestMethod]
		public void Test2338()
		{
			var db = new DBDataContext();
			var samling = db.Sessions.Single(_ => _.Number == 1 && _.Year == 2009);
			P20QuestionScraper.GetQChecked(2338, "", new List<string>() { "" },
				true, "/samling/20091/spoergsmaal/S2338/index.htm", samling, false);
		}

		[TestMethod]
		public void Test2643()
		{
			var db = new DBDataContext();
			var samling = db.Sessions.Single(_ => _.Number == 1 && _.Year == 2009);
			P20QuestionScraper.GetQChecked(2643, "", new List<string>() { "" },
				false, "/samling/20091/spoergsmaal/S2643/index.htm", samling, false);
		}

		[TestMethod]
		public void Test2695()
		{
			var db = new DBDataContext();
			var samling = db.Sessions.Single(_ => _.Number == 1 && _.Year == 2009);
			P20QuestionScraper.GetQChecked(2695,
				"Hvad er ministerens kommentarer til positionspapiret fra »92-gruppen« med titlen »Hvad der skal til for at nå FN's 2015-mål« jf. URU alm. del - bilag 246?",
				new List<string>() { "Udenrigsudvalget" },
				true, "/samling/20091/spoergsmaal/S2695/index.htm", samling, false);
		}

		[TestMethod]
		public void Test2566()
		{
			var db = new DBDataContext();
			var samling = db.Sessions.Single(_ => _.Number == 1 && _.Year == 2009);
			P20QuestionScraper.GetQChecked(2695,
				"Bekymrer det ikke ministeren, at UNI-C har monopol på udbud af it-styresystemet Easy-a til erhvervsskolerne, og at det dermed både kan sætte prisen selv og ikke udsættes for konkurrence til hele tiden at udvikle billigere og bedre systemer?",
				new List<string>() { "Uddannelsesudvalget" },
				true, "/samling/20091/spoergsmaal/S2566/index.htm", samling, false);
		}

		[TestMethod]
		public void Test22()
		{
			var db = new DBDataContext();
			var samling = db.Sessions.Single(_ => _.Number == 1 && _.Year == 2010);
			P20QuestionScraper.GetQChecked(22,
				"Er det udtryk for uenighed i Venstre, når den tidligere undervisningsminister i januar 2010 afviste offentliggørelse bl.a. med afsæt i forskningsresultater samt negative erfaringer fra det engelske skolesystem?",
				new List<string>() { "Uddannelsesudvalget" },
				true, "/samling/20101/spoergsmaal/S22/index.htm", samling, false);
		}
	}
}
