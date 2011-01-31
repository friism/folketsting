﻿<%@ Import Namespace="FT.Search"%>
<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<PaginationInfo>" %>

<div class="pagination">
    <% if (Model.HasPrevPage) { %>
    <a href="<%= Model.PrevPageUrl %>">&laquo; Previous</a>
    <% } else { %>
    <span class="disabledPage">&laquo; Previous</span>
    <% } %>

    <% foreach (var p in Model.Pages) { %>
        <% if (p == Model.CurrentPage) {%>
        <span class="currentPage"><%=p%></span>
        <% } else {%>
        <a href="<%=Model.PageUrlFor(p)%>"><%=p%></a>    
        <% }%>
    <% } %>

    <% if (Model.HasNextPage) { %>
    <a href="<%= Model.NextPageUrl %>">Next &raquo;</a>
    <% } else { %>
    <span class="disabledPage">Next &raquo;</span>
    <% } %>
</div>