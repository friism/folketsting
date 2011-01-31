<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Dictionary<string, IEnumerable<Politician>>>" %>

<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>

<% if(Model.Any()){ %>

<% foreach (var p in Model)
{
%>
<div class="column span-1 party-<%= 
		ViewConstants.GetCanonicalParty(p.Key) ?? "Other" %>">
	<% if (ViewConstants.GetCanonicalParty(p.Key) == null ||
	   ViewConstants.GetCanonicalParty(p.Key) == "Chairman")
 { %>
	&nbsp;<% }
 else
 { %>
	<img alt="partilogo" class="neglogo" src="
				<%= ResolveUrl("~/Graphics/"+ 
					ViewConstants.GetCanonicalParty(p.Key)  +
					"-neg.png")  %>" />
	<%} %>
</div>
<div class="column span-5 last">
	<ul class="nodec">
		<% foreach (var pol in p.Value.Select(_ => new { Name = _.FullName(), PoliticianId = _.PoliticianId }))
	 {
		%>
		<li>
			<%= Html.ActionLink(pol.Name, "Details", "Politician",
	new { polname = pol.Name.ToUrlFriendly(), polid = pol.PoliticianId }, null)%>
		</li>
		<%
			} %>
			
			<%--Crappy hack to avoid mess, insert ekstra item--%>
			<% if (p.Value.Count() == 1)
	  { %>
	  <li>&nbsp;</li>
			<%} %>
	</ul>
</div>
<%
	} %>

<%}else{ %>
&nbsp;
<% } %>