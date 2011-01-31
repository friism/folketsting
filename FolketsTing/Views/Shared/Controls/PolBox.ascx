<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<PolBoxViewModel>" %>
<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<div class="span-4 polbox">
	<div style="height: 100px" class="span-1 party-<%= 
			ViewConstants.GetCanonicalParty(Model.Pol.Party.Initials) ?? "Other" %>">
		<% if (ViewConstants.GetCanonicalParty(Model.Pol.Party.Initials) == null ||
		   ViewConstants.GetCanonicalParty(Model.Pol.Party.Initials) == "Chairman")
	 { %>
		&nbsp;<% }
	 else
	 { %>
		<img alt="partilogo" class="neglogo" src="
					<%= ResolveUrl("~/Graphics/"+ 
						ViewConstants.GetCanonicalParty(Model.Pol.Party.Initials) +
						"-neg.png")  %>" />
		<%} %>
	</div>
	<div class="span-3 last">
		<% if (Model.Pol.ImageId != null)
	 { %>
		<a href="<%= Url.Action("Details", "Politician", 
					new { polname = Model.Pol.FullName().ToUrlFriendly(), polid = Model.Pol.PoliticianId}) %>">
			<%= Html.Image(
					Url.Action(
						"GetScaledImage", 
						"File", 
						new { imageid = Model.Pol.ImageId, 
							imagename = Model.Pol.FullName().Replace(" ", "").ToUrlFriendly(),
							width = 100,
							height = 100 
						}),
					Model.Pol.FullName())	
			%>
		</a>
		<% }
	 else
	 {  %>
		&nbsp;
		<%} %>
	</div>
	<div class="span-4">
		<span>
			<%= Html.LinkTo(Model.Pol) %><%= Model.Title != null ? " (" + Model.Title + ")" : "" %>
		</span>
	</div>
</div>
