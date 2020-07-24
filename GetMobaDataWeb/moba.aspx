<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="moba.aspx.cs" Inherits="GetMobaDataWeb.moba" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no" />
    <title>攻略奧義配置參考</title>
    <link rel="stylesheet" href="https://unpkg.com/purecss@0.6.2/build/pure-min.css"/>
</head>
<body>
<form id="form1" runat="server" class="pure-form">
    <div style="margin:10px">
        <div style="text-align: center; margin: 10px;">
            選擇英雄：<asp:DropDownList ID="ddlHero" runat="server"></asp:DropDownList>&nbsp;<asp:Button ID="btnSearch" runat="server" Text="查詢" OnClick="btnSearch_Click" CssClass="pure-button pure-button-primary" />
        </div>
        <asp:Literal ID="liData" runat="server"></asp:Literal>
    </div>
    <div style="text-align: center;">
        更新日期：2020-07-24
    </div>
</form>
</body>
</html>
