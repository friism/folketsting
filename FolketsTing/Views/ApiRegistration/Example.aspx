<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<NewApiUserViewModel>" %>

<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<%@ Import Namespace="Microsoft.Web.Mvc" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Se eksempel på API brug
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<div class="span-24">
		<h1>
			Eksempel på API brug</h1>
	</div>
	<div class="span-24 intro">
		<script type="text/javascript" src="<%= ResolveUrl("~/Scripts/underscore-min.js") %>"></script>
		<script type="text/javascript" src="http://maps.google.com/maps/api/js?sensor=false"></script>
		<script type="text/x-handlebars-template" name="politician">
			<div id="pol_{{Id}}">
				<p>
					<a class="politicianlink" id="{{Id}}" href="#">
						<strong>{{Firstname}} {{Lastname}}</strong></a>
					: kr. {{TotalCost.toFixed(0)}}
				</p>
				<ul class="triplist" style="display:none">
				</ul>
			</div>
		</script>
		<script type="text/x-handlebars-template" name="triplistitem">
			<li>
				<a id="{{Id}}" href="#" class="triplink">{{Place}}</a>
			</li>
		</script>
		<script type="text/x-handlebars-template" name="infowindow">
			<p>
				
			<p>
		</script>
		<script type="text/javascript">
			$(document).ready(function () {
				var latlng = new google.maps.LatLng(-34.397, 150.644);
				var myOptions = {
					zoom: 8,
					center: latlng,
					mapTypeId: google.maps.MapTypeId.ROADMAP
				};

				var map = new google.maps.Map(document.getElementById("map"),
					myOptions);

				var copenhagenPos = new google.maps.LatLng(55.676294, 12.568116);

				_.templateSettings = {
					start: '{{',
					end: '}}',
					interpolate: /\{\{(.+?)\}\}/g
				};
				var template = _.template($('script[name=politician]').html());
				var triplistitemtemplate = _.template($('script[name=triplistitem]').html());
				var infowindowtemplate = _.template($('script[name=infowindow]').html());
				var polquery =
					'http://data.folketsting.dk/Service.svc/Politician/?$expand=CommitteeTripParticipant&$select=Firstname,Lastname,PoliticianId,CommitteeTripParticipant/CommitteeTripId&$format=json&$callback=render&callback=render&apikey=7mRWp6WmTzsbAkMwvHxy';
				var tripquery =
					'http://data.folketsting.dk/Service.svc/CommitteeTrip/?$expand=CommitteeTripParticipant,CommitteeTripDestination&$select=Purpose,ActualExpenses,Budget,NonPolParticipants,CommitteeTripId,Place,CommitteeTripParticipant/CommitteeTripParticipantId,CommitteeTripDestination/Lat,CommitteeTripDestination/Lng,CommitteeTripDestination/PlaceNameName&$format=json&$callback=callback1&callback=callback1&apikey=7mRWp6WmTzsbAkMwvHxy';

				var overlays = [];
				var triplines = {};

				var politicians, trips;

				$('a.triplink').live('click', function (ev) {
					ev.preventDefault();
					var tripid = $(this).attr('id');
					// hide the other ones
					_(overlays).each(function (l) { l.setMap(null); });
					triplines[tripid].line.setMap(map);
					var bounds = new google.maps.LatLngBounds();
					bounds.extend(copenhagenPos);
					_(triplines[tripid].markers).each(function (m) {
						m.setMap(map);
						bounds.extend(m.getPosition());
					});
					map.fitBounds(bounds);
				});

				function callback1(result) {
					// parse the trip result
					trips = _.reduce(result["d"], function (m, t) {
						var tripid = t.CommitteeTripId;

						var participantcount = t.NonPolParticipants + t.CommitteeTripParticipant.length;
						var ammount = (parseFloat(t.ActualExpenses) === 0) ?
							parseFloat(t.Budget) : parseFloat(t.ActualExpenses);
						var participantcost = ammount / participantcount;

						var waypoints = _(t.CommitteeTripDestination).map(function (d) {
							return {
								pos: new google.maps.LatLng(d.Lat, d.Lng),
								place: d.PlaceNameName
							};
						});

						m[tripid] = {
							Place: t.Place,
							ParticipantCount: participantcount,
							ParticipantCost: participantcost,
							Waypoints: waypoints,
							Id: t.CommitteeTripId,
							Purpose: t.Purpose
						};
						return m;
					}, {});
					$.ajax({
						dataType: "jsonp",
						url: polquery,
						jsonpCallback: "render",
						success: render
					});
				}

				function render(result) {
					// parse the poilitician result
					politicians =
						_(result["d"]).chain().map(function (p) {
							var poltrips = _.map(p.CommitteeTripParticipant, function (ctp) {
								return ctp["CommitteeTripId"];
							}, []);

							var totalcosts = _.reduce(poltrips, function (m, t) {
								return m + trips[t].ParticipantCost;
							}, 0);

							return {
								Firstname: p.Firstname,
								Lastname: p.Lastname,
								Trips: poltrips,
								TotalCost: totalcosts,
								Id: p.PoliticianId
							};
						}, [])
						.select(function (n) { return n.TotalCost > 0; })
						.sortBy(function (n) { return n.TotalCost; }).value();

					var fordisplay = politicians.splice(politicians.length - 10, 10);

					// set politicians to be object hash, more useful
					politicians = {};
					_(fordisplay).each(function (p) {
						politicians[p.Id] = p;
					});

					var html = _.reduce(fordisplay,
								function (memo, pol) {
									return memo + template(pol);
								},
								'');
					$('#main').html(html);

					$('.politicianlink').click(function (ev) {
						ev.preventDefault();
						var polid = $(this).attr('id');
						var politician = politicians[polid];

						// render html list to go under politician name
						var polhtml = _(politician.Trips).reduce(function (memo, trip) {
							return memo + triplistitemtemplate(trips[trip]);
						}, '');

						$('ul.triplist').hide('fast');
						$('#pol_' + polid + ' ul.triplist').html(polhtml).show('fast');

						_(overlays).each(function (o) {
							o.setMap(null);
						});
						overlays = [];
						triplines = {};

						var bounds = new google.maps.LatLngBounds();

						_(politician.Trips).each(function (tId) {
							var trip = trips[tId];
							var path = [];
							// start in cph
							path.push(copenhagenPos);
							var linemarkers = [];
							_(trip.Waypoints).each(function (w) {
								var marker = new google.maps.Marker(
									{ position: w.pos, map: map, title: w.place });
								overlays.push(marker);
								bounds.extend(marker.getPosition());
								path.push(marker.getPosition());
								linemarkers.push(marker);
							});
							// end in cph
							path.push(copenhagenPos);
							var line = new google.maps.Polyline({ map: map, path: path });
							triplines[tId] = { line: line, markers: linemarkers };
							overlays.push(line);
						});
						map.fitBounds(bounds);
					});
				};

				$.ajax({
					dataType: "jsonp",
					url: tripquery,
					jsonpCallback: "callback1",
					success: callback1
				});
			});
		</script>
		<p>
			<strong>Her kan du se et eksempel på af Folkets Ting's API. Det virker udelukkende ved
				Javascript kald til API'et. Fra API'et hentes information om politikere og rejser,
				og det kombineres til at vise de ti politikere med de største rejsebudgetter.</strong>
		</p>
	</div>
	<div class="span-24 topmargin container">
		<div id="main" class="">
		</div>
		<div id="map" class="" style="width: 800px; height: 600px;">
		</div>
	</div>
</asp:Content>
