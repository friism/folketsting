using System;
using FT.DB;

namespace FolketsTing.Controllers
{
	public class CommentFeedObject
	{
		public string CommentBody;
		public string target_name;
		public string target_type;
		public string target_link;
		public string target_title;
		public string target_content;
		public Politician pol;
		public string pol_link;
		public User user;
		public string user_link;
	}

	public class FacebookFeedHandler
	{
		/// <summary>
		/// Possible values of TemplateId.
		/// </summary>
		public enum FeedTemplateIds : long
		{
			PublishPersonCommentStory = 120878090568,
			PublishLawCommentStory = 120876720568,
			PublishP20CommentStory = 146948820568
		}

		public class FeedResponse
		{
			public string method;
			public Content content;
		}

		public struct Content
		{
			public Feed feed;
		}

		public struct Feed
		{
			public string template_id;
			public TemplData template_data;
		}

		public struct TemplData
		{
			public string target_name;
			public string target_type;
			public string target_title;
			public string target_content;
			public string comment_body;
			public string site_link;
			public string debate_link;
			public Array images;
		}

		public struct FeedImage
		{
			public string src;
			public string href;
		}

		/// <summary>
		/// Builds and returns a FeedResponse object from the supped CommentFeedObject 
		/// while examining which template to use
		/// from the target_ids parameter.
		/// </summary>
		/// <param name="comment">CommentFeedObject to make feedresponse for.</param>
		/// <returns>A new FeedResponse object.</returns>
		public FeedResponse GetResponse(CommentFeedObject comment)
		{
			long template_id = (long)Enum.Parse(typeof(FeedTemplateIds),
				FeedTemplateIds.PublishLawCommentStory.ToString());
			switch (comment.target_type)
			{
				case "tale":
					template_id = (long)Enum.Parse(typeof(FeedTemplateIds),
						FeedTemplateIds.PublishPersonCommentStory.ToString());
					break;
				case "lov":
					template_id = (long)Enum.Parse(typeof(FeedTemplateIds),
						FeedTemplateIds.PublishLawCommentStory.ToString());
					break;
				case "§20 spørgsmål":
				case "§20 svar":
					template_id = (long)Enum.Parse(typeof(FeedTemplateIds),
						FeedTemplateIds.PublishP20CommentStory.ToString());
					break;
				default:
					{
						// TODO, do something intelligent
						return null;
					}
			}

			string image_link = "http://folketsting.dk/Graphics/ch.jpg";

			if (comment.pol != null)
			{
				if (comment.pol.Initials != null)
					image_link = "http://www.ft.dk/billeder/" + comment.pol.Initials + ".jpg";
				string name = comment.pol.FullName();
				comment.target_name = FacebookWrappedLink(comment.pol_link,
					(name.Substring(name.Length - 1)) == "s" ?
						comment.pol.FullName() + "'" : comment.pol.FullName() + "s");
			}
			else if (comment.user != null)
			{
				comment.target_name = FacebookWrappedLink(comment.user_link,
					comment.user.Username + "'s");
			}
			else
				comment.target_name = FacebookWrappedLink(comment.target_link,
					comment.target_name);

			return new FeedResponse
			{
				method = "feedStory",
				content = new Content
				{
					feed = new Feed
					{
						template_id = template_id.ToString(),
						template_data = new TemplData
						{
							comment_body = String.Format("\"{0}\"", comment.CommentBody),
							target_name = comment.target_name,
							target_type = comment.target_type,
							target_title = FacebookWrappedLink(comment.target_link,
								"\"" + comment.target_title + "\""),
							target_content = comment.target_content,
							debate_link = FacebookWrappedLink(comment.target_link,
								"Se hele debatten"),
							site_link = FacebookWrappedLink("", "Folkets Ting"),
							images = new FeedImage[]
							{
								new FeedImage
								{
									src = image_link,
									href = "http://www.folketsting.dk" + comment.target_link
								}
							},
						}
					}
				}
			};
		}

		private static string FacebookWrappedLink(string link_href, string link_text)
		{
			return String.Format("<a href='http://www.folketsting.dk{0}' target='_blank'>{1}</a>",
				link_href, link_text);
		}
	}
}
