<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="registerTitle" ContentPlaceHolderID="TitleContent" runat="server">
	Ny bruger | Folkets Ting
</asp:Content>
<asp:Content ID="registerContent" ContentPlaceHolderID="MainContent" runat="server">
	<div class="span-24">
	<h1>
		Opret ny bruger</h1>
	<p>
		For at kunne kommentere og stemme på Folkets Ting skal du oprette en bruger. Brug
		formularen nedenfor.
	</p>
	<p>
		Password skal være på mindst
		<%=Html.Encode(ViewData["PasswordLength"])%>
		bogstaver.
	</p>
	<%= Html.ValidationSummary("Noget gik galt. Ret venligst fejl og prøv igen.")%>
	<% using (Html.BeginForm())
	{ %>
	<div>
		<fieldset>
			<legend>Bruger information</legend>
			<p>
				<label for="username">
					Brugernavn:</label>
				<%= Html.TextBox("username") %>
				<%= Html.ValidationMessage("username", "Problem med brugernavn") %>
			</p>
			<p>
				<label for="email">
					E-mail:</label>
				<%= Html.TextBox("email") %>
				<%= Html.ValidationMessage("email", "Problem med email") %>
			</p>
			<p>
				<label for="password">
					Password:</label>
				<%= Html.Password("password") %>
				<%= Html.ValidationMessage("password", "Problem med password") %>
			</p>
			<p>
				<label for="confirmPassword">
					Bekræft password:</label>
				<%= Html.Password("confirmPassword") %>
				<%= Html.ValidationMessage("confirmPassword", "Problem med password")%>
			</p>
			<p>
				<input type="submit" value="Opret" />
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
