<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="logout.aspx.cs" Inherits="istracker_asp.net.logout" %>
<%@ Register src="~/Header.ascx" tagname="Header" tagprefix="abc" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div id="header">
        <abc:Header ID="abcHeader" runat="server" />
    </div>
    <div>
    
    </div>
        <asp:Label ID="Label1" runat="server" Text="Do you want to logout?" />
        <asp:Button ID="Button1" runat="server" Text="Yes" OnClick="Button1_Click" />
        <asp:Button ID="Button2" runat="server" Text="No" OnClick="Button2_Click" />
    </form>
</body>
</html>
