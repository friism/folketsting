<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<FrontpageViewModel>" %>

<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Livedebat fra Folketingssalen | Folkets Ting
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<div class="span-24">
		<h1>
			Livedebat fra Folketingssalen</h1>
	</div>
	<div class="span-3">
		&nbsp;</div>
	<div class="span-18">
		<h3>
			Live-diskuter politikernes taler med dine venner på Facebook</h3>
		<div>
			<embed height="424" align="middle" width="668" pluginspage="http://www.adobe.com/go/getflashplayer"
				wmode="transparent" type="application/x-shockwave-flash" allowfullscreen="true"
				allowscriptaccess="always" loop="false" play="true" name="folketingetPlayer"
				bgcolor="#000000" quality="high" src="http://ft.arkena.tv/flash/folketingetPlayer.swf?popup=true&amp;xml=http%3A%2F%2Fft.arkena.tv%2Fxml%2Fcore_player_clip_data_v2.php%3Fwtve%3D187%26wtvl%3D2%26wtvk%3D012536940751284">
		</div>
	</div>
	<div class="span-3 last">
		&nbsp;</div>
	<div class="span24">
		<div class="span-12">
			<fb:live-stream width="460" height="424" xid="livedebate_folketingssalen">
			</fb:live-stream>
			<script type="text/javascript">
				$(document).ready(function () {
					FB.init("32ea3676df27273d35d2dccf64b22fe5", "/xd_receiver.htm");
				});
			</script>
		</div>
		<div class="span-12 last">
			<div id="twtr-search-widget">
			</div>
			<script src="http://widgets.twimg.com/j/1/widget.js"></script>
			<link href="http://widgets.twimg.com/j/1/widget.css" type="text/css" rel="stylesheet">
			<script>
				new TWTR.Widget({
					search: 'ftlive',
					id: 'twtr-search-widget',
					loop: false,
					title: 'Debat i Folketingsalen (tag med #ftlive)',
					subject: 'Hvad mener du?',
					width: 460,
					height: 424,
					theme: {
						shell: {
							background: '#D3E1F0',
							color: '#ffffff'
						},
						tweets: {
							background: '#ffffff',
							color: '#444444',
							links: '#1985b5'
						}
					}
				}).render().start();
			</script>
		</div>
	</div>
</asp:Content>
