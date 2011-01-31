<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<NewP20QuestionViewModel>" %>

<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Foresl� �20 sp�rgsm�l | Folkets Ting
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<% bool isauth = User.Identity.IsAuthenticated; %>
	<% 
		var textareaatts = new Dictionary<string, object>() { };
		if (!isauth)
			textareaatts.Add("disabled", "disabled");
		var textboxatts = new Dictionary<string, object>() { { "class", "text" } };
		if (!isauth)
			textboxatts.Add("disabled", "disabled");
	%>
	<% string error = "*"; %>
	<div class="span-24">
	<h1>
		Foresl� �20 sp�rgsm�l</h1>
		</div>
	<div class="span-24 intro">
		<p>
			<strong>Her kan du foresl� �20 sp�rgsm�l. Andre brugere p� Folkets Ting kan debattere
				dit forslag og m�ske bliver det taget op af et folketingsmedlem og stillet til en
				af regeringens ministre. Fat dig kort og pr�cist. Du kan tagge dit sp�rgsm�l efter
				det er oprettet.</strong>
		</p>
	</div>
	<div class="span-12 topmargin">
		<% using (Html.BeginForm<P20QuestionController>(_ => _.New(null, false), FormMethod.Post, new { id = "p20qform" }))
	 {%>
		<fieldset>
			<legend>Foresl� �20 sp�rgsm�l</legend>
			<%--			<p>
				<%= Html.ValidationSummary("Ret venligts hvor kr�vet") %></p>
--%>
			<% if(!isauth){ %>
			<p class="notice">
				Du skal v�re logget ind for at kunne stille �20 sp�rgsm�l. Logind er midlertidigt sl�et fra.
<%--				<%= Html.ActionLink("Log ind", "LogOn", "Account", new { returnurl = Request.RawUrl }, null)%>
				eller
				<%= Html.ActionLink("opret bruger", "Register", "Account", new { returnurl = Request.RawUrl }, null)%>.--%>
			</p>
			<%} %>
			<p>
				<label for="Title">
					Titel</label>
				<%= Html.ValidationMessageFor(_ => _.Question.Title, error, new { @class = "val_error" })%>
				<span>(f.eks. "Tronf�lgeloven")</span>
				<br />
				<%= Html.TextBoxFor(_ => _.Question.Title, (IDictionary<string, object>) textboxatts)%>
				<br />
				<span id="titlecounter"></span>
			</p>
			<p>
				<label for="Question">
					Sp�rgsm�l</label>
				<%= Html.ValidationMessageFor(_ => _.Question.Question, error, new { @class = "val_error" })%>
				<span>(f.eks. "Mener statsministren, at den offentligt finansierede kampagne om tronf�lgeloven
					er politisk neutral?". <em>Kun en s�tning.</em>)</span>
				<br />
				<%= Html.TextAreaFor(_ => _.Question.Question, (IDictionary<string,object>) textareaatts)%>
				<br />
				<span id="questioncounter"></span>
			</p>
			<p>
				<label for="background">
					Baggrund</label><%= Html.ValidationMessageFor(_ => _.Question.Background, error, new { @class = "val_error" })%>
				<span>(Beskriv baggrunden for sp�rgsm�let, referer gerne til avis-artikler etc.)</span>
				<br />
				<%= Html.TextAreaFor(_ => _.Question.Background, (IDictionary<string,object>) textareaatts)%>
				<br />
				<span id="backgroundcounter"></span>
			</p>
			<p>
				<label for="minister">
					Minister</label>
					<%= Html.ValidationMessageFor(_ => _.Question.AskeeTitle, error, new { @class = "val_error" })%>
				<%= Html.DropDownListFor(_ => _.Question.AskeeTitle,
					from m in Model.Ministrys
					select new SelectListItem() { Text = m.Substring(0,1).ToUpper() + m.Substring(1), Value = m },
										"Ingen valgt", (IDictionary<string,object>) textareaatts)
				%>
			</p>
			<p id="captcha">
				<script type="text/javascript">
					var RecaptchaOptions = {
						theme: 'clean'
					};
				</script>
				<script type="text/javascript" src="http://api.recaptcha.net/challenge?k=<%= ViewConstants.ReCaptchaKey %>">
				</script>
				<%= Html.ValidationMessage("captcha", "Forkert CAPTCHA", new { @class = "val_error" })%>
			</p>
			<p>
				<%= Html.SubmitButton("send","Send") %></p>
		</fieldset>
		<% } %>
	</div>
	<div class="span-12 last topmargin">
		<h3>
					Senest stillede sp�rgsm�l</h3>
								<% Html.RenderPartial("~/Views/Shared/Controls/P20QList.ascx",
			new P20QListViewModel { Questions = Model.LatestQuestions, Mode = FolketsTing.Controllers.P20QListViewModel.RenderMode.QuestionText }); %>
	</div>
	<script type="text/javascript" src="<%= ResolveUrl("~/Scripts/twitterCounter.js") %>"></script>
	<script type="text/javascript" src="<%= ResolveUrl("~/Scripts/jquery.charcounter.js") %>"></script>
	<script type="text/javascript">
		google.setOnLoadCallback(function() {
			$("#Question_Title").charCounter(80, { container: "#titlecounter", format: "%1 tegn tilbage" });
			$("#Question_Question").charCounter(200, { container: "#questioncounter", format: "%1 tegn tilbage" });
			$("#Question_Background").charCounter(600, { container: "#backgroundcounter", format: "%1 tegn tilbage" });
		});
	</script>
</asp:Content>