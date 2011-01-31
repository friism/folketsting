<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<ISearchResultItem>>" %>
<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<%@ Import Namespace="ScrapeDB.Model" %>
<p></p>
<%--<% foreach (var r in Model.Take(1))
   {
%>
<div class="itemsummary first">
	<div>
		<% Html.RenderPartial("~/Views/Shared/Controls/SearchResult.ascx", r); %>
	</div>
</div>
<%} %>
<% foreach (var r in Model.Skip(1))
   {
%>
<div class="itemsummary">
	<div>
		<% Html.RenderPartial("~/Views/Shared/Controls/SearchResult.ascx", r); %>
	</div>
</div>
<%
	}  %>
--%>