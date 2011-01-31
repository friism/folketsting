<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<P20QuestionIndexViewModel>" %>

<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	§20 spørgsmål | Folkets Ting
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<div class="span-24">
		<h1>
			§20 spørgsmål</h1>
	</div>
	<div class="span-24 intro">
		<p>
			<strong>§20 spørgsmål kan stilles af Folketingets medlemmer til regeringens ministre
				som beskrevet i §20 i forretningsordenen. På Folkets Ting kan du også stille spørgsmål
				som kan blive debatteret og måske taget op af et folketingsmedlem. Klik på et spørgsmål
				for at komme til dets oversigtsside. Her kan du debattere spørgsmålet.</strong>
		</p>
	</div>
	<div class="span-24 intro">
		<h3>
			<%= Html.ActionLink<P20QuestionController>(_ => _.New(), "Foreslå §20 spørgsmål")%></h3>
	</div>
	<div class="span-24 topmargin">
		<div class="span-24">
			<div class="span-8">
				<h3>
					Senest stillet af Tinget</h3>
				<% Html.RenderPartial("~/Views/Shared/Controls/P20QList.ascx",
			new P20QListViewModel { Questions = Model.LatestByParliament, Mode = P20QListViewModel.RenderMode.TitleOnly }); %>
			</div>
			<div class="span-8 last">
				<h3>
					Senest besvaret</h3>
				<% Html.RenderPartial("~/Views/Shared/Controls/P20QList.ascx",
			new P20QListViewModel { Questions = Model.LatestAnsweredByParliament, Mode = P20QListViewModel.RenderMode.TitleOnly }
			  ); %>
			</div>
			<div class="span-8 last">
				<h3>
					Mest debaterede spørgsmål</h3>
				<% Html.RenderPartial("~/Views/Shared/Controls/P20QList.ascx",
		new P20QListViewModel { Questions = Model.Debated, Mode = P20QListViewModel.RenderMode.TitleOnly }
		   ); %>
			</div>
		</div>
		<div class="span-24">
			<div class="span-8">
				<h3>
					Senest stillet af Folket</h3>
				<% Html.RenderPartial("~/Views/Shared/Controls/P20QList.ascx",
			new P20QListViewModel { Questions = Model.LatestByPeople, Mode = P20QListViewModel.RenderMode.TitleOnly }
			   ); %>
			</div>
			<div class="span-8">
				<h3>
					Populære stillet af Folket</h3>
				<% Html.RenderPartial("~/Views/Shared/Controls/P20QList.ascx",
			new P20QListViewModel { Questions = Model.PopularByPeople, Mode = P20QListViewModel.RenderMode.TitleOnly }
			   ); %>
			</div>
			<div class="span-8">
				&nbsp;
			</div>
		</div>
	</div>
	<div class="span-24">
		<p>
			Se
			<%= Html.ActionLink("alle spørgsmål", "All", new {
	})%></p>
	</div>
</asp:Content>
