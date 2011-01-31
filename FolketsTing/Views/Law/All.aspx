<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<LawsViewModel>" %>
<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Alle love | Folkets Ting
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<div class="span-24">
    <h2>Alle love</h2>
<ul>
		<% foreach (var law in Model.Laws)
	 {
		%>
		<li>
			<%= Html.ActionLink(
				law.ShortName, 
				"Details", 
				new { lawname = law.ShortName.ToUrlFriendly(),
					  lawid = law.LawId})
								  %>
		</li>
		<%	 
			}  %>
	</ul>
	</div>
</asp:Content>
