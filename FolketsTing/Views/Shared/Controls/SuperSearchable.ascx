<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<SuperSearchable>" %>
<%@ Import Namespace="FolketsTing.Views" %>
<%@ Import Namespace="FT.Search" %>

<div class="searchresult">
<style>
    .searchresult h3 
    {
        vertical-align: middle;
    }
</style>
<h3><% string link; %>
        <% switch (Model.CollapseEntityType) %>
        <% { %>
        <% case CollapseEntityTypes.Law: %>
            <span style="font-size: 20px; width: 30px; display:inline-block;"><%= "§" %></span>
                    <%= Model.Title%></h3>
            <% break; %>
        <% case CollapseEntityTypes.Politician: %>
            <% link = Url.Action("Details", "Politician",
                    new { polname = Model.Title.ToUrlFriendly(), polid = Model.PoliticianIds.First() }); %>
            <a href="<%= link %>">
                <% if (Model.ImageIds.First() > 0) %>
                <% { %>
                    <%= Html.Image(Url.Action("GetScaledImage", "File",
                            new
                            {
                                imageid = Model.ImageIds.First(),
                                imagename = Model.Title.Replace(" ", "").ToUrlFriendly(),
                                width = 45,
                                height = 45
                            }), Model.Title)%></a>
                            <a href="<%=link %>"><%= Model.Title%></a></h3>
                <% } %>
                <% else %>
                <% { %>
                   <%= Html.Image(ResolveUrl("~/Graphics/user.png"), new { width = 45, height= 45})%></a><a href="<%= link %>"><%= Model.Title %></a></h3>
                <% } %>
            <% break; %>
        <% } %>
    <% if (Model.Subtitle != null) %>
    <% { %>
        <h4><%= Model.Subtitle %></h4>
    <% } %>

    <% if (Model.Summary != null)  %>
    
    <% { %>
        <a href="#" class="showmore">Vis mere</a>
        <div class="additionalresult" style="display:none">
            <p><%= Model.Summary%></p>
        </div>
    <% } %>
</div>

<% if (Model.CollapseEntityType == CollapseEntityTypes.Law) %>
<% { %>
<% if (Model.SolrHightlights.Count() > 0)%>
    <% { %>
            <div class="searchresult">
                <% foreach (var highKv in Model.SolrHightlights.Take(3))
                   { %>
                   <div>
                    <% Html.RenderPartial("controls/SearchableHighlight", highKv); %>
                   </div>
                <% } %>
                <% if (Model.SolrHightlights.Count() > 3) %>
                <% { %>
                    <% foreach (var highKv in Model.SolrHightlights.Skip(3))
                       { %>
                       <div style="display:none" class="additionalresult">
                        <% Html.RenderPartial("controls/SearchableHighlight", highKv); %>
                       </div>
                    <% } %>
                    <a href="#" class="showmore">Vis mere</a><br />
                <% }%>
            </div>
    <%} %>
<% } %>