<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="upload.aspx.cs" Inherits="istracker_asp.net.upload" %>
<%@ Register src="~/Header.ascx" tagname="Header" tagprefix="abc" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>

    <form id="upload" runat="server">
    <div id="header">
        <abc:Header ID="abcHeader" runat="server" />
    </div> 
    <div>
    
        <asp:FileUpload ID="fileToUpload" runat="server" EnableViewState="False" />
        <asp:Button ID="submit" runat="server" OnClick="Button1_Click" Text="Upload" EnableViewState="False" />
    
    </div>
        <p>
            <asp:Label ID="BotLabel" runat="server"></asp:Label>
        </p>
    </form>
</body>
</html>
