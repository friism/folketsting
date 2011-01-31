<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="loginTitle" ContentPlaceHolderID="TitleContent" runat="server">
	Log ind | Folkets Ting
</asp:Content>
<asp:Content ID="loginContent" ContentPlaceHolderID="MainContent" runat="server">
	<script type="text/javascript">
		google.setOnLoadCallback(function() {
			$("#username").focus();
		});
	</script>
<div class="span-24">
	<h1>
		Log ind</h1>
	<p>
		Indtast brugernavn og password.
		<%= Html.ActionLink("Opret bruger", "Register") %>
		hvis du er ny på tinge.
	</p>
	<%--<%= Html.ValidationSummary("Noget gik galt. Ret venligst fejl og prøv igen.") %>--%>
	<% using (Html.BeginForm())
	{ %>
	<div>
		<fieldset>
			<legend>Login oplysninger</legend>
			<p>
				<label for="username">
					Brugernavn:</label>
				<%= Html.TextBox("username") %>
				<%= Html.ValidationMessage("username", "Problem med brugernavn") %>
			</p>
			<p>
				<label for="password">
					Password:</label>
				<%= Html.Password("password") %>
				<%= Html.ValidationMessage("password", "Problem med password") %>
			</p>
			<p>
				<%= Html.CheckBox("rememberMe") %>
				<label class="inline" for="rememberMe">
					Husk mig?</label>
			</p>
			<p>
				<input type="submit" value="Log ind" />
			</p>
			<% if (ViewData["returnurl"] != null)
	  { %>
			<input type="hidden" name="returnurl" value="<%= ViewData["returnurl"] %>" />
			<%} %>
		</fieldset>
	</div>
	<% } %>
	</div>
</asp:Content>
