<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<UserViewModel>" %>

<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<%@ Import Namespace="FT.DB" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	<%= Model.User.Username.ToLower() %> | Folkets Ting
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<div class="span-24">
	<h1>
		<%= Model.User.Username.ToLower() %></h1>
		</div>
	<div class="span-24">
		<p>
			<%= Model.User.Username.ToLower() %>
			blev oprettet på Folkets Ting
			<%= Model.User.CreatedOn.Value.ToString(ViewConstants.DateFormat) %>
			og har 0 karma.
		</p>
	</div>
	<div class="span-24">
		<h3>Seneste aktivitet</h3>
		<p>(på vej)</p>
	</div>
</asp:Content>
