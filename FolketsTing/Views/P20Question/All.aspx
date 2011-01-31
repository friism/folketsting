<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<AllP20ViewModel>" %>
<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Alle §20 spørgsmål | Folkets Ting
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<div class="span-24">
    <h2>Alle §20 spørgsmål</h2>
<ul>
		<% foreach (var q in Model.Questions)
	 {
		%>
		<li>
			<%= Html.ActionLink(
				q.Question, 
				"Details", 
				new { questiontext = q.Question.ToUrlFriendly(),
					  qid = q.P20QuestionId})
								  %>
		</li>
		<%	 
			}  %>
	</ul>
	</div>
</asp:Content>
