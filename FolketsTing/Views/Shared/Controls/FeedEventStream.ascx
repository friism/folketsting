<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<FeedEvent>>" %>
<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FT.Model" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<% if (Model.Any())
   {
       foreach (var e in Model)
       {
%>
<div class="span-12 feedevent">
    <h4>
        <a href="<%= e.ActionUrl %>">
            <%= e.ActionText%></a>
        <% if (!string.IsNullOrEmpty(e.SubjectText))
           { %>
        <%= e.Binder %>
        <a href="<%= e.SubjectUrl %>">
            <%= e.SubjectText%></a>
        <%} %>
    </h4>
    <%if (!string.IsNullOrEmpty(e.BodyText))
      { %>
    <blockquote>
        <p>
            "<%= e.BodyText%>"</p>
    </blockquote>
    <%} // if bodytext %>
    <em>(for
        <%= ViewConstants.GetTimeSpanString(e.date)%>
        <% if (e.Comments != null)
           { %>
        -
        <%= e.Comments %>
        <%= e.Comments == 1 ? "kommentar" : "kommentarer" %><%} %>)</em>
</div>
<% }
   } %>
