<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<Politician>>" %>
<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>

<% foreach (var p in Model.Select(p =>
		new
		{
			Name = p.Firstname + " " + p.Lastname,
			p.PoliticianId,
			Views = p.Views(),
			CommentCount = p.CommentCount(),
			p.ImageId,
			PartyString = p.Party.Initials,
		}))
   {
%>
<%--<li>--%>
<div>
	<% if (p.ImageId != null)
	{ %>
	<%--<div>--%>
		<a href="<%= Url.Action("Details", "Politician", 
					new { name = p.Name.ToUrlFriendly(), id = p.PoliticianId}) %>">
			<%= Html.Image(
					Url.Action(
						"GetScaledImage", 
						"File",
										new
										{
											imageid = p.ImageId, 
							imagename = p.Name.Replace(" ", "").ToUrlFriendly(),
							width = 25,
							height = 25 
						}),
					p.Name)%>
		</a>
	<%--</div>--%>
	<% } %>
	<%--<div>--%>
		<%= Html.ActionLink(
				p.Name + " ("+ p.PartyString +")", 
				"Details", "Politician",
							new
							{
								name = p.Name.ToUrlFriendly(),
								id = p.PoliticianId
							}, null)
		%>
		(<%= ViewConstants.GetCommentCountString(p.CommentCount)  %>,
		<%= ViewConstants.GetViewCountString(p.Views)%>)
	<%--</div>--%>
	<%--</li>--%>
</div>
<%
	}  %>
<%--</ul>--%>
