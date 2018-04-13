<%@ Language=VBScript %>
<!--#INCLUDE VIRTUAL = "/cheminv/api/apiUtils.asp"-->
<!--#INCLUDE VIRTUAL = "/cheminv/gui/guiUtils.asp"-->
<!--#INCLUDE VIRTUAL = "/cfserverasp/source/ado.inc"-->
<!--#INCLUDE VIRTUAL = "/cfserverasp/source/xml_source/RS2HTML.asp"-->
<%
Dim Conn
Dim Cmd
Dim RS
Dim rsPlate

if Session("viewWellPlateFilter") = "" then
	viewWellPlateFilter = "wellformat"
else
	viewWellPlateFilter = Session("viewWellPlateFilter")
end if

dbkey = "ChemInv"
plateID = Request("PlateID")
compoundID = Request("CompoundID")
refresh = Request("refresh")
if refresh = "True" then
	Session("sPlateTab") = ""
end if
Call GetInvConnection()
SelectWell = Session("SelectWell")
if lcase(SelectWell) = "undefined" then Session("SelectWell") = 0
%>	
<!--#INCLUDE VIRTUAL = "/cheminv/gui/GetPlateAttributes.asp"-->
<html>
<head>
<style>
		A {Text-Decoration: none;}
		.TabView {color:#000000; font-size:8pt; font-family: verdana}
		A.TabView:LINK {Text-Decoration: none; color:#000000; font-size:8pt; font-family: verdana}
		A.TabView:VISITED {Text-Decoration: none; color:#000000; font-size:8pt; font-family: verdana}
		A.TabView:HOVER {Text-Decoration: underline; color:#4682b4; font-size:8pt; font-family: verdana}
</style>
<SCRIPT LANGUAGE="JavaScript" SRC="/cheminv/utils.js"></SCRIPT>
<script LANGUAGE="javascript" src="/cheminv/Choosecss.js"></script>
<script LANGUAGE="javascript" src="/cheminv/gui/refreshGUI.js"></script>
<script language="JavaScript">
<!--Hide JavaScript
   // Posts the form when a tab is clicked
   function postDataFunction(sTab) {
	//document.form1.action = "ViewPlate.asp?TB=" + sTab
	document.form1.action = "ViewPlateFrame.asp?TB=" + sTab + "&PlateID=<%=plateID%>&WellCriterion=<%=wellCriterion%>&GetData=db"
	document.form1.submit()
	}
//-->
</script>
</head>
<body>
<table border="0" cellspacing="0" cellpadding="2" width="100%" align="left">
	<tr>
		<td align="right" valign="top" nowrap>
			<%if Application("UseCustomTabFrameLinks") then%>
				<!--#INCLUDE VIRTUAL = "/cheminv/custom/gui/plate_tab_frame_links.asp"-->
			<%else%>
				<!--#INCLUDE VIRTUAL = "/cheminv/gui/plate_tab_frame_links.asp"-->
			<%end if%>
		</td>
	</tr>
</table>
<BR clear=all>	
<!--#INCLUDE VIRTUAL = "/cheminv/gui/PlateViewTabs.asp"-->
<script language="javascript">
//alert(parent.parent.TreeFrame);
//alert(parent.parent.parent.TreeFrame);
//alert(parent.parent.parent.parent.TreeFrame);
//alert(parent);
//alert(parent.parent);
//alert(parent.name);
//alert(parent.parent.name);
//alert(parent.parent.parent);
//alert(parent.parent.parent.parent);
</script>
<form name="form1" action="echo.asp" xaction="NewLocation_action.asp" method="POST">
<input type="hidden" name="PlateID" value="<%=plateID%>">
<%
sPlateTab = Session("sPlateTab")
Select Case sPlateTab
	Case "Summary"

		'Response.Write SQL
		Set oTemplate = Server.CreateObject("MSXML2.FreeThreadedDOMDocument")
		oTemplate.load(Server.MapPath("/" & Application("AppKey") & "/config/xml_templates/ViewPlate_Summary.xml"))
		Set mainTable = oTemplate.selectSingleNode("/DOCUMENT/DISPLAY/TABLE_ELEMENT")
		'Response.Write mainTable.xml & "=xml<BR>"
		'Set newNode = oTemplate.createNode(1, "FIELD", "")

		For each key in custom_plate_fields_dict
			Set newNode = CreateFieldNode(oTemplate, ucase(key), "GrayedText", null, null, null, custom_plate_fields_dict.Item(key) & ":", "RightAlign", 1, null, null, 1, "#" & ucase(key) & "#")
			Set currNode = mainTable.insertBefore(newNode,null)
		Next 
	

		'Set currNode = mainTable.insertBefore(newNode,null)

		HTML = RS2HTML(RS,oTemplate,null,null,null,null,null,null,null)
		'create parent plate links
		if len(Session("plParent_Plate_ID_FK")) > 0  then
			arrParentPlateID = split(Session("plParent_Plate_ID_FK"),",")
			arrParentBarcode = split(Session("plParent_Plate_Barcode"),",")
			arrParentLocationID = split(Session("plParent_Plate_Location_ID"),",")
			for i = 0 to ubound(arrParentPlateID)
				parentLinks = parentLinks & "<span id=""Parent Plate:"" title=""""><A CLASS=""MenuLink"" HREF=""#"" TITLE=""Parent Plate"" ONCLICK=""SelectLocationNode(0," & arrParentLocationID(i) & ", 0, '" & TreeViewOpenNodes1 & "'," & arrParentPlateID(i) & ",1);"" onmouseover=""javascript:this.style.cursor='hand';"" onmouseout=""javascript:this.style.cursor='default';"">" & arrParentBarcode(i) & "</a></span>&nbsp;<BR>"
			next
		end if
		Response.Write "<table cellpadding=""0"" cellspacing=""0"" border=""0""><tr>"
		Response.Write "<td>"
		Response.Write replace(HTML,"PARENTPLATEREPLACEMENT",parentLinks)
		Response.Write "</td>"
		Response.Write "<td valign=""bottom""><a CLASS=""MenuLink""  HREF=""#"" TITLE=""Plate Lineage"" ONCLICK=""OpenDialog('/cheminv/gui/LineageTree.asp?refresh=1&assetType=plate&selectedID=" & plateID & "', 'LineageDiag', 5);"">Plate Lineage</a></td>"
		Response.Write "</tr></table>"		
	Case "PlateViewer"
		if Application("RegServerName") <> "NULL" then
			displayFields = "WellFormat,Name,CAS,MW,RegBatchID,Weight_String,Concentration_String,Solvent"
		else
			displayFields = "WellFormat,Name,CAS,MW,Weight_String,Concentration_String,Solvent"
		End if
%>
<!--#INCLUDE VIRTUAL = "/cheminv/gui/writePlateXMLIsland.asp"-->
<div id="plateViewer" style="POSITION:Absolute;top:60;left:10;visibility:visible;z-index=1">
<table style="font-size:7pt; font-family: Lucida Console; table-layout:fixed; border-collapse: collapse;" cellspacing="0" cellpadding="1" bordercolor="#666666" id="tbl" DATASRC DATAFLD="name" border="1">
	<col width="30">
	<%
		For i=0 to NumCols-1
			Response.Write "<col width=""" & cellWidth & """>"
		Next
	%>
	<thead>
		<th align="center">
			<a href="#" onclick="document.all.hiddenSelector.style.visibility = 'visible';document.all.cboField.click()" title="Click to select displayed value"><img SRC="../graphics/desc_arrow.gif" border="0" WIDTH="12" HEIGHT="6"></a>
			<a id="hiddenWellSelector" target="wellJSFrame"></a>
			<div id="hiddenSelector" style="POSITION:Absolute;top:0;left:0;visibility:hidden;z-index=2">
			<select ID="cboField" size="7">	
				<option VALUE></option>
				<option VALUE="wellformat">Well Format</option>
				<option VALUE="name">Cell Name</option>
				<option VALUE="cas">CAS Number</option>
				<option VALUE="mw">MW</option>
				<%if Application("RegServerName") <> "NULL" then%>
					<option VALUE="regbatchid">Reg Batch ID</option>
				<%End if%>
				<option VALUE="weight_string">Weight</option>
				<option VALUE="Concentration_String">Concentration</option>
				<option VALUE="Solvent">Solvent</option>
			</select>
			</div>
		</th>
	<%
		For i=0 to NumCols-1
			Response.Write "<th>" & colName_arr(0,i) & "</th>" & vblf
		Next
	%>
	</thead>
	<tr height="20">
		<th><span DATAFLD="rowname"></span></th>
		<%
		
		For i=1 to NumCols
			Response.Write "<td align=center><div DATAFORMATAS=html DATAFLD=""col" & i &"""></div></td>" & vblf
		Next
		%>
	</tr>
</table>
</div>
<script LANGUAGE="javascript">
//hide the wait gif

document.all.waitGIF.style.display = "none";
//open the selected well if there is one
//SelectWell = top.ListFrame.document.all.SelectWell.value;
SelectWell = "<%=Session("SelectWell")%>";

if (SelectWell != "0") {
	var hiddenWellSelector = document.anchors("hiddenWellSelector");
	hiddenWellSelector.href =	"ViewWellFrame.asp?wellID=" + SelectWell + "&filter=" + '<%=viewWellPlateFilter%>'; 	
	//alert(hiddenWellSelector.href);
	hiddenWellSelector.click();
}
function viewWell(well_id){
	alert(well_id);
}
document.all.cboField.options[1].selected = true;

tbl.dataFld = "<%=viewWellPlateFilter%>";
//tbl.dataFld = document.all.cboField.options(document.all.cboField.selectedIndex).value;
tbl.dataSrc = "#xmlDoc"; 

</script>


<script FOR="cboField" EVENT="onchange">
  
  tbl.dataSrc = ""; // unbind the table

  // set the binding to the requested field
  tbl.dataFld = this.options(this.selectedIndex).value;

  tbl.dataSrc = "#xmlDoc"; // rebind the table
  document.all.hiddenSelector.style.visibility = 'hidden';
  wellFilter = tbl.dataFld;
  //WellFormat,Name,CAS,MW,RegBatchID,Supplier_Compound_ID,Weight_String,Concentration_String,Solvent
  
</script>

<!--<br><br><table>	<tr>		<td>			<table DATASRC="#xmlDoc" DATAFLD="customer" style="table-layout:fixed" BORDER>  <col width="150">  				<col width="150">  				<thead>					<th>NAME</th>					<th>ID</th>				</thead>  				<tr>    					<td><span DATAFLD="name"></span></td>    					<td><span DATAFLD="custID"></span></td>  				</tr>			</table>		</td>		<td>			<table DATASRC="#xmlDoc" DATAFLD="item" style="table-layout:fixed" BORDER>  				<col width="150"><col width="150">  				<thead>					<th>ITEM</th>					<th>PRICE</th>				</thead>  				<tr>    					<td><span DATAFLD="name"></span></td>    					<td><span DATAFLD="price"></span></td>  				</tr>			</table>		</td>	</tr></table>-->
<!--#INCLUDE VIRTUAL = "/cheminv/custom/gui/custom_plate_tab_cases.asp"-->
<%end select%>
	</form>
</body>
</html>