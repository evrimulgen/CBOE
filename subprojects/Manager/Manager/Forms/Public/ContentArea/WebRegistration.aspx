﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebRegistration.aspx.cs" Inherits="Manager.Forms.Public.ContentArea.WebRegistration" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script type="text/javascript">
        var isChrome = !!window.chrome && !!window.chrome.webstore;
        if (!isChrome) {
            alert("We usually recommended to use Google Chrome browser for accessing WebRegistration.");
            window.location.href = "/Registration.Server";
        } else {
            window.location.href = "/Registration.Server";
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
    </div>
    </form>
</body>
</html>
