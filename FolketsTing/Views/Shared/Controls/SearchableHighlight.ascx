<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<KeyValuePair<Searchable, IDictionary<string, ICollection<string>>>>" %>
<%@ Import Namespace="FT.Search" %>
<%@ Import Namespace="FolketsTing.Views" %>
<%@ Import Namespace="FT.DB" %>

    <% string link;
       if (Model.Key.EntityType == EntityTypes.Speech)
       {
           link = Url.Action("Deliberation", "Law",
                      new
                      {
                          lawname = Model.Key.Title.ToUrlFriendly(),
                          lawid = Model.Key.CollapseEntityId,
                          deliberationnr = Model.Key.LawStageEnum.UrlValue(),
                      }) + "#par-" + Model.Key.EntityId;
       }
       else
       {
           link = Url.Action("LawText", "Law",
                new
                {
                    lawname = Model.Key.Title.ToUrlFriendly(),
                    lawid = Model.Key.CollapseEntityId,
                    stage = Model.Key.LawStageEnum == LawStage.First ?
                        "fremsat" : (Model.Key.LawStageEnum == LawStage.Second ?
                            "aftersec" : "vedtaget")
                }) + "#" + Model.Key.SolrId;
       } %>
        <a href="<%= link %>">
        <% if (Model.Value.Values.Count() > 0)
    { %>
        <% foreach (var highlight in Model.Value.Values.ToList()) %>
        <% { %>
            <%= highlight.First().ToString().TrimStart(new char[] {' ', ',' })  %>
        <% } %>
    <% } %>
    <% else %>
    <% { %>
        <% if (Model.Key.Content != null) %>
        <% { %>
            <%= Model.Key.Content.Substring(0, Math.Min(Model.Key.Content.Length, 400))%>...
        <% } %>
    <% } %>
    </a>
    <br />
        <% Html.RenderPartial("controls/Searchable", Model.Key); %>
<br /><br />