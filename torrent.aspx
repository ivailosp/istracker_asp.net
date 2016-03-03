<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="torrent.aspx.cs" Inherits="istracker_asp.net.torrent" %>
<%@ Register src="~/Header.ascx" tagname="Header" tagprefix="abc" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="torrent" runat="server">
    <div id="header">
        <abc:Header ID="abcHeader" runat="server" />
    </div> 
    <div>
        <asp:Label ID="TopLabel" runat="server" EnableViewState="False"></asp:Label>
        <asp:DataGrid ID="TorrentList" runat="server" EnableViewState="False">
        </asp:DataGrid>
    
    </div>
        <asp:Label ID="BotLabel" runat="server" EnableViewState="False"></asp:Label>
    </form>
</body>
</html>
