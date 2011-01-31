<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<P20QListViewModel>" %>
<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<% 
	var qs = Model.Questions
	   .Select(q =>
		new
		{
			q.Title,
			q.P20QuestionId,
			CommentCount = q.CommentCount(),
			Views = q.Views(),
			q.Question
		}
	).ToList(); 
%>
<% foreach (var q in qs.Take(1))
   {
%>
<div class="itemsummary first">
	<div>
		<%= Html.ActionLink(
				q.Title, 
				"Details", "P20Question",
							new
							{
								questiontext = q.Question.ToUrlFriendly(),
								qid = q.P20QuestionId
							}, null)
		%>
	</div>
	<% if (Model.Mode == FolketsTing.Controllers.P20QListViewModel.RenderMode.QuestionText)
	{ %>
	<div>
		<p>
			<%= q.Question %>
		</p>
	</div>
	<%} %>
	<div>
		<%= ViewConstants.GetCommentCountString(q.CommentCount)  %>,
		<%= ViewConstants.GetViewCountString(q.Views)%>
	</div>
</div>
<%} %>
<% foreach (var q in qs.Skip(1))
   {
%>
<div class="itemsummary">
	<div>
		<%= Html.ActionLink(
				q.Title,
				"Details", "P20Question",
							new
							{
								questiontext = q.Question.ToUrlFriendly(),
								qid = q.P20QuestionId
							}, null)
		%>
	</div>
	<div>
		<%= ViewConstants.GetCommentCountString(q.CommentCount)  %>,
		<%= ViewConstants.GetViewCountString(q.Views)%>
	</div>
</div>
<%
	}  %>
