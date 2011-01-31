<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" 
    Inherits="System.Web.Mvc.ViewPage<LawVoteViewModel>" %>

<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Afstemningsresultater for
    <%= ViewData.Model.Law.ShortName %>
    | Folkets Ting
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="span-24">
    <h1>
        <%= ViewData.Model.Law.ShortName %>
        - <%= ViewData.Model.Law.FtId %></h1>
        </div>
    <div class="span-24">
        <script type="text/javascript" src="<%= ResolveUrl("~/Scripts/piechartheader.js") %>"></script>

        <% foreach (var lv in ViewData.Model.Law.LawVotes.
            OrderByDescending(lv => lv.IsFinal).ThenBy(lv => lv.Date))
     {
        %>
        <div class="span-24 votecontainer">
            <h3>
                <%= lv.Name %></h3>
            <% Html.RenderPartial("~/Views/Shared/Controls/VotePieCharts.ascx",
            new PieChartViewModel()
                {
                    PartyVotes = ViewConstants.GetPartyVotes(lv.LawVoteId).ToList(),
                    LawVoteId = lv.LawVoteId
                }); %>
            <% Html.RenderPartial("~/Views/Shared/Controls/VoteOverview.ascx",
            lv); %>
        </div>
        <%
            } %>
    </div>
</asp:Content>
