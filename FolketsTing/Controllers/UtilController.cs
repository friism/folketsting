using System.Web.Mvc;
using FT.Model;

namespace FolketsTing.Controllers
{
	public class UtilController : Controller
	{
		private readonly IUtilRepository _utilRep;

		public UtilController()
		{
			_utilRep = new UtilRepository();
		}

		[OutputCache(Duration = 600, VaryByParam = "*")]
		public ActionResult StopWords()
		{
			var words = _utilRep.GetDKStopWords();
			return this.Json(words, JsonRequestBehavior.AllowGet);
		}
	}
}
