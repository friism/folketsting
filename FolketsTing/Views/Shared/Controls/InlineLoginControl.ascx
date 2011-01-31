<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<p>
	Du skal være logget ind for at stemme og kommentere. Log ind er midlertigt ikke tilgængeligt.
	<%--
		Du skal være logget ind for at stemme og kommentere.
		<%= Html.ActionLink("Log ind", "LogOn", "Account", null, null)%>
		eller
		<%= Html.ActionLink("opret bruger", "Register", "Account", null, null)%>.
	--%>
</p>
