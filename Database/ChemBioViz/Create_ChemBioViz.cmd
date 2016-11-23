ECHO OFF
TITLE Create DB Objects for ChemBioViz Applications
CLS
ECHO You are about to destroy any previous contents of 
ECHO the ChemBioViz database objects if they exist.
ECHO To abort close this window or press Ctrl-C.
Pause
cd %1
CD sql
ECHO ************************************  
ECHO Creating ChemBioViz database objects, privileges, roles, and default users
ECHO ************************************     
   

sqlplus /NOLOG  @HEADERFILE_CREATE_ChemBioViz.sql
notepad log_Create_ChemBioViz.txt



