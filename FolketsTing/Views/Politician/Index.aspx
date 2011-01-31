<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<PoliticianIndexViewModel>" %>

<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Politikere | Folkets Ting
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="span-24">
        <h1>
            Politikere</h1>
    </div>
    <div class="span-24 intro">
        <p>
            <strong>Her kan du finde de politikere der er valgt ind i folketinget. Klik på en politiker
                for at se hans seneste aktivitet på tinge.</strong>
        </p>
    </div>
    <div class="span-24">
        <p>
            Se
            <%= Html.ActionLink("alle politikere", "All", new { })%></p>
    </div>
    <div class="span-24">
        <div class="span-8">
            <h3>
                Hyppigst viste</h3>
            <% Html.RenderPartial("~/Views/Shared/Controls/PolList.ascx",
            ViewData.Model.MostVisited); %>
        </div>
        <div class="span-8">
            <h3>
                Mest Aktive</h3>
            <% Html.RenderPartial("~/Views/Shared/Controls/PolList.ascx",
            ViewData.Model.MostActive); %>
        </div>
        <div class="span-8 last">
            <h3>
                Mest debateret</h3>
            <% Html.RenderPartial("~/Views/Shared/Controls/PolList.ascx",
            ViewData.Model.MostDebated); %>
        </div>
    </div>
</asp:Content>
