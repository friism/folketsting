using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using FT.DB;
using System.Text.RegularExpressions;
using System.Globalization;
using FileHelpers;
using Geo = EB.Crime.Geocoding;

namespace FT.Scraper
{
	public class TripScraper
	{
		private static object dblock = new object();

		public static void GeoCode()
		{
			var db = new DBDataContext();

			foreach (var trip in db.CommitteeTrips.Where(_ => !_.CommitteeTripDestinations.Any()))
			{
				if (!string.IsNullOrEmpty(trip.Place))
				{
					Console.WriteLine("geocoding " + trip.Place);
					string[] dests = { };
					if (trip.Place.Contains(" og "))
					{
						dests = trip.Place.Split(
							new string[] { ",", "og" }, StringSplitOptions.RemoveEmptyEntries)
							.Select(_ => _.Trim()).ToArray();
					}
					else
					{
						dests = new string[] { trip.Place };
					}

					var poss = dests.Select(_ => new { pos = Geo.Geocoder.GeoCode(_, false), place = _ });
					var destinations = poss.Where(_ => _.pos != null).Select(_ => new CommitteeTripDestination
					{
						CommitteeTripId = trip.CommitteeTripId,
						Lat = _.pos.Lat,
						Lng = _.pos.Lng,
						PlaceNameName = _.place.Trim()
					});
					db.CommitteeTripDestinations.InsertAllOnSubmit(destinations);
				}
			}
			db.SubmitChanges();

			Console.WriteLine("all done");
		}

		public static void GetTrips()
		{
			string url = "http://www.ft.dk/Aktuelt/Kalender/Rejser.aspx?from=01-01-2004&to=31-12-2020&committee=all&delegationOrCommission=all&member=all";
			HtmlDocument doc = Scrape2009.GetDoc(url);

			var rows =
				doc.SelectHtmlNodes("//div[@class='contentLine']/table[@class='calendarList']/tr").
				Skip(1);

			rows.AsParallel().WithDegreeOfParallelism(1).ForAll(_ => HandleRow(_));
		}

		private static void HandleRow(HtmlNode row)
		{
			var onclick = row.Attributes["onclick"].Value;
			var url = onclick.Split('\'')[1];
			string ftid = url.Split('{')[1].Replace("}", "");

			var db = new DBDataContext();
			var trip = db.CommitteeTrips.SingleOrDefault(_ => _.FTId == ftid);

			if (trip != null && trip.ActualExpenses.HasValue && trip.ActualExpenses != 0)
			{
				// this trip is prolly completely accounted for
				return;
			}

			var cells = row.SelectHtmlNodes("td");

			var startstring = cells.ElementAt(0).InnerText.Trim();
			var startdate = DateTime.ParseExact(startstring, "dd-mm-yyyy", null);

			var endstring = cells.ElementAt(1).InnerText.Trim();
			var enddate = DateTime.ParseExact(endstring, "dd-mm-yyyy", null);

			if (startdate.Year < 1900 || enddate.Year < 1900)
			{
				return;
			}

			var commname = cells.ElementAt(2).InnerText.Trim();
			
			var committee = Scrape2009.GetCommitteeId(commname, db);

			var purpose = cells.ElementAt(5).InnerText.Trim();
			var place = cells.ElementAt(3).InnerText.Trim();
			if (place.ToLower().Contains("aflyst") || purpose.ToLower().Contains("aflyst"))
			{
				// give up
				return;
			}

			var doc = Scrape2009.GetDoc("http://www.ft.dk" + url);

			var menudiv = doc.DocumentNode.SelectSingleNode("//div[@id='menuSkip']");
			if (menudiv.InnerText.ToLower().Contains("afbud"))
			{
				return;
			}

			var participantnode = menudiv.SelectHtmlNodes("p/h3").
				SingleOrDefault(_ => _.InnerText.Trim() == "Deltagere");

			if (participantnode == null)
			{
				// no politicians went, discard
				return;
			}

			var participants = participantnode.
				NextSibling.SelectHtmlNodes("li/a").
				Select(_ => _.Attributes["href"].Value);

			var polids = participants.Select(_ => Scrape2009.GetPoliticianByUrl(_, db));

			var otherparticipantnode = menudiv.SelectHtmlNodes("p/h3").
				SingleOrDefault(_ => _.InnerText.Trim() == "Øvrige deltagere");

			var othercount = 0;
			if (otherparticipantnode != null)
			{
				othercount = OtherMemberCount(otherparticipantnode);
			}

			var budgetstring = menudiv.SelectHtmlNodes("p/h3").
				Single(_ => _.InnerText.Trim() == "Budget").
				NextSibling.InnerText.Trim().Split(' ')[0]
				.Replace(".", "").Replace(",", "");

			var spendstring = menudiv.SelectHtmlNodes("p/h3").
				Single(_ => _.InnerText.Trim() == "Regnskab").
				NextSibling.InnerText.Trim().Split(' ')[0]
				.Replace(".", "").Replace(",", "");

			var provider = new CultureInfo("da-dk");
			//var provider = new CultureInfo("en-us");
			var budget = decimal.Parse(budgetstring, provider);
			var spend = decimal.Parse(spendstring, provider);

			lock (dblock)
			{
				if (trip == null)
				{
					trip = new CommitteeTrip();
					db.CommitteeTrips.InsertOnSubmit(trip);
				}
					
				trip.ActualExpenses = spend;
				trip.Budget = budget;
				trip.CommitteeId = committee;
				trip.EndDate = enddate;
				trip.Place = place;
				trip.Purpose = purpose;
				trip.StartDate = startdate;
				trip.NonPolParticipants = othercount;
				trip.FTId = ftid;
				trip.Uri = url;

				db.SubmitChanges();

				var newpols = polids.Where(_ => 
					_.HasValue && 
					!trip.CommitteeTripParticipants.Any(p => p.Politician.PoliticianId == _.Value)
					);

				var tripparticipants = newpols.Select(_ =>
					new CommitteeTripParticipant
					{
						CommitteeTripId = trip.CommitteeTripId,
						ParticipantId = _.Value
					});

				var partstodelete = trip.CommitteeTripParticipants.
					Where(_ => !polids.Contains(_.ParticipantId));
				db.CommitteeTripParticipants.DeleteAllOnSubmit(partstodelete);

				db.CommitteeTripParticipants.InsertAllOnSubmit(tripparticipants);
				db.SubmitChanges();
			}
		}

		private static int OtherMemberCount(HtmlNode node)
		{
			int res = 0;
			node = node.NextSibling;
			while (node != null && node.Name.ToLower() != "h3")
			{
				if (!string.IsNullOrEmpty(node.InnerText.Trim()))
				{
					res++;
				}

				node = node.NextSibling;
			}
			return res;
		}
	}
}
