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
    public class DT_12_EditDocumentTypeController : ApiController
    {
        //--------------------------------------------------------------------------
        SQL_Tranceiver SQ = new SQL_Tranceiver();
        //--------------------------------------------------------------------------
        public string Post(string DTID, string DTN, string DTC)
        {
            string Res = "";
            try
            {
                string TEr1 = ""; string TEr2 = ""; bool Err = false;
                DTID = DTID.Trim();
                DTN = DTN.Trim();
                DTC = DTC.Trim();
                if (DTN == "") { Err = true; TEr1 = "Document name field is Required"; }
                if (DTC == "") { Err = true; TEr2 = "Document code field is Required"; }
                if (Err == false)
                {
                    DataTable DT = new DataTable();
                    DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select DocType_ID From Template_03_DocumentType Where (DocType_ID = '" + DTID + "')");
                    if (DT.Rows == null) { Err = true; TEr1 = "DocumentType ID not valid, try again"; }
                    if (Err == false) { if (DT.Rows.Count != 1) { Err = true; TEr1 = "DocumentType ID not valid, try again"; } }
                    if (Err == false)
                    {
                        DT = new DataTable();
                        DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select DocType_ID From Template_03_DocumentType Where (DocType_Name = '" + DTN + "') And (DocType_ID <> '" + DTID + "')");
                        if (DT.Rows != null) { if (DT.Rows.Count != 0) { Err = true; TEr1 = "Document name field value is duplicate"; } }
                        DT = new DataTable();
                        DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select DocType_ID From Template_03_DocumentType Where (DocType_Code = '" + DTC + "') And (DocType_ID <> '" + DTID + "')");
                        if (DT.Rows != null) { if (DT.Rows.Count != 0) { Err = true; TEr2 = "Document code field value is duplicate"; } }
                        if (Err == false)
                        {
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Update Template_03_DocumentType Set [DocType_Name] = '" + DTN + "',[DocType_Code] = '" + DTC + "' Where (DocType_ID = '" + DTID + "')");
                            DT = new DataTable();
                            DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select DocType_Name,DocType_Code From Template_03_DocumentType_V Where (DocType_ID = '" + DTID + "')");
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
