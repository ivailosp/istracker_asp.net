<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="show.aspx.cs" Inherits="istracker_asp.net.show" %>
<%@ Register src="~/Header.ascx" tagname="Header" tagprefix="abc" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="show" runat="server">
    <div id="header">
        <abc:Header ID="abcHeader" runat="server" />
    </div>
    <asp:Label ID="NameLabel" runat="server" Text="Name: " EnableViewState="False"></asp:Label>
    <br />
    <asp:Label ID="DateLabel" runat="server" Text="Date: " EnableViewState="False"></asp:Label>
    <br />
    <asp:Label ID="SizeLabel" runat="server" Text="Size: " EnableViewState="False"></asp:Label>
    <br />
    <asp:Label ID="FilesLabel" runat="server" Text="Files:<br>" EnableViewState="False"></asp:Label>
    <br />
    </form>
</body>
</html>
