CREATE OR REPLACE TRIGGER "REGDB"."TRG_FRAGMENTS_BU" BEFORE
  UPDATE ON REGDB.FRAGMENTS FOR EACH ROW
BEGIN
  :new.MODIFIED := sysdate;
END;
/
