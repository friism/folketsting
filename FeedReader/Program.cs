using System;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using FT.DB;

namespace FeedReader
{
	class Program
	{
		static void Main(string[] args)
		{
			var reader = XmlReader.Create("http://blog.folketsting.dk/feed/");
			var feed = SyndicationFeed.Load<SyndicationFeed>(reader);

			var db = new DBDataContext();
			foreach (var item in feed.Items)
			{
				var post = db.BlogPosts.SingleOrDefault(_ => _.WordpressId == item.Id);
				if (post != null)
				{
					post.Title = item.Title.Text;
					post.Summary = item.Summary.Text;
					post.PermaLink = item.Links[0].Uri.AbsoluteUri;
					db.SubmitChanges();
				}
				else
				{
					post = new BlogPost()
					{
						WordpressId = item.Id,
						Title = item.Title.Text,
						Summary = item.Summary.Text,
						Date = item.PublishDate.DateTime,
						PermaLink = item.Links[0].Uri.AbsoluteUri,
					};
					db.BlogPosts.InsertOnSubmit(post);
				}
				db.SubmitChanges();
			}
			Console.WriteLine("press the any key ...");
			Console.ReadKey();
		}
	}
}
