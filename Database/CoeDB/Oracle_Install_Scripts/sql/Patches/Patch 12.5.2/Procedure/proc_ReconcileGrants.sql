create or replace PROCEDURE ReconcileGrants
(pGroupID IN COEGROUPPEOPLE.GROUP_ID%TYPE,
pUserID IN COEGROUPPEOPLE.PERSON_ID%TYPE,
pRoleID IN COEGROUPROLE.ROLE_ID%TYPE,
pIsAdded IN integer
)

IS
RoleName varchar2(50);
RoleId NUMBER;
PERSONID varchar2(50);
COUNTER NUMBER;
ROLECOUNT NUMBER;
PEOPLE_USERID COEGROUPPEOPLE.PERSON_ID%TYPE;
source_cursor    INTEGER;
RoleCoutner NUMBER;
  CURSOR c1 IS
        SELECT
              SECURITY_ROLES.ROLE_NAME,PEOPLE.USER_ID,COEGROUPROLE.ROLE_ID
        FROM
              SECURITY_ROLES, COEGROUPROLE , people
        WHERE
              security_roles.role_id = COEGROUPROLE.ROLE_ID
              AND coegrouprole.group_id = pGroupID
              AND  people.person_id = pUserID;

        CURSOR c2 IS
        SELECT
              PEOPLE.USER_ID,SECURITY_ROLES.role_name,PEOPLE.PERSON_ID,ROLE_ID
        FROM
              COEGROUPPEOPLE,PEOPLE,SECURITY_ROLES
        WHERE
               PEOPLE.PERSON_ID = COEGROUPPEOPLE.PERSON_ID
               AND COEGROUPPEOPLE.group_id = pGroupID
               AND SECURITY_ROLES.role_id = pRoleID;


    pragma autonomous_transaction;
BEGIN
 source_cursor := DBMS_SQL.open_cursor;
IF pUserID IS NOT NULL THEN
	OPEN c1;
	LOOP
	FETCH c1 INTO RoleName,PERSONID,RoleId;
		EXIT WHEN c1%NOTFOUND;
		SELECT COUNT(*) INTO ROLECOUNT FROM dba_roles WHERE role = UPPER(RoleName);
		SELECT COUNT(*) INTO RoleCoutner from  dba_role_privs where UPPER (dba_role_privs.grantee) = UPPER (PERSONID) AND UPPER (dba_role_privs.granted_role) = UPPER(RoleName);
		IF pIsAdded = 1 THEN
			IF ROLECOUNT >0 THEN
			EXECUTE IMMEDIATE 'grant ' ||  RoleName || ' to ' || '"' || PERSONID || '"';
			END IF;
		ELSE
			SELECT COUNT(PERSON_ID) INTO COUNTER FROM COEGROUPROLE,COEGROUPPEOPLE WHERE COEGROUPROLE.GROUP_ID = COEGROUPPEOPLE.GROUP_ID AND COEGROUPROLE.GROUP_ID<>pGroupID AND COEGROUPPEOPLE.PERSON_ID = pUserID AND COEGROUPROLE.ROLE_ID = RoleId;
			IF NOT COUNTER > 0 and  ROLECOUNT >0 and RoleCoutner >0 THEN
				EXECUTE IMMEDIATE 'revoke ' ||   RoleName || ' from ' || '"' || PERSONID || '"';
			END IF;
		END IF;
	END LOOP;
	CLOSE c1;
ELSE
	open c2;
	LOOP
	FETCH c2 INTO PERSONID,RoleName,PEOPLE_USERID, RoleId;
		EXIT WHEN c2%NOTFOUND;
   		SELECT COUNT(*) INTO ROLECOUNT FROM dba_roles WHERE role = UPPER(RoleName);
		SELECT COUNT(*) INTO RoleCoutner from  dba_role_privs where UPPER (dba_role_privs.grantee) = UPPER (PERSONID) AND UPPER (dba_role_privs.granted_role) = UPPER(RoleName);
		IF pIsAdded = 1 AND ROLECOUNT >0 THEN
		EXECUTE IMMEDIATE ' grant ' || RoleName || ' to ' || '"' || PERSONID || '"';
		ELSE
			SELECT COUNT(PERSON_ID) INTO COUNTER FROM COEGROUPROLE,COEGROUPPEOPLE WHERE COEGROUPROLE.GROUP_ID = COEGROUPPEOPLE.GROUP_ID AND COEGROUPROLE.GROUP_ID <> pGroupID AND COEGROUPPEOPLE.PERSON_ID = PEOPLE_USERID AND COEGROUPROLE.ROLE_ID = RoleId;
			IF NOT COUNTER > 0 and ROLECOUNT >0 and RoleCoutner>0 THEN
				EXECUTE IMMEDIATE ' revoke ' || RoleName || ' from ' || '"' || PERSONID || '"';
			END IF;
		END IF;
	END LOOP;
	CLOSE c2;
END IF;
End;
/