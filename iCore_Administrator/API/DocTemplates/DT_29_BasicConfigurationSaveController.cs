using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Script.Serialization;
using iCore_Administrator.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace iCore_Administrator.API.DocTemplates
{

    public class Res_Structure_Public
    {
        public object d { get; set; }
        public string e { get; set; }
    }

    public class DocumentRelationInfo
    {
        public string CountryID { get; set; }
        public string StateID { get; set; }
        public string DocumentTypeID { get; set; }
        public string DocumentID { get; set; }
        public string UserID { get; set; }
    }

    public class TemplateBasicConfig
    {
        public string V1 { get; set; }
        public string V2 { get; set; }
        public string V3 { get; set; }
        public string V4 { get; set; }
        public string V5 { get; set; }
        public string V6 { get; set; }
        public string V7 { get; set; }
        public string V8 { get; set; }
        public string V9 { get; set; }
        public string V10 { get; set; }
        public string V11 { get; set; }
        public string V12 { get; set; }
        public string V13 { get; set; }
        public string V14 { get; set; }
        public string V15 { get; set; }
        public string V16 { get; set; }
        public string V17 { get; set; }
        public string V18 { get; set; }
        public string V19 { get; set; }
    }

    public class DT_29_BasicConfigurationSaveController : ApiController
    {
        //--------------------------------------------------------------------------
        SQL_Tranceiver SQ = new SQL_Tranceiver();
        PublicFunctions PB = new PublicFunctions();
        //--------------------------------------------------------------------------
        public async Task<string> PostAsync()
        {
            string LastRes = "";
            try
            {
                DocumentRelationInfo DBI = new DocumentRelationInfo();
                var H_Request = Request;
                var H_Headers = H_Request.Headers;
                if (H_Headers.Contains("CID")) { DBI.CountryID = H_Headers.GetValues("CID").First(); }
                if (H_Headers.Contains("SID")) { DBI.StateID = H_Headers.GetValues("SID").First(); }
                if (H_Headers.Contains("DTID")) { DBI.DocumentTypeID = H_Headers.GetValues("DTID").First(); }
                if (H_Headers.Contains("DID")) { DBI.DocumentID = H_Headers.GetValues("DID").First(); }
                if (H_Headers.Contains("UID")) { DBI.UserID = H_Headers.GetValues("UID").First(); }
                DataTable DT = new DataTable();
                DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Document_Name From Template_04_Document Where (Country_ID = '" + DBI.CountryID + "') And (State_ID = '" + DBI.StateID + "') And (DocType_ID = '" + DBI.DocumentTypeID + "') And (Document_ID = '" + DBI.DocumentID + "')");
                if (DT.Rows != null)
                {
                    if (DT.Rows.Count == 1)
                    {
                        string H_raw = await RawContentReader.Read(H_Request);
                        H_raw = H_raw.Trim();
                        if (H_raw != "")
                        {
                            DataTable DT2 = new DataTable();
                            DT2 = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Document_ID From Template_05_BaseData Where (Country_ID = '" + DBI.CountryID + "') And (State_ID = '" + DBI.StateID + "') And (DocType_ID = '" + DBI.DocumentTypeID + "') And (Document_ID = '" + DBI.DocumentID + "')");
                            bool InsNew = true;
                            if (DT2.Rows != null)
                            {
                                if (DT2.Rows.Count == 1)
                                {
                                    InsNew = false;
                                }
                                else
                                {
                                    InsNew = true;
                                }
                            }
                            else
                            {
                                InsNew = true;
                            }
                            string FunDate = PB.Get_Date();
                            string FunTime = PB.Get_Time();
                            if (InsNew == true)
                            {
                                SQ.Execute_TSql(DataBase_Selector.Administrator, "Delete From Template_05_BaseData Where (Country_ID = '" + DBI.CountryID + "') And (State_ID = '" + DBI.StateID + "') And (DocType_ID = '" + DBI.DocumentTypeID + "') And (Document_ID = '" + DBI.DocumentID + "')");
                                SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Template_05_BaseData Values ('" + DBI.CountryID + "','" + DBI.StateID + "','" + DBI.DocumentTypeID + "','" + DBI.DocumentID + "','" + DBI.UserID + "','" + FunDate + "','" + FunTime + "','" + DBI.UserID + "','" + FunDate + "','" + FunTime + "')");
                            }
                            else
                            {
                                SQ.Execute_TSql(DataBase_Selector.Administrator, "Update Template_05_BaseData Set [Update_UserID] = '" + DBI.UserID + "',[Update_Date] = '" + FunDate + "',[Update_Time] = '" + FunTime + "' Where (Country_ID = '" + DBI.CountryID + "') And (State_ID = '" + DBI.StateID + "') And (DocType_ID = '" + DBI.DocumentTypeID + "') And (Document_ID = '" + DBI.DocumentID + "')");
                            }
                            TemplateBasicConfig TBI = new TemplateBasicConfig();
                            TBI = Newtonsoft.Json.JsonConvert.DeserializeObject<TemplateBasicConfig>(H_raw);
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Delete From Template_06_BasicConfiguration Where (CID = '" + DBI.CountryID + "') And (SID = '" + DBI.StateID + "') And (DTID = '" + DBI.DocumentTypeID + "') And (DID = '" + DBI.DocumentID + "')");
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Template_06_BasicConfiguration Values ('" + DBI.CountryID + "','" + DBI.StateID + "','" + DBI.DocumentTypeID + "','" + DBI.DocumentID + "','" + TBI.V1 + "','" + TBI.V2 + "','" + TBI.V3 + "','" + TBI.V4 + "','" + TBI.V5 + "','" + TBI.V6 + "','" + TBI.V7 + "','" + TBI.V8 + "','" + TBI.V9 + "','" + TBI.V10 + "','" + TBI.V11 + "','" + TBI.V12 + "','" + TBI.V13 + "','" + TBI.V14 + "','" + TBI.V15 + "','" + TBI.V16 + "','" + TBI.V17 + "','" + TBI.V18 + "','" + TBI.V19 + "')");
                            LastRes = "OK";
                        }
                        else
                        {
                            LastRes = "ER4"; // Request Body Null
                        }
                    }
                    else
                    {
                        LastRes = "ER3"; // DT Return Multi or Zero Row
                    }
                }
                else
                {
                    LastRes = "ER2"; // DT Return Null Row
                }
            }
            catch (Exception)
            {
                LastRes = "ER1"; // Function error.
            }
            return LastRes;
        }
        //--------------------------------------------------------------------------
    }
}
