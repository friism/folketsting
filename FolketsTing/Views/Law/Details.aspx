<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<LawDetailsViewModel>" %>

<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<asp:Content ID="Content2" ContentPlaceHolderID="TitleContent" runat="server">
	<%= Model.Law.ShortName%>
	| Folkets Ting
</asp:Content>
<asp:Content ContentPlaceHolderID="HeadContent" runat="server">
	<link href="<%= ResolveUrl("~/Scripts/jquery-autocomplete/jquery.autocomplete.css") %>"
		type="text/css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
	<script type="text/javascript" src="<%= ResolveUrl("~/Scripts/jquery-autocomplete/jquery.autocomplete.pack.js") %>"></script>
	<script type="text/javascript" src="<%= ResolveUrl("~/Scripts/jquery-autocomplete/jquery.bgiframe.min.js") %>"></script>
	<script type="text/javascript">
		google.load("jqueryui", "1.7.2");
	</script>
	<div class="span-24">
		<h1>
			<%= Model.Law.ShortName %>
			-
			<%= Model.Law.FtId %></h1>
	</div>
	<div class="span-20">
		<h3>
			<%= Model.Law.Subtitle %></h3>
		<span><em>
			<%= Model.Law.Ministry.Name %>
			-
			<%= Model.Law.Session.Number %>. folketingssamling
			<%= Model.Law.Session.Year%></em></span>
		<% Html.RenderPartial("Controls/AddThis"); %>
		<div class="span-20">
			<% if (Model.Law.ProposedLaws.Count == 1 &&
		  Model.Law.ProposedLaws.Single().IsMinister.Value &&
			Model.Law.ProposedLaws.Single().Politician == null)
	  { %>
			<%= Model.Law.ProposedLaws.Single().Title%>
			<%}
	  else if (Model.Law.ProposedLaws.Count == 1 &&
		 Model.Law.ProposedLaws.Single().IsMinister.Value)
	  { %>
			<% Html.RenderPartial("~/Views/Shared/Controls/PolBox.ascx",
		   new PolBoxViewModel() { Pol = Model.Law.ProposedLaws.Single().Politician, Title = Model.Law.ProposedLaws.Single().Title }
			); %>
			<% }
	  else
	  {
		  foreach (var pl in Model.Law.ProposedLaws)
		  {
			   
			%>
			<% Html.RenderPartial("~/Views/Shared/Controls/PolBox.ascx",
		   new PolBoxViewModel() { Pol = pl.Politician, Title = null }
			); %>
			<% }
	   } %>
		</div>
		<div>
			<p class="grey">
				<%= Model.Law.Summary %></p>
		</div>
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
					_ => _.Details(t.Key), Html.Encode(t.Key), new { @class = "post-tag", rel = "tag" })%>
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
				<%= ViewConstants.GetTimesString(Model.Views + 1) %></p>
			<p class="label-key">
				kommenteret</p>
			<p class="label-value">
				<%= ViewConstants.GetTimesString(Model.Law.CommentCount())%>
			</p>
			<p class="label-key">
				seneste aktivitet</p>
			<p class="label-value last">
				<%= ViewConstants.GetTimeSpanString(Model.LatestActivity) %></p>
		</div>
		<% Html.RenderPartial("~/Views/Shared/Controls/TagForm.ascx",
			Model.TagVM); %>
	</div>
	<div class="span-24">
		<h4>
			Status for loven</h4>
		<table class="lawstatus">
			<%
				string col1class = Model.Law.Paragraphs.Any(p => p.Stage == LawStage.First) ? "done" : "notdone";
				string col2class = Model.FirstNegot != null ? "done" : "notdone";
				string col3class = Model.SecondNegot != null ? "done" : "notdone";
				string col4class = Model.Law.Paragraphs.Any(p => p.Stage == LawStage.Second) ? "done" : "notdone";
				string col5class = Model.ThirdNegot != null ? "done" : "notdone";
				string col6class = Model.Law.LawVotes.Any() ? "done" : "notdone";
				string col7class = Model.Law.Paragraphs.Any(p => p.Stage == LawStage.Third) ? "done" : "notdone";														
			%>
			<tbody>
				<tr>
					<td class="<%= col1class %>" >
						<%= Model.IsProposed ?
							Html.ActionLink("Fremsat", "LawText",
								new
								{
									lawname = Model.Law.ShortName.ToUrlFriendly(),
									lawid = Model.Law.LawId,
									stage = "fremsat",
						}) : MvcHtmlString.Create("Fremsat")%>
					</td>
					<td class="<%= col2class %>">
						<% if (Model.FirstNegot != null)
		 {
						%>
						<%= Html.ActionLink("1. behandling", "Deliberation",
								new
								{
									lawname = Model.Law.ShortName.ToUrlFriendly(),
									lawid = Model.Law.LawId,
									deliberationnr = 1
								})%>
						<%}
		 else
		 {			  %>
						1. behandling
						<%} %>
					</td>
					<td class="<%= col3class %>">
						<%= Model.SecondNegot == null ? 
							MvcHtmlString.Create("2. behandling") :  Html.ActionLink("2. behandling", "Deliberation",
																new
																{
																	lawname = Model.Law.ShortName.ToUrlFriendly(),
										lawid = Model.Law.LawId,
										deliberationnr = 2		
						})%>
					</td>
					<td class="<%= col4class %>">
						<%= Model.IsAfterSecond ?
							Html.ActionLink("2. behandling lovtekst", "LawText",
																new
																{
																	lawname = Model.Law.ShortName.ToUrlFriendly(),
										lawid = Model.Law.LawId,
										stage = "aftersec"
																}) : MvcHtmlString.Create("2. behandling lovtekst")%>
					</td>
					<td class="<%= col5class %>">
						<%= Model.ThirdNegot == null ? 
							MvcHtmlString.Create("3. behandling") :  
							Html.ActionLink("3. behandling", "Deliberation",
								new {
																	lawname = Model.Law.ShortName.ToUrlFriendly(),
										lawid = Model.Law.LawId,
										deliberationnr = 3
						})%>
					</td>
					<td class="<%= col6class %>">
						<%= Model.Law.LawVotes.Any() ? 
							Html.ActionLink("Afstemning", "Votes", 
								new { lawname = Model.Law.ShortName.ToUrlFriendly(),
									lawid = Model.Law.LawId}) :
											MvcHtmlString.Create("Afsteming") %>
					</td>
					<td class="<%= col7class %>">
						<%= Model.IsPassed ?
							 Html.ActionLink("Vedtaget", "LawText",
									new
									{
										lawname = Model.Law.ShortName.ToUrlFriendly(),
										lawid = Model.Law.LawId,
										stage = "vedtaget"
									})
									: MvcHtmlString.Create("Vedtaget")
						%>
					</td>
				</tr>
				<tr>
					<td class="<%= col1class %>">
						<%= Model.Law.Proposed == null ?
														"(ukendt)" : Model.Law.Proposed.Value.ToString(ViewConstants.DateFormat)%>
					</td>
					<td class="<%= col2class %>">
						<%= Model.FirstNegot == null ?
							"" : Model.FirstNegot.Date.Value.ToString(ViewConstants.DateFormat) %>
					</td>
					<td class="<%= col3class %>">
						<%= Model.SecondNegot == null ?
							"" : Model.SecondNegot.Date.Value.ToString(ViewConstants.DateFormat) %>
					</td>
					<td class="<%= col4class %>">
						<% if (Model.Law.SecondDeliberation.HasValue)
		 { %>
						<%= Model.Law.SecondDeliberation.Value.ToString(ViewConstants.DateFormat)%>
						<%} %>
					</td>
					<td class="<%= col5class %>">
						<%= Model.ThirdNegot == null ?
							"" : Model.ThirdNegot.Date.Value.ToString(ViewConstants.DateFormat) %>
					</td>
					<td class="<%= col6class %>">
						<% if (Model.FinalVoteDate.HasValue)
		 { %>
						<%= Model.FinalVoteDate.Value.ToString(ViewConstants.DateFormat)%>
						<%} %>
					</td>
					<td class="<%= col7class %>">
						<%= Model.Law.Passed == null ?
							"" : Model.Law.Passed.Value.ToString(ViewConstants.DateFormat) %>
					</td>
				</tr>
				<tr>
					<td class="<%= col1class %>">
						<em>
							<%= ViewConstants.GetCommentCountString(Model.ProposedCommCount) %></em>
					</td>
					<td class="<%= col2class %>">
						<em>
							<%= ViewConstants.GetCommentCountString(Model.FirstCommCount) %></em>
					</td>
					<td class="<%= col3class %>">
						<em>
							<%= ViewConstants.GetCommentCountString(Model.SecondCommCount) %></em>
					</td>
					<td class="<%= col4class %>">
						<em>
							<%= ViewConstants.GetCommentCountString(Model.AfterSecCommCount) %></em>
					</td>
					<td class="<%= col5class %>">
						<em>
							<%= ViewConstants.GetCommentCountString(Model.ThirdCommCount) %></em>
					</td>
					<td class="<%= col6class %>">
					</td>
					<td class="<%= col7class %>">
						<em>
							<%= ViewConstants.GetCommentCountString(Model.PassedCommCount) %></em>
					</td>
				</tr>
			</tbody>
		</table>
	</div>
	<% if (Model.PieChartData != null)
	{ %>
	<div class="span-24">
		<h4>
			Tinget stemmer</h4>
		<script type="text/javascript" src="<%= ResolveUrl("~/Scripts/piechartheader.js") %>"></script>
		<% Html.RenderPartial("~/Views/Shared/Controls/VotePieCharts.ascx",
			Model.PieChartData); %>
	</div>
	<div class="span-24">
		<h4>
			Folket stemmer</h4>
		<% Html.RenderPartial("~/Views/Shared/Controls/VotePeoplePieCharts.ascx",
			Model.Vote); %>
	</div>
	<%} %>
</asp:Content>
