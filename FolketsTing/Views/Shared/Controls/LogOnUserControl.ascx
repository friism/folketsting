<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%--<%@ OutputCache Duration="0" VaryByParam="none" %>--%>
<%
	if (Request.IsAuthenticated) {
%>
		<b><%= Html.ActionLink(Html.Encode(Page.User.Identity.Name), "Details", "User", new { uname = Page.User.Identity.Name }, null)%></b>
		[ <%= Html.ActionLink("log af", "LogOff", "Account") %> ]
<%
	}
	else {
%> 
		[ <%= Html.ActionLink("log ind", "LogOn", "Account", new { returnurl = Server.UrlEncode(Request.Url.PathAndQuery) }, null)%> ]
<%
	}
%>
