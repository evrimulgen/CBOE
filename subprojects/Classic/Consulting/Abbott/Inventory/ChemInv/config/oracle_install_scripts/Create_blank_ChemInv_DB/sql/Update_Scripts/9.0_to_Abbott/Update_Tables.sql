--Copyright Cambridgesoft Corp 2001-2005 all rights reserved


-- TABLES
-- -------------------------------------

-- INV_CONTAINER_BATCHES: New Table
CREATE TABLE "INV_CONTAINER_BATCHES"(
	"BATCH_ID" NUMBER NOT NULL,
	"BATCH_FIELD_1" VARCHAR2(100),
	"BATCH_FIELD_2" VARCHAR2(100),
	"BATCH_FIELD_3" VARCHAR2(100),
    CONSTRAINT "INV_CONTAINER_BATCHES_PK"
		PRIMARY KEY("BATCH_ID") USING INDEX TABLESPACE &&indexTableSpaceName
	);

ALTER TABLE INV_CONTAINER_BATCHES ADD(
	"CONTAINER_STATUS_ID_FK" NUMBER(4),
	CONSTRAINT "CONTAINER_STATUS_ID_FK"
		FOREIGN KEY ("CONTAINER_STATUS_ID_FK")
		REFERENCES "INV_CONTAINER_STATUS" ("CONTAINER_STATUS_ID")
  );

ALTER TABLE INV_CONTAINER_BATCHES ADD (
	"MINIMUM_STOCK_THRESHOLD" NUMBER(4)
);

ALTER TABLE INV_CONTAINER_BATCHES MODIFY (
	"MINIMUM_STOCK_THRESHOLD" NUMBER
);

ALTER TABLE INV_CONTAINER_BATCHES ADD (
   	"COMMENTS" CLOB, 
	"FIELD_1" VARCHAR2(2000),
	"FIELD_2" VARCHAR2(2000),
	"FIELD_3" VARCHAR2(2000),
	"FIELD_4" VARCHAR2(2000),
	"FIELD_5" VARCHAR2(2000),
	"DATE_1" DATE,
	"DATE_2" DATE)
	LOB (COMMENTS) STORE AS(
	DISABLE STORAGE IN ROW
	TABLESPACE &&lobsTableSpaceName
	NOCACHE
	CHUNK 2K
	PCTVERSION 10
	STORAGE(INITIAL &&lobXML NEXT &&lobXML)
);

CREATE TABLE "INV_GRAPHIC_TYPES"(
	"GRAPHIC_TYPE_ID" NUMBER NOT NULL,
	"GRAPHIC_TYPE_NAME" VARCHAR2(50),
    CONSTRAINT "INV_GRAPHIC_TYPE_PK"
		PRIMARY KEY("GRAPHIC_TYPE_ID") USING INDEX TABLESPACE &&indexTableSpaceName
	);

CREATE TABLE "INV_GRAPHICS"(
	"GRAPHIC_ID" NUMBER NOT NULL,
	"GRAPHIC_NAME" VARCHAR2(50),
	"GRAPHIC_TYPE_ID_FK" NUMBER(4),
	"GRAPHIC_WIDTH" NUMBER(4),
	"GRAPHIC_HEIGHT" NUMBER(4),
	"GRAPHIC_ALT" VARCHAR2(50),
	"URL_ACTIVE" VARCHAR(100),
	"URL_INACTIVE" VARCHAR(100),
    CONSTRAINT "INV_GRAPHIC_PK"
		PRIMARY KEY("GRAPHIC_ID") USING INDEX TABLESPACE &&indexTableSpaceName,
	CONSTRAINT "INV_GRAPHIC_TYPE_ID_FK" 
		FOREIGN KEY ("GRAPHIC_TYPE_ID_FK")
		REFERENCES "INV_GRAPHIC_TYPES" ("GRAPHIC_TYPE_ID")
	);


-- Create the INV_DOC_TYPES table
CREATE TABLE "INV_DOC_TYPES"(
   	"DOC_TYPE_ID" NUMBER(4) NOT NULL, 
	"TYPE_NAME" VARCHAR2(100), 
	CONSTRAINT "INV_DOC_TYPE_PK" 
		PRIMARY KEY("DOC_TYPE_ID") USING INDEX TABLESPACE &&indexTableSpaceName
	);


-- Create the INV_DOCS table
CREATE TABLE "INV_DOCS"(
   	"DOC_ID" NUMBER(4) NOT NULL, 
   	"TABLE_NAME" VARCHAR2(200),
   	"FIELD_NAME" VARCHAR2(200),
   	"FIELD_VALUE" VARCHAR2(200),
   	"DOC_TYPE_ID_FK" NUMBER(4),
   	"DOC" CLOB, 
   	"DATE_CREATED" DATE,
   	CONSTRAINT "INV_DOCS_PK" 
		PRIMARY KEY("DOC_ID") USING INDEX TABLESPACE &&indexTableSpaceName,
    CONSTRAINT "INV_DOCS_DOCTYPEID_FK" 
	FOREIGN KEY("DOC_TYPE_ID_FK") 
	   	REFERENCES "INV_DOC_TYPES"("DOC_TYPE_ID")) 
	LOB (DOC) STORE AS(
	DISABLE STORAGE IN ROW
	TABLESPACE &&lobsTableSpaceName
	NOCACHE
	CHUNK 2K
	PCTVERSION 10
	STORAGE(INITIAL &&lobXML NEXT &&lobXML)
	);


-- INV_UNIT_CONVERSION_FORUMLA: New Table
CREATE TABLE "INV_UNIT_CONVERSION_FORMULA"(
	"FROM_UNIT_ID_FK" NUMBER(4) not null,
	"TO_UNIT_ID_FK" NUMBER(4) not null,
	"OPERATION" VARCHAR2(50),
	"INTERMED_UNIT_ID_FK" NUMBER(4),
	CONSTRAINT "FROM_UNIT_ID_FK" 
		FOREIGN KEY ("FROM_UNIT_ID_FK")
		REFERENCES "INV_UNITS" ("UNIT_ID") ON DELETE CASCADE,
	CONSTRAINT "TO_UNIT_ID_FK" 
		FOREIGN KEY ("TO_UNIT_ID_FK")
		REFERENCES "INV_UNITS" ("UNIT_ID") ON DELETE CASCADE,
	CONSTRAINT "INTERMED_UNIT_ID_FK"
		FOREIGN KEY ("INTERMED_UNIT_ID_FK")
		REFERENCES "INV_UNITS" ("UNIT_ID") ON DELETE CASCADE
	);


-- INV_ORG_UNIT: New Table
CREATE TABLE "INV_ORG_UNIT"(
	"ORG_UNIT_ID" NUMBER NOT NULL,
	"ORG_NAME" VARCHAR2(100) not null,
	"ORG_TYPE_ID_FK" NUMBER(4) not null,
    CONSTRAINT "INV_ORG_UNIT_PK"
		PRIMARY KEY("ORG_UNIT_ID") USING INDEX TABLESPACE &&indexTableSpaceName,
	CONSTRAINT "INV_ORG_TYPE_ID_FK" 
		FOREIGN KEY ("ORG_TYPE_ID_FK")
		REFERENCES "INV_ENUMERATION" ("ENUM_ID")
	);


-- INV_ORG_ROLES: New Table
CREATE TABLE "INV_ORG_ROLES"(
	"ORG_ROLE_ID" NUMBER NOT NULL,
	"ROLE_ID_FK" NUMBER(4) not null,
	"ORG_UNIT_ID_FK" NUMBER(4) not null,
    CONSTRAINT "INV_ORG_ROLE_ID_PK"
		PRIMARY KEY("ORG_ROLE_ID") USING INDEX TABLESPACE &&indexTableSpaceName,
	CONSTRAINT "INV_ROLES_ORG_UNIT_ID_FK" 
		FOREIGN KEY ("ORG_UNIT_ID_FK")
		REFERENCES "INV_ORG_UNIT" ("ORG_UNIT_ID"),
	CONSTRAINT "INV_ORG_ROLE_ID_FK" 
		FOREIGN KEY ("ROLE_ID_FK")
		REFERENCES CS_SECURITY.SECURITY_ROLES ("ROLE_ID")
	);


-- INV_ORG_USERS: New Table
CREATE TABLE "INV_ORG_USERS"(
	"ORG_USER_ID" NUMBER NOT NULL,
	"USER_ID_FK" VARCHAR2(100) not null,
	"ORG_UNIT_ID_FK" NUMBER(4) not null,
    CONSTRAINT "INV_ORG_USER_ID_PK"
		PRIMARY KEY("ORG_USER_ID") USING INDEX TABLESPACE &&indexTableSpaceName,
	CONSTRAINT "INV_ORG_USERS_UNIT_ID_FK" 
		FOREIGN KEY ("ORG_UNIT_ID_FK")
		REFERENCES "INV_ORG_UNIT" ("ORG_UNIT_ID"),
	CONSTRAINT "INV_ORG_USER_ID_FK" 
		FOREIGN KEY ("USER_ID_FK")
		REFERENCES "PEOPLE" ("USER_ID")
	);


-- INV_CONTAINERS: Alter Table
ALTER TABLE INV_CONTAINERS ADD(
	"BATCH_ID_FK" NUMBER(4),
	--"PROJECT_ID_FK" NUMBER(4),
	CONSTRAINT "INV_CONT_BATCH_ID_FK"
		FOREIGN KEY ("BATCH_ID_FK")
		REFERENCES "INV_CONTAINER_BATCHES" ("BATCH_ID")
  );


-- INV_REQUESTS: Alter Table
ALTER TABLE INV_REQUESTS ADD(
	--"PROJECT_ID_FK" NUMBER(4),
	"BATCH_ID_FK" NUMBER(4),
	"FIELD_1" VARCHAR2(2000),
	"FIELD_2" VARCHAR2(2000),
	"FIELD_3" VARCHAR2(2000),
	"FIELD_4" VARCHAR2(2000),
	"FIELD_5" VARCHAR2(2000),
	"DATE_1" DATE,
	"DATE_2" DATE,
	CONSTRAINT "INV_REQUESTS_BATCH_ID_FK" 
		FOREIGN KEY ("BATCH_ID_FK")
		REFERENCES "INV_CONTAINER_BATCHES" ("BATCH_ID")
  );

-- INV_REQUESTS: Modify Table
ALTER TABLE INV_REQUESTS MODIFY CONTAINER_ID_FK NULL;
ALTER TABLE INV_REQUESTS MODIFY USER_ID_FK NULL;
ALTER TABLE INV_REQUESTS ADD(
	"ORG_UNIT_ID_FK" NUMBER(4),
	CONSTRAINT "ORG_UNIT_ID_FK"
		FOREIGN KEY ("ORG_UNIT_ID_FK")
		REFERENCES "INV_ORG_UNIT" ("ORG_UNIT_ID")
  );
ALTER TABLE INV_REQUESTS ADD(
	"ASSIGNED_USER_ID_FK" VARCHAR2(100),
	CONSTRAINT "INV_REQ_ASSIGNED_USER_ID_FK" 
		FOREIGN KEY ("ASSIGNED_USER_ID_FK")
		REFERENCES "PEOPLE" ("USER_ID")
  );
ALTER TABLE INV_REQUESTS DISABLE CONSTRAINT INV_REQUESTS_USERID_FK;
ALTER TABLE INV_REQUESTS DISABLE CONSTRAINT ORG_UNIT_ID_FK;
ALTER TABLE INV_REQUESTS ADD(
	"QTY_DELIVERED" NUMBER
  );



-- INV_LOCATIONS: Alter Table
ALTER TABLE INV_LOCATIONS ADD (
	"COLLAPSE_CHILD_NODES" NUMBER(1)
);

-- INV_GRID_FORMAT: Alter Table
ALTER TABLE INV_GRID_FORMAT ADD (
	"CELL_NAMING" NUMBER(1),
	"NAME_DELIMETER" VARCHAR2(50)
);

-- INV_LOCATION_TYPES: Alter Table
ALTER TABLE INV_LOCATION_TYPES ADD (
	"GRAPHIC_ID_FK" NUMBER(4),
	CONSTRAINT "LOCATION_TYPES_GRAPHIC_ID_FK"
		FOREIGN KEY ("GRAPHIC_ID_FK")
		REFERENCES "INV_GRAPHICS" ("GRAPHIC_ID")
);

-- NEW INDICES
-- -------------------------------------
CREATE INDEX CONTAINER_BATCH_ID_FK_IDX ON INV_CONTAINERS(BATCH_ID_FK) TABLESPACE &&indexTableSpaceName;
CREATE INDEX CONTAINER_REG_ID_FK_IDX ON INV_CONTAINERS(REG_ID_FK) TABLESPACE &&indexTableSpaceName;
CREATE INDEX CONTAINER_BATCH_NUMBER_FK_IDX ON INV_CONTAINERS(BATCH_NUMBER_FK) TABLESPACE &&indexTableSpaceName;
CREATE INDEX REQUESTS_BATCH_ID_FK_IDX ON INV_REQUESTS(BATCH_ID_FK) TABLESPACE &&indexTableSpaceName;
CREATE INDEX DOC_TYPE_ID_FK_IDX ON INV_DOCS(DOC_TYPE_ID_FK) TABLESPACE &&indexTableSpaceName;
CREATE INDEX BATCHES_CONT_STATUS_ID_FK_IDX ON INV_CONTAINER_BATCHES(CONTAINER_STATUS_ID_FK) TABLESPACE  &&indexTableSpaceName;


-- SEQUENCES  
-- -------------------------------------
CREATE SEQUENCE SEQ_INV_CONTAINER_BATCHES INCREMENT BY 1 START WITH 1;
CREATE SEQUENCE SEQ_INV_GRAPHICS INCREMENT BY 1 START WITH 1;
CREATE SEQUENCE SEQ_INV_GRAPHIC_TYPES INCREMENT BY 1 START WITH 1;
CREATE SEQUENCE SEQ_DOC_ID INCREMENT BY 1 START WITH 1;
CREATE SEQUENCE SEQ_DOC_TYPE_ID INCREMENT BY 1 START WITH 1;
CREATE SEQUENCE SEQ_ORG_UNIT_ID INCREMENT BY 1 START WITH 1;
CREATE SEQUENCE SEQ_INV_ORG_ROLE_ID INCREMENT BY 1 START WITH 1;
CREATE SEQUENCE SEQ_INV_ORG_USER_ID INCREMENT BY 1 START WITH 1;


-- TRIGGERS
-- -------------------------------------
CREATE OR REPLACE TRIGGER "TRG_INV_CONTAINER_BATCHES_ID"
    BEFORE INSERT
    ON "INV_CONTAINER_BATCHES"
    FOR EACH ROW
BEGIN
	if :new.batch_id is null then
		select SEQ_INV_CONTAINER_BATCHES.nextval into :new.batch_id from dual;
	end if;
END;
/

CREATE OR REPLACE TRIGGER "TRG_INV_GRAPHIC_TYPES_ID"
    BEFORE INSERT
    ON "INV_GRAPHIC_TYPES"
    FOR EACH ROW
BEGIN
	if :new.graphic_type_id is null then
		select SEQ_INV_GRAPHIC_TYPES.nextval into :new.graphic_type_id from dual;
	end if;
END;
/

CREATE OR REPLACE TRIGGER "TRG_INV_GRAPHICS_ID"
    BEFORE INSERT
    ON "INV_GRAPHICS"
    FOR EACH ROW
BEGIN
	if :new.graphic_id is null then
		select SEQ_INV_GRAPHICS.nextval into :new.graphic_id from dual;
	end if;
END;
/

CREATE OR REPLACE TRIGGER "TRG_INV_DOCS_ID"
    BEFORE INSERT
    ON "INV_DOCS"
    FOR EACH ROW
BEGIN
	if :new.doc_id is null then
		select SEQ_DOC_ID.nextval into :new.doc_id from dual;
	end if;
END;
/

CREATE OR REPLACE TRIGGER "TRG_INV_DOC_TYPES_ID"
    BEFORE INSERT
    ON "INV_DOC_TYPES"
    FOR EACH ROW
BEGIN
	if :new.doc_type_id is null then
		select SEQ_DOC_TYPE_ID.nextval into :new.doc_type_id from dual;
	end if;
END;
/

CREATE OR REPLACE TRIGGER "TRG_INV_ORG_UNIT_ID"
    BEFORE INSERT
    ON "INV_ORG_UNIT"
    FOR EACH ROW
BEGIN
	if :new.org_unit_id is null then
		select SEQ_ORG_UNIT_ID.nextval into :new.org_unit_id from dual;
	end if;
END;
/

CREATE OR REPLACE TRIGGER "TRG_INV_ORG_USER_ID"
    BEFORE INSERT
    ON "INV_ORG_USERS"
    FOR EACH ROW
BEGIN
	if :new.org_user_id is null then
		select SEQ_INV_ORG_USER_ID.nextval into :new.org_user_id from dual;
	end if;
END;
/


CREATE OR REPLACE TRIGGER "TRG_INV_ORG_ROLE_ID"
    BEFORE INSERT
    ON "INV_ORG_ROLES"
    FOR EACH ROW
BEGIN
	if :new.org_role_id is null then
		select SEQ_INV_ORG_ROLE_ID.nextval into :new.org_role_id from dual;
	end if;
END;
/

