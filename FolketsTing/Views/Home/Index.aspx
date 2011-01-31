<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<FrontpageViewModel>" %>

<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Forside | Folkets Ting
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<div class="span-24">
	<h1>
		Velkommen til Folkets Ting</h1>
		</div>
	<div class="span-24">
		<div class="span-24 intro">
			<p>
				<strong>Folkets Ting er demokrati som hjemmeside. På Folkets Ting kan du følge med i
					de love og betænkninger der debatteres i Folketinget. Du kan også se hvad politikerne
					laver på Borgen. Bedst af alt giver Folkets Ting dig mulighed for at tale igen:
					Du kan kommentere paragraffer i lovtekster, du kan skrive hvad du mener om politikernes
					taler og du kan stemme for eller imod lovene.
					<%= Html.ActionLink("Hvordan kommer du i gang?","Howto", "Home")%></strong>
			</p>
<%--			<p>
				(Folkets Ting er ikke opdateret fordi Folketinget har ændret strukturen på deres hjemmeside. Vi arbejder på en opdatering.)
			</p>
--%>		</div>
		<div class="span-24">
			<div class="span-12">
				<div class="span-12">
					<h3 class="inline">
						Senest fremsatte love</h3><span> (<%= Html.ActionLink<LawController>(_ => _.Index(), "se flere love") %>)</span>
					<% Html.RenderPartial("~/Views/Shared/Controls/LawList.ascx",
			Model.Proposed); %>
				</div>
				<div class="span-12 topmargin">
					<h3 class="inline">
						Mest debaterede politikere</h3><span> (<%= Html.ActionLink<PoliticianController>(_ => _.Index(), "se flere politikere") %>)</span>
					<% Html.RenderPartial("~/Views/Shared/Controls/PolList.ascx",
			ViewData.Model.MostDebated); %>
				</div>
			</div>
			<div class="span-12 last">
				<h3>
					Nyheder fra bloggen</h3>
					<%--(<a href="http://folketsting.wordpress.com/">gå til bloggen</a>)--%>
				<%--<ul class="nodec">--%>
				<% foreach (var post in Model.News)
	   {
				%>
				<%--<li>--%>
				<div class="span-12 feedevent">
					<h4>
						<%= post.Title %></h4>
					<blockquote>
						<%= post.Summary %>
					</blockquote>
					<a href="<%= post.PermaLink %>">læs resten</a> <em>(for
						<%= ViewConstants.GetTimeSpanString(post.Date.Value) %>)</em>
					<%--</li>--%>
				</div>
				<%
					} %>
				<%--</ul>--%>
			</div>
		</div>
	</div>
</asp:Content>
