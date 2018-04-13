using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Oracle.DataAccess.Client;
using Csla.Data;
using CambridgeSoft.COE.Framework.Common;
using CambridgeSoft.COE.Framework.Properties;
using CambridgeSoft.COE.Framework.COELoggingService;
using CambridgeSoft.COE.Framework.COEConfigurationService;
using Oracle.DataAccess.Types;
using CambridgeSoft.COE.Framework.Common.SqlGenerator;
using CambridgeSoft.COE.Framework.Common.SqlGenerator.Queries;
using CambridgeSoft.COE.Framework.Common.SqlGenerator.NonQueries;
using CambridgeSoft.COE.Framework.COEDatabasePublishingService;



namespace CambridgeSoft.COE.Framework.COEDatabasePublishingService
{
    public class OracleDataAccessClientDAL : CambridgeSoft.COE.Framework.COEDatabasePublishingService.DAL
    {
        [NonSerialized]
        static COELog _coeLog = COELog.GetSingleton("COEDatabasePublishing");

        /// <summary>
        /// to return the next id
        /// </summary>
        /// <param name="dataView"></param>
        /// <returns></returns>
        public override int GetNewID()
        {
            string sql = "SELECT " + _coeSchemaTableName + "_SEQ.NEXTVAL FROM DUAL";

            int id = -1;

            DbCommand dbCommand = DALManager.Database.GetSqlStringCommand(sql);

            try
            {
                id = Convert.ToInt32(DALManager.ExecuteScalar(dbCommand));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return id;
        }

        /// <summary>   
        /// Check owner in the database
        /// <returns>true, if owner and password are correct</returns>
        /// </summary>
        public override bool AuthenticateUser(string ownerName, string password)
        {
            // Coverity Fix CID - 11816
            using (Oracle.DataAccess.Client.OracleConnection con = new Oracle.DataAccess.Client.OracleConnection())
            {
                try
                {
                    con.ConnectionString = "Data Source=" + DALManager.DatabaseData.DataSource + ";User Id=" + ownerName + ";Password=" + password + ";";
                    con.Open();
                    con.Close();
                    return true;
                }
                catch
                {
                    con.Close();
                    return false;
                }
            }
        }       

        internal override void GrantTable(string databaseName, string tableName)
        {
            if (databaseName != Resources.CentralizedStorageDB) // To avoid: ORA-01749: you may not GRANT/REVOKE privileges to/from yourself
            {
                string sql = "GRANT SELECT ON " + databaseName + "." + tableName + " TO " + Resources.CentralizedStorageDB;
                DbCommand dbCommand = DALManager.Database.GetSqlStringCommand(sql);

                int recordsAffected = -1;
                try
                {
                    recordsAffected = DALManager.ExecuteNonQuery(dbCommand);
                }
                catch (Exception ex)
                {
                    //avoiding re throwing exception as there are mostly no to be thrown. IE: You cannot revoke privs that you have not granted
                }
            }
        }

        internal override void RevokeTable(string databaseName, string tableName)
        {
            if (databaseName != Resources.CentralizedStorageDB) // To avoid: ORA-01749: you may not GRANT/REVOKE privileges to/from yourself
            {
                string sql = "REVOKE SELECT ON " + databaseName + "." + tableName + " FROM " + Resources.CentralizedStorageDB;
                DbCommand dbCommand = DALManager.Database.GetSqlStringCommand(sql);

                int recordsAffected = -1;
                try
                {
                    recordsAffected = DALManager.ExecuteNonQuery(dbCommand);
                }
                catch (Exception ex)
                {
                    //avoiding re throwing exception as there are mostly no to be thrown. IE: You cannot revoke privs that you have not granted
                }
            }
        }

        /// <summary>   
        /// Executes two queries defined in the method.
        /// <returns>DataSet Containing two tables, one containing the list of columns of each table in the database and the other containing the relationships </returns>
        /// <param name="OwnerName">The owner/schema to get the info from.</param>
        /// </summary>
        public override DataSet Get_DatabaseSchema(string OwnerName)
        {
            DataSet returnDataSet = new DataSet();
            DataTable TableColumns = new DataTable();
            DataTable Relationships = new DataTable();

            try
            {
                string GetDatabaseTableColumnsQuery = @"WITH describe_data AS
                                                (SELECT TC.owner,
                                                        AO.object_name AS table_name,
                                                        TC.column_name,
                                                        TC.data_type,                                                       
                                                        TC.data_precision,
                                                        TC.data_scale,
                                                        AO.object_type AS type,
                                                        d.isPrimaryKey IS_PRIMARY_KEY,
                                                        row_number() OVER (ORDER BY tc.table_name, tc.column_name) rn
                                                  FROM All_Tab_Columns TC
                                                   INNER JOIN All_Objects AO
                                                     ON AO.object_name = TC.table_name
                                                   LEFT OUTER JOIN (SELECT 'true' as isPrimaryKey, cc.column_name, cc.table_name
				                                                FROM All_Cons_Columns CC, All_Constraints C
				                                                WHERE CC.constraint_name = C.constraint_name AND
				                                                 C.constraint_type = 'P' AND
				                                                 C.owner = CC.owner) d ON d.table_name = tc.table_name AND
                                                                        d.column_name = tc.column_name
                                                  WHERE TC.owner =" + DALManager.BuildSqlStringParameterName("pOwner") + @" AND
                                                        TC.table_name NOT LIKE 'COE%' AND
                                                        AO.OBJECT_TYPE IN('TABLE', 'VIEW') AND
                                                        AO.STATUS='VALID' AND
                                                        AO.OBJECT_NAME NOT LIKE ('BIN$%')) -- if recycle bin is enabled, this indicates a table was dropped see http://download.oracle.com/docs/cd/B19306_01/backup.102/b14192/flashptr004.htm for further details
                                                SELECT DISTINCT table_name,
                                                       column_name,
                                                       data_type,                                                      
                                                       data_precision,
                                                       data_scale,
                                                       IS_PRIMARY_KEY,
                                                       type
                                                 FROM describe_data
                                                 WHERE rn BETWEEN 1 AND 1000000
                                                 ORDER BY TABLE_NAME, TYPE, COLUMN_NAME";

                DbCommand dbCommand = DALManager.Database.GetSqlStringCommand(GetDatabaseTableColumnsQuery);
                DALManager.Database.AddParameter(dbCommand, DALManager.BuildSqlStringParameterName("pOwner"), DbType.AnsiString, 200, ParameterDirection.Input, true, 0, 0, string.Empty, DataRowVersion.Current, OwnerName);
                IDataReader dbDataReader = DALManager.ExecuteReader(dbCommand);
                using (DataReaderAdapter dbDataAdapter = new DataReaderAdapter())
                {
                    dbDataAdapter.FillFromReader(TableColumns, dbDataReader);

                    dbDataReader.Close();

                    string relationshipquery = @"
                SELECT 	C.COLUMN_NAME AS FK_Column ,
		                C.TABLE_NAME AS FK_TABLE_NAME,
		                B.TABLE_NAME AS PK_TABLE_NAME,
		                B.COLUMN_NAME AS PK_Column
                FROM 	ALL_CONSTRAINTS A,
		                ALL_CONS_COLUMNS B,
		                ALL_CONS_COLUMNS C
                WHERE 	A.R_CONSTRAINT_NAME=B.CONSTRAINT_NAME
                AND 	A.CONSTRAINT_NAME=C.CONSTRAINT_NAME
                AND 	A.OWNER=C.OWNER
                AND 	A.OWNER=B.OWNER
                AND 	A.TABLE_NAME=C.TABLE_NAME
                AND 	B.POSITION=C.POSITION
                AND 	A.OWNER=" + DALManager.BuildSqlStringParameterName("pOwner") + @"
                ORDER BY A.CONSTRAINT_NAME, C.POSITION";

                    dbCommand = DALManager.Database.GetSqlStringCommand(relationshipquery);
                    DALManager.Database.AddParameter(
                        dbCommand,
                        DALManager.BuildSqlStringParameterName("pOwner"),
                        DbType.AnsiString,
                        200,
                        ParameterDirection.Input,
                        true,
                        0,
                        0,
                        string.Empty,
                        DataRowVersion.Current,
                        OwnerName);
                    dbDataReader = DALManager.ExecuteReader(dbCommand);
                    dbDataAdapter.FillFromReader(Relationships, dbDataReader);

                    dbDataReader.Close();
                }
                returnDataSet.Tables.Add(TableColumns);
                returnDataSet.Tables.Add(Relationships);
            }
            catch (Exception ex)
            {
            }

            return returnDataSet;
        }

        /// <summary>
        /// Populate all the Owners in the database
        /// </summary>
        /// <returns>List of Owners </returns>
        public override DataTable GetUnPublishedDatabases()
        {
            DataTable returnDataTable = new DataTable();
            try
            {
                string DatabaseOwnerListsQuery = @"
                    SELECT 	DISTINCT owner
                    FROM 	dba_objects
                    WHERE 	owner NOT IN('SYS', 'SYSTEM')
                    AND 	owner NOT IN(SELECT name FROM " + _coeSchemaTableName + @")
                    ORDER BY owner";

                DbCommand dbCommand = DALManager.Database.GetSqlStringCommand(DatabaseOwnerListsQuery);

                IDataReader dbDataReader = DALManager.ExecuteReader(dbCommand);
                using (DataReaderAdapter dbDataAdapter = new DataReaderAdapter())
                {
                    dbDataAdapter.FillFromReader(returnDataTable, dbDataReader);
                    dbDataReader.Close();
                }

            }
            catch (Exception ex)
            {
            }
            return returnDataTable;
        }

        public override void RevokeProxy(string schemaName)
        {
            string sql = string.Empty;
            int recordsAffected = 0;
            //We won't disallow COEDB, REGDB, CHEMACXDB, CHEMINVDB2 from connecting, ever.
            if (schemaName.ToUpper() != Resources.CentralizedStorageDB && schemaName.ToUpper() != Resources.RegistrationDatabaseName
                && schemaName.ToUpper() != Resources.ChemACXDatabaseName && schemaName.ToUpper() != Resources.ChemInvDB2DatabaseName)
            {
                try
                {
                    sql = "ALTER USER " + schemaName + " REVOKE CONNECT THROUGH COEUSER";
                    DbCommand dbCommand = DALManager.Database.GetSqlStringCommand(sql);
                    recordsAffected = DALManager.ExecuteNonQuery(dbCommand);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public override void GrantProxy(string schemaName)
        {
            string sql = string.Empty;
            int recordsAffected = 0;

            try
            {
                sql = "ALTER USER " + schemaName + " GRANT CONNECT THROUGH COEUSER";
                DbCommand dbCommand = DALManager.Database.GetSqlStringCommand(sql);
                recordsAffected = DALManager.ExecuteNonQuery(dbCommand);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override int RemovePrivileges(string schemaName)
        {
            string sql = string.Empty;
            int recordsAffected = -1;

            sql = "declare";
            sql += "\nexistsRole number(1) := 0;";
            sql += "\nexistsDocMgr number(1) := 0;";		//CBOE-1793 
            sql += "\nbegin";
            //CBOE-1793 In docmgr scripts, DOCMANAGER_BROWSER role is by default present. This is conflicting with DVM's publish/
            // unpublish functionality. Here we have to remove browser & admin roles added by DVM but we will keep browser role 
            // added by docmanager scripts. ASV 12SEP13
            if (schemaName.ToUpper() == "DOCMGR")
            {
                sql += "\nSELECT COUNT(T1.PRIVILEGE_TABLE_INT_ID) into existsRole FROM " + Resources.SecurityDatabaseName + ".SECURITY_ROLES T1, " + Resources.SecurityDatabaseName + ".PRIVILEGE_TABLES T2 WHERE T1.PRIVILEGE_TABLE_INT_ID = T2.PRIVILEGE_TABLE_ID AND T1.COEIDENTIFIER = 'DocManager' AND T1.role_NAME = '" + schemaName + "_BROWSER';";
                sql += "\nif(existsRole < 1) then";
                sql += "\nselect count(*) into existsRole from dba_roles where role = '" + schemaName + "_BROWSER';";
                sql += "\nif(existsRole > 0) then";
                sql += "\n  EXECUTE IMMEDIATE ('DROP ROLE " + schemaName + "_BROWSER');";
                sql += "\nend if;";
                sql += "\nend if;";
                sql += "\nDELETE FROM " + Resources.SecurityDatabaseName + ".SECURITY_ROLES WHERE ROLE_NAME= '" + schemaName + "_BROWSER'" +
                    " AND PRIVILEGE_TABLE_INT_ID in (SELECT distinct T1.PRIVILEGE_TABLE_INT_ID FROM " + Resources.SecurityDatabaseName + ".SECURITY_ROLES T1," + Resources.SecurityDatabaseName + ".PRIVILEGE_TABLES T2 WHERE T1.PRIVILEGE_TABLE_INT_ID = T2.PRIVILEGE_TABLE_ID AND T1.COEIDENTIFIER = 'DOCMGR' AND T1.role_NAME = '" + schemaName + "_BROWSER');";
            }
            else
            {
                sql += "\nselect count(*) into existsRole from dba_roles where role = '" + schemaName + "_BROWSER';";
                sql += "\nif(existsRole > 0) then";
                sql += "\n  EXECUTE IMMEDIATE ('DROP ROLE " + schemaName + "_BROWSER');";
                sql += "\nend if;";
                sql += "\nDELETE FROM " + Resources.SecurityDatabaseName + ".SECURITY_ROLES WHERE ROLE_NAME= '" + schemaName + "_BROWSER';";
            }

            sql += "\nDELETE FROM " + Resources.SecurityDatabaseName + ".COE_PRIVILEGES WHERE ROLE_INTERNAL_ID IN (select ROLE_ID FROM " + Resources.SecurityDatabaseName + ".security_roles where role_name = '" + schemaName + "_BROWSER');";
            sql += "\nselect count(*) into existsRole from dba_roles where role = '" + schemaName + "_ADMIN';";
            sql += "\nif(existsRole > 0) then";
            sql += "\n  EXECUTE IMMEDIATE ('DROP ROLE " + schemaName + "_ADMIN');";
            sql += "\nend if;";
            sql += "\nDELETE FROM " + Resources.SecurityDatabaseName + ".COE_PRIVILEGES WHERE ROLE_INTERNAL_ID IN (select ROLE_ID FROM " + Resources.SecurityDatabaseName + ".security_roles where role_name = '" + schemaName + "_ADMIN');";
            sql += "\nDELETE FROM " + Resources.SecurityDatabaseName + ".SECURITY_ROLES WHERE ROLE_NAME= '" + schemaName + "_ADMIN';";
            sql += "end;";
            try
            {
                DbCommand dbCommand = DALManager.Database.GetSqlStringCommand(sql);
                recordsAffected = DALManager.ExecuteNonQuery(dbCommand);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return recordsAffected;
        }

        public override int InsertPrivileges(string schemaName)
        {
            CheckSecurityPrivilege();
            StringBuilder sql = new StringBuilder("declare ");
            sql.Append("schemaBrowserExists number(1) := 0; ");
            sql.Append("schemaAdminExists number(1) := 0; ");
            sql.Append("begin ");
            sql.Append("select count(*) into schemaBrowserExists from dba_roles where role = '" + schemaName + @"_BROWSER'; ");
            sql.Append("if(schemaBrowserExists < 1) then ");
            sql.Append("EXECUTE IMMEDIATE ('CREATE ROLE " + schemaName + @"_BROWSER NOT IDENTIFIED'); ");
            sql.Append("end if; ");
            sql.Append("EXECUTE IMMEDIATE ('GRANT ''CONNECT'' TO " + schemaName + @"_BROWSER'); ");
            sql.Append("EXECUTE IMMEDIATE ('GRANT CSS_USER TO " + schemaName + @"_BROWSER'); ");
            sql.Append("EXECUTE IMMEDIATE ('GRANT " + schemaName + @"_BROWSER TO CSS_ADMIN'); ");
            sql.Append("INSERT INTO " + Resources.SecurityDatabaseName + @".SECURITY_ROLES (ROLE_ID,PRIVILEGE_TABLE_INT_ID,ROLE_NAME,COEIDENTIFIER) VALUES(" + Resources.SecurityDatabaseName + @".SECURITY_ROLES_SEQ.NEXTVAL,(select privilege_table_id from " + Resources.SecurityDatabaseName + @".privilege_tables where privilege_table_name='COE_PRIVILEGES'),'" + schemaName + @"_BROWSER','" + schemaName + @"'); ");
            sql.Append("INSERT INTO " + Resources.SecurityDatabaseName + @".COE_PRIVILEGES(ROLE_INTERNAL_ID,CAN_BROWSE, CAN_UPDATE,CAN_INSERT,CAN_DELETE) VALUES((select max(role_id) from " + Resources.SecurityDatabaseName + @".security_roles),'1','0','0','0'); ");
            sql.Append("select count(*) into schemaAdminExists from dba_roles where role = '" + schemaName + @"_ADMIN'; ");
            sql.Append("if(schemaAdminExists < 1) then ");
            sql.Append("EXECUTE IMMEDIATE ('CREATE ROLE " + schemaName + @"_ADMIN NOT IDENTIFIED'); ");
            sql.Append("end if; ");
            sql.Append("EXECUTE IMMEDIATE ('GRANT ''CONNECT'' TO " + schemaName + @"_ADMIN'); ");
            sql.Append("EXECUTE IMMEDIATE ('GRANT CSS_USER TO " + schemaName + @"_ADMIN'); ");
            sql.Append("EXECUTE IMMEDIATE ('GRANT " + schemaName + @"_ADMIN TO CSS_ADMIN'); ");
            sql.Append("INSERT INTO " + Resources.SecurityDatabaseName + @".SECURITY_ROLES (ROLE_ID,PRIVILEGE_TABLE_INT_ID,ROLE_NAME,COEIDENTIFIER) VALUES(" + Resources.SecurityDatabaseName + @".SECURITY_ROLES_SEQ.NEXTVAL,(select privilege_table_id from " + Resources.SecurityDatabaseName + @".privilege_tables where privilege_table_name='COE_PRIVILEGES'),'" + schemaName + @"_ADMIN','" + schemaName + @"'); ");
            sql.Append("INSERT INTO " + Resources.SecurityDatabaseName + @".COE_PRIVILEGES(ROLE_INTERNAL_ID,CAN_BROWSE, CAN_UPDATE,CAN_INSERT,CAN_DELETE) VALUES((select max(role_id) from " + Resources.SecurityDatabaseName + @".security_roles),'1','1','1','1'); ");
            sql.Append("end;");
            int recordsAffected = -1;
            try
            {
                DbCommand dbCommand = DALManager.Database.GetSqlStringCommand(sql.ToString());
                recordsAffected = DALManager.ExecuteNonQuery(dbCommand);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return recordsAffected;
        }

        private void CheckSecurityPrivilege()
        {
            try
            {
                string sql = string.Empty;
                int privilege_table_id = -1;
                DbCommand dbCommand = null;
                sql = "SELECT privilege_table_id  FROM COEDB.privilege_tables  WHERE privilege_table_name='COE_PRIVILEGES'";
                dbCommand = DALManager.Database.GetSqlStringCommand(sql);
                privilege_table_id = Convert.ToInt32(DALManager.ExecuteScalar(dbCommand));
                if (privilege_table_id <= 0)
                {
                    sql = string.Empty;
                    sql = "INSERT INTO COEDB.PRIVILEGE_TABLES (PRIVILEGE_TABLE_ID,APP_NAME, APP_URL,PRIVILEGE_TABLE_NAME,TABLE_SPACE)";
                    sql += "values(PRIVILEGE_TABLES_seq.NextVal,'COE', '', 'COE_PRIVILEGES', 'T_COE')";
                    dbCommand = DALManager.Database.GetSqlStringCommand(sql);
                    DALManager.ExecuteNonQuery(dbCommand);
                }
            }
            catch
            {
                throw;
            }
        }

        public override DataTable GetIndexTypeFields(string database)
        {
            DataTable returnDataTable = new DataTable();
            try
            {
                string sql = "SELECT table_name, column_name FROM " + ConfigurationUtilities.GetChemEngineSchema(database) + ".all_csc_indexes WHERE owner = " + DALManager.BuildSqlStringParameterName("database");
                DbCommand dbCommand = DALManager.Database.GetSqlStringCommand(sql);
                DALManager.Database.AddInParameter(dbCommand, DALManager.BuildSqlStringParameterName("database"), DbType.AnsiString, database);

                IDataReader dbDataReader = DALManager.ExecuteReader(dbCommand);
                using (DataReaderAdapter dbDataAdapter = new DataReaderAdapter())
                {
                    dbDataAdapter.FillFromReader(returnDataTable, dbDataReader);
                    dbDataReader.Close();
                }
            }
            catch (Exception ex)
            {
                returnDataTable = null;
            }

            return returnDataTable;
        }

        /// <summary>
        /// Retrieves the index information from the selected database.
        /// </summary>
        /// <param name="database">Name of the database, used as database owner.</param>
        /// <returns>Returns datatable containing information about the index fields and columns on the selected database.</returns>
        public override DataTable GetIndexFields(string database)
        {
            DataTable returnDataTable = new DataTable();
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT AI.index_name AS indexName, AIC.column_name, AI.table_name FROM all_indexes AI ");
                sql.Append("INNER JOIN all_ind_columns AIC ON AI.table_name= AIC.table_name AND ");
                sql.Append("AI.index_name = AIC.index_name and AI.table_owner= AIC.table_owner ");
                sql.AppendFormat("where AI.table_owner = {0}", DALManager.BuildSqlStringParameterName("database"));

                DbCommand dbCommand = DALManager.Database.GetSqlStringCommand(sql.ToString());
                DALManager.Database.AddInParameter(dbCommand, DALManager.BuildSqlStringParameterName("database"), DbType.AnsiString, database);

                IDataReader dbDataReader = DALManager.ExecuteReader(dbCommand);
                using (DataReaderAdapter dbDataAdapter = new DataReaderAdapter())
                {
                    dbDataAdapter.FillFromReader(returnDataTable, dbDataReader);
                    dbDataReader.Close();
                }
            }
            catch (Exception ex)
            {
                returnDataTable = null;
            }

            return returnDataTable;
        }

        /// <summary>
        /// Retrieves the index information from the selected database and table.
        /// </summary>
        /// <param name="database">Name of the database, used as database owner.</param>
        /// <param name="table">Name of the table to find the index information</param>
        /// <returns>Returns datatable containing information about the index fields and columns on the selected database.</returns>
        public override DataTable GetIndexFields(string database, string table)
        {
            DataTable returnDataTable = new DataTable();
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT AI.index_name AS indexName, AIC.column_name, AI.table_name FROM dba_indexes AI ");
                sql.Append("INNER JOIN dba_ind_columns AIC ON AI.table_name= AIC.table_name AND ");
                sql.Append("AI.index_name = AIC.index_name and AI.table_owner= AIC.table_owner ");
                sql.AppendFormat("WHERE AI.table_owner = {0} AND AI.table_name = {1}", DALManager.BuildSqlStringParameterName("database"), DALManager.BuildSqlStringParameterName("tableName"));

                DbCommand dbCommand = DALManager.Database.GetSqlStringCommand(sql.ToString());
                DALManager.Database.AddInParameter(dbCommand, DALManager.BuildSqlStringParameterName("database"), DbType.AnsiString, database);
                DALManager.Database.AddInParameter(dbCommand, DALManager.BuildSqlStringParameterName("tableName"), DbType.AnsiString, table);

                IDataReader dbDataReader = DALManager.ExecuteReader(dbCommand);
                using (DataReaderAdapter dbDataAdapter = new DataReaderAdapter())
                {
                    dbDataAdapter.FillFromReader(returnDataTable, dbDataReader);
                    dbDataReader.Close();
                }
            }
            catch (Exception ex)
            {
                returnDataTable = null;
            }
            return returnDataTable;
        }

        /// <summary>
        /// Function for create index on database
        /// </summary>
        /// <param name="DatabaseName">Name of the database</param>
        /// <param name="TableName">Name of Table</param>
        /// <param name="ColumnName">Name of column on which index to be created</param>
        /// <returns>success of index creation</returns>
        public override Boolean CreateIndex(string DatabaseName, string TableName, string ColumnName)
        {
            string sql = null;
            DbCommand dbCommand = null;
            string sIndexName = TableName + "_" + ColumnName + "IX";
            if (sIndexName.Length > 30)   //Oracle limitation : index name length must be less than 30 chars
            {
                sIndexName = ColumnName.Length <= 28 ? ColumnName + "IX" : ColumnName.Remove(28) + "IX";
            }

            sql = "CREATE INDEX \"" + DatabaseName + "\".\"" + sIndexName + "\" ON \"" +
                DatabaseName + "\".\"" + TableName + "\" (\"" + ColumnName + "\")";
            dbCommand = DALManager.Database.GetSqlStringCommand(sql);
            try
            {
                DALManager.ExecuteNonQuery(dbCommand);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return false;
        }

        internal override int InsertCOEDatabaseDataView(string ownerName, string serializedCOEDataView)
        {
            //retrieving the id to be inserted
            int id = GetNewID();
            string sql = "INSERT INTO " + _coeSchemaTableName +
                 "(ID,NAME,COEDATAVIEW,DATE_CREATED)VALUES " +
                 "( " + DALManager.BuildSqlStringParameterName("pId") +
                 " ," + DALManager.BuildSqlStringParameterName("pName") +
                 " , " + DALManager.BuildSqlStringParameterName("pCOEDataView") +
                 " , " + DALManager.BuildSqlStringParameterName("pDateCreated") + ")";

            OracleCommand dbCommand = (OracleCommand)DALManager.Database.GetSqlStringCommand(sql);

            dbCommand.Parameters.Add("pId", OracleDbType.Int32, id, ParameterDirection.Input);
            dbCommand.Parameters.Add("pName", OracleDbType.Varchar2, ownerName.ToUpper(), ParameterDirection.Input);
            dbCommand.Parameters.Add("pCOEDataView", OracleDbType.XmlType, serializedCOEDataView, ParameterDirection.Input);
            dbCommand.Parameters.Add("pDateCreated", OracleDbType.Date, DateTime.Now, ParameterDirection.Input);
            int recordsAffected = -1;
            try
            {
                recordsAffected = DALManager.ExecuteNonQuery(dbCommand);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return id;
        }

        public override SafeDataReader GetCOEDatabaseDataView(string ownerName)
        {
            SafeDataReader safeReader = null;
            string sql = "SELECT d.id, d.name, d.coedataview.getClobVal() COEDATAVIEW, d.date_created FROM " + _coeSchemaTableName + " d WHERE d.name=" + DALManager.BuildSqlStringParameterName("pOwnerName");
            DbCommand dbCommand = DALManager.Database.GetSqlStringCommand(sql);
            DALManager.Database.AddParameter(dbCommand, "pOwnerName", DbType.AnsiString, 200, ParameterDirection.Input, true, 0, 0, string.Empty, DataRowVersion.Current, ownerName);
            try
            {
                safeReader = new SafeDataReader(DALManager.Database.ExecuteReader(dbCommand));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return safeReader;
        }

        public override SafeDataReader GetPublishedDatabases()
        {
            DbCommand dbCommand = null;
            string sql = "SELECT D.ID, D.NAME, D.DATE_CREATED, D.COEDATAVIEW.getClobVal() COEDATAVIEW FROM " + _coeSchemaTableName + " D";
            dbCommand = DALManager.Database.GetSqlStringCommand(sql);
            SafeDataReader safeReader = new SafeDataReader(DALManager.Database.ExecuteReader(dbCommand));
            return safeReader;
        }        

        private void DropSequence(OracleConnection conn, OracleTransaction tran, string sequenceName)
        {
            string sql = "DROP SEQUENCE " + sequenceName;

            OracleCommand dbCommand = CreateOracleCommand(conn, tran, sql);

            dbCommand.ExecuteNonQuery();
        }

        private void TryDropTable(OracleConnection conn, OracleTransaction tran, string tableName)
        {
            try
            {
                string sql = "DROP TABLE " + tableName;

                OracleCommand dbCommand = CreateOracleCommand(conn, tran, sql);

                dbCommand.ExecuteNonQuery();
            }
            catch (OracleException oe)
            {
                if (!oe.Message.Contains("ORA-00942"))
                {
                    throw;
                }
            }
        }

        private OracleCommand CreateOracleCommand(OracleConnection conn, OracleTransaction tran, string sql)
        {
            OracleCommand dbCommand = new OracleCommand(sql);
            dbCommand.Connection = conn;
            dbCommand.Transaction = tran;

            return dbCommand;
        }

        internal override void UpdateCOEDatabaseDataView(string ownerName, string serializedCOEDataView, DateTime dateModified)
        {
            string sql = "UPDATE " + _coeSchemaTableName +
                 " SET COEDATAVIEW= " + DALManager.BuildSqlStringParameterName("pCOEDataView") +
                 " ,DATE_CREATED= " + DALManager.BuildSqlStringParameterName("pDateCreated") +
                 " WHERE Name= " + DALManager.BuildSqlStringParameterName("pOwnerName");

            OracleCommand dbCommand = (OracleCommand)DALManager.Database.GetSqlStringCommand(sql);

            dbCommand.Parameters.Add("pCOEDataView", OracleDbType.XmlType, serializedCOEDataView, ParameterDirection.Input);
            dbCommand.Parameters.Add("pDateCreated", OracleDbType.Date, DateTime.Now, ParameterDirection.Input);
            dbCommand.Parameters.Add("pOwnerName", OracleDbType.Varchar2, ownerName.ToUpper(), ParameterDirection.Input);
            int recordsAffected = -1;

            try
            {
                recordsAffected = DALManager.ExecuteNonQuery(dbCommand);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }       
        
    }
}