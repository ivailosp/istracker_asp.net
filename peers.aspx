<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="peers.aspx.cs" Inherits="istracker_asp.net.peers" %>
<%@ Register src="~/Header.ascx" tagname="Header" tagprefix="abc" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="peers" runat="server">
    <div id="header">
        <abc:Header ID="abcHeader" runat="server" />
    </div>
    <div>
        <asp:Label ID="Msg" runat="server" EnableViewState="False"></asp:Label>
        <asp:DataGrid ID="PeerList" runat="server" EnableViewState="False">
        </asp:DataGrid>
    </div>
    </form>
</body>
</html>
