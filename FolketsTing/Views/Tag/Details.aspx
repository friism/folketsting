<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<TagViewModel>" %>

<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<%@ Import Namespace="FT.DB" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Mest aktivt '<%= Html.Encode(Model.TagName)%>' indhold | Folkets Ting
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<div class="span-24">
		<h1>
			Indhold tagget '<%= Html.Encode(Model.TagName)%>'</h1>
	</div>
	<div class="span-24">
		<% foreach (var r in Model.Results)
	 {%>
<%--		<% switch (r.GetType().Name)
	 {
		%>
		<% case "Law": %>
		<% var l = r as Law; %>
		<div class="searchresult">
			<h4>
				<%= Html.LinkTo(l, "Lov — " + l.ShortName)%>
			</h4>
			<div>
				<p>
					<%= ViewConstants.GetHighlighted(
						new List<string>() { l.Summary, l.Subtitle }, new List<string>(){ Model.TagName }, 400)%>
				</p>
			</div>
		</div>
		<% break; %>
		<% case "P20Question": %>
		<% var q = r as P20Question; %>
		<div class="searchresult">
			<h4>
				<%= Html.LinkTo(q, "§20 spørgsmål — " + q.Title)%>
			</h4>
			<div>
				<p>
					<%= ViewConstants.GetHighlighted(
			new List<string>() { q.Question, q.Background }, new List<string>(){ Model.TagName}, 400) %>
				</p>
			</div>
		</div>
		<% break; %>
		<% default: throw new ArgumentException(Model.GetType().Name); %>
		<%
			} %>--%>
		<%} %>
	</div>
</asp:Content>
