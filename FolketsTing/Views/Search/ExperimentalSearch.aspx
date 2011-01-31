<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<ExperimentalSearchViewModel>" %>

<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FT.Search" %>
<%@ Import Namespace="FT.Search.Helpers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Eksperimentiel Søgning | Folkets Ting
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="span-24">
        <script type="text/javascript">
        	    google.load("jqueryui", "1.7.2");
        </script>
        <div class="span-24 searchbox">
                <% using (Html.BeginForm<SearchController>(_ => _.ExperimentalSearch(null), FormMethod.Get, new { id = "advanced_search_page_form" })) %>
                <% {  %>
                    <%= Html.TextBox("query", Model.Results.Search.FreeSearch)%>
                <%} %>
                <% if (Model.Results.Search.Facets.Count() > 0) %>
                <% { %>
                    <div id="selectedFilters">
                            <ul>
                            <% foreach (var f in Model.Results.Search.Facets)
                               { %>
                                    <a class="removeFacet" href="<%= Url.RemoveFacet(f.Key) %>"><li><%= f.Value%><span>x</span> </li></a>
                            <% } %>
                            </ul>
                    </div>
                <% } %>
            </div>
        </div>
        <div class="span-16 border" id="results_box">
            <div id="result_stats" class="span-16 border">
            <p>Viser <%= Model.Results.PaginationInfo.FirstItemIndex %> - <%= Model.Results.PaginationInfo.LastItemIndex%> resultater af
                 <b><%= Model.Results.PaginationInfo.TotalItemCount %> </b>
                
                 (<%= TimeSpan.FromMilliseconds(Model.Results.QueryTime).TotalSeconds %> s)
            </p>
            </div>
            
            <div class="span-16">
                <% if (Model.Results.DidYouMean != "") %>
                <% { %>
                    <p>Mente du <em>
                        <a href="<%= Url.SetParameter("query", Model.Results.DidYouMean) %>"> 
                                <%= Model.Results.DidYouMean %>
                        </a></em>?
                    </p>
                <% } %>
                 <% foreach (var ss in Model.Results.SuperSearchables)
                    { %>
                    <div></div>
                        <br />
                        <% Html.RenderPartial("controls/SuperSearchable", ss); %>
                        <% if (ss.CollapseCount != 0)
                           { %>
                            <p><b><%= ss.CollapseCount%></b> <%= ss.CollapseCount == 1 ? "mere resultat" : "flere resultater" %> i denne lov.</p>
                        <% } %>
                        <br />
                    <%}%>
                    <div class="span-16">
                        <% Html.RenderPartial("controls/Pagination", Model.Results.PaginationInfo); %>
                        <% Html.Repeat(new[] { 10, 30, 50, 100 }, ps => { %>
                            <% if (ps == Model.Results.Search.PageSize) { %>
                            <span><%= ps%></span>
                            <% } else { %>
                            <a href="<%= Url.SetParameters(new {pagesize = ps, page = 1}) %>"><%= ps%></a>
                            <% } %>
                        <% }, () => { %> | <% }); %>
                        resultater pr side
                    </div>
            </div>
        </div>
        <div class="span-8 last">
            <div id="filters">        
            <% foreach (var f in Model.Results.Facets) { %> 
                <label for="Facet"><%= Html.SolrFieldPropName<Searchable>(f.Key).FacetInflector() %></label>
                <ul>
                    <% foreach (var fv in f.Value) { %>
                    <li><a href="<%= Url.SetFacet(f.Key, fv.Key) %>"><%= fv.Key %></a> <span>(<%= fv.Value %>)</span></li>
                    <%} %>
                </ul>
            <% } %>
            </div>
        </div>
    </div>
            	<script type="text/javascript">
            	    google.setOnLoadCallback(function () {
            	        // this hooks up any "show more" links in the result
            	        $('a.showmore').click(function (e) {
            	            e.preventDefault();
            	            var divs = $(this).parents('div.searchresult').find('div.additionalresult');

            	            if ($(this).hasClass('shown')) {
            	                divs.slideUp('fast');
            	                $(this).html('Vis mere');
            	            } else {
            	                divs.slideDown('fast');
            	                $(this).html('Skjul mere');
            	            }
            	            $(this).toggleClass('shown');
            	            $(this).parents('div.searchresult').toggleClass('shown');
            	        });
            	    });
	            </script>
<style>
    #advanced_search_page_form input {
    -moz-background-clip:border;
        -moz-background-inline-policy:continuous;
        -moz-background-origin:padding;
        background:white;
        border:1px solid #bbb;
        color:black;
        font-family:Arial,Helvetica,sans-serif;
        font-size:1.364em;
        font-weight:bold;
        height:1.4em;
        padding:0.267em 0.6em;
        width:28em;
        -moz-border-radius: 3px;
    }
    
    #advanced_search_page_form input:focus 
    {
        background: #E7F8FC;
    }
    
    #advanced_search_page_form input:hover
    {
        background: #E7F8FC;
    }
    
    
    div.searchbox 
    {
        padding-left: 15px;
        padding-top: 5px;
        padding-bottom: 5px;
    }
    #selectedFilters 
    {
        width: 100%;
        float: left;
        margin-bottom: 5px;
        -moz-border-radius: 5px;
    }
    #selectedFilters ul 
    {
        -moz-background-clip:border;
        -moz-background-inline-policy:continuous;
        -moz-background-origin:padding;
        float:left;
        list-style-image:none;
        list-style-position:outside;
        list-style-type:none;
        margin: 0px;
        width: 100%;
    }
    
    #selectedFilters li
    {
        border: 1px solid #ccc;
        height:15px;
        width:auto;
        display: block;
        padding: 7px 10px 11px;
        margin: 3px;
        cursor: pointer;
        font-weight: bold;
        color: Black;
        float: left;
        -moz-border-radius: 3px;
    }
    
    #selectedFilter li a
    {
        outline: none;
        color: yellow;
    }
    
    #selectedFilters li:hover
    {
        background: #EEEEEE;
        color: black;
    }
    
    .removeFacet span
    {
        border: 1px solid #ccc;
        background-color: #eee;
        font-weight: bold;
        margin-left: 5px;
        display: inline-block;
        width: 15px;
        text-align: center;
        -moz-border-radius: 2px;
    }
    
    #selectedFilters li:hover a span
    {
        background-color: White;
    }
    
    #selectedFilters a
    {
        display:block;
        text-decoration: none;
    }
    
    div#result_stats
    {
        height: 20px;
        background-color: #F0F7F9;
        text-align: right;
    }
    
    #result_stats p
    {
        text-align: right;
    }
    
    div#filters
    {
        padding-top: 5px;
        padding-left: 20px;
    }
    
    div#pagination
    {
        text-align:center;
    }
    
    div#results_box
    {
        border-top: 1px solid;
        border-color: #ddd;
    }
    
    </style>
</asp:Content>
