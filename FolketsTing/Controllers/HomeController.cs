using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FT.DB;
using FT.Model;

namespace FolketsTing.Controllers
{
	[HandleError]
	[OutputCache(Duration = 60, VaryByParam = "*", VaryByCustom="userName")]
	public class HomeController : Controller
	{
		private readonly IPoliticianRepository _polRep;
		private readonly ILawRepository _lawRep;

		public HomeController()
		{
			_polRep = new PoliticianRepository();
			_lawRep = new LawRepository();
		}

		public ActionResult Index()
		{
			var db = new DBDataContext();
			var laws = db.Laws.Where(l => l.SessionId == 2 || l.SessionId == 3 || l.SessionId == 1);
			var pols = db.Politicians.Where(p => p.FullName().ToLower() != "formanden");
			var lastweek = DateTime.Now.AddDays(-7);
			return View("Index", new FrontpageViewModel()
			{
				News = db.BlogPosts.OrderByDescending(bp => bp.Date).Take(5),
				MostDebated = _polRep.MostDebated(lastweek, 10),
				Proposed = _lawRep.RecentProposed(10),
				Breadcrumb = new List<Breadcrumb> { Breadcrumb.Home },
				MetaDescription = "Folkets Ting er demokrati som hjemmeside"
			});
		}

		public ActionResult Howto()
		{
			return View("Howto", null);
		}

		public ActionResult About()
		{
			return View();
		}

		public ActionResult Terms()
		{
			return View("Terms");
		}

		public ActionResult LiveDebate()
		{
			return View("LiveDebate", new FrontpageViewModel
				{
					MetaDescription = "Live-diskuter politikernes taler med dine venner på Facebook",
					Breadcrumb = new List<Breadcrumb> { 
						Breadcrumb.Home,  
						new Breadcrumb("Live", "Home", "LiveDebate"),
					},
				}
			);
		}

		public ActionResult Contact()
		{
			return View("Contact");
		}
	}

	public class FrontpageViewModel : BaseViewModel
	{
		public IEnumerable<BlogPost> News { get; set; }
		public IEnumerable<Law> Proposed { get; set; }
		public IEnumerable<Politician> MostDebated { get; set; }
	}
}
