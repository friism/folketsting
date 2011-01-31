using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using FT.DB;
using System.Data.Linq;

namespace FT.Model
{
    public interface IHitRepository
    {
		void SaveHit(int contentId, ContentType contentType, string IPAddress);
		int ViewCount(int elementid, ContentType type);
    }

	public class HitRepository : Repository, IHitRepository
    {
		public void SaveHit(int contentId, ContentType contentType, string IP)
		{
			var address = IPAddress.Parse(
						  IP).GetAddressBytes();

			DB.Hits.InsertOnSubmit(
				new Hit()
				{
					ContentId = contentId,
					ContentType = contentType,
					Date = DateTime.Now,
					IP = address
				});

			DB.SubmitChanges();
		}

		private static Func<DBDataContext, int, ContentType, int> viewcount =
			CompiledQuery.Compile((DBDataContext DB, int elementid, ContentType type) =>
				(
					from v in DB.Hits
						where v.ContentType == type && v.ContentId == elementid
						group v by new { v.IP, v.Date.Date, v.Date.Hour, v.Date.Minute } into v2
						select v2.Key).Count()
				);

		public int ViewCount(int elementid, ContentType type)
		{
			return viewcount(DB, elementid, type);
		}
    }
}
