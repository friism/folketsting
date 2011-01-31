<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Searchable>" %>
<%@ Import Namespace="FT.Search" %>
<%@ Import Namespace="FolketsTing.Views" %>
<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Views" %>
    <i>
    <% if (Model.EntityType == EntityTypes.LawChange ||
           Model.EntityType == EntityTypes.Paragraf ||
           Model.EntityType == EntityTypes.SubChange ||
           Model.EntityType == EntityTypes.Section) %>
    <% { %>
        <%= Model.DocTypeHumanized %> ved <%= Model.StageHumanized.ToLower() %> af loven
    <% } %>
    <% else if (Model.EntityType == EntityTypes.Speech) %>
    <% { %>
        <%= Model.DocTypeHumanized %></b> af 
        		<a href="<%= Url.Action("Details", "Politician", 
					new { polname = Model.PoliticianName.ToUrlFriendly(), polid = Model.PoliticianId}) %>">
        <%= Model.PoliticianName %></a>
        ved <%= Model.StageHumanized.ToLower() %> af loven
    <% } %>
    </i>