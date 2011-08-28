<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<FolketsTing.Controllers.TripViewModel>" %>

<%@ Import Namespace="FolketsTing.Views" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	<%= Model.Trip.Place %>
	| Folkets Ting
</asp:Content>
<asp:Content ContentPlaceHolderID="HeadContent" runat="server">
	<script type="text/javascript" src="<%= ResolveUrl("~/Scripts/underscore-min.js") %>"></script>
	<script type="text/javascript" src="http://maps.google.com/maps/api/js?sensor=false"></script>
	<script type="text/javascript">
		$(document).ready(function () {
			var options = {
				mapTypeId: google.maps.MapTypeId.ROADMAP
			};
			var map = new google.maps.Map(document.getElementById("map"),
				options);

			var destinations = <%= Model.DestinationJson %>;
			var bounds = new google.maps.LatLngBounds();
			path = _(destinations).map(function (x) {
				var latLng = new google.maps.LatLng(x.lat, x.lng);
				bounds.extend(latLng);
				return latLng;
			});
			map.fitBounds(bounds);
			var line = new google.maps.Polyline({ map: map, path: path});
		});
	</script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<div class="span-24">
		<h1>
			<%= Model.Trip.Place %>, <%: Model.Trip.StartDate.ToString(ViewConstants.DateFormat) %> - <%: Model.Trip.EndDate.ToString(ViewConstants.DateFormat) %>
		</h1>
	</div>
	<div class="span-8">
		<div class="span-8">
			<h2>Formål</h2>
			<p><%: Model.Trip.Purpose %></p>
		</div>
		<div class="span-8">
			<p>
				<strong>Budget</strong>: <%: Model.Trip.Budget %> 
			</p>
			<p>
				<strong>Faktisk forbrug</strong>: <%: Model.Trip.ActualExpenses %>
			</p>
			<p>
				<strong>Start</strong>: <%: Model.Trip.StartDate.ToString(ViewConstants.DateFormat) %>
			</p>
			<p>
				<strong>Slut</strong>: <%: Model.Trip.EndDate.ToString(ViewConstants.DateFormat)%>
			</p>
		</div>
		<div class="span-8">
			<h2>Deltagere</h2>
			<ul>
				<% foreach (var participant in Model.Trip.CommitteeTripParticipants) %>
				<% { %>
					<li><%: Html.LinkTo(participant.Politician) %></li>
				<% } %>
			</ul>
		</div>
	</div>
	<div class="span-16 last">
		<div id="map" style="width: 600px; height: 500px;">
		</div>
	</div>
</asp:Content>
