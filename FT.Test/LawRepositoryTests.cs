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
	class LawRepositoryTests
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

		private void RunTest(Func<LawRepository, IEnumerable<object>> method)
		{
			var rep = new LawRepository();

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
		public void LawRepository_TopDebated()
		{
			Func<LawRepository, IEnumerable<object>> meth =
				r => r.TopDebated(DateTime.Now.AddDays(-7), 10).Select(lawselector);
			RunTest(meth);
		}

		[Test]
		public void LawRepository_TopDebated_NoSelect()
		{
			Func<LawRepository, IEnumerable<object>> meth =
				r => r.TopDebated(DateTime.Now.AddDays(-7), 10);
			RunTest(meth);
		}

		[Test]
		public void LawRepository_Popular()
		{
			Func<LawRepository, IEnumerable<object>> meth =
				r => r.Popular(DateTime.Now.AddDays(-7), 10).Select(lawselector);
			RunTest(meth);
		}

		[Test]
		public void LawRepository_Popular_NoSelect()
		{
			Func<LawRepository, IEnumerable<object>> meth =
				r => r.Popular(DateTime.Now.AddDays(-7), 10);
			RunTest(meth);
		}

		[Test]
		public void LawRepository_RecentInDeliberation()
		{
			Func<LawRepository, IEnumerable<object>> meth = 
				r => r.RecentInDeliberation(LawStage.First, 10).Select(lawselector);
			RunTest(meth);
		}

		[Test]
		public void LawRepository_RecentInDeliberation_NoSelect()
		{
			Func<LawRepository, IEnumerable<object>> meth =
				r => r.RecentInDeliberation(LawStage.First,10);
			RunTest(meth);
		}

		[Test]
		public void LawRepository_LawCommentCount()
		{
			var rep = new LawRepository();

			foreach (var item in Enumerable.Range(1, 10))
			{
				var bar = rep.CommentCount(654);
			}

			var watch = Stopwatch.StartNew();
			var comments = rep.CommentCount(654);
			watch.Stop();
			Trace.Write(string.Format("Elapsed {0} milliseconds", watch.ElapsedMilliseconds));

		}

	}
}
