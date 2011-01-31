<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<P20QuestionDetailViewModel>" %>

<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<asp:Content ID="Content2" ContentPlaceHolderID="TitleContent" runat="server">
	<%= Model.Question.Question.Truncate(100, true) %>
	| Folkets Ting
</asp:Content>
<asp:Content ID="headcontent" ContentPlaceHolderID="HeadContent" runat="server">
	<script type='text/javascript' src='http://www.scribd.com/javascripts/view.js'></script>
</asp:Content>
<asp:Content ID="notifycontent" ContentPlaceHolderID="NotifyContent" runat="server">
	<% Html.RenderPartial("~/Views/Shared/Controls/CommentNotify.ascx"); %>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
	<script type="text/javascript" src="<%= ResolveUrl("~/Scripts/jquery-autocomplete/jquery.autocomplete.pack.js") %>"></script>
	<script type="text/javascript" src="<%= ResolveUrl("~/Scripts/jquery-autocomplete/jquery.bgiframe.min.js") %>"></script>
	<link href="<%= ResolveUrl("~/Scripts/jquery-autocomplete/jquery.autocomplete.css") %>"
		type="text/css" rel="stylesheet" />
	<script type="text/javascript">
		google.load("jqueryui", "1.7.2");
	</script>
	<div class="span-24">
		<h1>
			<%= Model.Question.Title %>
		</h1>
		<h3>
			<%= Model.Question.Question /*.Truncate(100, true)*/%></h3>
	</div>
	<div class="span-20">
		<span><em>
			<% if (Model.Question.FTId.HasValue)
			   { %>
			S<%= Model.Question.FTId%>
			-
			<% } %>
			<%= Model.Question.AskDate.Value.ToString(ViewConstants.DateFormat)%>
			<%--            -
			<%= Model.Question.Session.Number %>. folketingssamling
			<%= Model.Question.Session.Year %>--%>
		</em></span>
		<% Html.RenderPartial("Controls/AddThis"); %>
		<div class="span-20">
			<div class="span-4">
				<span>Stillet af:</span>
				<% if (Model.Question.Type == QuestionType.Politician)
				   { %>
				<% Html.RenderPartial("~/Views/Shared/Controls/PolBox.ascx",
		   new PolBoxViewModel() { Pol = Model.Question.AskerPol, Title = null }
			); %>
				<% }
				   else
				   {
				%>
				<% Html.RenderPartial("~/Views/Shared/Controls/UserBox.ascx",
		   new UserBoxViewmodel { User = Model.Question.AskerUser }); %>
				<%
					}		 %>
			</div>
			<div class="span-4 last">
				<span>Stillet til:</span>
				<% Html.RenderPartial("~/Views/Shared/Controls/PolBox.ascx",
		   new PolBoxViewModel() { Pol = Model.Question.AskeePol, Title = Model.Question.AskeeTitle }
			); %>
			</div>
		</div>
		<div class="span-20 commentable question <%= Model.Question.P20QuestionId %>">
			<div class="span-19 content">
				<p>
					<em>Spørgsmål: </em>
					<%= Model.Question.Question %>
					<%--					<br />
					&nbsp;
					--%>
					<span class="commentcontrol" style="display: none;"><a class="js" href="#">Kommentarer</a>
					</span>
				</p>
			</div>
			<div class="span-1 last">
				<% if (Model.QuestionCommentCount > 0)
				   { %>
				<a class="js" href="#">(<%= Model.QuestionCommentCount %>)</a>
				<% }
				   else
				   { %>
				&nbsp;
				<% } %>
			</div>
		</div>
		<% if (!string.IsNullOrEmpty(Model.Question.Background) && !Model.Question.Background.ToLower().StartsWith("ingen"))
		   {%>
		<div class="span-20 commentable background <%= Model.Question.P20QuestionId %>">
			<div class="span-19 content">
				<p class="grey">
					<em>Baggrund: </em>
					<%= Model.Question.Background %>
					<span class="commentcontrol" style="display: none;"><a class="js" href="#">Kommentarer</a>
					</span>
				</p>
			</div>
			<div class="span-1 last">
				<% if (Model.BackgroundCommentCount > 0)
				   { %>
				<a class="js" href="#">(<%= Model.BackgroundCommentCount%>)</a>
				<% }
				   else
				   { %>
				&nbsp;
				<% } %>
			</div>
		</div>
		<%} %>
		<% if (Model.Question.Document != null)
		   { %>
		<div id="embedded_flash" class="span-19">
			<a href="http://www.scribd.com">Scribd</a>
		</div>
		<script type="text/javascript">
			var scribd_doc = 
				scribd.Document.getDoc(<%= Model.Question.Document.ScribdId %>, 
					'<%= Model.Question.Document.ScribdAccessKey %>');

			scribd_doc.addParam('jsapi_version', 1);
			scribd_doc.write('embedded_flash');
		</script>
		<%} %>
		<%--<% if (Model.Question.AnswerParas.Any())
	 { %>
		<div>
			<span><em>Svar (<%= Model.Question.AnswerDate.Value.ToString(ViewConstants.DateFormat) %>):
			</em></span>
			<% foreach (var p in Model.Question.AnswerParas.OrderBy(_ => _.Number).
		  Select(_ => new { _.AnswerParaId, CommentCount = _.CommentCount(), _.ParText }))
	  {
			%>
			<div class="span-20 commentable answer <%= p.AnswerParaId %>">
				<div class="span-19 content">
					<p>
						<%= p.ParText%>
						<span class="commentcontrol" style="display: none;"><a class="js" href="#">
							Kommentarer</a> </span>
					</p>
				</div>
				<div class="span-1 last">
					<% if (p.CommentCount > 0)
		{ %>
					<a class="js" href="#">(<%= p.CommentCount %>)</a>
					<% }
		else
		{ %>
					&nbsp;
					<% } %>
				</div>
			</div>
			<%
				} %>
		</div>
		<%} %>--%>
	</div>
	<div class="span-4 last lawstats">
		<div>
			<p class="label-key">
				tagget</p>
			<div class="tag-container">
				<% if (Model.TagVM.Tags.Any())
				   {
					   foreach (var t in Model.TagVM.Tags)
					   { %>
				<%=
			   Html.ActionLink<TagController>(
					_ => _.Details(t.Key), t.Key, new { @class = "post-tag", rel = "tag" })%>
				<span class="item-multiplier">×
					<%= t.Value%></span><br />
				<%}
				   }
				   else
				   {			%>
				<em>(ingen tags endnu)</em>
				<%} %>
			</div>
			<span class="add-tag"><a id="edittags" href="#">tilføj tags</a></span>
			<p class="label-key">
				vist</p>
			<p class="label-value">
				<%--Factor in this hit--%>
				<%= ViewConstants.GetTimesString(Model.Question.Views() + 1) %>
			</p>
			<p class="label-key">
				kommenteret</p>
			<p class="label-value">
				<%= ViewConstants.GetTimesString(Model.Question.CommentCount())%>
			</p>
			<p class="label-key">
				seneste aktivitet</p>
			<p class="label-value last">
				<%= ViewConstants.GetTimeSpanString(Model.LatestActivity) %>
			</p>
		</div>
		<% Html.RenderPartial("~/Views/Shared/Controls/TagForm.ascx",
			Model.TagVM); %>
	</div>
	<script type="text/javascript" src="<%= ResolveUrl("~/Scripts/Comments.js") %>"></script>
	<script type="text/javascript" src="<%= ResolveUrl("~/Scripts/jquery.cookie.js") %>"></script>
	<script type="text/javascript" src="<%= ResolveUrl("~/Scripts/notificationbar.js") %>"></script>
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
				var elementtype;
				if($(this).parents().hasClass('background'))
				{
					elementtype = '<%= CommentType.QuestionBackground %>';
				}
				else if($(this).parents().hasClass('question'))
				{
					elementtype = '<%= CommentType.Question %>';
				}
				else if($(this).parents().hasClass('answer'))
				{
					elementtype = '<%= CommentType.Answer %>';
				}
				else
				{
					alert('unknown elementtype');
				}

				commentable_click(e, $(this),
					'<%= ResolveUrl("~/Graphics/ajax-loader.gif")%>',
					'<%= Url.Action("Comments", "Comments") %>/',
					elementtype, 
					'<%= HttpUtility.UrlEncode(Request.RawUrl) %>'
					);
			});
		});
	</script>
</asp:Content>
