<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<PoliticiansViewModel>" %>

<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Alle politikere | Folkets Ting
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<h2>
		Alle politikere</h2>
	<ul>
		<% foreach (var p in Model.Politicians.Select(p => new {name = p.FullName(), politiker_id = p.PoliticianId}))
	 {
		%>
		<li>
			<%= 
Html.ActionLink(
				p.name, 
				"Details",
							new
							{
								polname = p.name.ToUrlFriendly(),
								polid = p.politiker_id
							})								  %>
		</li>
		<%	 
			}  %>
	</ul>
</asp:Content>
