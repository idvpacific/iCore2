using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Script.Serialization;
using iCore_Administrator.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace iCore_Administrator.API.DocTemplates
{
    public class DocumentRelationInfoBICP
    {
        public string CountryID { get; set; }
        public string StateID { get; set; }
        public string DocumentTypeID { get; set; }
        public string DocumentID { get; set; }
        public string UserID { get; set; }
    }

    public class BIColorPicker
    {
        public string V1 { get; set; }
        public string V2 { get; set; }
        public string V3 { get; set; }
        public string V4 { get; set; }
        public string V5 { get; set; }
        public string V6 { get; set; }
    }
    public class DT_35_BIColorsSaveController : ApiController
    {
        //--------------------------------------------------------------------------
        SQL_Tranceiver SQ = new SQL_Tranceiver();
        PublicFunctions PB = new PublicFunctions();
        //--------------------------------------------------------------------------
        public async Task<string> PostAsync(int DelData)
        {
            string LastRes = "";
            try
            {
                DocumentRelationInfoBICP DBI = new DocumentRelationInfoBICP();
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
                        if (DelData == 1)
                        {
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Delete From Template_11_BackImage_ColorPicker Where (CID = '" + DBI.CountryID + "') And (SID = '" + DBI.StateID + "') And (DTID = '" + DBI.DocumentTypeID + "') And (DID = '" + DBI.DocumentID + "')");
                            LastRes = "OK";
                        }
                        else
                        {
                            string H_raw = await RawContentReader.Read(H_Request);
                            H_raw = H_raw.Trim();
                            if (H_raw != "")
                            {
                                BIColorPicker FIE = new BIColorPicker();
                                FIE = Newtonsoft.Json.JsonConvert.DeserializeObject<BIColorPicker>(H_raw);
                                SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Template_11_BackImage_ColorPicker Values ('" + DBI.CountryID + "','" + DBI.StateID + "','" + DBI.DocumentTypeID + "','" + DBI.DocumentID + "','" + FIE.V1 + "','" + FIE.V2 + "','" + FIE.V3 + "','" + FIE.V4 + "','" + FIE.V5 + "','" + FIE.V6 + "')");
                                LastRes = "OK";
                            }
                            else
                            {
                                LastRes = "ER4"; // Request Body Null
                            }
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
    }
}
