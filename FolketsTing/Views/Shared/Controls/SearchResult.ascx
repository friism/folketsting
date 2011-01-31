<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<SearchResultItemViewModel>" %>
<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<%@ Import Namespace="FT.Model" %>
<% switch (Model.Item.Item.GetType().Name)
   {
%>
<% case "Law":
		   {%>
<% 
	var l = Model.Item.Item as Law; %>
<% if (Model.Item.Type == SearchResType.FirstDelib || Model.Item.Type == SearchResType.SecondDelib || Model.Item.Type == SearchResType.ThirdDelib)
   {
	   var delibnr = -1;
	   if (Model.Item.Type == SearchResType.FirstDelib) delibnr = 1;
	   else if (Model.Item.Type == SearchResType.SecondDelib) delibnr = 2;
	   else delibnr = 3;
	   var speech = Model.Item.Speech;
		   //l.Deliberations.Single(_ => _.Number == delibnr).Speeches.Single(_
		   // => _.SpeechId == Model.Item.SpeechId);
	   string link;
	   string linktext;

	   switch (Model.Item.Type)
	   {
		   case SearchResType.FirstDelib:
			   {
				   link = Url.Action("Deliberation", "Law",
					   new
					   {
						   lawname = l.ShortName.ToUrlFriendly(),
						   lawid = l.LawId,
						   deliberationnr = 1
					   }) + "#par-" + speech.SpeechParas.OrderBy(sp => sp.Number).First().SpeechParaId;
				   linktext = "Lov — debat, første behandling — " + l.ShortName;
				   break;
			   }
		   case SearchResType.SecondDelib:
			   {
				   link = Url.Action("Deliberation", "Law",
					   new
					   {
						   lawname = l.ShortName.ToUrlFriendly(),
						   lawid = l.LawId,
						   deliberationnr = 2
					   }) + "#par-" + speech.SpeechParas.OrderBy(sp => sp.Number).First().SpeechParaId;
				   linktext = "Lov — debat, anden behandling — " + l.ShortName;
				   break;
			   }
		   case SearchResType.ThirdDelib:
			   {
				   link = Url.Action("Deliberation", "Law",
					   new
					   {
						   lawname = l.ShortName.ToUrlFriendly(),
						   lawid = l.LawId,
						   deliberationnr = 3
					   }) + "#par-" + speech.SpeechParas.OrderBy(sp => sp.Number).First().SpeechParaId;
				   linktext = "Lov — debat, tredje behandling — " + l.ShortName;
				   break;
			   }
		   default: throw new ArgumentException(Model.Item.Type.ToString());
	   }
%>
<h4>
	<a href="<%= link %>">
		<%=linktext %></a></h4>
<% if (Model.IsFirstOfMany)
   { %>
<span><a href="#" class="showmore">Vis flere resultater fra denne side</a> </span>
<%} %>
<div>
	<p>
		<em>
			<%= speech.Politician.Name %>: </em>
		<%= ViewConstants.GetHighlighted(
	new List<string>() { speech.SpeechTextFT }, Model.Query != null ? Model.Query.Split(' ') : null, 400)%>
	</p>
</div>
<%}
   else
   {%>
<h4>
	<% switch (Model.Item.Type)
	{
	%>
	<% case SearchResType.PropLT: %>
	<%= Html.ActionLink<LawController>(_ => _.LawText(l.ShortName.ToUrlFriendly(), l.LawId, "fremsat"), "Lov — Lovtekst, fremsat — " + l.ShortName) %>
	<% break; %>
	<% case SearchResType.SecondLT: %>
	<%= Html.ActionLink<LawController>(_ => _.LawText(l.ShortName.ToUrlFriendly(), l.LawId, "aftersec"), "Lov — Lovtekst, 2. behandling — " + l.ShortName)%>
	<% break; %>
	<% case SearchResType.PassLT: %>
	<%= Html.ActionLink<LawController>(_ => _.LawText(l.ShortName.ToUrlFriendly(), l.LawId, "vedtaget"), "Lov — Lovtekst, vedtaget — " + l.ShortName)%>
	<% break; %>
	<%
		default: throw new ArgumentException();
   } %>
</h4>
<div>
	<p>
		<% string highlighstring;
   switch (Model.Item.Type)
   {
	   case SearchResType.PropLT:
		   highlighstring = l.ProposedLawTextFT; break;
	   case SearchResType.SecondLT:
		   highlighstring = l.SecondLawTextFT; break;
	   case SearchResType.PassLT:
		   highlighstring = l.PassedLawTextFT; break;
	   default: throw new ArgumentException();
   }
		%>
		<%= ViewConstants.GetHighlighted(
	new List<string>() { highlighstring },  Model.Query != null ? Model.Query.Split(' ') : null, 400)%>
	</p>
</div>
<%} %>
<% break;
		   }%>
<% case "P20Question":
		   { %>
<% P20Question question = Model.Item.Item as P20Question; %>
<h4>
	<%= Html.LinkTo(question,
				"§20 " + (Model.Item.Type == SearchResType.Question ? "spørgsmål" : "svar") + " — " + question.Title
			)%></h4>
<% if (Model.IsFirstOfMany)
   { %>
<span><a href="#" class="showmore">Vis flere resultater fra denne side</a> </span>
<%} %>
<div>
	<p>
		<em>
			<% if (Model.Item.Type == SearchResType.Answer)
	  { %>
			<%--Output answering minister--%>
			<%= question.Askee.Name%>
			<% }
	  else if (question.Type == QuestionType.Politician)
	  { %>
			<%--It's a question asked by a politician--%>
			<%= question.AskerPol.Name%>
			<%}
	  else
	  {%>
			<%--Must have been asked by a user--%>
			<%= question.AskerUser.Username%>
			<%} %>
			:</em>
		<%= ViewConstants.GetHighlighted(
	new List<string>() { Model.Item.Type == SearchResType.Question ? question.QuestionFT : question.AnswerFT },  Model.Query != null ? Model.Query.Split(' ') : null, 400)%>
	</p>
</div>
<% break;
		   }%>
<% default: throw new ArgumentException(Model.Item.GetType().Name); %>
<%
	} %>
