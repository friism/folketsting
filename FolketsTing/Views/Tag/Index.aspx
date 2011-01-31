<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<TagIndexViewModel>" %>

<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<%@ Import Namespace="FT.DB" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Tagget indhold | Folkets Ting
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<div class="span-24">
		<h1>
			Tagcloud</h1>
	</div>
	<div class="span-24 tagcloud">
	<% var tagcount = Model.TagCloudData.Sum(_ => _.Value); %>
		<% foreach (var tag in Model.TagCloudData)
	 {
		 %>
		 <%= Html.ActionLink(tag.Key, "Details","Tag",
		 	new { tag = tag.Key },
								 new { @class = ViewConstants.GetTagClass(tag.Value, Model.TotalTags) })%>
		 <%
	 } %>
	</div>
</asp:Content>
