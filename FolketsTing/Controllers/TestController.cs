using System.Collections.Generic;
using System.Web.Mvc;

namespace FolketsTing.Controllers
{
	public class TestController : Controller
	{
		[AcceptVerbs(HttpVerbs.Get)]
		public ActionResult TravelTest()
		{
			var vm = new BaseViewModel
				{
					Breadcrumb = new List<Breadcrumb>
						{
							Breadcrumb.Home,
						},
					MetaDescription = "Testing travel display"
				};

			return View("TravelTest", vm);
		}
	}
}
