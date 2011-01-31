using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Linq;
using System.ServiceModel.Web;
using System.Web;
using FT.Data;
using DataServicesJSONP;
using System.ServiceModel;
using System.Text;
using FT.DB;
using System.Data.Linq;

namespace FT.ODataApi
{
	[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
	[JSONPSupportBehavior]
	public class Service : DataService<FolketsTingEntities>
	{
		private Dictionary<string, object> APIKeys
		{
			get
			{
				var keys = HttpContext.Current.Cache["ApiKeys"] as Dictionary<string, object>;
				if (keys == null)
					keys = PopulateApiKeys();
				return keys;
			}
		}

		private Dictionary<string, object> PopulateApiKeys()
		{
			var keys = new Dictionary<string, object>();

			foreach (var k in (new DBDataContext()).ApiUsers.Select(_ => _.ApiKey))
			{
				keys.Add(k, null);
			}
			HttpContext.Current.Cache.Insert("ApiKeys", keys);
			return keys;
		}

		public static void InitializeService(DataServiceConfiguration config)
		{
			config.SetEntitySetAccessRule("Category", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("Committee", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("CommitteeTrip", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("CommitteeTripDestination", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("CommitteeTripParticipant", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("Deliberation", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("Document", EntitySetRights.AllRead);
			//config.SetEntitySetAccessRule("Image", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("ItemCategory", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("ItemCommittee", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("Law", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("LawChange", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("LawVote", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("Ministry", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("P20Question", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("Paragraph", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("Party", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("Politician", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("PoliticianLawVote", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("ProposedLaw", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("Section", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("Session", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("Speaker", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("Speech", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("SpeechPara", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("SubChange", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("Tag", EntitySetRights.AllRead);

			config.SetEntitySetPageSize("*", 100);
			config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
			config.UseVerboseErrors = true;
		}

		protected override void OnStartProcessingRequest(ProcessRequestArgs args)
		{
			base.OnStartProcessingRequest(args);

			//Cache for 10 minutes based on querystring
			HttpContext context = HttpContext.Current;
			HttpCachePolicy c = HttpContext.Current.Response.Cache;
			c.SetCacheability(HttpCacheability.ServerAndPrivate);
			c.SetExpires(HttpContext.Current.Timestamp.AddSeconds(600));
			c.VaryByHeaders["Accept"] = true;
			c.VaryByHeaders["Accept-Charset"] = true;
			c.VaryByHeaders["Accept-Encoding"] = true;
			c.VaryByParams["*"] = true;

			if (HttpContext.Current.Request.Url.Segments.Last().Replace("/", "") != "$metadata")
			{
				var apikey = HttpContext.Current.Request["apikey"];
				if (string.IsNullOrEmpty(apikey))
				{
					throw new DataServiceException("ApiKey required");
				}
				if (!APIKeys.ContainsKey(apikey))
				{
					// may be a new user, reload the keys
					PopulateApiKeys();
					if (!APIKeys.ContainsKey(apikey))
					{
						throw new DataServiceException("ApiKey required");
					}
				}
			}
		}
	}
}
