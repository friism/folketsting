using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using FT.DB;

namespace FolketsTing.Controllers
{
	public class ApiRegistrationController : Controller
	{
		private int apiKeyLength = 20;

		[OutputCache(Duration = 60, VaryByParam = "*", VaryByCustom = "userName")]
		[AcceptVerbs(HttpVerbs.Get)]
		public ActionResult New()
		{
			var vm = new NewApiUserViewModel
			{
				Breadcrumb = new List<Breadcrumb>
				{
					Breadcrumb.Home,
					new Breadcrumb("Nøgle", "ApiRegistration", "New")
				},
				MetaDescription = "Lav API-nøgle til Folkets Ting API",
				ApiUser = new ApiUser(),
			};
			return View("New", vm);
		}

		[OutputCache(Duration = 60, VaryByParam = "*", VaryByCustom = "userName")]
		[AcceptVerbs(HttpVerbs.Get)]
		public ActionResult Example()
		{
			var vm = new NewApiUserViewModel
			{
				Breadcrumb = new List<Breadcrumb>
				{
					Breadcrumb.Home,
					new Breadcrumb("Nøgle", "ApiRegistration", "New"),
					new Breadcrumb("Eksempel", "ApiRegistration", "Example")
				},
				MetaDescription = "Se eksempel på API brug",
				ApiUser = new ApiUser(),
			};
			return View("Example", vm);
		}

		[CaptchaValidator]
		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult New(
				[Bind(Prefix = "ApiUser", Include = "EmailAddress,IntendedUse")] 
					ApiUser user,
				bool captchaValid
			)
		{
			if (!captchaValid)
			{
				ViewData.ModelState.AddModelError("captcha", "CAPTCHA forkert");
			}

			if (string.IsNullOrEmpty(user.EmailAddress))
			{
				ViewData.ModelState.AddModelError("ApiUser.EmailAddress", "Ingen email adresse angivet");
			}

			var db = new DBDataContext();
			if (db.ApiUsers.Any(_ => _.EmailAddress == user.EmailAddress))
			{
				ViewData.ModelState.AddModelError("ApiUser.EmailAddress", "Email adresse findes allerede, skriv til friism@gmail.com hvis du glemt nøgle");
			}

			if (!ModelState.IsValid)
			{
				// try again
				return View("New", new NewApiUserViewModel
					{
						Breadcrumb = new List<Breadcrumb>
						{
							Breadcrumb.Home,
						},
						MetaDescription = "Lav API-nøgle til Folkets Ting API",
						ApiUser = user,
					});
			}
			else
			{
				// we have a live one
				string apikey = GetKey(apiKeyLength);
				// make sure the apikey is distinct
				
				while(db.ApiUsers.Any(_ => _.ApiKey == apikey))
				{
					apikey = GetKey(apiKeyLength);
				}

				user.ApiKey = apikey;
				user.CreatedDate = DateTime.Now;
				
				db.ApiUsers.InsertOnSubmit(user);
				db.SubmitChanges();

				return RedirectToAction("Created", "ApiRegistration", new { key = apikey });
			}
		}

		[AcceptVerbs(HttpVerbs.Get)]
		public ActionResult Created(string key)
		{
			var vm = new NewApiUserViewModel
			{
				Breadcrumb = new List<Breadcrumb>
				{
					Breadcrumb.Home,
				},
				MetaDescription = "API-nøgle klar",
				ApiUser = (new DBDataContext()).ApiUsers.Single(_ => _.ApiKey == key),
			};
			return View("Created", vm);
		}

		private static string GetKey(int length)
		{
			string allowedCharacters = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";
			StringBuilder key = new StringBuilder();
			Random rand = new Random();
			for (int i = 0; i < length; i++)
			{
				key.Append(allowedCharacters[rand.Next(0, (allowedCharacters.Length - 1))]);
			}
			return key.ToString();
		}
	}

	public class NewApiUserViewModel : BaseViewModel
	{
		public ApiUser ApiUser { get; set; }
	}
}
