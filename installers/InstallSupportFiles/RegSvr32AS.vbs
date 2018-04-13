' Script RegSrv32AS.vbs
' This script allows registering a COM object using RegSvr32.exe
' using the credentials of an arbitrary user.
' It works by runing RegSvr32 command within a RUNAS.exe command
' RUNAS.exe does not support passing in the password in the command line
' so the password is passed via sendKeys

On Error Resume Next

Dim WshShell,oArgs,FSO
set oArgs=wscript.Arguments

if InStr(oArgs(0),"?")<>0 then
	wscript.echo VBCRLF & "? HELP ?" & VBCRLF
	Usage
end if

if oArgs.Count <3 then
	wscript.echo VBCRLF & "! Usage Error !" & VBCRLF
	Usage
end if

sUser=oArgs(0)
sPass=oArgs(1)&VBCRLF
sDllPath=oArgs(2)
if Ubound(oArgs,1) > 2 then sFlags1 = oArgs(3)
if Ubound(oArgs,1) > 3 then sFlags2 = oArgs(4)

set WshShell = CreateObject("WScript.Shell")
set WshEnv = WshShell.Environment("Process")
WinPath = WshEnv("SystemRoot")&"\System32\runas.exe"
sCmd = "runas /user:" & sUser & " " & CHR(34) & "regsvr32.exe " & sFlags1 & " " & sFlags2 & " \""" & sDllPath & "\""" & CHR(34)
rc=WshShell.Run(sCmd, 1, FALSE)
Wscript.Sleep 3000 'need to give time for window to open.
WshShell.AppActivate(WinPath) 'make sure we grab the right window to send password to
Wscript.Sleep 3000
WshShell.SendKeys sPass 'send the password to the waiting window.
Wscript.Sleep 2000
set WshShell=Nothing
set oArgs=Nothing
set WshEnv=Nothing
wscript.quit

'************************
'* Usage Subroutine *
'************************
Sub Usage()
On Error Resume Next
msg= "Usage: cscript|wscript RegSvr32AS.vbs Username Password PathtoCOMServer [/s /u]" & _ 
	 VBCRLF & VBCRLF & _
	 "Optional silent and unregister flags are passed to RegSvr32" &_
	 VBCRLF & VBCRLF & _
	 "You should use the full path where necessary and put long file names or commands" & _
	 "with parameters in quotes.  " & VBCRLF & VBCRLF & "For example:" & VBCRLF & _ 
	 "cscript RegSvr32AS.vbs camsoft\dgb mypwd " & CHR(34) & "C:\Program Files\CambridgeSoft\ChemOfficeEnterprise11.0.1.0\Common\dlls\ScptUtl.OCX" & _
	 CHR(34) & VBCRLF & VBCRLF & "cscript vbrunas.vbs /?|-? will display this message."
wscript.echo msg
wscript.quit
end sub