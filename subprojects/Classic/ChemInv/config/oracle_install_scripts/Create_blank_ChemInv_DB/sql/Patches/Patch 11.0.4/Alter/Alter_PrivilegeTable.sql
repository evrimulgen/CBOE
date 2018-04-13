Alter table CHEMINV_PRIVILEGES
add(
INV_SAMPLE_REQUEST NUMBER(1),
INV_SAMPLE_APPROVE NUMBER(1),
INV_SAMPLE_DISPENSE NUMBER(1));

update CHEMINV_PRIVILEGES
set
INV_SAMPLE_REQUEST =0,
INV_SAMPLE_APPROVE =0,
INV_SAMPLE_DISPENSE =0;

update CHEMINV_PRIVILEGES
set
INV_SAMPLE_REQUEST =1,
INV_SAMPLE_APPROVE =1,
INV_SAMPLE_DISPENSE =1
where ROLE_INTERNAL_ID in (select role_id from SECURITY_ROLES where ROLE_NAME in ('INV_ADMIN'));


update CHEMINV_PRIVILEGES
set
INV_SAMPLE_REQUEST =1
where ROLE_INTERNAL_ID in (select role_id from SECURITY_ROLES where ROLE_NAME in ('INV_BROWSER','INV_CHEMIST','INV_RECEIVING','INV_FINANCE','INV_REGISTRAR'));
/
