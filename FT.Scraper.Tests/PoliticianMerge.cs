using System;
using System.Collections.Generic;
using System.Linq;
using FT.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FT.Scraper.Tests
{
	[TestClass]
	public class PoliticianMerge
	{
		[TestMethod]
		public void Merge()
		{
			var db = new DBDataContext();
			var old = db.Politicians.Single(x => x.PoliticianId == 1312);
			var newPolitician = db.Politicians.Single(x => x.PoliticianId == 1359);

			AssignPolitician(old.AskeeP20Questions, newPolitician, (x, y) => x.Politician = y);
			AssignPolitician(old.AskerP20Questions, newPolitician, (x, y) => x.Politician = y);
			AssignPolitician(old.CommitteeTripParticipants, newPolitician, (x, y) => x.Politician = y);
			AssignPolitician(old.PoliticianLawVotes, newPolitician, (x, y) => x.Politician = y);
			AssignPolitician(old.ProposedLaws, newPolitician, (x, y) => x.Politician = y);
			AssignPolitician(old.Speakers, newPolitician, (x, y) => x.Politician = y);
			AssignPolitician(old.Speeches, newPolitician, (x, y) => x.Politician = y);

			db.SubmitChanges();
		}

		private void AssignPolitician<T>(IEnumerable<T> collection, Politician newPolitician,
			Action<T, Politician> setter) where T : class
		{
			foreach (var c in collection.ToList())
			{
				setter(c, newPolitician);
			}
		}
	}
}
