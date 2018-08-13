<script language = "javascript">
//Copyright 2001-2002 CambridgeSoft Corporation All Rights Reserved%>
//PURPOSE OF FILE: TO add custom javascript functions to an applciation
//All form files generated by the wizard have a #INCLUDE for this file. Add the #INCLUDE to form files
//that you might add to the application.



//////////////////////////////////////////////////////////////////////////////////////////
// Displays the zoom button and calls ACX_doStructureZoom() when clicked
//
function ACX_getStrucZoomBtn(fullSrucFieldName, BaseID, structDataObjName, gifWidth, gifHeight, gifName){
    var outputval = ""
    if (!gifName) gifName = "zoom_btn.gif"
    var buttonGifPath = button_gif_path + gifName
    var params = "&quot;" + fullSrucFieldName + "&quot;," + BaseID + ",&quot;" + structDataObjName + "&quot;"
	
    if(typeof gifWidth != "undefined"){
        params +=  "," + gifWidth + "," + gifHeight	  
    }
    outputval = '<A HREF ="Show Structure in larger window" onclick="ACX_doStructureZoom(' + params + ');return false;"><IMG SRC="' +  buttonGifPath + '" BORDER="0"></A>'
    document.write (outputval)
}

//////////////////////////////////////////////////////////////////////////////////////////
// Pops up a window with zoom_structure.asp in it
//
function ACX_doStructureZoom(fullSrucFieldName, BaseID, structDataObjName, gifWidth, gifHeight){
    var z = ""
    var attribs = 'width=450,height=450,left=0,top=0,xpos=0,ypos=0,status=no,resizable=yes';
    var url = app_Path + "/zoom_structure.asp?baseid="+ BaseID + "&dbname=" + dbname + "&fullSrucFieldName=" + fullSrucFieldName + "&structDataObjName=" + structDataObjName;
	
    if ((typeof gifWidth != "undefined") && (gifWidth > 0)){
        url += "&gifWidth=" + gifWidth + "&gifHeight=" + gifHeight;
    }
	
    if (z.name == null){
        z = window.open(url,"zoom_structure", attribs);
        z.name = "zoom_structure"
    }
    else{
        z.focus()
    }
}

//////////////////////////////////////////////////////////////////////////////////////////
// Generic Cookie Reader Function
//
function ReadCookie(cookiename){
    var allcookies = unescape(document.cookie);      //CSBR 148189 SJ Resolving the location id issue in the search page
    var  pos = allcookies.indexOf(cookiename + "=");
    if (pos != -1){
        var start = pos + cookiename.length + 1;
        var end = allcookies.indexOf(";",start);  
        if (end == -1){
            end= allcookies.length;
        }
        var cookiestr = allcookies.substring(start,end); //CSBR 148189
        var out = cookiestr;
        return out;
    }
    else { 
        var out = "";
        return out;
    }
}
	
//Prints the most important frame/s
function InvPrintCurrentPage(){
    if (MainWindow.formmode == "edit"){ 
        MainWindow.parent.focus();
    }
    else{
        MainWindow.focus();
    }
    window.print();
}

function getOpenFileButton(fieldname){
    var outpuval = ""
    outputval = '<a href="javascript:MainWindow.doOpenFileLoadWindow(&quot;' + fieldname + '&quot;)">'
    outputval = outputval + '<img SRC="/cfserverasp/source/graphics/open_file_btn.gif" BORDER="0"></a>'
    document.write(outputval)
}
function doOpenFileLoadWindow(fieldname){
    var w = ""
    if (w.name == null){                    
        var w = window.open("/<%=application("appkey")%>/Load_IDS.asp?dbname=<%=dbkey%>&formgroup=<%=formgroup%>&fieldname=" + escape(fieldname),"load_ids_from_file","width=450,height=30,scrollbars=yes,status=yes,resizable=yes");
        w.focus()}
    else{
        w.focus()}
}
 
//////////////////////////////////////////////////////////////////////////////////////////
// Displays the zoom button and calls ACX_doStructureZoom() when clicked
//
function getStrucCopyBtn(tablename, fieldname, uniqueid) {
    var outputval = ""
    var buttonGifPath = button_gif_path + "copy_icon.png"    
    var params = "&quot;" + tablename + "&quot;,&quot;" + fieldname + "&quot;," + uniqueid 
    outputval = '<A HREF ="Copy Structure" onclick="doStructureCopy(' + params + ');return false;"><IMG SRC="' +  buttonGifPath + '" BORDER="0"></A>'
    document.write (outputval)
}
 
function doStructureCopy(structDataObjName, isDialog) {
    //var base64_cdx_name = tablename + fieldname  + '_' + uniqueid + '_orig';
    var base64_cdx = (isDialog) ? opener.document.getElementById(structDataObjName).value : document.getElementById(structDataObjName).value;
    var b64 = base64_cdx.replace(new RegExp('<br>', 'g'), '');
    chemdrawjs.loadB64CDX(b64);
    var textField = document.createElement('textarea');    
    document.body.appendChild(textField);
    textField.innerText = chemdrawjs.getCDXML();
    textField.select();
    document.execCommand('copy');
    textField.remove();    
}

function doStructureCopyIndividual() {
    var textField = document.createElement('textarea');    
    document.body.appendChild(textField);
    textField.innerText = chemdrawjs.getCDXML();
    textField.select();
    document.execCommand('copy');
    textField.remove();    
}

</script>



