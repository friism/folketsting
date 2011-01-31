<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<FolketsTing.Controllers.DeliberationViewModel>" %>

<%@ Import Namespace="FolketsTing.Views" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FT.DB" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	<%= ViewData.Model.Deliberation.Number.UrlValue() + ". behandling af " + ViewData.Model.Law.ShortName %>
	| Folkets Ting
</asp:Content>
<asp:Content ContentPlaceHolderID="NotifyContent" runat="server">
	<% Html.RenderPartial("~/Views/Shared/Controls/CommentNotify.ascx"); %>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<div class="span-24">
		<h1>
			<%= ViewData.Model.Deliberation.Number.UrlValue() + ". behandling af " + ViewData.Model.Law.ShortName %></h1>
		<% foreach (var tale in ViewData.Model.Deliberation.Speeches.OrderBy(_ => _.SpeechNr).
		 Select(_ => new { _.Politician, _.SpeechParas, _.IsTemp }))
		   {
		%>
		<div class="span-24 noflow">
			<div class="column span-1 party-<%= 
			tale.Politician == null ? "Chairman" : ViewConstants.GetCanonicalParty(tale.Politician.Party.Initials) ?? "Other" %>">
				<% if (tale.Politician == null || ViewConstants.GetCanonicalParty(tale.Politician.Party.Initials) == null ||
			ViewConstants.GetCanonicalParty(tale.Politician.Party.Initials) == "Chairman")
					{ %>
				&nbsp;<% }
					else
					{ %>
				<img alt="partilogo" class="neglogo" src="
					<%= ResolveUrl("~/Graphics/"+ 
						ViewConstants.GetCanonicalParty(tale.Politician.Party.Initials)  +
						"-neg.png")  %>" />
				<%} %>
			</div>
			<div class="column span-3 polcol">
				<% if (tale.Politician != null && tale.Politician.Firstname != "Formanden")
				   { %>
				<%= Html.LinkTo(tale.Politician) %>
				<% if (tale.Politician.ImageId.HasValue)
				   { %>
				<a href="<%= Url.Action("Details", "Politician", 
					new { polname = tale.Politician.FullName().ToUrlFriendly(), polid = tale.Politician.PoliticianId}) %>">
					<%= Html.Image(
					Url.Action(
						"GetScaledImage",
						"File",
						new { imageid = tale.Politician.ImageId, 
							imagename = tale.Politician.FullName().Replace(" ", "").ToUrlFriendly(),
							  width = 100,
							  height = 100 
						}),
					tale.Politician.FullName()) %>
				</a>
				<% } %>
				<%}
				   else
				   { %>
				<%= "Formanden" /* tale.Politician.FullName() */%>
				<%} %>
			</div>
			<div class="column span-20 last">
				<% foreach (var p in tale.SpeechParas.OrderBy(_ => _.Number).
		   Select(_ => new { _.ParText, CommentCount = _.CommentCount(), _.SpeechParaId }))
				   { %>
				<div class="span-20 commentable speech <%= p.SpeechParaId %>">
					<a name="par-<%= p.SpeechParaId %>"></a>
					<div class="span-19 content">
						<p>
							<%= p.ParText%>
							<span class="commentcontrol" style="display: none;">
								<% if (!tale.IsTemp.HasValue || !tale.IsTemp.Value)
								   { %>
								<a class="js" href="#">Kommentarer</a>
								<%}
								   else
								   {
								%><em>(kladde, kan ikke kommenteres)</em><%
																			 } %>
							</span>
						</p>
					</div>
					<div class="span-1 last">
						<% if (p.CommentCount > 0)
						   { %>
						<a class="js" href="#">(<%= p.CommentCount%>)</a>
						<% }
						   else
						   { %>
						&nbsp;
						<% } %>
					</div>
				</div>
				<%} %>
			</div>
		</div>
		<%
			} %>
	</div>
	<script type="text/javascript">
		google.load("jqueryui", "1.7.2");
	</script>
	<script type="text/javascript" src="<%= ResolveUrl("~/Scripts/jquery.cookie.js") %>"></script>
	<script type="text/javascript" src="<%= ResolveUrl("~/Scripts/notificationbar.js") %>"></script>
	<script type="text/javascript" src="<%= ResolveUrl("~/Scripts/Comments.js") %>"></script>
	<div id="authdialog" style="display: none;">
		<% Html.RenderPartial("Controls/InlineLoginControl"); %>
			<a id="closedialog" href="#">Luk</a></p>
	</div>
	<script type="text/javascript">
		// setting some available vars, this is bad design
		var isauthed = <%= User.Identity.IsAuthenticated.ToString().ToLower() %>;
		var voteurl = '<%= Url.Action("Vote", "Comment") %>';
		$(document).ready(function() {
			handnotific();
			init_effects();
			// this makes the comment links go boom
			$("div.commentable a.js").click(function(e) {
				e.preventDefault();
				
				commentable_click(e, $(this),
					'<%= ResolveUrl("~/Graphics/ajax-loader.gif")%>',
					'<%= Url.Action("Comments", "Comments") %>/',
					'<%= CommentType.Speech %>',
					'<%= HttpUtility.UrlEncode(Request.RawUrl) %>');
			});
		});
	</script>
</asp:Content>
