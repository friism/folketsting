using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FT.Model;
using FT.DB;
using FolketsTing.Controllers;
using System.Diagnostics;

namespace FT.Test
{
	[TestFixture]
	class PoliticianRepositoryTests
	{
		Func<Law, object> lawselector = l =>
				new
				{
					l.ShortName,
					l.LawId,
					l.Subtitle,
					CommentCount = l.CommentCount(),
					Views = l.Views()
				};
		// 378 is haarder, 229 is aaen
		private static int polid = 229;

		private void RunTest(Func<PoliticianRepository, IEnumerable<object>> method)
		{
			var rep = new PoliticianRepository();

			foreach (var item in Enumerable.Range(1, 10))
			{
				var bar = method(rep).ToList();
			}

			var laws = method(rep);
			var watch = Stopwatch.StartNew();
			var foo = laws.ToList();
			watch.Stop();
			Trace.Write(string.Format("Elapsed {0} milliseconds", watch.ElapsedMilliseconds));
		}

		[Test]
		public void PoliticianRepository_DebatedFeed()
		{
			Func<PoliticianRepository, IEnumerable<object>> meth =
				r => r.DebatedFeed(polid, 25);
			RunTest(meth);
		}

		[Test]
		public void PoliticianRepository_LatestFeed()
		{
			Func<PoliticianRepository, IEnumerable<object>> meth =
				r => r.LatestFeed(polid, 25);
			RunTest(meth);
		}

		[Test]
		public void PoliticianRepository_ActivityStats()
		{
			Func<PoliticianRepository, IEnumerable<object>> meth =
				r => r.ActivityStats(polid, 24);
			RunTest(meth);
		}

		
	}
}
