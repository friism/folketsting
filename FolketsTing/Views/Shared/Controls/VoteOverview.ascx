<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<LawVote>" %>
<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<div class="span-6 noflow">
		<% Html.RenderPartial("~/Views/Shared/Controls/VoteOverviewColumn.ascx",
			ViewConstants.GetPartyForVote(ViewData.Model, 0)); %>
</div>
<div class="span-6 noflow">
		<% Html.RenderPartial("~/Views/Shared/Controls/VoteOverviewColumn.ascx",
			ViewConstants.GetPartyForVote(ViewData.Model, 1)); %>
</div>
<div class="span-6 noflow">
		<% Html.RenderPartial("~/Views/Shared/Controls/VoteOverviewColumn.ascx",
			ViewConstants.GetPartyForVote(ViewData.Model, 2)); %>
</div>
<div class="span-6 noflow last">
		<% Html.RenderPartial("~/Views/Shared/Controls/VoteOverviewColumn.ascx",
			ViewConstants.GetPartyForVote(ViewData.Model, 3)); %>
</div>
