using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using CambridgeSoft.COE.Framework.Common;
using System.Data;//Jerry
using System.Xml;
using CambridgeSoft.COE.Framework.COELoggingService;

namespace CambridgeSoft.COE.Framework.COEExportService
{
    internal class SDFFlatFileUncorrelated : FormatterBase, IFormatter
    {
        [NonSerialized]
        static COELog _coeLog = COELog.GetSingleton("COEExport");
        /// <summary>
        /// Overridden ModifyResultsCriteria to format structure fields to return molfiles
        /// </summary>
        /// <param name="resultsCriteria">originating results criteria object</param>
        /// <returns>results critiria modified to include criteria itme in desired format</returns>
        protected override void Modify(ResultsCriteria.ResultsCriteriaTable resultsCriteriaTable, int fieldID)
        {
            ResultsCriteria.CDXToMolFile newCriteria = new ResultsCriteria.CDXToMolFile(fieldID);
            newCriteria.Alias = "CDXTOMol_" + fieldID;
            bool alreadyExists = false;

            foreach (ResultsCriteria.IResultsCriteriaBase rc in resultsCriteriaTable.Criterias)
            {
                if (rc.Alias == newCriteria.Alias)
                {
                    alreadyExists = true;
                    break;
                }
            }

            if (!alreadyExists)
                resultsCriteriaTable.Criterias.Add(newCriteria);
        }


        /// <summary>
        /// reshape a dataset as an SDF flatfile that is uncorrelated
        /// </summary>
        /// <param name="dataSet">dataset to reshape</param>
        /// <returns>a sdf flat file containing molfiles for structures</returns>
        public string FormatDataSet(System.Data.DataSet dataSet, COEDataView dataView, ResultsCriteria resultCriteria)
        {
            resultCriteria = resultCriteria.RemoveGrandChild(dataView, resultCriteria);
            Common.SqlGenerator.MetaData.DataView _dataView = new CambridgeSoft.COE.Framework.Common.SqlGenerator.MetaData.DataView();
            _dataView.LoadFromXML(dataView.ToString());
            Common.SqlGenerator.MetaData.ResultsCriteria _resultCriteria = new CambridgeSoft.COE.Framework.Common.SqlGenerator.MetaData.ResultsCriteria();
            _resultCriteria.LoadFromXML(resultCriteria.ToString());

            XmlDocument _xmlDocument = new XmlDocument();
            _xmlDocument.LoadXml(resultCriteria.ToString());
            List<List<string>> tableFiledsIdList = new List<List<string>>();
            XmlNodeList tabXmlNodeList = _xmlDocument.DocumentElement.ChildNodes[0].ChildNodes;


            foreach (XmlElement tabElement in tabXmlNodeList)
            {
                if (!tabElement.Attributes["id"].Value.Trim().Equals(_dataView.GetBaseTableId().ToString()))
                {
                    XmlNodeList fieldXmlNodeList = tabElement.ChildNodes;
                    List<string> filedsIdList = new List<string>();
                    foreach (XmlElement fieldElement in fieldXmlNodeList)
                    {
                        string sFieldName = GetChildFieldName(fieldElement, _dataView);
                        if (!String.IsNullOrEmpty(sFieldName))
                            filedsIdList.Add(sFieldName.Trim());
                    }
                    tableFiledsIdList.Add(filedsIdList);
                }
            }

            List<int> tablesIdList = new List<int>();
            List<int> childTableIdList = new List<int>();
            List<int>[] tablesID = _resultCriteria.GetTableIds(_dataView);
            for (int i = 0; i < tablesID.Length; i++)
            {
                tablesIdList.Add(int.Parse(tablesID[i][0].ToString()));
            }

            for (int itemIndex = 0; itemIndex < tablesID.Length; itemIndex++)
            {
                if (tablesID[itemIndex][0] != _dataView.GetBaseTableId())
                    childTableIdList.Add(int.Parse(tablesID[itemIndex][0].ToString()));
            }

            string baseTableName = "Table_" + _dataView.GetBaseTableId().ToString();
            string fulBaseTabName = _dataView.GetTableName(_dataView.GetBaseTableId());
            DataTable baseDatatab = dataSet.Tables[baseTableName];

            int btColumnCount = baseDatatab.Columns.Count;
            List<string> btColNameList = new List<string>();
            int btRecordIndex = 1;

            DataTable filtChildDt = new DataTable();
            DataRow[] filtDataRow;
            List<string> tempList = new List<string>();
            string st_resultCriteriahdTabName = string.Empty;
            string strTemp = string.Empty;
            string strOutSDF = string.Empty;
            string strCDXTOMOL = string.Empty;

            int cdxColIndex = -1;
            for (int btColumnIndex = 0; btColumnIndex < btColumnCount; btColumnIndex++)
            {
                String colname = baseDatatab.Columns[btColumnIndex].ColumnName;
                btColNameList.Add(colname);
                if (colname.Contains("CDXTOMol_"))
                    cdxColIndex = btColumnIndex;
            }

            //loop the base table row
            for (int btRowIndex = 0; btRowIndex < dataSet.Tables[baseTableName].Rows.Count; btRowIndex++, btRecordIndex++)
            {
                //save the base table first row info to a list
                /* CSBR-161868 Log file information is not correct or appropriate when we Import an Exported file from CBV     
                   Checking whether the structure is available or not, If available then strCDXTOMOL will hold it 
                   otherwise it will hold the value in the value in else */
                if (cdxColIndex != -1 && !String.IsNullOrEmpty(baseDatatab.Rows[btRowIndex][cdxColIndex].ToString())) //Fixed CSBR-161868  
                    strCDXTOMOL = baseDatatab.Rows[btRowIndex][cdxColIndex].ToString(); //+ "\r\n";//\r\n Fixecd CSBR-166992, CSBR-166995 
                else
                    strCDXTOMOL = "\r\n" + "CsStruct  NA" + "\r\n\r\n" + "  0  0  0  0  0  0  0  0  0  0999 V2000" + "\r\n" + "M  END" + "\r\n"; //Fixed CSBR-158168, CSBR-166992, CSBR-166995

                for (int colNamLstItmIndex = 0; colNamLstItmIndex < btColNameList.Count; colNamLstItmIndex++)
                {
                    if (colNamLstItmIndex != cdxColIndex)
                        strTemp = strTemp + ">  <" + fulBaseTabName + "." + baseDatatab.Columns[colNamLstItmIndex].ColumnName + "> (" + btRecordIndex + ")\r\n" + baseDatatab.Rows[btRowIndex][colNamLstItmIndex] + "\r\n\r\n";
                }
                if (childTableIdList.Count > 0)
                {
                    for (int chdTabIdIndex = 0; chdTabIdIndex < childTableIdList.Count; chdTabIdIndex++)
                    {
						// CBOE-303, CBOE-1763, CBOE-1764 get the table alias name instead of table name
                        string childTableName = _dataView.GetTableAliasName(childTableIdList[chdTabIdIndex]);
                        //filt the child table
                        st_resultCriteriahdTabName = "Table_" + childTableIdList[chdTabIdIndex].ToString();
                        /* CBOE-311 Fuji -- Included single quote at RHS */
                        filtDataRow = dataSet.Tables[st_resultCriteriahdTabName].Select(String.Concat("[", dataSet.Tables[st_resultCriteriahdTabName].ParentRelations[0].ChildColumns[0].ColumnName, "]") + "= '" + baseDatatab.Rows[btRowIndex][dataSet.Tables[st_resultCriteriahdTabName].ParentRelations[0].ParentColumns[0].ColumnName] + "'" );
                        filtChildDt = dataSet.Tables[st_resultCriteriahdTabName].Clone();
                        foreach (DataRow dr in filtDataRow)
                        {
                            filtChildDt.ImportRow(dr);
                        }
                        if (filtChildDt.Rows.Count > 0)
                        {
                            for (int filtChildDtIndex = 0; filtChildDtIndex < filtChildDt.Rows.Count; filtChildDtIndex++)
                            {
                                string rowStr = string.Empty;

                                for (int colIndex = 0; colIndex < filtChildDt.Columns.Count; colIndex++)
                                {
                                    rowStr = rowStr + ">  <" + childTableName + "." + filtChildDt.Columns[colIndex].ColumnName + "> (" + btRecordIndex + ")\r\n" + filtChildDt.Rows[filtChildDtIndex][colIndex] + "\r\n\r\n";
                                }
                                strOutSDF = strOutSDF + strCDXTOMOL + strTemp + rowStr + "$$$$\r\n"; //Fixed CSBR-166992 and CSBR-166995
                            }
                        }
                        else
                        {
                            string rowStr = string.Empty;

                            for (int colIndex = 0; colIndex < filtChildDt.Columns.Count; colIndex++)
                            {
                                rowStr = rowStr + ">  <" + childTableName + "." + filtChildDt.Columns[colIndex].ColumnName + "> (" + btRecordIndex + ")\r\n \r\n\r\n";
                                
                            }
                            strOutSDF = strOutSDF + strCDXTOMOL + strTemp + rowStr + "$$$$\r\n"; //Fixed CSBR-166992 and CSBR-166995
                        }

                        filtDataRow = null;
                    }
                }
                else
                    strOutSDF = strOutSDF + strCDXTOMOL + strTemp + "$$$$\r\n"; //Fixed CSBR-166992 and CSBR-166995
                strTemp = string.Empty;
            }
            strOutSDF = strOutSDF.TrimEnd('\r', '\n'); // Fixed 160850
            return strOutSDF;

        }
    }
}