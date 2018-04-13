CREATE OR REPLACE  FUNCTION "&&SchemaName"."RETIRECONTAINER"           
(
    pContainerID in varchar2,
    pQtyRemaining in Inv_containers.Qty_Remaining%Type,
    pContainerStatusID in Inv_Containers.Container_Status_ID_FK%type,
    pLocationID in Inv_Containers.Location_Id_FK%type
)
return inv_containers.Container_ID%Type
IS
source_cursor integer;
rows_processed integer;
containers_t STRINGUTILS.t_char;
excess_contents exception;
container_type_not_allowed exception;
pragma exception_init (excess_contents, -2290);
BEGIN
if is_container_type_allowed(NULL, pLocationID) = 0 then
  RAISE container_type_not_allowed;
end if;
  
source_cursor := dbms_sql.open_cursor;
dbms_sql.parse(source_cursor,'Update inv_containers set Qty_Remaining =' || pQtyRemaining || ', Container_Status_ID_FK =' || pContainerStatusID || ',Location_ID_FK =' || pLocationID || ' WHERE Container_ID IN (' || pContainerID || ')' , dbms_sql.NATIVE);
rows_processed := dbms_sql.execute(source_cursor);

containers_t := STRINGUTILS.split(pContainerID,',');
  
FOR i in containers_t.First..containers_t.Last
Loop
      Reservations.ReconcileQtyAvailable(containers_t(i)); 
End loop;  
RETURN 1;
 
exception
WHEN excess_contents then
  RETURN -103;
WHEN container_type_not_allowed then
  RETURN -128;
END RetireContainer;
/
show errors;