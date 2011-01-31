<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<Law>>" %>
<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>

<% var laws =  Model.Select(l =>
		new
		{
			l.ShortName,
			l.LawId,
			l.Subtitle,
			CommentCount = l.CommentCount(),
			Views = l.Views()
		}).ToList(); %>

<% foreach (var l in laws.Take(1))
   {
%>

<div class="itemsummary first">
	<div>
		<%= Html.ActionLink(
				l.ShortName, 
				"Details", "Law",
				new { lawname = l.ShortName.ToUrlFriendly(),
					  lawid = l.LawId}, null)
		%>
	</div>
	<div>
		<%= ViewConstants.GetCommentCountString(l.CommentCount)  %>,
		<%= ViewConstants.GetViewCountString(l.Views)%>
	</div>
</div>
<%} %>

<% foreach (var l in laws.Skip(1))
   {
%>
<div class="itemsummary">
	<div>
		<%= Html.ActionLink(
				l.ShortName, 
				"Details", "Law",
				new { lawname = l.ShortName.ToUrlFriendly(),
					  lawid = l.LawId}, null)
		%>
	</div>
	<div>
		<%= ViewConstants.GetCommentCountString(l.CommentCount)  %>,
		<%= ViewConstants.GetViewCountString(l.Views)%>
	</div>
</div>
<%
	}  %>


