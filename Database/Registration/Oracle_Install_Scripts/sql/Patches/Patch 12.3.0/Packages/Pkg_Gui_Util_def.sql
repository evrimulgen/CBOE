prompt 
prompt Starting "pkg_Gui_Util_def.sql"...
prompt 

CREATE OR REPLACE PACKAGE &schemaName.."GUI_UTIL"
AS 
  --Types
  TYPE CURSOR_TYPE IS REF CURSOR;
  TYPE FRAGMENT_IDS IS TABLE OF vw_fragment.fragmentid%type INDEX BY BINARY_INTEGER;

  --Procedures
  PROCEDURE GETPROJECTNAMEVALUELIST(O_RS OUT CURSOR_TYPE);
  PROCEDURE GETPREFIXNAMEVALUELIST (O_RS OUT CURSOR_TYPE);
  PROCEDURE GETACTIVECHEMISTNAMEVALUELIST (O_RS OUT CURSOR_TYPE);
  PROCEDURE GETCHEMISTNAMEVALUELIST (O_RS OUT CURSOR_TYPE);
  PROCEDURE GETFRAGMENTTYPESVALUELIST (O_RS OUT CURSOR_TYPE);
  PROCEDURE GETFRAGMENTSLIST (O_RS OUT CURSOR_TYPE);
  PROCEDURE getfragmentlistbyids(
    AIdList IN FRAGMENT_IDS
    , O_RS OUT CURSOR_TYPE
  );
  PROCEDURE GETMATCHEDFRAGMENTSLIST (
    AStructure IN CLOB
    , O_RS OUT CURSOR_TYPE
  );
  PROCEDURE GETPROJECTLIST (O_RS OUT CURSOR_TYPE);
  PROCEDURE GETACTIVEPROJECTLISTBYPERSONID (
    O_RS OUT CURSOR_TYPE
    , APersonID in Number
    , AType IN Char:='A'
  );
  PROCEDURE GETIDENTIFIERLIST (
    O_RS OUT CURSOR_TYPE
    , AType IN Char:='A'
  );
  PROCEDURE GETSEQUENCELIST (
    O_RS OUT CURSOR_TYPE
    , ASeqTypeID in Number
  );
  PROCEDURE GETSEQUENCELISTBYPERSONID (
    O_RS OUT CURSOR_TYPE
    , ASeqTypeID in Number
    , APersonID in Number
  );
  PROCEDURE GETTEMPREGISTRIESCOUNT (ACount OUT NUMBER);
  PROCEDURE GETTEMPSUBMREGISTRIESCOUNT (ACount OUT NUMBER);
  PROCEDURE GETTEMPAPPROVEDREGISTRIESCOUNT (ACount OUT NUMBER);
  PROCEDURE GETPERMREGISTRIESCOUNT (ACount OUT NUMBER);
  PROCEDURE GETAPPROVEDREGISTRIESCOUNT (ACount OUT NUMBER);
  PROCEDURE GETDUPLICATEDCOMPOUNDCOUNT (ACount OUT NUMBER);

END "GUI_UTIL";
/
