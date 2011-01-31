<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<LawTextViewModel>" %>
<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	<%--TODO: this should include "fremsat" whatever--%>
	Teksten til
	<%= Model.Law.ShortName %>
	<%= Model.StageString.ToLower() %>
	| Folkets Ting
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="NotifyContent" runat="server">
	<% Html.RenderPartial("~/Views/Shared/Controls/CommentNotify.ascx"); %>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<div class="span-24">
		<h1>
			<%= Model.Law.ShortName %>
			- <%= Model.Law.FtId %></h1>
		<h4>
			Lovtekst
			<%= Model.StageString.ToLower() %></h4>
		<div class="span-24 intro">
			<p>
				<strong>Advarsel! Lovteksterne fra Folketingets hjemmeside er meget dårligt formaterede.
					Der kan derfor forekomme fejl, mangler og mærkværdigheder i teksterne vi viser
					her. </strong>
			</p>
		</div>
		<% if (Model.Law.Paragraphs.Where(p => p.Stage == Model.Stage).Count() > 0)
	 { %>
		<% foreach (var p in Model.Law.Paragraphs.
			Where(p => p.Stage == Model.Stage).OrderBy(p => p.Number).ThenBy(p => p.Letter).
	Select(_ => new { _.ParagraphId, _.Number, _.Letter, _.IsChange, _.ChangeText, _.LawChanges, _.Sections }))
	 {
		%>
		<div>
			<div class="span-23">
				<h5 class="center">
                    <a name="Paragraph<%= p.ParagraphId %>"></a>
					§<%= p.Number + p.Letter ?? ""%></h5>
				<% if (p.IsChange.Value)
	   { %>
				<p>
					<%= p.ChangeText %></p>
			</div>
			<% foreach (var c in p.LawChanges.OrderBy(c => c.Number).
		  Select(__ => new { __.SubChanges, __.NoformChangeText, __.LawChangeId, CommentCount = __.CommentCount() }))
	        {
			%>
            
			<div class="span-24 change commentable lawchange <%= c.LawChangeId %>" >
            <a name="LawChange<%= c.LawChangeId %>"></a>
				<div class="span-23 content">
					<p>
						<%= c.NoformChangeText%>
						<% if (!c.SubChanges.Any())
		 { %>
						<span class="commentcontrol" style="display: none;"><a class="js" href="#">
							Kommentarer</a></span>
						<%} %>
					</p>
					<%if (c.SubChanges.Any())
	   {%><ul>
		   <% foreach (var sc in c.SubChanges.OrderBy(_ => _.Number).Take(c.SubChanges.Count - 1))
		{
		   %>
		   <li>
               <a name="SubChange<%= sc.SubchangeId %>"></a>
			   <%= sc.Text%>
		   </li>
		   <%
			   } // foreach subchange %>
		   <li><% SubChange last_subchange = c.SubChanges.OrderBy(_ => _.Number).Last(); %>
               <a name="SubChange<%= last_subchange.SubchangeId%>"></a>
			   <%= last_subchange.Text%>
			   <span class="commentcontrol" style="display: none;"><a class="js" href="#">
				   Kommentarer</a></span> </li>
	   </ul>
					<%  } //any subchanges %>
				</div>
				<div class="span-1 last">
					<% if (c.CommentCount > 0)
		{ %>
					<a class="js" href="#">(<%= c.CommentCount %>)</a>
					<% }
		else
		{ %>
					&nbsp;
					<% } %>
				</div>
			</div>
			<% } // foreach lawchange %>
			<% }// is change paragraph
	   {
		   foreach (var s in p.Sections.OrderBy(_ => _.Number).
			   Select(__ => new { __.Text, __.SectionId, CommentCount = __.CommentCount() }))
		   {
			%>
			<div class="span-24 section commentable section <%= s.SectionId %>" >
                <a name="section<%= s.SectionId %>"></a>
				<div class="span-23 content">
					<p>
						<%= s.Text%>
						<span class="commentcontrol" style="display: none;"><a class="js" href="#">
							Kommentarer</a></span>
					</p>
				</div>
				<div class="span-1 last">
					<% if (s.CommentCount > 0)
		{ %>
					<a class="js" href="#">(<%= s.CommentCount %>)</a>
					<% }
		else
		{ %>
					&nbsp;
					<% } %>
				</div>
			</div>
			<%
				} // foreach section%>
			<%
				} // is not change paragraph %>
		</div>
		<%
			} // foreach paragraph %>
		<%} // any paragraphs
	 else
	 { %>
		<p>
			Folkets Ting har desværre ikke teksten til denne lov. <a href="#">Hvorfor?</a>.
		</p>
		<%} %>
	</div>
	<div id="authdialog" style="display: none;">
		<% Html.RenderPartial("Controls/InlineLoginControl"); %>
		<a id="closedialog" href="#">Luk</a></p>
	</div>

	<script type="text/javascript">
		google.load("jqueryui", "1.7.2");
	</script>

	<script type="text/javascript" src="<%= ResolveUrl("~/Scripts/Comments.js") %>"></script>

	<script type="text/javascript" src="<%= ResolveUrl("~/Scripts/jquery.cookie.js") %>"></script>

	<script type="text/javascript" src="<%= ResolveUrl("~/Scripts/notificationbar.js") %>"></script>

	<script type="text/javascript">
		var isauthed = <%= User.Identity.IsAuthenticated.ToString().ToLower() %>;
		var voteurl = '<%= Url.Action("Vote", "Comment") %>';

		$(document).ready(function() {
			handnotific();
			
			init_effects();

			// this makes the comment links go boom
			$("div.commentable a.js").click(function(e) {
				e.preventDefault();
				// figure out wether this is a lawchange or a section
				var elementtype;
				if($(this).parents().hasClass('lawchange'))
				{
					elementtype = '<%= CommentType.Change %>';
				}
				else if($(this).parents().hasClass('section'))
				{
					elementtype = '<%= CommentType.Section %>';
				}
				else
				{
					alert('unknown elementtype');
				}
				commentable_click(e, $(this),
					'<%= ResolveUrl("~/Graphics/ajax-loader.gif")%>',
					'<%= Url.Action("Comments", "Comments") %>/',
					elementtype,
					'<%= HttpUtility.UrlEncode(Request.RawUrl) %>',
					23
				)
			}
			);
		});
	</script>

</asp:Content>
