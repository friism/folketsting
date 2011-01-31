using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FT.DB;
using FT.Model;

namespace FolketsTing.Controllers
{
	public class TagController : Controller
	{
		private readonly ITagRepository _tagRep;

		public TagController()
		{
			_tagRep = new TagRepository();
		}

		[OutputCache(Duration = 60, VaryByParam = "*")]
		public ActionResult Find(string q, int limit)
		{
			var tags = _tagRep.Find(q, limit);
			return Content(string.Join("\n", tags.ToArray()));
		}

		[OutputCache(Duration = 60, VaryByParam = "*")]
		public ActionResult Index()
		{
			TagIndexViewModel vm = new TagIndexViewModel
			{
				MetaDescription = "Tagget indhold på Folkets Ting",
				Breadcrumb = new List<Breadcrumb>() { 
					Breadcrumb.Home,
					Breadcrumb.TagIndex
				},
				TotalTags = _tagRep.TotalTags(),
				TagCloudData = _tagRep.TagCloudData()

			};
			return View("Index", vm);
		}

		[OutputCache(Duration = 60, VaryByParam = "*")]
		public ActionResult Details(string tag)
		{
			TagViewModel vm = new TagViewModel() {
				MetaDescription = string.Format("Indhold tagget '{0}' på Folkets Ting", tag),
				Breadcrumb = new List<Breadcrumb>() { 
					Breadcrumb.Home,
					Breadcrumb.TagIndex,
					new Breadcrumb(tag, "Tag", "Details", new { @tag = tag }),
				},
				TagName = tag 
			};

			return View("Details", vm);
		}

		[AcceptVerbs(HttpVerbs.Post)]
		[Authorize]
		public ActionResult AddTags(string name, int id, ContentType type, string tags)
		{
			// nuke existing tags for this user
			_tagRep.DeleteTagsByUser(id, type, this.User().UserId);

			// this seems too easy
			var thetags = tags.Split(new char[] { ' ' }).Select(
				_ => _.Trim().ToLower()).Except(new string[] { " ", "" }).Distinct();
			_tagRep.InsertTags(thetags, this.User().UserId, id, type);

			switch (type)
			{
				case ContentType.Law:
					{
						return RedirectToAction("Details", "Law", new
						{
							lawname = name,
							lawid = id
						});
					}
				case ContentType.P20Question:
					{
						return RedirectToAction("Details", "P20Question", new
						{
							questiontext = name,
							qid = id
						});
					}
				default: throw new ArgumentException(string.Format("Unknown contentType: {0}", type));
			}
		}
	}

	public class TagViewModel : BaseViewModel
	{
		public string TagName { get; set; }
		public IDictionary<string, string> Results { get; set; }
	}

	public class TagIndexViewModel : BaseViewModel
	{
		public Dictionary<string, int> TagCloudData { get; set; }
		public int TotalTags { get; set; }
	}

	public class TagControlViewModel
	{
		public TagControlViewModel(ITagRepository _tagRep, int elementid, ContentType type)
		{
			CommonSiteTags = _tagRep.TopSiteShuffled();
			Tags = _tagRep.CountedTags(elementid, type);
			CommonTags = _tagRep.TopShuffled(elementid, type);
			Type = type;
			ElementId = elementid;
		}

		public Dictionary<string, int> Tags { get; set; }
		public IEnumerable<string> UserTags { get; set; }
		public IEnumerable<string> CommonTags { get; set; }
		public IEnumerable<string> CommonSiteTags { get; set; }

		public string ElementName { get; set; }
		public int ElementId { get; set; }
		public ContentType Type { get; set; }

	}
}
