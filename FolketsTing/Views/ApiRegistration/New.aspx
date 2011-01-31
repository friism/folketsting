<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<NewApiUserViewModel>" %>

<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<%@ Import Namespace="Microsoft.Web.Mvc" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	F� en API-n�gle
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<% 
		string error = "*";
		var textboxatts = new Dictionary<string, object>() { { "class", "text" } };
		var textareaatts = new Dictionary<string, object>() { };
	%>
	<div class="span-24">
		<h1>
			Lav API-n�gle</h1>
	</div>
	<div class="span-24 intro">
		<p>
			<strong>Du skal have en n�gle for at bruge Folkets Tings API. Du kan f� en n�gle straks
				ved at udfylde formularen herunder. Der er lavet et
				<%= Html.ActionLink("eksempel", "Example", "ApiRegistration", null, null) %> p� brug af API'et.</strong>
		</p>
	</div>
	<div class="span-16 topmargin">
		<% using (Html.BeginForm<ApiRegistrationController>(
			   _ => _.New(null, false), FormMethod.Post, new { id = "apiregform" }))
	 {%>
		<fieldset>
			<legend>Lav API-n�gle</legend>
			<p>
				<%= Html.ValidationSummary("Fejl:") %></p>
			<p>
				<label for="EmailAddress">
					Email adresse</label>
				<%= Html.ValidationMessageFor(
					 _ => _.ApiUser.EmailAddress, error, new { @class = "val_error" })%>
				<span>(S� vi kan kontakte dig i tilf�lde af problemer) </span>
				<br />
				<%= Html.TextBoxFor(
					_ => _.ApiUser.EmailAddress, (IDictionary<string, object>) textboxatts)%>
			</p>
			<p>
				<label for="IntendedUse">
					Forventet brug</label>
				<%= Html.ValidationMessageFor(
					_ => _.ApiUser.IntendedUse, error, new { @class = "val_error" })%>
				<span>(Ikke p�kr�vet, men skriv gerne hvad du vil bruge API'et til) </span>
				<br />
				<%= Html.TextAreaFor(
					_ => _.ApiUser.IntendedUse, (IDictionary<string, object>)textareaatts)%>
				<br />
				<span id="questioncounter"></span>
			</p>
			<p id="captcha">
				<script type="text/javascript">
					var RecaptchaOptions = {
						theme: 'clean'
					};
				</script>
				<script type="text/javascript" src="http://api.recaptcha.net/challenge?k=<%= ViewConstants.ReCaptchaKey %>">
				</script>
				<%= Html.ValidationMessage("captcha", "*", new { @class = "val_error" })%>
			</p>
			<p>
				<%= Html.SubmitButton("send","Send") %></p>
		</fieldset>
		<% } %>
	</div>
</asp:Content>
