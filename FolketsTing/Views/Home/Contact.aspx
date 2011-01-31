<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<FrontpageViewModel>" %>

<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Om | Folkets Ting
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<div class="span-24">
		<h1>
			Om Folkets Ting</h1>
		<p>
			Du kan læse om <a href="http://folketsting.wordpress.com/2009/05/19/folkets-ting-lanceret/">
				hvorfor der er brug for Folkets Ting</a> på bloggen.
		</p>
		<p>
			Hvis du har spørgsmål, kommentarer eller forslag til sitet (<em>ikke</em> til politikerne)
			kan du bruge "Feedback" dimsen der gerne skulle dukke op til venstre på skærmen
			på alle sider. Du er også velkommen til at kontakte Michael Friis. Hans kontaktoplysninger
			kan findes ved at følge dette <a href="http://friism.com/michael-friis">link</a>.
		</p>
		<p>
			Michael fik ideen til Folkets Ting fra britiske og amerikanske ækvivalenter som
			<a href="http://www.publicwhip.org.uk/">The Public Whip</a>, <a href="http://www.theyworkforyou.com/">
				They Work for You</a> og <a href="http://www.opencongress.org/">Open Congress</a>.
			Han programmerede sitet i foråret og sommeren 2009. <a href="http://twitter.com/runesoerensen">
				Rune Sørensen</a> hjalp med at lave Facebook integration og RSS feeds.
		</p>
		<p>
			<%--			<script src="http://static.ak.facebook.com/js/api_lib/v0.4/FeatureLoader.js.php/en_US"
				type="text/javascript"></script>
--%>

			<script type="text/javascript">
				$(document).ready(function() {
					FB.init("28c698725e51a3a7c47476e9993383ec");
				});
			</script>

			<fb:fan profile_id="87656729788" stream="" connections="" width="300">
			</fb:fan>
			<div style="font-size: 8px; padding-left: 10px">
				<a href="http://www.facebook.com/pages/Folkets-Ting/87656729788">Folkets Ting</a>
				på Facebook</div>
		</p>
	</div>
</asp:Content>
