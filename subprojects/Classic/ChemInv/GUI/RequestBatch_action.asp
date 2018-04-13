<%@ Language=VBScript %>
<!--#INCLUDE VIRTUAL = "/cheminv/gui/guiUtils.asp"-->
<%
'Prevent page from being cached
Response.ExpiresAbsolute = Now()

Dim httpResponse
Dim FormData
Dim ServerName

action = Lcase(Request("action"))
subaction = lCase(Request("subaction"))
ServerName = Request.ServerVariables("Server_Name")
Credentials = "&CSUserName=" & Server.URLEncode(Session("UserName" & "cheminv")) & "&CSUSerID=" & Server.URLEncode(Session("UserID" & "cheminv"))
FormData = Request.Form & Credentials

'Response.Write(replace(FormData,"&","<br />"))
'Response.End

if Session("fileName")<>"" then 
    FormData = FormData & "&FName=" & Server.URLEncode(Session("fileName"))
    FormData = FormData & "&FileFullPath=" & Server.URLEncode(Session("fileServerPath"))
    Session("fileName")=""
    Session("fileServerPath")="" 
end if 

RequestType = Request("RequestType")
if RequestType = "reservation" then
	RequestText = "Reservation"
	RequestTextAction = "Reserve"
	RequestTextPast = "Reserved"
else
	RequestText = "Request"
	RequestTextAction = "Request"
	RequestTextPast = "Requested"
end if


Select Case action
	Case "edit"
		APIURL = "/cheminv/api/UpdateBatchRequest.asp"
		if subaction = "convert" or subaction = "partial" then
			message = RequestText & " has been processed."
		else
			message = RequestText & " has been edited."
		end if
		if subaction = "partial" then 'and (Request("PartialQtyRequired") <>  Request("QtyRequired")) then

			'-- Update existing RESERVATION
			'-- ---------------------------
			FormData = "cLocationID=" & Request("cLocationID")
			FormData = FormData & "&RequestTypeID=" & Request("RequestTypeID")
			FormData = FormData & "&RequestStatusID=9"
			FormData = FormData & "&RequestID=" & Request("RequestID")
			FormData = FormData & "&OrgUnitID=" & Request("OrgUnitID")
			FormData = FormData & "&BatchID=" & Request("BatchID")
			FormData = FormData & "&LocationID=" & Request("LocationID")
			FormData = FormData & "&lpLocationBarCode=" & Request("lpLocationBarCode")
			FormData = FormData & "&DateRequired=" & Request("DateRequired")
			FormData = FormData & "&QtyRequired=" & Request("QtyRequired")
			FormData = FormData & "&FIELD_1=" & Request("FIELD_1")
			FormData = FormData & "&FIELD_2=" & Request("FIELD_2")
			FormData = FormData & "&SpecialInstructions=" & Request("specialInstructions")
			FormData = FormData & "&ProofApprovalFilename=" & Request("ProofApprovalFilename")
			FormData = FormData & "&ProofApprovalFiletype=" & Request("ProofApprovalFiletype")
			FormData = FormData & "&ProofApprovalFilesize=" & Request("ProofApprovalFilesize")
			FormData = FormData & Credentials
			APIURL = "/cheminv/api/UpdateBatchRequest.asp"
			httpResponse = CShttpRequest2("POST", ServerName, APIURL, "ChemInv", FormData)
			'Response.Write("<br /><br />" & replace(FormData,"&","<br>"))
			'Response.Write("###" & httpResponse & "###")

			'-- Create new REQUEST
			'-- ------------------
			APIURL = "/cheminv/api/CreateBatchRequest.asp"
			FormData = "cLocationID=" & Request("cLocationID")
			FormData = FormData & "&RequestTypeID=" & Request("RequestTypeID")
			FormData = FormData & "&RequestStatusID=2"
			FormData = FormData & "&UserID=" & Request("UserID")
			FormData = FormData & "&BatchID=" & Request("BatchID")
			FormData = FormData & "&LocationID=" & Request("LocationID")
			FormData = FormData & "&lpLocationBarCode=" & Request("lpLocationBarCode")
			FormData = FormData & "&DateRequired=" & Request("DateRequired")
			FormData = FormData & "&QtyRequired=" & Request("PartialQtyRequired")
			FormData = FormData & "&FIELD_1=" & Request("FIELD_1")
			FormData = FormData & "&FIELD_2=" & Request("FIELD_2")
			FormData = FormData & "&ShipToName=" & Request("ShipToName")
			FormData = FormData & "&RequiredUOM=" & Request("RequiredUOM")
			FormData = FormData & Credentials
			APIURL = "/cheminv/api/CreateBatchRequest.asp"
			httpResponse = CShttpRequest2("POST", ServerName, APIURL, "ChemInv", FormData)
			'Response.Write("<br /><br />" & replace(FormData,"&","<br>"))
			'Response.Write("###" & httpResponse & "###")

		else

			APIURL = "/cheminv/api/UpdateBatchRequest.asp"
			httpResponse = CShttpRequest2("POST", ServerName, APIURL, "ChemInv", FormData)
		end if
	Case "create"
		APIURL = "/cheminv/api/CreateBatchRequest.asp"
		message = RequestText & " has been processed."
		httpResponse = CShttpRequest2("POST", ServerName, APIURL, "ChemInv", FormData)
	Case "cancel"
		APIURL = "/cheminv/api/CancelRequest.asp"		
		message = RequestText & " has been canceled."
		httpResponse = CShttpRequest2("POST", ServerName, APIURL, "ChemInv", FormData)
End Select



%>
<html>
<head>
<title><%=Application("appTitle")%> -- <%=RequestText%> an Inventory Batch</title>
<SCRIPT LANGUAGE=javascript src="/cheminv/Choosecss.js"></SCRIPT>
<SCRIPT LANGUAGE=javascript src="/cheminv/gui/refreshGUI.js"></SCRIPT>
<script language="JavaScript">
<!--Hide JavaScript
	window.focus();
	var openNodes = "<%=Session("TreeViewOpenNodes" & TreeID)%>";
//-->
function ReloadOpener()
{
    opener.DialogWindow=null;   
    if(opener.document.form1.FormStep){    
        if(opener.document.form1.FormStep.value == 2){
            opener.document.form1.FormStep.value = 1; 
            opener.document.all.form1.action='#';
            opener.document.form1.submit();
            }
        else{
             opener.location.reload();
        }
    }
    else{
         opener.location.reload();
    }
}
</script>
</head>
<body>
<br><br><br><br><br><br>
<table align="center" border="0" cellpadding="0" cellspacing="0" bgcolor="#ffffff">
	<tr>
		<td height="50" valign="middle" nowrap>
			<%
			If IsNumeric(httpresponse) then 
				If Clng(httpResponse) > 0 then
					'-- Do not default to Requests tab for batches
					'Session("sTab") = "Requests"
					LocationName = Replace(Session("CurrentLocationName"), "\", "\\")
					Response.Write "<center><SPAN class=""GuiFeedback"">" & message & "</SPAN><br /><br />"
					Response.write "Please note your RequestID: " & httpResponse & "</center>"
					Response.Write "<P><center><a HREF=""Ok"" onclick=""if (opener){ReloadOpener();} window.close(); return false;""><img SRC=""/cheminv/graphics/sq_btn/ok_dialog_btn.gif"" border=""0"" title=""Close dialog window""></a></center>"		
				else				
					Response.Write "<center><P><CODE>" & Application(httpResponse) & "</CODE></P></center>"
					Response.Write "<center><SPAN class=""GuiFeedback"">" & RequestText & " could not be processed</SPAN></center>"
					Response.Write "<P><center><a HREF=""Ok"" onclick=""if (opener) {opener.DialogWindow=null; opener.focus();} window.close(); return false;""><img SRC=""/cheminv/graphics/sq_btn/ok_dialog_btn.gif"" border=""0"" title=""Close dialog window""></a></center>"		
				End if
			Else
				Response.Write FormatApiError(APIURL, httpresponse)
				Response.end
			End if
			
			Function FormatApiError(APIURL, ErrMsg)
				FormatApiError = "<center><table width=""80%""><tr><th nowrap valign=""top"">API Error at:</th><td>" & APIURL & "</td></tr><tr><th nowrap valign=top>Oracle Error:</th><td>" & ErrMsg & "</td></tr></table></center>"
			End function
			%>
		</td>
	</tr>
</table>
</body>