<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="login.aspx.cs" Inherits="istracker_asp.net.login" %>
<%@ Register src="~/Header.ascx" tagname="Header" tagprefix="abc" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        .auto-style3 {
            height: 26px;
        }
        .auto-style4 {
            height: 23px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div id="header">
        <abc:Header ID="abcHeader" runat="server" />
    </div>
    <div>
        <table align="center" width="400" border="1" >
            <tr>
                <td colspan="2">Torrent login</td>
            </tr>
            <tr>
                <td colspan="2">Please enter password</td>
            </tr>
            <tr>
                <td width="200px" class="auto-style3"><asp:Label ID="Label1" runat="server" Text="User:"></asp:Label></td>
                <td class="auto-style3"><asp:TextBox ID="userName" runat="server" Width="180px"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="userName" ErrorMessage="usernae is missing" Display="None"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td width="200px"><asp:Label ID="Label2" runat="server" Text="Passowrd:"></asp:Label></td>
                <td><asp:TextBox ID="password" type="password" runat="server" Width="180px"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="password" ErrorMessage="password is missing" Display="None"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td colspan="2"><asp:Label ID="Error" runat="server" Text=""/><asp:ValidationSummary ID="ValidationSummary1" runat="server" /></td>
            </tr>
            <tr>
                <td colspan="2" align="right">
                    <asp:Button ID="Button2" runat="server" Text="Register" OnClick="Button2_Click" />
                    <asp:Button ID="Button1" runat="server" Text="Login" OnClick="Button1_Click" /></td>
            </tr>
        
        </table>
    
    </div>
    </form>
</body>
</html>