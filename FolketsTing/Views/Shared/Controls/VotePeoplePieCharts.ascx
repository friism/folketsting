<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<LawVote>" %>
<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<div class="span-6 center">
	<% Html.BeginForm("VoteYes", "Comment", FormMethod.Post);  %>
	<input type="hidden" name="lawvoteid" value="<%= Model.LawVoteId %>" />
	<% if (Page.User.Identity.IsAuthenticated)
	{ %>
	<p>
		Stem
		<input type="submit" name="button" value="For" />
	</p>
	<%}
	else
	{ %>
	<p>
		Stem
		<input type="submit" name="button" value="For" disabled="disabled" />
	</p>
	<% Html.RenderPartial("Controls/InlineLoginControl"); %>
	<%} %>
	<% Html.EndForm(); %>
</div>
<div class="span-6">
	<%= Model.UserLawVotes.Where(_ => _.Vote == 0).Count()%>
	for

	<div id="paye-chart-<%= Model.LawVoteId %>" class="span-6">
		(ingen)
	</div>
</div>
<div class="span-6">
	<%= Model.UserLawVotes.Where(_ => _.Vote == 1).Count()%>
	imod
	<div id="pnay-chart-<%= Model.LawVoteId %>" class="span-6">
		(ingen)
	</div>
</div>
<div class="span-6 center last">
	<% Html.BeginForm("VoteNo", "Comment", FormMethod.Post); %>
	<input type="hidden" name="lawvoteid" value="<%= Model.LawVoteId %>" />
	<% if (Page.User.Identity.IsAuthenticated)
	{ %>
	<p>
		Stem
		<input type="submit" name="button" value="Imod" />
	</p>
	<%}
	else
	{ %>
	<p>
		Stem
		<input type="submit" name="button" value="Imod" disabled="disabled" />
	</p>
	<% Html.RenderPartial("Controls/InlineLoginControl"); %>
	<%} %>
	<% Html.EndForm(); %>
</div>

<script type="text/javascript">
	var thecols = [{"id":"party","type":"string","label":"parti"},
		{"id":"votes","type":"number","label":"stemmer"}];
	var yesrows = [{c:[{v:'for'},{v:<%= Model.UserLawVotes.Where(v => v.Vote == 0).Count() %>}]}];
	var norows = [{c:[{v:'for'},{v:<%= Model.UserLawVotes.Where(v => v.Vote == 1).Count() %>}]}];
	var yescolours = new Array('#008000');
	var nocolours = new Array('#800000');

	drawPieChart('paye-chart-<%= Model.LawVoteId %>',thecols, yesrows, yescolours,
		<%= ViewConstants.GetPeopleVoteChartScale(Model, 0) %>);
	drawPieChart('pnay-chart-<%= Model.LawVoteId %>', thecols, norows, nocolours,
		<%= ViewConstants.GetPeopleVoteChartScale(Model, 1) %>);
</script>

