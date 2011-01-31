using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FT.DB;

namespace FolketsTing.Controllers
{
	public static class ControllerExtensions
	{
		public static Dictionary<string, User> usercache = new Dictionary<string,User>();
		public static User User(this Controller con)
		{
			if (con.User.Identity.IsAuthenticated)
			{
				if (!usercache.ContainsKey(con.User.Identity.Name))
				{
					var db = new DBDataContext();
					User theu = db.Users.SingleOrDefault(u => u.Username == con.User.Identity.Name);
					if (theu != null)
						usercache[con.User.Identity.Name] = theu;
					else return null;
				}
				return usercache[con.User.Identity.Name];
			}
			else
			{
				return null;
			}
		}
	}
}
