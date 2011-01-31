<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<PieChartViewModel>" %>
<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<div class="span-6">
	<%= Model.PartyVotes.Where(_ => _.Vote == 0).Sum(_ => _.Count)%>
	for
	<div id="aye-chart-<%= Model.LawVoteId %>" class="span-6">
	</div>
</div>
<div class="span-6">
	<%= Model.PartyVotes.Where(_ => _.Vote == 1).Sum(_ => _.Count)%>
	imod
	<div id="nay-chart-<%= Model.LawVoteId %>" class="span-6">
	</div>
</div>
<div class="span-6">
	<%= Model.PartyVotes.Where(_ => _.Vote == 2).Sum(_ => _.Count)%>
	afstår
	<div id="abstain-chart-<%= Model.LawVoteId %>" class="span-6">
	</div>
</div>
<div class="span-6 last">
	<%= Model.PartyVotes.Where(_ => _.Vote == 3).Sum(_ => _.Count)%>
	fraværende
	<div id="absent-chart-<%= Model.LawVoteId %>" class="span-6 last">
	</div>
</div>

<script type="text/javascript">
	var thecols = [{"id":"party","type":"string","label":"parti"},
		{"id":"votes","type":"number","label":"stemmer"}];

	// vars are declared elsewhere
	var ayedata = <%= ViewConstants.GGetVoteChartJSArray(Model.PartyVotes, 0) %>;
	var ayecolours = new Array(<%= ViewConstants.GGetVoteChartJSColourArray(Model.PartyVotes, 0) %>);
	drawPieChart('aye-chart-<%= Model.LawVoteId %>',thecols, ayedata, ayecolours,
		<%= ViewConstants.GetVoteChartScale(Model.PartyVotes, 0) %>);
	
	var naydata = <%= ViewConstants.GGetVoteChartJSArray(Model.PartyVotes, 1) %>;
	var naycolours = new Array(<%= ViewConstants.GGetVoteChartJSColourArray(Model.PartyVotes, 1) %>);
	drawPieChart('nay-chart-<%= Model.LawVoteId %>',thecols, naydata, naycolours,
		<%= ViewConstants.GetVoteChartScale(Model.PartyVotes, 1) %>);
	
	var abstaindata = <%= ViewConstants.GGetVoteChartJSArray(Model.PartyVotes, 2) %>;
	var abstaincolours = new Array(<%= ViewConstants.GGetVoteChartJSColourArray(Model.PartyVotes, 2) %>);
	drawPieChart('abstain-chart-<%= Model.LawVoteId %>',thecols, abstaindata, abstaincolours,
		<%= ViewConstants.GetVoteChartScale(Model.PartyVotes, 2) %>);
	
	var absentdata = <%= ViewConstants.GGetVoteChartJSArray(Model.PartyVotes, 3) %>;
	var absentcolours = new Array(<%= ViewConstants.GGetVoteChartJSColourArray(Model.PartyVotes, 3) %>);
	drawPieChart('absent-chart-<%= Model.LawVoteId %>',thecols, absentdata, absentcolours,
		<%= ViewConstants.GetVoteChartScale(Model.PartyVotes, 3) %>);
</script>

