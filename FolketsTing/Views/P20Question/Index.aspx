<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<P20QuestionIndexViewModel>" %>

<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	�20 sp�rgsm�l | Folkets Ting
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<div class="span-24">
		<h1>
			�20 sp�rgsm�l</h1>
	</div>
	<div class="span-24 intro">
		<p>
			<strong>�20 sp�rgsm�l kan stilles af Folketingets medlemmer til regeringens ministre
				som beskrevet i �20 i forretningsordenen. P� Folkets Ting kan du ogs� stille sp�rgsm�l
				som kan blive debatteret og m�ske taget op af et folketingsmedlem. Klik p� et sp�rgsm�l
				for at komme til dets oversigtsside. Her kan du debattere sp�rgsm�let.</strong>
		</p>
	</div>
	<div class="span-24 intro">
		<h3>
			<%= Html.ActionLink<P20QuestionController>(_ => _.New(), "Foresl� �20 sp�rgsm�l")%></h3>
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
					Mest debaterede sp�rgsm�l</h3>
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
					Popul�re stillet af Folket</h3>
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
			<%= Html.ActionLink("alle sp�rgsm�l", "All", new {
	})%></p>
	</div>
</asp:Content>
