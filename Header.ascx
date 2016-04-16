<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Header.ascx.cs" Inherits="istracker_asp.net.Header" %>
<asp:HyperLink ID="TorrentLink" runat="server" NavigateUrl="~/torrent.aspx">Torrens</asp:HyperLink>
<asp:HyperLink ID="UploadLink" runat="server" NavigateUrl="upload.aspx">Upload</asp:HyperLink>
<% if (Session["login"] != null) {  %>
<asp:HyperLink ID="LoginLink" runat="server" NavigateUrl="logout.aspx">Logout</asp:HyperLink>
<% } else { %>
<asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="login.aspx">Login</asp:HyperLink>
<%} %>
<br />
<br />