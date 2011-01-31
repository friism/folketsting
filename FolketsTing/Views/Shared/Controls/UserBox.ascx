<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<UserBoxViewmodel>" %>
<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<div class="span-4 polbox">
	<div style="height: 130px" class="span-1 party-Other">
		&nbsp;
	</div>
	<div class="span-3 last">
					<a href="<%= Url.Action("Details", "User", 
					new { uname = Model.User.Username }) %>">
					<%= Html.Image(ResolveUrl("~/Graphics/user.png"))%>
				</a>
	</div>
	<div class="span-4">
		<span>
			<%= Html.LinkTo(Model.User) %>
		</span>
	</div>
</div>
