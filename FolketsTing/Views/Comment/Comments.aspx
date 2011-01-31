<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<CommentsViewModel>" %>

<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<%@ Import Namespace="FT.DB" %>
<% foreach (var c in ViewData.Model.Comments)
   {
%>
<div id="<%= c.Id %>" class="comment prepend-<%= Math.Min(c.Depth, 5) %>">
	<div class="commentcontrol">
		<%= c.Score ?? 0 %>
		point (<%= c.VoteCount %>
		stemmer) af
		<%= Html.ActionLink(c.User.Username.ToLower(), "Details", "User", new { uname = c.User.Username },null)%>
		for
		<%= ViewConstants.GetTimeSpanString(c.Date.Value) %>
		| <a class="respond" href="#">besvar</a> | God kommentar? <a class="commentvote yes"
			href="#">Ja</a> / <a class="commentvote no" href="#">Nej</a> | <a class="complain"
				href="#">brok dig</a>
	</div>
	<div>
		<p>
			<%= Html.Encode(c.Text) %></p>
	</div>
	<%--Also include a comment form, this is somewhat silly--%>
	<div class="respond" style="display: none">
		<% using (Html.BeginForm<CommentController>(
	   _ => _.CreateComment(Model.ElementId, c.Id, null, Model.Type, false),
	FormMethod.Post, new { id = "comment-form-" + c.Id, @class = "comment-form" }))
	 {%>
		<fieldset>
			<legend>Besvar kommentaren</legend>
			<%= Html.TextArea("comment", new { rows = 5 })%>
			<p>
				<input type="submit" value="Send" />
			</p>
			<p>
				<a class="hidelink" href="#">skjul</a>
			</p>
			<%--			<p id="" style="display: none">
				Der opstod et problem.
			</p>
--%>
		</fieldset>
		<%} %>
	</div>
</div>
<%
	} %>
<%--Also give the fuckers a form--%>

<script src="http://static.ak.connect.facebook.com/js/api_lib/v0.4/XdCommReceiver.js"
	type="text/javascript"></script>

<script type="text/javascript">
	$(document).ready(function() {
		$('textarea#comment').focus();
		$(".comment-form").submit(function() {
			$.ajax({
				type: "POST",
				data: $(this).serialize(),
				dataType: "json",
				url: $(this).attr("action"),
				success: function(data) {
					on_comment_submitted( <%= ViewData.Model.ElementId %>, '<%= Model.Type %>', data);
				}
			});
			return false;
		});
	});
</script>

<% using (Html.BeginForm<CommentController>(
	   _ => _.CreateComment(Model.ElementId, -1, null, Model.Type, false),
	FormMethod.Post, new { id = "top-comment-form", @class = "comment-form" }))
   {%>
<fieldset>
	<legend>Din kommentar</legend>
	<% if (!User.Identity.IsAuthenticated)
	{ %>
	<%= Html.TextArea("comment", new { rows = 5, disabled = "disabled" })%>
	<p>
		<input type="submit" value="Send" disabled="disabled" />
		Du skal være logget ind for at stemme og kommentere. Log ind er midlertidigt ikke tilgængeligt.
		<%--<%= Html.ActionLink("Log ind", "LogOn", "Account", new { returnurl = Model.CurrentUrl }, null)%>--%>
		<%--eller--%>
		<%--<%= Html.ActionLink("opret bruger", "Register", "Account", new { returnurl = Model.CurrentUrl}, null)%>.</p>--%>
	<%}
	else
	{%>
	<%= Html.TextArea("comment", new { 
		rows = 5, 
 })%>
	<p>
		<%= Html.CheckBox("facebook_publish", true) %>
		Del på Facebook <img alt="facebook" src="<%= ResolveUrl("~/Graphics/fb.png")%>" /></p>
	<p>
		<input type="submit" value="Send" />
	</p>
	<%} %>
	<p id="commentstatus" style="display: none">
		Der opstod et problem.
	</p>
</fieldset>
<%} %>