<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<TripIndexViewModel>" %>

<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Udvalgsrejser | Folkets Ting
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<div class="span-24">
		<h1>
			Udvalgsrejser</h1>
	</div>
	<div class="span-24 intro">
		<p>
			<strong>Her kan du se Folketingsudvalgenes rejseaktivitet.</strong>
		</p>
	</div>
<%--<div class="span-24">
		<p>
			Se
			<%= Html.ActionLink("alle politikere", "All", new { })%></p>
	</div>
--%>
	<div class="span-24">
		<div class="span-8">
			<h3>
				Seneste
			</h3>
			<ul>
				<% foreach (var trip in Model.Latest) %>
				<% {%>
					<li>
						<%: Html.LinkTo(trip) %> - <%: trip.Committee.Name %>
					</li>
				<% } %>
			</ul>
		</div>
		<div class="span-8">
			<h3>
				Dyreste
			</h3>
			<ul>
				<% foreach (var trip in Model.MostExpensive) %>
				<% {%>
					<li>
						<%: Html.LinkTo(trip) %> - <%: trip.Committee.Name %>
					</li>
				<% } %>
			</ul>
		</div>
		<div class="span-8 last">
			<h3>
				Mest over budget
			</h3>
			<ul>
				<% foreach (var trip in Model.MostOverBudget) %>
				<% {%>
					<li>
						<%: Html.LinkTo(trip) %> - <%: trip.Committee.Name %>
					</li>
				<% } %>
			</ul>
		</div>
	</div>
</asp:Content>
