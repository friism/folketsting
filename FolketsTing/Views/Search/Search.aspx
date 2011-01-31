<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<SearchResultViewModel>" %>

<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<%@ Import Namespace="FT.Model" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	<%= !string.IsNullOrEmpty(Model.QueryText) ? "Søgeresultater for '" + Model.QueryText + "'" : "Søgning"%>
	| Folkets Ting
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

	<script type="text/javascript">
		google.load("jqueryui", "1.7.2");
	</script>

	<div class="span-24">
		<% if (Model.Results != null)
	 { %>
		<div class="span-16">
			<h1>
				Søgning efter '<%= Model.QueryText %>'</h1>
			<% 
				// this mumbo-jumbo is to avoid hitting the database twice
				var theresults = Model.Results.Select(_ => _.ToList()).ToList();
				if (theresults.Any())
				{
					foreach (var r in theresults)
					{%>
			<div class="searchresult">
				<%--This is the div holding each complete set--%>
			<% if (r.Count() == 1)
	  { %>
			<%--Just one result--%>
			<div>
				<% Html.RenderPartial("~/Views/Shared/Controls/SearchResult.ascx",
		   new SearchResultItemViewModel() { Query = Model.QueryText, Item = r.First() }
			); %>
			</div>
			<%}
	  else
	  { %>
			<div>
				<% Html.RenderPartial("~/Views/Shared/Controls/SearchResult.ascx",
		   new SearchResultItemViewModel() { Query = Model.QueryText, Item = r.First(), IsFirstOfMany = true }
			); %>
			</div>
			<% foreach (var rr in r.Skip(1))
	  {
			%>
			<div style="display: none;" class="additionalresult">
				<% Html.RenderPartial("~/Views/Shared/Controls/SearchResult.ascx",
		   new SearchResultItemViewModel() { Query = Model.QueryText, Item = rr }
			); %>
			</div>
			<%
				} %>
			<% } %>
		</div>
		<%} //foreach
				} // if
				else
				{ %>
		<p>
			Intet fundet, prøv en anden søgning.</p>
		<%} %>
		</div>
		<% }
	 else
	 { %>
		<div class="span-16 intro">
			<p>
				<strong>Brug boksen til højre til at angive søgekriterier eller boksen øverst til højre
					til hurtig søgning.</strong>
			</p>
		</div>
		<%} %>
		<div class="span-8 last">
			<% using (Html.BeginForm<SearchController>(_ => _.AdvancedSearch(null), FormMethod.Get, new { id = "advancedsearch" }))
	  {  %>
			<fieldset>
				<legend>Søgekriterier</legend>
				<p>
					<label for="Text">
						Tekst</label>
					<br />
					<%= Html.TextBoxFor(_ => _.AdvQuery.Text)%>
				</p>
				<p>
					<label for="From">
						Dato</label>
					<br />
					<%= Html.TextBoxFor(_ => _.AdvQuery.FromString, new { size = 12, @class = "datepicker" })%>
					til
					<%= Html.TextBoxFor(_ => _.AdvQuery.ToString, new { size = 12, @class = "datepicker" })%>
				</p>
				<p>
					<label for="Text">
						Person</label>
					<br />
					<%= Html.TextBoxFor(_ => _.AdvQuery.PolName)%>
				</p>
				<p>
					<label for="Text">
						Type</label>
					<br />
					<%= Html.DropDownListFor(_ => _.AdvQuery.Type, Model.AdvQuery.SelectTypes,
						"Alle", null)%>
				</p>
				<p>
					<%= Html.SubmitButton("","Søg") %>
				</p>
				<p>
					<em>Kun de ti mest relevante resultater vises. Vi arbejder på en forbedring.</em>
				</p>
			</fieldset>
			<%} %>
		</div>
	</div>

	<script type="text/javascript">
		google.setOnLoadCallback(function() {
			// this hooks up any "show more" links in the result
			$('a.showmore').click(function(e) {
				e.preventDefault();
				var divs = $(this).parents('div.searchresult').find('div.additionalresult');

				if ($(this).hasClass('shown')) {
					divs.slideUp('fast');
					$(this).html('Vis flere resultater fra denne side');
				} else {
					divs.slideDown('fast');
					$(this).html('Skjul ekstra resultater');
				}
				$(this).toggleClass('shown');
				$(this).parents('div.searchresult').toggleClass('shown');
			});

			// enable datepickers in the form
			$('input:text.datepicker').datepicker({
				dateFormat: 'dd-mm-yy',
				dayNamesMin: ['Ma', 'Ti', 'On', 'To', 'Fr', 'Lø', 'Sø'],
				monthNames: ['Januar', 'Februar',
					'Marts', 'April', 'Maj', 'Juni',
					'Juli', 'August', 'September', 'Oktober', 
					'November', 'December']
			});
		});
	</script>

</asp:Content>
