<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<FolketsTing.Controllers.PolViewModel>" %>

<%@ Import Namespace="FolketsTing.Views" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	<%= Model.Politician.FullName() %>
	| Folkets Ting
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="HeadContent" runat="server">
	<link id="Link1" rel="alternate" href="<%= ViewData.Model.ActivityFeedLink %>" title="<%= ViewData.Model.Politician.FullName()  %>'s aktivitet"
		type="application/rss+xml" />
	<link rel="stylesheet" type="text/css" href="http://visapi-gadgets.googlecode.com/svn/trunk/termcloud/tc.css" />
	<script type="text/javascript" src="http://visapi-gadgets.googlecode.com/svn/trunk/termcloud/tc.js"></script>

	<meta property="og:title" content="<%= Model.Politician.FullName() %>"/>
	<meta property="og:type" content="politician"/>
	<meta property="og:url" content="<%: Request.Url.AbsoluteUri %>"/>
	<meta property="fb:app_id" content="<%= ConfigurationManager.AppSettings["Facebook.AppId"] %>">
	<meta property="og:site_name" content="folketsting.dk"/>
	<meta property="og:image" content="<%: "http://folketsting.dk" + Url.Action(
						"GetScaledImage", 
						"File", 
						new { imageid = Model.Politician.ImageId, 
							imagename = Model.Politician.FullName().Replace(" ", "").ToUrlFriendly(),
							width = 100,
							height = 100 
						}) %>"/>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<div class="span-24">
		<h1>
			<%= Model.Politician.FullName()  %>
			(<%= Model.Politician.Party.Name %>)</h1>
	</div>
	<div class="span-24 polinfo">
		<% Html.RenderPartial("~/Views/Shared/Controls/PolBox.ascx",
		   new PolBoxViewModel() { Pol = Model.Politician, Title = null }
			); %>
		<div class="span-5">
			Medlem siden
			<%= ViewData.Model.Politician.JoinedString %>
			<br />
			<% if (!string.IsNullOrEmpty(ViewData.Model.Politician.EmailAdd))
			   { %>
			<a href="mailto:<%= ViewData.Model.Politician.EmailAdd %>">
				<%= ViewData.Model.Politician.EmailAdd%></a>
			<br />
			<%} %>
			<% if (!string.IsNullOrEmpty(ViewData.Model.Politician.Homepage))
			   { %>
			<a href="<%= ViewData.Model.Politician.Homepage.ToProperLink() %>">
				<%= ViewData.Model.Politician.Homepage %></a>
			<%} %>
			<fb:like href="<%: Request.Url.AbsoluteUri %>" layout="button_count" width="200"></fb:like>
			<%--<% Html.RenderPartial("Controls/AddThis"); %>--%>
		</div>
		<div class="span-15 last">
			<div class="span-15 last activitychart" id="activity-chart" style="width: 590px;
				height: 200px;">
			</div>
		</div>
	</div>
	<div class="span-12">
		<h3>
			Seneste aktivitet</h3>
		<% Html.RenderPartial("~/Views/Shared/Controls/FeedEventStream.ascx",
			ViewData.Model.Events); %>
	</div>
	<div class="span-12 last">
		<h3>
			Det taler
			<%= Model.Politician.Firstname.Split(' ').First() %>
			om</h3>
		<div class="span-12 word-cloud" id="word-cloud">
		</div>
		<h3>
			Mest debatterede emner</h3>
		<% if (Model.TopDebated.Any())
		   { %>
		<% Html.RenderPartial("~/Views/Shared/Controls/FeedEventStream.ascx",
			Model.TopDebated); %>
		<% }
		   else
		   {%>
		<p>
			<em>(ingen af
				<%= ViewData.Model.Politician.FullName() %>'s taler er debateret endnu, se at komme
				i gang!)</em></p>
		<%} %>
	</div>
	<script type="text/javascript">
		var baseurl = "<%= Request.Url.GetLeftPart(UriPartial.Authority) %>";
		google.load("visualization", "1", { packages: ["annotatedtimeline"] });
		google.setOnLoadCallback(drawChart);
		function drawChart() {
			var therows = <%= ViewData.Model.ActivityRows %>;
			var thecols = <%= ViewData.Model.ActivityCols %>;
			
			var acticityData = new google.visualization.DataTable(
				{cols: thecols, rows : therows }
			);

			var activitychart = new google.visualization.AnnotatedTimeLine(document.getElementById('activity-chart'));
			activitychart.draw(acticityData, { 
				displayZoomButtons: false, 
				displayRangeSelector: false, 
				scaleColumns: [0, 1], 
				scaleType: 'allmaximize',
				dateFormat: 'MMMM',
				numberFormats: '#',
				displayAnnotations: true
			});

			var wordrows = <%= ViewData.Model.WordRows %>;
			var wordcols = <%= ViewData.Model.WordCols %>;

			var worddata = new google.visualization.DataTable(
				{cols: wordcols, rows : wordrows }
			);

			var wordchart = new TermCloud(document.getElementById('word-cloud'));
			wordchart.draw(worddata, null);        
		}
	</script>
</asp:Content>
<%--<img alt="Aktivitet" src="http://chart.apis.google.com/chart?cht=lc&chs=590x200&chd=t:40,60,60,45,47,75,70,72|50,70,70,55,57,85,80,82&chds=0,100,0,100&chxt=x,y,r&chdl=Stemmer|Taler&chco=9E0023,003063" />--%>