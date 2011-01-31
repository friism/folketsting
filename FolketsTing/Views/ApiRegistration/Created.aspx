<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<NewApiUserViewModel>" %>

<%@ Import Namespace="FT.DB" %>
<%@ Import Namespace="FolketsTing.Controllers" %>
<%@ Import Namespace="FolketsTing.Views" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Din n�gle er klar
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <% string error = "*"; %>
    <div class="span-24">
        <h1>
            Din n�gle er klar</h1>
    </div>
<%--    <div class="span-24 intro">
        <p>
            <strong>Du skal have en n�gle for at bruge Folkets Tings API. Du kan f� en n�gle straks
                ved at udfylde formularen herunder.</strong>
        </p>
    </div>--%>
    <div class="span-24 topmargin">
        <p>
            Her er din n�gle, skriv den ned og gem den: <strong><%= Model.ApiUser.ApiKey %></strong>
        </p>
    </div>
</asp:Content>
