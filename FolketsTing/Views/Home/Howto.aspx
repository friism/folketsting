<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<FrontpageViewModel>" %>

<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Hvordan du kommer i gang | Folkets Ting
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<div class="span-24">
		<h1>
			Hvordan du kommer i gang</h1>
		<h3>
			Video</h3>
	</div>
	<div class="span-24">
		Herunder er en video der viser hvordan man bruger Folkets Ting. Der er lyd på, men
		lyd er ikke strengt nødvendig for at følge med.
	</div>
	<div class="span-24">
		<object classid='clsid:d27cdb6e-ae6d-11cf-96b8-444553540000' codebase='http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=9,0,115,0'
			width='950' height='585'>
			<param name='movie' value='http://screenr.com/Content/assets/screenr_0817090731.swf' />
			<param name='flashvars' value='i=4873' />
			<param name='allowFullScreen' value='true' />
			<embed src='http://screenr.com/Content/assets/screenr_0817090731.swf' flashvars='i=4873'
				allowfullscreen='true' width='950' height='585' pluginspage='http://www.macromedia.com/go/getflashplayer'></embed></object>
		<%--	<script type="text/javascript" src="<%= ResolveUrl("~/Scripts/swfobject.js") %>"></script>
	<script type="text/javascript">
		swfobject.registerObject("movie1", "9.0.0", "<%= ResolveUrl("~/Scripts/expressInstall.swf") %>");
    </script>
	<object id="movie1" classid="clsid:D27CDB6E-AE6D-11cf-96B8-444553540000" width="973" height="600">
        <param name="movie" value="http://www.itu.dk/~friism/files/ft_intro.swf" />
        <!--[if !IE]>-->
        <object type="application/x-shockwave-flash" 
			data="http://www.itu.dk/~friism/files/ft_intro.swf" width="967" height="758">
        <!--<![endif]-->
          <p>Alternative content</p>
        <!--[if !IE]>-->
        </object>
        <!--<![endif]-->
      </object>
--%>
	</div>
</asp:Content>
