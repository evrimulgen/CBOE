<%@ LANGUAGE="VBScript" %>
<% 'Copyright 1999-2003 CambridgeSoft Corporation. All rights reserved
'DO NOT EDIT THIS FILE%>

<%   dbkey = "reg"
 session.abandon
    Session("UserName" & dbkey) = ""
    Session("UserValidated" & dbkey) = 0
    Session("UserID" & dbkey) = ""
	'!DGB! 08/12/2002 Global login support  
	If Len(Request.Cookies("CS_SEC_UserName")) > 0 then
		response.redirect "/cs_security/login.asp?ClearCookies=true"
	Else
		response.redirect "/" & Application("AppKey") & "/login.asp"
	End if
%>
<!DOCTYPE HTML PUBLIC "-//IETF//DTD HTML//EN">
<html>

<head>
<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
<meta name="GENERATOR" content="Microsoft FrontPage 3.0">
<title></title>
</head>

<body>
</body>
</html>