﻿// The following code was generated by Microsoft Visual Studio 2005.
// The test owner should check each test for validity.
using NUnit.Framework;
using System;
using System.Text;
using System.Collections.Generic;
using System.Xml;
using System.Web.Services.Protocols;
using CambridgeSoft.COE.Framework.Common;
using System.Data;
using System.IO;
using CambridgeSoft.COE.Framework.NUnitTests.FrameworkProxy;
using System.Xml.Serialization;
using CambridgeSoft.COE.Framework.COEDatabasePublishingService;

namespace CambridgeSoft.COE.Framework.NUnitTests
{
    /// <summary>
    ///This is a test class for CambridgeSoft.COE.Framework.Web.COEFrameworkServices and is intended
    ///to contain all CambridgeSoft.COE.Framework.Web.COEFrameworkServices Unit Tests
    ///</summary>
    [TestFixture]
    public class COEFrameworkServicesTest
    {
        private static string _pathToDataviewXmls = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.IndexOf("CambridgeSoft.COE.Framework.NUnitTests")) + @"CambridgeSoft.COE.Framework.NUnitTests" + @"\TestXML";
        private static string _pathToSearchXmls = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.IndexOf("CambridgeSoft.COE.Framework.NUnitTests")) + @"CambridgeSoft.COE.Framework.NUnitTests" + @"\Search Tests\COESearchTest XML\OrderedSearchTestXml";
        private static FrameworkProxy.COECredentials _credentials = new FrameworkProxy.COECredentials();
        private const string USERNAME = "cssadmin";
        private const string PASSWORD = "cssadmin";
        private const string SERVICEURL = "http://localhost/COEFrameworkServices/COEFrameworkServices.asmx";
        private FrameworkProxy.COEFrameworkServices _service = new FrameworkProxy.COEFrameworkServices();
        private static string _searchCriteria;
        private static string _resultsCriteria;
        private static string _pagingInfo;
        private static string _dataView;

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //
        [TestFixtureSetUp]
        public static void MyClassInitialize()
        {
            _credentials.UserName = USERNAME;
            _credentials.Password = PASSWORD;

            _searchCriteria = GetSearchCriteria();
            _resultsCriteria = GetResultsCriteria();
            _dataView = GetDataView();
            _pagingInfo = GetFullPagingInfo();
        }
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //
        //[TestFixtureTearDown]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //
        [SetUp]
        public void MyTestInitialize()
        {
            _service.COECredentialsValue = _credentials;
        }
        //
        //Use TestCleanup to run code after each test has run
        //
        //[TearDown]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        #region Search
        /// <summary>
        ///A test for DoSearch (string, string, string, string)
        ///</summary>
        [Test]
        public void DoSearchTest()
        {
            string searchResponseStr = _service.DoSearch(_searchCriteria, _resultsCriteria, _pagingInfo, _dataView);
            SearchResponse response = new SearchResponse();
            response.GetFromXml(searchResponseStr);
            
            Assert.IsTrue(response.ResultsDataSet != null);

            foreach(DataRow row in response.ResultsDataSet.Tables[0].Rows)
            {
                Console.WriteLine(row["MolWeight"].ToString());
            }
        }

        /// <summary>
        ///A test for GetData (string, string, string)
        ///</summary>
        [Test]
        public void GetDataTest()
        {
            PagingInfo pi = new PagingInfo();
            int start = 1;
            int rowNum = 1;

            pi.Start = start;
            pi.RecordCount = 10;
            string searchResponseStr = _service.DoSearch(_searchCriteria, _resultsCriteria, pi.ToString(), _dataView);

            SearchResponse response = new SearchResponse();
            response.GetFromXml(searchResponseStr);

            Assert.IsTrue(response.ResultsDataSet != null);

            foreach(DataRow row in response.ResultsDataSet.Tables[0].Rows)
            {
                Console.WriteLine(rowNum++.ToString() + " " + row["MolWeight"].ToString());
            }
            pi.HitListID = response.PagingInfo.HitListID;
            pi.HitListType = response.PagingInfo.HitListType;
            start += pi.RecordCount;
            Console.WriteLine("Page Break... \n\n");
            while(start < 300)
            {
                pi.Start = start;
                pi.RecordCount = 10;
                string dataSetStr = _service.GetData(_resultsCriteria, pi.ToString(), _dataView);
                StringReader reader = new StringReader(dataSetStr);
                DataSet ds = new DataSet();
                ds.ReadXml(reader);

                Assert.IsTrue(ds != null);

                foreach(DataRow row in ds.Tables[0].Rows)
                {
                    Console.WriteLine(rowNum++.ToString() + " " + row["MolWeight"].ToString());
                }
                start += pi.RecordCount;
                Console.WriteLine("Page Break... \n\n");
            }
        }

        /// <summary>
        ///A test for GetFilteredData (string, string, string,string,bool)
        ///</summary>
        [Test]
        public void GetFilteredDataTest()
        {
            PagingInfo pi = new PagingInfo();
            int start = 1;
            int rowNum = 1;

            while (start < 300)
            {
                pi.Start = start;
                pi.RecordCount = 10;

                string dataSetStr = _service.GetFilteredData(_searchCriteria, _resultsCriteria, pi.ToString(), _dataView, true);
                StringReader reader = new StringReader(dataSetStr);
                DataSet ds = new DataSet();
                ds.ReadXml(reader);

                Assert.IsTrue(ds != null);
                if (start == 1)
                {
                    Console.WriteLine("******   " + ds.Tables[0].TableName + "   ******");
                    Console.WriteLine();
                    Console.WriteLine("ROW # \t MOL ID \t MolWeight \t MOL NAME");
                    Console.WriteLine();
                }

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    Console.WriteLine(rowNum++.ToString() + "\t" + row["MOL_ID"].ToString() + "\t" + row["MolWeight"].ToString() + "      \t" + row["MOLNAME"].ToString());
                }
                start += pi.RecordCount;
                Console.WriteLine("\t\t\t\t\t\tPage Break...");
            }
        }

        [Test]
        public void DoSearchSimpleTest()
        {
            SearchInput input = new SearchInput();
            input.FieldCriteria = new string[] { "MOLTABLE.MOLID<5" };
            string[] resultFields = new string[] { "MOLTABLE.MOLID", "MOLTABLE.STRUCTURE", "SYNONYMS.CHEMNAME"};
            int dvId = 5002;
            ResultPageInfo pageInfo = new ResultPageInfo();
            pageInfo.Start = 1;
            pageInfo.PageSize = 10;

            DataResult result = _service.DoSearchSimple(input, resultFields, pageInfo, dvId);

            Assert.IsTrue(result.ResultSet.Length > 0, "DoSearchSimpleTest failed");
            Assert.AreEqual(result.resultPageInfo.PageSize, 4, "DoSearchSimpleTest failed");
        }

        [Test]
        public void GetIdsSimpleTest()
        {
            SearchInput input = new SearchInput();
            input.FieldCriteria = new string[] { "STRUCTURES.BASE64_CDX SUBSTRUCTURE C1CCC1" };
            string pkField = "STRUCTURES.U_ID";
            int dvId = 8001;
            ResultPageInfo pageInfo = new ResultPageInfo();
            pageInfo.Start = 1;
            pageInfo.PageSize = 10;

            DataListResult result = _service.GetIdsSimple(input, pkField, pageInfo, dvId);

            Assert.AreEqual(result.ResultList.Length, 1, "DoSearchSimpleTest failed");
            Assert.AreEqual(result.resultPageInfo.PageSize, 1, "DoSearchSimpleTest failed");
        }

        [Test]
        public void GetDataPageSimpleTest()
        {
            //TODO: implement this test.
        }

        #region Private Methods
        private static string GetDataView()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(_pathToSearchXmls + @"\DataView.xml");
            return doc.OuterXml;
        }

        private static string GetResultsCriteria()
        {
            return GetResultsCriteria(@"\ResultsCriteria.xml");
        }

        private static string GetResultsCriteria(string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(_pathToSearchXmls + filename);
            return doc.OuterXml;
        }

        private static string GetSearchCriteria()
        {
            return GetSearchCriteria(@"\SearchCriteria.xml");
        }

        private static string GetSearchCriteria(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(_pathToSearchXmls + fileName);
            return doc.OuterXml;
        }

        private static string GetFullPagingInfo()
        {
            PagingInfo pi = new PagingInfo();
            pi.Start = 1;
            pi.RecordCount = 50000;
            return pi.ToString();
        }
        #endregion
        #endregion
        
        #region Dataview
        /// <summary>
        ///A test for InsertDataView (string, string, string, bool, int, string, string)
        ///</summary>
        [Test]
        public void InsertDataViewTest()
        {
            int dataviewid = _service.InsertDataView("CHEMINVDB2", "TEST" + GenerateRandomNumber(), "TEST", true, 5007, USERNAME, BuildCOEDataViewFromXML());
            Assert.IsTrue(dataviewid > 0, "COEFrameworkServicesTest.InsertDataViewTest failed.");

            _service.DeleteDataView(dataviewid);
        }

        /// <summary>
        ///A test for UpdateDataView (int, string, string, string, bool, int, string, string)
        ///</summary>
        [Test]
        public void UpdateDataViewTest()
        {
            string randomAppend = GenerateRandomNumber();

            int dataviewid = _service.InsertDataView("CHEMINVDB2", "TEST" + randomAppend, "TEST", true, 5007, USERNAME, BuildCOEDataViewFromXML());
            FrameworkProxy.COEDataView dataview = _service.GetDataView(dataviewid);
            int newBaseTable = --dataview.basetable;
            dataview.name = "TESTDATAVIEW" + randomAppend;
            _service.UpdateDataView(dataviewid, "CHEMINVDB2", "TESTDATAVIEW" + randomAppend, "TEST", true, 5007, USERNAME, ref dataview);

            Assert.AreEqual("TESTDATAVIEW" + randomAppend, dataview.name, "COEFrameworkServicesTest.UpdateDataViewTest failed.");
            Assert.AreEqual(newBaseTable, dataview.basetable, "COEFrameworkServicesTest.UpdateDataViewTest failed.");

            _service.DeleteDataView(dataviewid);
        }

        /// <summary>
        ///A test for DeleteDataView (int)
        ///</summary>
        [Test]
        public void DeleteDataViewTest()
        {
            int dataviewid = _service.InsertDataView("CHEMINVDB2", "TEST" + GenerateRandomNumber(), "TEST", true, 5007, USERNAME, BuildCOEDataViewFromXML());
            int deleted = _service.DeleteDataView(dataviewid);
            
            Assert.AreEqual(1, deleted, "COEFrameworkServicesTest.DeleteDataViewTest failed.");

            try
            {
                FrameworkProxy.COEDataView dv = _service.GetDataView(dataviewid);
            }
            catch(SoapException se)
            {
                if(!se.Message.Contains("DataView does not exist or logged in user lacks permissions to use the dataview"))
                    Assert.Fail("Expected exception was not thrown");
            }
        }

        /// <summary>
        ///A test for GetDataView (int)
        ///</summary>
        [Test]
        public void GetDataViewTest()
        {
            string randomAppend = GenerateRandomNumber();
            int dataviewid = _service.InsertDataView("CHEMINVDB2", "TEST" + randomAppend, "TEST", true, 5007, USERNAME, BuildCOEDataViewFromXML());
            FrameworkProxy.COEDataView dv = _service.GetDataView(dataviewid);
            Assert.IsTrue(dv != null, "COEFrameworkServicesTest.GetDataViewTest failed.");
            Assert.AreEqual("TEST" + randomAppend, dv.name, "COEFrameworkServicesTest.GetDataViewTest failed.");

            _service.DeleteDataView(dataviewid);
        }

        /// <summary>
        ///A test for GetDataViewDataListByDatabase (string)
        ///</summary>
        [Test]
        public void GetDataViewDataListByDatabaseTest()
        {
            FrameworkProxy.COEDataView[] dvList = _service.GetDataViewDataListByDatabase("CHEMINVDB2");
            Assert.IsTrue(dvList != null, "COEFrameworkServicesTest.GetDataViewDataListByDatabaseTest failed.");
            Assert.IsTrue(dvList.Length > 0, "COEFrameworkServicesTest.GetDataViewDataListByDatabaseTest failed.");
        }

        /// <summary>
        ///A test for GetDataViewDataListByUser (string, string)
        ///</summary>
        [Test]
        public void GetDataViewDataListByUserTest()
        {
            FrameworkProxy.COEDataView[] dvList = _service.GetDataViewDataListByUser("COEDB");
            Assert.IsTrue(dvList != null, "COEFrameworkServicesTest.GetDataViewDataListByUserTest failed.");
            Assert.IsTrue(dvList.Length > 0, "COEFrameworkServicesTest.GetDataViewDataListByUserTest failed.");
        }

        /// <summary>
        ///A test for GetDataViewListforAllDatabases ()
        ///</summary>
        [Test]
        public void GetDataViewListforAllDatabasesTest()
        {
            FrameworkProxy.COEDataView[] dvList = _service.GetDataViewListforAllDatabases();
            Assert.IsTrue(dvList != null, "COEFrameworkServicesTest.GetDataViewListforAllDatabasesTest failed.");
            Assert.IsTrue(dvList.Length > 0, "COEFrameworkServicesTest.GetDataViewListforAllDatabasesTest failed.");
        }


        /// <summary>
        /// PublishTableToDataviewTest
        /// </summary>
        [Test]
        public void PublishTableToDataviewTest()
        {
            FrameworkProxy.COEDataView dv = new FrameworkProxy.COEDataView();
            int dataviewid = 5000;
            string result = string.Empty;

            try
            {
                dv = Framework.Common.Utilities.XmlDeserialize<FrameworkProxy.COEDataView>(@"<?xml version=""1.0"" encoding=""utf-8""?><COEDataView xmlns=""COE.COEDataView"" basetable=""1429"" database=""COEDB"" dataviewid=""5000"">
  <tables>
    <table id=""1429"" name=""ASSAYEDCOMPOUNDS"" alias=""Samples"" database=""COETEST"" primaryKey=""1448"">
      <fields id=""1448"" name=""PUBCHEM_COMPOUND_CID"" alias=""PubChem CID"" dataType=""INTEGER"" indexType=""NONE"" mimeType=""NONE"" visible=""1"" sortOrder=""0"" />
      <fields id=""1430"" name=""BASE64_CDX"" alias=""Structure"" dataType=""TEXT"" indexType=""CS_CARTRIDGE"" mimeType=""NONE"" visible=""1"" sortOrder=""1"" />
      <fields id=""1457"" name=""PUBCHEM_MOLECULAR_FORMULA"" alias=""Formula"" dataType=""TEXT"" indexType=""NONE"" mimeType=""NONE"" visible=""1"" sortOrder=""2"" />
      <fields id=""1458"" name=""PUBCHEM_MOLECULAR_WEIGHT"" alias=""MW"" dataType=""REAL"" indexType=""NONE"" mimeType=""NONE"" visible=""1"" sortOrder=""3"" />
      <fields id=""1459"" name=""PUBCHEM_OPENEYE_CAN_SMILES"" alias=""Smiles"" dataType=""TEXT"" indexType=""NONE"" mimeType=""NONE"" visible=""1"" sortOrder=""4"" />
 		</table>
    <table id=""1901"" name=""AID629ERA"" alias=""ERalpha Primary Screen"" database=""COETEST"" primaryKey=""1906"" >
      <fields id=""1903"" name=""% Inhibition"" alias=""% Inhibition"" dataType=""REAL"" indexType=""NONE"" mimeType=""NONE"" visible=""1"" />
      <fields id=""1904"" name=""PUBCHEM_ACTIVITY_OUTCOME"" alias=""Active?"" dataType=""INTEGER"" indexType=""NONE"" mimeType=""NONE"" visible=""1"" />
      <fields id=""1905"" name=""PUBCHEM_ACTIVITY_SCORE"" alias=""Score"" dataType=""INTEGER"" indexType=""NONE"" mimeType=""NONE"" visible=""1"" />
      <fields id=""1906"" name=""PUBCHEM_CID"" alias=""PubChem CID"" dataType=""INTEGER"" indexType=""NONE"" mimeType=""NONE"" visible=""1"" />
    </table>
    <table id=""1879"" name=""AID1078ERA"" alias=""ERalpha Dose Response"" database=""COETEST"" primaryKey=""1884"">
      <fields id=""1881"" name=""EC50 (1 nM E2)"" alias=""EC50 (1 nM E2)"" dataType=""REAL"" indexType=""NONE"" mimeType=""NONE"" visible=""1"" sortOrder=""1"" />
      <fields id=""1882"" name=""PUBCHEM_ACTIVITY_OUTCOME"" alias=""Active?"" dataType=""INTEGER"" indexType=""NONE"" mimeType=""NONE"" visible=""1"" sortOrder=""2"" />
      <fields id=""1883"" name=""PUBCHEM_ACTIVITY_SCORE"" alias=""Score"" dataType=""INTEGER"" indexType=""NONE"" mimeType=""NONE"" visible=""1"" sortOrder=""3"" />
      <fields id=""1884"" name=""PUBCHEM_CID"" alias=""PubChem CID"" dataType=""INTEGER"" indexType=""NONE"" mimeType=""NONE"" visible=""1"" sortOrder=""4"" />
    </table>
  </tables>
  <relationships>
    <relationship parentkey=""1448"" childkey=""1906"" parent=""1429"" child=""1901"" jointype=""OUTER"" />
    <relationship parentkey=""1448"" childkey=""1884"" parent=""1429"" child=""1879"" jointype=""OUTER"" />
  </relationships>
</COEDataView>");
                try
                {
                    dv = _service.GetDataView(dataviewid);
                }
                catch
                {
                    dataviewid = _service.InsertDataView("COEDB", "MOLTABLE TEST", "MOLTABLE TEST", true, dataviewid, USERNAME, dv);
                }

                COEDatabaseBO coetest = COEDatabaseBO.Get("COETEST");
                if (coetest.COEDataView == null) //Prerequisite is to have, at least once, published the schema
                {
                    coetest = COEDatabaseBO.New("COETEST");
                    coetest.Publish("ORACLE");
                }

                string tableName = "COETEST.AID630SMAD";
                result = _service.PublishTableToDataview(dataviewid, tableName, "PUBCHEM_CID", "PUBCHEM_COMPOUND_CID", "PUBCHEM_CID", FrameworkProxy.JoinTypes.INNER);

                dv = _service.GetDataView(dataviewid);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n\n" + ex.StackTrace);
            }
            finally
            {
                _service.DeleteDataView(dataviewid);
            }
            
            Assert.AreEqual("OK", result, "The table was not added.");
            Assert.IsTrue(dv.tables[dv.tables.Length - 1].name == "AID630SMAD");
        }

        #region Private Methods
        public FrameworkProxy.COEDataView BuildCOEDataViewFromXML()
        {
            //Load DataViewSerialized.XML
            XmlDocument doc = new XmlDocument();
            doc.Load(_pathToDataviewXmls + "\\COEDataViewForTests.xml");
            return Framework.Common.Utilities.XmlDeserialize<FrameworkProxy.COEDataView>(doc.OuterXml);
        }

        private string GenerateRandomNumber()
        {
            string miliseconds = DateTime.Now.Millisecond.ToString();
            int length = miliseconds.Length;
            while(length < 3)
            {
                miliseconds = miliseconds.Insert(0, "0");
                length++;
            }
            return miliseconds.Substring(length - 3, 3);
        }
        #endregion
        #endregion
    }


}