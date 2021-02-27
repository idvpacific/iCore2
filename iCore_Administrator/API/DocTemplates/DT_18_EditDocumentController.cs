using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Web.Http;
using iCore_Administrator.Modules;

namespace iCore_Administrator.API.DocTemplates
{
    public class DT_18_EditDocumentController : ApiController
    {
        //--------------------------------------------------------------------------
        SQL_Tranceiver SQ = new SQL_Tranceiver();
        //--------------------------------------------------------------------------
        public string Post(string CID, string SID, string DTID, string DID, string DN, string DC)
        {
            string Res = "";
            try
            {
                string TEr1 = ""; string TEr2 = ""; bool Err = false;
                CID = CID.Trim();
                SID = SID.Trim();
                DTID = DTID.Trim();
                DID = DID.Trim();
                DN = DN.Trim();
                DC = DC.Trim();
                if ((DTID == "") || (DTID == "0")) { Err = true; TEr1 = "Document Type not valid"; }
                if ((SID == "") || (SID == "0")) { Err = true; TEr1 = "State not valid"; }
                if ((CID == "") || (CID == "0")) { Err = true; TEr1 = "Country not valid"; }
                if ((DID == "") || (DID == "0")) { Err = true; TEr1 = "Document not valid"; }
                if (DN == "") { Err = true; TEr1 = "Document name field is required"; }
                if (DC == "") { Err = true; TEr2 = "Document code field is required"; }
                if (Err == false)
                {
                    DataTable DT = new DataTable();
                    DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Document_ID From Template_04_Document Where (Document_ID = '" + DID + "')");
                    if (DT.Rows == null) { Err = true; TEr1 = "Document ID not valid, try again"; }
                    if (Err == false) { if (DT.Rows.Count != 1) { Err = true; TEr1 = "Document ID not valid, try again"; } }
                    if (Err == false)
                    {
                        DT = new DataTable();
                        DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Document_ID From Template_04_Document Where (Document_Name = '" + DN + "') And (Country_ID = '" + CID + "') And (State_ID = '" + SID + "') And (DocType_ID = '" + DTID + "') And (Document_ID <> '" + DID + "')");
                        if (DT.Rows != null) { if (DT.Rows.Count != 0) { Err = true; TEr1 = "Document name field value is duplicate"; } }
                        DT = new DataTable();
                        DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Document_ID From Template_04_Document Where (Document_Code = '" + DC + "') And (Country_ID = '" + CID + "') And (State_ID = '" + SID + "') And (DocType_ID = '" + DTID + "') And (Document_ID <> '" + DID + "')");
                        if (DT.Rows != null) { if (DT.Rows.Count != 0) { Err = true; TEr2 = "Document code field value is duplicate"; } }
                        if (Err == false)
                        {
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Update Template_04_Document Set [Document_Name] = '" + DN + "',[Document_Code] = '" + DC + "' Where (Country_ID = '" + CID + "') And (State_ID = '" + SID + "') And (DocType_ID = '" + DTID + "') And (Document_ID = '" + DID + "')");
                            DT = new DataTable();
                            DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Document_Name,Document_Code From Template_04_Document_V Where (Country_ID = '" + CID + "') And (State_ID = '" + SID + "') And (DocType_ID = '" + DTID + "') And (Document_ID = '" + DID + "')");
                            if (DT.Rows != null)
                            {
                                if (DT.Rows.Count == 1)
                                {
                                    Res = "OK-" + DT.Rows[0][0].ToString().Trim() + "-" + DT.Rows[0][1].ToString().Trim();
                                }
                                else
                                {
                                    Res = "ERR5-Server read data error- ";
                                }
                            }
                            else
                            {
                                Res = "ERR4-Server read data error- ";
                            }
                        }
                        else
                        {
                            Res = "ERR3-" + TEr1.Trim() + "-" + TEr2.Trim();
                        }
                    }
                    else
                    {
                        Res = "ERR2-" + TEr1.Trim() + "-" + TEr2.Trim();
                    }
                }
                else
                {
                    Res = "ERR1-" + TEr1.Trim() + "-" + TEr2.Trim();
                }
            }
            catch (Exception)
            {
                Res = "ERA";
            }
            return Res.Trim();
        }
    }
}
