using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileHelpers;
using FT.DB;

namespace FT.Scraper
{
	class TripExportFoo
	{
		private static Func<CommitteeTrip, decimal> getammount = t => t.ActualExpenses.Value == 0 ? t.Budget.Value : t.ActualExpenses.Value;
		//private static Func<CommitteeTrip, bool> tripfilter = c => c.StartDate.Year > 2006 && c.StartDate.Year < 2010;
		private static Func<CommitteeTrip, bool> tripfilter = c => true;

		public static void Export()
		{
			var db = new DBDataContext();
			IEnumerable<TripExport> trips = db.CommitteeTrips.Where(t => tripfilter(t)).ToList().Select(_ =>
				new TripExport
				{
					Budget = _.Budget.Value,
					Expense = _.ActualExpenses.Value,
					OtherParticpantCount = _.NonPolParticipants.Value,
					ParticipantNames =
						_.CommitteeTripParticipants.ToList().
						Select(p => p.Politician.Firstname + " " + p.Politician.Lastname).
						ToList().
						Aggregate((a, b) => a + ", " + b).Escape(),
					Place = _.Place.Escape(),
					PolParticipantCount = _.CommitteeTripParticipants.Count,
					Purpose = _.Purpose.Escape(),
					TotalParticipantCount = _.CommitteeTripParticipants.Count + _.NonPolParticipants.Value,
					url = string.Format(
					"http://www.ft.dk/Folketinget/udvalg_delegationer_kommissioner/Udvalg/Det_energipolitiske_udvalg/Rejser/Vis.aspx?itemid={0}",
					"{" + _.FTId + "}"),
					Excess = (_.Budget.Value != 0 && _.ActualExpenses.Value != 0) ?
						((_.ActualExpenses.Value - _.Budget.Value) / _.Budget.Value) * 100 : 0,
				});

			var eng = new FileHelperEngine<TripExport>();
			eng.WriteFile("trips.csv", trips.OrderByDescending(_ => _.Expense));

			//ExportPols(getammount, db, tripPolAmmounts);
		}

		//public static void ExportCommittees()
		//{
		//    var db = new DBDataContext();
		//    var coms = from ct in db.CommitteeTrips
		//               join c in db.Committees on ct.CommitteeId equals c.CommitteeId
		//               group c by c.Name into g
		//               select new { Name = g.Key, Ammount = g.Sum(_ =>  };

		//    var eng = new FileHelperEngine<TripExport>();
		//    eng.WriteFile("trips.csv", trips.OrderByDescending(_ => _.Expense));

		//    //ExportPols(getammount, db, tripPolAmmounts);
		//}

		public static void ExportPols()
		{
			var db = new DBDataContext();
			var tripPolAmmounts = new Dictionary<int, decimal>();
			foreach (var t in db.CommitteeTrips.ToList())
			{
				tripPolAmmounts.Add(
							   t.CommitteeTripId,
							   (getammount(t))
							   / ((decimal)(t.CommitteeTripParticipants.Count
									+ t.NonPolParticipants))
						   );
			}

			var trippols = db.Politicians.Select(_ =>
					new { _.Firstname, _.Lastname, _.CommitteeTripParticipants }).ToList().
					Where(p => p.CommitteeTripParticipants.
						Where(_ => tripfilter(_.CommitteeTrip)).Sum(
						ctp => getammount(ctp.CommitteeTrip)) > 0);

			IEnumerable<PolExport> pols = trippols.Select(tp =>
				new PolExport
				{
					Name = tp.Firstname + " " + tp.Lastname,
					NumTrips = tp.CommitteeTripParticipants.
						Where(_ => tripfilter(_.CommitteeTrip)).Count(),
					TripPlaces = tp.CommitteeTripParticipants.
						Where(_ => tripfilter(_.CommitteeTrip)).
						Select(_ => _.CommitteeTrip.Place).ToList().Aggregate((a, b) => a + ", " + b),
					Expenses = tp.CommitteeTripParticipants.
						Where(_ => tripfilter(_.CommitteeTrip)).
						Sum(_ => tripPolAmmounts[_.CommitteeTripId])
				}
			);

			var eng = new FileHelperEngine<PolExport>();
			eng.WriteFile("polsbyammount.csv", pols.OrderByDescending(_ => _.Expenses));
		}

		public static void ExportCommittees()
		{
			var db = new DBDataContext();

			var coms = db.Committees.ToList().Select(c =>
				new CommitteeExport
				{
					TotalSpent = c.CommitteeTrips.Sum(ct => getammount(ct)),
					CommitteeName = c.Name,
					TripCount = c.CommitteeTrips.Count()
					//Politicians = c.CommitteeTrips.SelectMany(ct =>
					//    ct.CommitteeTripParticipants.Select(ctp => 
					//        ctp.Politician.Firstname + " " + ctp.Politician.Lastname)).
					//        Aggregate((a,b) => a + ", " + b)
				}
				);

			var eng = new FileHelperEngine<CommitteeExport>();
			eng.WriteFile("allcoms.csv", coms.OrderByDescending(_ => _.TotalSpent));
		}

		public static void Compute()
		{
			var db = new DBDataContext();

			Func<CommitteeTrip, decimal> getammount = t => t.ActualExpenses.Value == 0 ? t.Budget.Value : t.ActualExpenses.Value;

			var tripPolAmmounts = new Dictionary<int, decimal>();

			foreach (var t in db.CommitteeTrips)
			{
				tripPolAmmounts.Add(
							   t.CommitteeTripId,
							   (getammount(t))
							   / ((decimal)(t.CommitteeTripParticipants.Count + t.NonPolParticipants))
						   );
			}

			var pols = db.Politicians.ToList().Select(p => new
			{
				Name = p.Firstname + " " + p.Lastname,
				Ammount = p.CommitteeTripParticipants.
					Sum(_ => tripPolAmmounts[_.CommitteeTripId]),
				Pol = p,
			});

			Console.WriteLine("Starting...");
			foreach (var p in pols.Where(_ => _.Ammount > 0).OrderByDescending(_ => _.Ammount))
			{
				Console.WriteLine(p.Name + " " + p.Ammount);
				foreach (var part in p.Pol.CommitteeTripParticipants)
				{
					Console.WriteLine("    " +
						part.CommitteeTrip.Place + " " +
						getammount(part.CommitteeTrip));
				}

			}
			Console.WriteLine("please press the any key");
			Console.ReadKey();
		}
	}

	public static class StringExtensions
	{
		public static string Escape(this string s)
		{
			return s.Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ').Replace(';', ' ');
		}
	}

	[DelimitedRecord(";")]
	public class PolExport
	{
		public string Name { get; set; }
		public int NumTrips { get; set; }
		public decimal Expenses { get; set; }
		public string TripPlaces { get; set; }
	}

	[DelimitedRecord(";")]
	public class TripExport
	{
		public string Place { get; set; }
		public string Purpose { get; set; }
		public decimal Budget { get; set; }
		public decimal Expense { get; set; }
		public int OtherParticpantCount { get; set; }
		public int PolParticipantCount { get; set; }
		public int TotalParticipantCount { get; set; }
		public string ParticipantNames { get; set; }
		public string url { get; set; }
		public decimal Excess { get; set; }
	}

	[DelimitedRecord(";")]
	public class WonkyTripExport
	{
		public string Place { get; set; }
		public string Purpose { get; set; }
		public decimal Budget { get; set; }
		public decimal Expenses { get; set; }
		public decimal Excess { get; set; }
		public string url { get; set; }
	}

	[DelimitedRecord(";")]
	public class CommitteeExport
	{
		public string CommitteeName { get; set; }
		public int TripCount { get; set; }

		public decimal TotalSpent { get; set; }
		public string Politicians { get; set; }
	}
}
