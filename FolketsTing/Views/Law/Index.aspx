<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<LawIndexViewModel>" %>

<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Love | Folkets Ting
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="HeadContent" runat="server">
    <link id="Link1" rel="alternate" href='<%= ViewData.Model.RecentlyProposedFeedLink %>'
        title="Senest fremsatte love" type="application/rss+xml" />
    <link id="Link2" rel="alternate" href='<%= ViewData.Model.RecentlyVotedFeedLink %>'
        title="Senest vedtagne love" type="application/rss+xml" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="span-24">
        <h1>
            Love</h1>
    </div>
    <div class="span-24 intro">
        <p>
            <strong>Her kan du se oversigter over de love der er på vej gennem Folketinget. Klik
                på en lov for at komme til dens oversigtsside. På lovens oversigtsside kan du komme
                videre til lovtekster, debatter og afstemninger.</strong>
        </p>
    </div>
    <div class="span-24 topmargin">
        <div class="span-12">
            <h3>
                Hyppigst viste denne uge</h3>
            <% Html.RenderPartial("~/Views/Shared/Controls/LawList.ascx",
            Model.Pop); %>
        </div>
        <div class="span-12 last">
            <h3>
                Mest debaterede denne uge</h3>
            <% Html.RenderPartial("~/Views/Shared/Controls/LawList.ascx",
            Model.Debated); %>
        </div>
    </div>
    <div class="span-24 topmargin">
        <div class="span-12">
            <h3>
                Senest fremsat</h3>
            <% Html.RenderPartial("~/Views/Shared/Controls/LawList.ascx",
            Model.Proposed); %>
        </div>
        <div class="span-12 last">
            <h3>
                Senest vedtaget</h3>
            <% Html.RenderPartial("~/Views/Shared/Controls/LawList.ascx",
            Model.Voted); %>
        </div>
    </div>
    <div class="span-24 topmargin">
        <div class="span-8">
            <h3>
                Senest i 1. behandling</h3>
            <% Html.RenderPartial("~/Views/Shared/Controls/LawList.ascx",
            ViewData.Model.First); %>
        </div>
        <div class="span-8">
            <h3>
                Senest i 2. behandling</h3>
            <% Html.RenderPartial("~/Views/Shared/Controls/LawList.ascx",
            ViewData.Model.Second); %>
        </div>
        <div class="span-8 last">
            <h3>
                Senest i 3. behandling</h3>
            <% Html.RenderPartial("~/Views/Shared/Controls/LawList.ascx",
            ViewData.Model.Third); %>
        </div>
    </div>
    <div class="span-24">
        <p>
            Se
            <%= Html.ActionLink("alle love", "All", new { })%></p>
    </div>
</asp:Content>
