<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<TagControlViewModel>" %>
<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<div id="tagdialog" style="display: none;">
	<% if (!Context.User.Identity.IsAuthenticated)
	{ %>
	<% Html.RenderPartial("Controls/InlineLoginControl"); %>
	<%}
	else
	{ %>
	<% using (Html.BeginForm<TagController>(
		  _ => _.AddTags(
				  Model.ElementName,//.ToUrlFriendly(),
				  Model.ElementId,Model.Type, ""),
			  FormMethod.Post,
			  new { id = "tagform" }))
	{ %>
	<fieldset>
		<legend>Tilføj tags</legend>
		<%--<p>--%>
			    <label for="tags">
			<% if (Model.UserTags != null) { %>
				    Dine tags</label>
			    <%= Html.TextBox("tags", string.Join(" ", Model.UserTags.ToArray()), new { @class = "tags"})%>
			<% } %>
			<br />
			<em>Adskil tags med mellemrum. Eksempel: tronfølge larsløkke (ikke lars løkke)</em>
			<br />
			Andre brugere har tagget med:
			<% if (Model.CommonTags.Any())
	  {
		  foreach (var t in Model.CommonTags)
		  {
			%><a class="taglink" href="#"><%= t%></a>
			<%
				}
		}
	  else
	  {
			%><em>(ingen tags)</em><%
									   } %>
			<br />
			Populære tags på Folkets Ting:
			<% if (Model.CommonSiteTags.Any())
	  {
		  foreach (var t in Model.CommonSiteTags)
		  {
			%>
			<a class="taglink" href="#">
				<%= t%></a>
			<%
				}
		}
	  else
	  {
			%>
			<em>(ingen tags)</em>
			<%
				}
			%>
		<%--</p>--%>
	</fieldset>
	<% }
	  } %>
	<a id="savetags" href="#">gem</a> | <a id="closedialog" href="#">afbryd</a>
</div>
<script type="text/javascript">
	google.setOnLoadCallback(function() {
		// get dialog going
		$('#tagdialog').dialog(
				{ autoOpen: false, modal: true, buttons: {}, resizable: false, width: 600 }
			);
		// enable close link
		$('a#closedialog').click(function(e) {
			e.preventDefault();
			$('#tagdialog').dialog('close')
		});

		$('a#edittags').click(function(e) {
			e.preventDefault();
			$('#tagdialog').dialog('open');
		});

		$('a#savetags').click(function(e) {
			e.preventDefault();
			$('#tagform').submit();
		});

		$('a.taglink').click(function(e) {
			e.preventDefault();
			var currBoxVal = $('input#tags').val().toLowerCase();
			var clickedTag = $(this).html().trim();
			if (currBoxVal.indexOf(clickedTag) != -1) {
				// it's already in the box, remove
				if (currBoxVal.trim() === clickedTag) {
					$('input#tags').val('');
				}
				else {
					// look in the middle or end of string
					var regstring = '[\\s]+' + clickedTag + '($|[\\s]+)';
					var reg = new RegExp(regstring, 'g');
					// replace with space
					var newBoxVal = currBoxVal.replace(reg, ' ');

					//also try start of string
					regstring = '(^|[\\s]+)' + clickedTag + '[\\s]+';
					reg = new RegExp(regstring, 'g');
					// replace with empty string
					newBoxVal = newBoxVal.replace(reg, '');

					$('input#tags').val(newBoxVal);
				}
			} else {
				// it's not there, add it
				if (currBoxVal === '')
					$('input#tags').val(clickedTag);
				else
					$('input#tags').val(currBoxVal.trim() + ' ' + clickedTag);
			}
		});

		$('#tags').autocomplete('<%= Url.Action("Find","Tag") %>',
				{
					multiple: true,
					multipleSeparator: " "
				});
	});
</script>