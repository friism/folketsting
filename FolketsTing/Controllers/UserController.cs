using System.Linq;
using System.Web.Mvc;
using FT.DB;

namespace FolketsTing.Controllers
{
	public class UserController : Controller
	{
		public ActionResult Details(string uname)
		{
			var db = new DBDataContext();
			User u = db.Users.Where(_ => _.Username.ToLower() == uname.ToLower()).Single();

			return View("Details", new UserViewModel() { User = u});
		}
	}

	public class UserViewModel : BaseViewModel
	{
		public User User { get; set; }
	}

	public class UserBoxViewmodel
	{
		public User User { get; set; }
	}
}
