using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;
using NUnit.Framework;
using FT.Model;
using FT.DB;
using FolketsTing.Controllers;
using System.Diagnostics;
using FolketsTing.Views;

namespace FT.Test
{
	[TestFixture]
	class RandomTests
	{
		[Test]
		public void LawRepository_TopDebated()
		{
			DBDataContext db = new DBDataContext();
			foreach (var lv in db.PoliticianLawVotes.Take(100))
			{
				//Trace.WriteLine(lv.Politician.Party.Initials);
			}

			foreach (var p in db.Parties)
			{
				//Trace.WriteLine(p.Initials);
			}

			var foo = from plw in db.PoliticianLawVotes
					  join p in db.Politicians on plw.PoliticianId equals p.PoliticianId
					  where plw.LawVoteId == 421 //&& plw.Vote == votetype
					  group plw by new { p.Party.Initials, plw.Vote } into ps
					  orderby ps.Key.Initials
					  select new PartyVote()
					  {
						  Party = ps.Key.Initials,
						  Vote = ps.Key.Vote.Value,
						  Count = ps.Count()
					  };

			foreach (var item in foo)
			{
				Trace.WriteLine(item.Party);
			}
		}

	}
}
