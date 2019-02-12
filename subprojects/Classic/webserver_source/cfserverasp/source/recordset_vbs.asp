<%'Copyright 1999-2003 CambridgeSoft Corporation. All rights reserved
'DO NOT EDIT THIS FILE%>
	<%Dim baserecordcount
	Dim end_index
	Dim current_index
	Dim trial_index
	
	
	
	'!DGB! 10/17/01 added UCASE!
	if UCase(Session("reload_basers" & dbkey & formgroup)) = "TRUE" then
		DoReloadBaseRS dbkey, formgroup
		'since it has been done turn the variable
		'LJB 2/1/2004 turn session variable to false so RS is not constantly reloaded!!!
		Session("reload_basers" & dbkey & formgroup) = "FALSE"
	end if
	
	if isArray(Session("Base_RS" & dbkey & formgroup)) then
		bNumRecords =UBound(Session("Base_RS" & dbkey & formgroup),2) + 1
		Session("Base_RSRecordCount" & dbkey & formgroup)=bNumRecords
	else
		Session("Base_RSRecordCount" & dbkey & formgroup)=0
	end if
		
	Response.Write "<script language=javascript>more_available=""" & strTrueFalse(Session("MoreMolids" & dbkey & formgroup)) & """;"
	Response.Write " totalrecords =""" & bNumRecords & """;" 
	Response.Write " base_records_found = """ & bNumRecords & """;"
	Response.Write "</script>"

	if Not Session("no_gui" & dbkey & formgroup) = True then
		If Session("TooManyHitsCount" & dbkey & formgroup) <> "" then
			bOverLimit = Session("TooManyHitsCount" & dbkey & formgroup) > Application("TooManyHitsMaximumRetrievable")
			Response.Write "</table><form>"
			Response.Write "<font face=arial size=2><BR><BR>"
			Response.write "Your search criteria matched " & Session("TooManyHitsCount" & dbkey & formgroup) & " records."	
			
			if bOverLimit then
				Response.Write "<BR><BR><font color=red>You have exceeded the maximum number of hits set by the administator ("  & Application("TooManyHitsWarningThreshHold") &  ").</font>"
			end if
			Response.Write "<BR><BR>What would you like to do?<BR><BR>"
			Response.Write "<UL>"
			if NOT bOverLimit then
				Response.Write "	<li><a href=""#"">Retrive all</a> matching records." 
			End if
			Response.Write "	<li><a href=""#"" onclick=""getAction('edit_query');return false;"">Refine</a> your search."
			Response.Write "	<li>Retrieve the <a href=""" & dbkey & "_action.asp?formgroup=" & formgroup & "&dataaction=limited_search&dbname=" & dbkey & "&limitType=1"">first " & Application("TooManyHitsWarningThreshHold") & "</a> records."
			if (Application("TooManyHitsMaximumRetrievable") > Application("TooManyHitsWarningThreshHold")) AND (Session("TooManyHitsCount" & dbkey & formgroup) > Application("TooManyHitsMaximumRetrievable"))  then
				Response.Write "	<li>Retrieve the <a href=""" & dbkey & "_action.asp?formgroup=" & formgroup & "&dataaction=limited_search&dbname=" & dbkey & "&limitType=2"">first " & Application("TooManyHitsMaximumRetrievable") & "</a> records."
			End if
			response.write  "</ul>"
			Response.Write "</font>"
		elseif (Session("fEmptyRecordset" & dbkey & formgroup) = True) OR (Session("error" & dbkey & formgroup)= True) then 
			Session("RecordRange" & dbkey & formgroup) = ""
			
			Response.Write "<br /><font color=""red"" style=""padding-left:20px"">" & Session("Message" & dbkey & formgroup) & "</font><br />" 
			'LJB 8_31_2004. Check for errors were the table does not exist. This usually indicates the user does not have privileges and they should be alerted.
			if Session("TablePermissions" & dbkey & formgroup) <> "" then
				Response.write "<br>"
				Response.write Session("TablePermissions" & dbkey & formgroup)
			end if
			
		Else
		on error resume next
		
		totalRecords = Session("Base_RSRecordCount" & dbkey & formgroup)
		if UCase(formmode) = "LIST" then
		if (instr(Application("prefs_formgroups"), formgroup)> 0 OR Application("prefs_formgroups")= "") then
			if not dbkey <> "" then
				dbkey = request("dbname")
			end if
			PageSizeCookie = Session("UserNumListView" & dbkey)
			if PageSizeCookie <> "" then
				Select Case PageSizeCookie
					Case "all_records"
						resultListSize = totalRecords
					Case Else
						resultListSize = CLng(PageSizeCookie)
						if CLng(resultListSize) > CLng(totalRecords) then
							resultListSize = totalRecords
						end if
				end select
				tPageSize = CLng(resultListSize)
			else
				resultListSize =GetFormGroupVal(dbkey, formgroup, kNumListView)
					
				tPageSize = CLng(resultListSize)
			end if
		else
			resultListSize =GetFormGroupVal(dbkey, formgroup, kNumListView)
			tPageSize = CLng(resultListSize)

		end if
		else
			tPageSize = CLng(1)
		end if
		current_index = Session("CurrentRecord" & dbkey & formgroup)
		last_starting_index = Session("LastStartingIndex" & dbkey & formgroup)
		'jhs 
		'dont look for formgroup for biosar
		if lcase(dbkey) = "biosar_browser" then
			PagingMove = Request.Cookies("PagingMove" & dbkey)
		else
			PagingMove = Request.Cookies("PagingMove" & dbkey & formgroup)
		end if	
		
		if PagingMove <> "" then
				Select Case PagingMove
					Case "get_more"
						gotoRec=Request.Cookies("GoToRecord" & dbkey & formgroup)
						current_index =gotoRec + 1
					Case "current_list"
						current_index = last_starting_index
					Case "next_record"
						current_index = last_starting_index + tPageSize
					Case "previous_record"
					
						current_index = last_starting_index - tPageSize
						if current_index < 0 then
							current_index = 1
						end if
					Case "first_record"
						current_index = 1
					Case "last_record"
						remainder = totalRecords Mod tPageSize
						if remainder = 0 then
							current_index = (totalRecords - tPageSize)+1		
						else
							current_index = (totalRecords - remainder)+1
						end if
					Case "goto_record"
						gotoValue = ""
						formchange=Request.QueryString("form_change")
						
						if formchange = "true" then
							gotoValue = Request("indexvalue")
							
							if Not gotoValue > 0 then
								gotoValue = Request.Cookies("GoToRecord" & dbkey & formgroup)
							end if
						else
							gotoValue = Request.Cookies("GoToRecord" & dbkey & formgroup)
						end if
						
						if Not gotoValue <> "" then
							gotoValue = 1
						else
							gotoValue = Clng(gotoValue)
						end if
					
						if not UCase(formmode) = "LIST" then
							current_index = gotoValue
						else 'if list view
							if gotoValue <= tPageSize  then
								current_index = 1
							else
								wholeNumber = gotoValue\tPageSize 'returns integer value
								remainder=gotoValue Mod tPageSize
								if remainder > 0 then
									pagenumber = wholeNumber
								else
									pagenumber = wholeNumber -1
								end if
								current_index = (pagenumber * tPageSize) + 1
							end if
						end if 'list view
					End Select				
			
		Else 
			if Session("override_current_index" & dbkey & formgroup) = true then
				current_index = request("indexvalue")
				Session("override_current_index" & dbkey & formgroup) = false
			else
				current_index = 1
			end if
		End if
		'start output
		'current_index = Request.QueryString("BaseCurrentIndex")
		
		if Request.QueryString("jumptoindex") <> "" then
			current_index = Request.QueryString("jumptoindex")
		End if
		
		if (current_index = "") or (CLng(current_index) > CLng(totalRecords)) or (current_index=0) then
			current_index = 1
		end if
		if tPageSize <= 0 then tPageSize = CLng(1)
	    current_index = ((current_index - 1) \ tPageSize ) * tPageSize + 1
		starting_index = current_index
		trial_index = Clng(current_index) + Clng(tPageSize) - 1
	
		on error resume next
		if  Clng(trial_index) < Clng(totalRecords) then 
			end_index = trial_index
		else
			end_index = totalRecords
		End if
	if UCase(formmode) = "LIST" then
		Session("RecordRange" & dbkey & formgroup) = starting_index & "-" & end_index
	else
		Session("RecordRange" & dbkey & formgroup) = end_index
	end if
		Session("CurrentRecord" & dbkey & formgroup) = end_index
	'set at start and atend flags
	Session("AtStart" & dbkey & formgroup) = "False" 'set default
	if tPageSize > 1 then
		if end_index < tPageSize + 1 then
			Session("AtStart" & dbkey & formgroup) = "True"
		End if
	else
		if end_index = 1 then 
			Session("AtStart" & dbkey & formgroup) = "True"
		End if
	end if
	Session("AtEnd" & dbkey & formgroup) = "False" 'set default
	if tPageSize > 1 then
		if end_index = totalrecords then
			Session("AtEnd" & dbkey & formgroup) = "True"
		else
			if Clng(totalrecords - end_index) < Clng(tPageSize) then
				remainder = totalrecords Mod tPageSize
				if Clng(totalrecords - remainder) > Clng(end_index) then
					Session("AtEnd" & dbkey & formgroup) = "True"	
				end if
			end if
		end if
	else
		if end_index = totalrecords then
			Session("AtEnd" & dbkey & formgroup) = "True"
		end if
	end if
%>
<script language="javascript">
	setCookie("PagingMove" + MainWindow.dbname +  MainWindow.formgroup, "current_list",1)
</script>
<%
		'start output


		Session("BaseCurrentIndex" & dbkey & formgroup) = current_index -1
		Session("LastStartingIndex" & dbkey & formgroup) = current_index
		Session("LastTotalRecords" & dbkey & formgroup) = total_records
		current_index = Clng(current_index)
		end_index = Clng(end_index)
			if Application("DISABLE_CORE_RS_LOOPING") = 0 then
				start_loop = current_index -1
				end_loop = end_index -1
			else
				start_loop = 0
				end_loop = 0
			end if
			Dim r
			for r = start_loop to end_loop
		
				basearray = Session("Base_RS" & dbkey & formgroup)
				BaseID = basearray(0,r)
				if  Session("bypass_ini" & dbkey & formgroup) = true then
					Session("BaseID" & dbkey & formgroup) = BaseID
				end if
				
				'get current record and total records in base recordset
				BaseRunningIndex = r + 1
				BaseTotalRecords= Session("Base_RSRecordCount" & dbkey & formgroup)
				'set values for table specific items to base items since this is the base recordset
				BaseActualIndex =r + 1
				Session("BaseActualIndex" & dbkey & formgroup) = BaseActualIndex
		
			


%>
