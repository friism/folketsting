using System.Collections.Generic;

namespace FolketsTing.Controllers
{
	public class BaseViewModel
	{
		public string MetaDescription { get; set; }
		public List<Breadcrumb> Breadcrumb { get; set; }
		public string CountLink { get; set; }

		public BaseViewModel()
		{
		}
	}

	public class Breadcrumb
	{
		private static Breadcrumb _home = new Breadcrumb("Folkets Ting", "Home", "Index");
		public static Breadcrumb Home { get { return _home; } }

		private static Breadcrumb _p20index = new Breadcrumb("§20 spørgsmål", "P20Question", "Index");
		public static Breadcrumb P20Index { get { return _p20index; } }

		private static Breadcrumb _lawindex = new Breadcrumb("Love", "Law", "Index");
		public static Breadcrumb LawIndex { get { return _lawindex; } }

		private static Breadcrumb _search = new Breadcrumb("Søgning", "Search", "Search");
		public static Breadcrumb Search { get { return _search; } }

		private static Breadcrumb _tagindex = new Breadcrumb("Tags", "Tag", "Index");
		public static Breadcrumb TagIndex { get { return _tagindex; } }

		private static Breadcrumb _polindex = new Breadcrumb("Politikere", "Politician", "Index");
		public static Breadcrumb PolIndex { get { return _polindex; } }

		public string Text { get; set; }
		public string Controller { get; set; }
		public string Action { get; set; }
		public object RVals { get; set; }

		public Breadcrumb(string text, string controller, string action) : 
			this(text, controller, action, null)
		{
		}

		public Breadcrumb(string text, string controller, string action, object rvals)
		{
			Text = text;
			Controller = controller;
			Action = action;
			RVals = rvals;
		}
	}
}
