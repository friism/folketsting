using System.Linq;
using FT.DB;

namespace FT.Model
{
	public class TripRepository : Repository
	{
		public CommitteeTrip GetTripById(int id)
		{
			return DB.CommitteeTrips.SingleOrDefault(x => x.CommitteeTripId == id);
		}

		public IQueryable<CommitteeTrip> GetTrips()
		{
			return DB.CommitteeTrips.Where(
				x => x.Place != null && x.Place != "" &&
					x.CommitteeTripDestinations.Any() &&
					x.CommitteeTripParticipants.Any());
		}
	}
}
