using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using FolketsTing.Views;
using FT.DB;
using FT.Model;

namespace FolketsTing.Controllers
{
	public class TripController : Controller
	{
		public ActionResult Index()
		{
			var trips = new TripRepository().GetTrips();
			var viewModel = new TripIndexViewModel
			{
				Breadcrumb = new List<Breadcrumb>
				{
					Breadcrumb.Home,
					Breadcrumb.TripIndex,
				},
				MostExpensive = trips.OrderByDescending(x => x.ActualExpenses).Take(10),
				Latest = trips.OrderByDescending(x => x.StartDate).Take(10),
				MostOverBudget = trips.Where(x => x.Budget != 0).OrderByDescending(x => x.ActualExpenses - x.Budget).Take(10),
			};
			return View(viewModel);
		}

		public ActionResult Details(int id)
		{
			var trip = new TripRepository().GetTripById(id);
			var titleAndDate = (trip.Place ?? "") + trip.StartDate.ToString(ViewConstants.DateFormat);

			var copenhagen = new
			{
				lat = 55.676294d,
				lng = 12.568116d,
				title = "København"
			};

			var destinations = (new[] { copenhagen })
				.Concat(trip.CommitteeTripDestinations.Select(x => new
					{
						lat = x.Lat.Value,
						lng = x.Lng.Value,
						title = x.PlaceNameName,
					}))
				.Concat(new[] { copenhagen });

			return View(new TripViewModel
			{
				MetaDescription = trip.Place,
				Trip = trip,
				Breadcrumb = new List<Breadcrumb>
					{
						Breadcrumb.Home,
						Breadcrumb.TripIndex,
						new Breadcrumb(titleAndDate, "Trip", "Details",
							new {
								title = titleAndDate,
								id = trip.CommitteeTripId,
							}),
					},
				DestinationJson = new JavaScriptSerializer().Serialize(destinations),
			});
		}
	}

	public class TripViewModel : BaseViewModel
	{
		public CommitteeTrip Trip { get; set; }
		public string DestinationJson { get; set; }
	}

	public class TripIndexViewModel : BaseViewModel
	{
		public IEnumerable<CommitteeTrip> MostExpensive { get; set; }
		public IEnumerable<CommitteeTrip> Latest { get; set; }
		public IEnumerable<CommitteeTrip> MostOverBudget { get; set; }
	}
}
