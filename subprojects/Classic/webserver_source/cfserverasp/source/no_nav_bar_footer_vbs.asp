<%' Copyright 1999-2003 CambridgeSoft Corporation. All rights reserved
'DO NOT EDIT THIS FILE%>




<%WriteAppletCode()%>
<form name="nav_variables" method="post" Action="<%=Session("CurrentLocation" & dbkey & formgroup)%>">
<input type = "hidden" name = "RecordRange" Value =  "<%=Session("RecordRange" & dbkey & formgroup)%>">
<input type = "hidden" name = "CurrentRecord" Value =  "<%=Session("RecordRange" & dbkey & formgroup)%>">
<input type = "hidden" name = "AtStart" Value =  "<%=Session("AtStart" & dbkey & formgroup)%>">
<input type = "hidden" name = "AtEnd" Value =  "<%=Session("AtEnd" & dbkey & formgroup)%>">
<input type = "hidden" name = "Base_RSRecordCount" Value =  "<%=Session("Base_RSRecordCount" & dbkey & formgroup)%>">
<input type = "hidden" name = "TotalRecords" Value =  "<%=Session("Base_RSRecordCount" & dbkey & formgroup)%>">
<input type = "hidden" name = "PagingMove" Value =  "<%=Session("PagingMove" & dbkey & formgroup)%>">
<input type = "hidden" name = "CommitType" Value = "<%=commit_type%>">
<input type = "hidden" name = "TableName" Value =  "<%=table_name%>">
<input type = "hidden" name = "UniqueID" Value =  "<%=uniqueid%>">
<input type = "hidden" name = "CurrentIndex" Value =  "<%=currentindex%>">
<input type = "hidden" name = "BaseActualIndex" Value =  "<%=BaseActualIndex%>">

</form>
<script language = "javascript">
window.onload = function(){loadframes()}
	function loadframes(){
		pageLoadTiming()
		loadUserInfoFrame()
		loadNavBarFrame()

		DoAfterOnLoad ? AfterOnLoad():true;
	}


</script> 
