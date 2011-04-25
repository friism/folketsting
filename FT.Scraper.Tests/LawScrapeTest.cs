using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FT.DB;

namespace FT.Scraper.Tests
{
	[TestClass]
	public class LawScrapeTest
	{
//        [TestMethod]
//        public void TestL127_2009()
//        {
//            var db = new DBDataContext();
//            var rows = Scrape2009.GetLawRows(2009, 1);
//            var samling = db.Sessions.Single(_ => _.Number == 1 && _.Year == 2009);
//            rows.AsParallel().WithDegreeOfParallelism(20).ForAll(
//                _ => Scrape2009.GetLaw(_, samling,
//                    "L 127", 2009, 1
//                    //null, null, null
//                    )
//);

//        }




		//        [TestMethod]
		//        public void TestL173_2009()
		//        {
		//            var db = new DBDataContext();
		//            var rows = Scrape2009.GetLawRows(2009, 1);
		//            var samling = db.Sessions.Single(_ => _.Number == 1 && _.Year == 2009);
		//            rows.AsParallel().WithDegreeOfParallelism(20).ForAll(
		//                _ => Scrape2009.GetLaw(_, samling,
		//                    "L 173", 2009, 1
		//                    //null, null, null
		//                    )
		//            );

		//        }

		[TestMethod]
		public void TestL196_2009()
		{
			var db = new DBDataContext();
			var samling = db.Sessions.Single(_ => _.Number == 1 && _.Year == 2009);
			Scrape2009.GetLaw("L 96", "Forslag til lov om ændring af lov om detailsalg fra butikker m.v. (Liberalisering af reglerne om åbningstid).",
				"Økonomi- og Erhvervsministeriet",
				"Erhvervsudvalget (ERU)", "Stadfæstet",
				"/samling/20091/lovforslag/L96/index.htm", samling, null, null, null, false);
		}

		[TestMethod]
		public void TestL12_2009()
		{
			var db = new DBDataContext();
			var samling = db.Sessions.Single(_ => _.Number == 1 && _.Year == 2009);
			Scrape2009.GetLaw("L 12", "Forslag til lov om ændring af lov om registrering af ledningsejere. (Udvidelse af dækningsområde m.v.).",
				"Økonomi- og Erhvervsministeriet",
				"Erhvervsudvalget (ERU)", "Stadfæstet",
				"/samling/20091/lovforslag/L12/index.htm", samling, null, null, null, false);
		}

		[TestMethod]
		public void TestL2_2009()
		{
			var db = new DBDataContext();
			var samling = db.Sessions.Single(_ => _.Number == 1 && _.Year == 2009);
			Scrape2009.GetLaw("L 2", "Forslag til lov om anlæg af Frederikssundmotorvejen mellem Motorring 4 og Frederikssund.",
				"Transportministeriet",
				"Trafikudvalget (TRU)", "Stadfæstet",
				"/samling/20091/lovforslag/L2/index.htm", samling, null, null, null, false);
		}

		[TestMethod]
		public void TestL39_2009()
		{
			var db = new DBDataContext();
			var samling = db.Sessions.Single(_ => _.Number == 1 && _.Year == 2009);
			Scrape2009.GetLaw("L 39",
				"Forslag til lov om ændring af lov om landbrugsejendomme. (Ophævelse af regler om husdyrhold og arealkrav, ophævelse af arealgrænse for erhvervelse og forpagtning, ændring af reglerne om bopælspligt og lempelse af reglerne om personers og selskabers erhvervelse).",
				"Ministeriet for Fødevarer, Landbrug og Fiskeri",
				"Udvalget for Fødevarer, Landbrug og Fiskeri (FLF)", "Stadfæstet",
				"http://www.ft.dk/samling/20091/lovforslag/L40/index.htm", samling, null, null, null, false);
		}

		[TestMethod]
		public void TestL24_2010()
		{
			var db = new DBDataContext();
			var samling = db.Sessions.Single(_ => _.Number == 1 && _.Year == 2010);
			Scrape2009.GetLaw("L 24",
				"Forslag til lov om ophævelse af lov om bonus til elever i ungdomsuddannelse med lønnet praktik. (Afskaffelse af bonus til elever i ungdomsuddannelse med lønnet praktik).",
				"Undervisningsministeriet",
				"Uddannelsesudvalget (UDU)", "Fremsat",
				"/samling/20101/lovforslag/L24/index.htm", samling, null, null, null, false);
		}

		[TestMethod]
		public void TestL29_2010()
		{
			var db = new DBDataContext();
			var samling = db.Sessions.Single(_ => _.Number == 1 && _.Year == 2010);
			Scrape2009.GetLaw("L 29",
				"Forslag til lov om beskikkede bygningssagkyndige m.v.",
				"Økonomi- og Erhvervsministeriet",
				"Boligudvalget (BOU)", "1. beh./Henvist til udvalg",
				"/samling/20101/lovforslag/L29/index.htm", samling, null, null, null, false);
		}

		[TestMethod]
		public void TestL32_2010()
		{
			var db = new DBDataContext();
			var samling = db.Sessions.Single(_ => _.Number == 1 && _.Year == 2010);
			Scrape2009.GetLaw("L 32",
				"Forslag til lov om ophævelse af lov om udbetaling af ydelser til militære invalider og deres efterladte i de sonderjyske landsdele samt om ændring af ligningsloven og lov om aktiv socialpolitik. (Konsekvensændringer af ligningsloven og lov om aktiv socialpolitik ved ophævelse af lov om udbetaling af ydelser til militære invalider og deres efterladte i de sonderjyske landsdele).",
				"Beskæftigelsesministeriet",
				"Arbejdsmarkedsudvalget (AMU)", "3. behandlet, vedtaget",
				"/samling/20101/lovforslag/L32/index.htm", samling, null, null, null, true);
		}

		[TestMethod]
		public void TestL3_2010()
		{
			var db = new DBDataContext();
			var samling = db.Sessions.Single(_ => _.Number == 1 && _.Year == 2010);
			Scrape2009.GetLaw("L 3",
				"Forslag til lov om ændring af straffeloven og retsplejeloven. (Skærpede straffe for hæleri og etablering af tæt samarbejde om udveksling af oplysninger mellem kriminalforsorgen, de sociale myndigheder og politiet i forbindelse med dømtes løsladelse (KSP-samarbejdet)).",
				"Justitsministeriet",
				"Retsudvalget (REU)", "Stadfæstet",
				"/samling/20101/lovforslag/L3/index.htm#dok", samling, null, null, null, false);
		}

		[TestMethod]
		public void TestL11_2010()
		{
			var db = new DBDataContext();
			var samling = db.Sessions.Single(_ => _.Number == 1 && _.Year == 2010);
			Scrape2009.GetLaw("L 11",
					"Forslag til lov om ændring af lov om miljøbeskyttelse. (Gennemførelse af EU-regler om fremme af renere og mere energieffektive køretøjer til vejtransport m.v.).",
					"Miljøministeriet",
					"Miljø- og Planlægningsudvalget (MPU)", "Stadfæstet",
					"/samling/20101/lovforslag/L11/index.htm#dok", samling, null, null, null, false);
		}
	}
}
