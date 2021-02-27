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
    public class DT_10_AddNewDocumentTypeController : ApiController
    {
        //--------------------------------------------------------------------------
        SQL_Tranceiver SQ = new SQL_Tranceiver();
        PublicFunctions PB = new PublicFunctions();
        //--------------------------------------------------------------------------
        public string Post(string DTN, string DTC, string UI)
        {
            string Res = "";
            try
            {
                string TEr1 = ""; string TEr2 = ""; bool Err = false;
                DTN = DTN.Trim();
                DTC = DTC.Trim();
                UI = UI.Trim();
                if (DTN == "") { Err = true; TEr1 = "Document name field is required"; }
                if (DTC == "") { Err = true; TEr2 = "Document code field is required"; }
                if (Err == false)
                {
                    DataTable DT = new DataTable();
                    DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select DocType_ID From Template_03_DocumentType Where (DocType_Name = '" + DTN + "')");
                    if (DT.Rows != null) { if (DT.Rows.Count != 0) { Err = true; TEr1 = "Document name field value is duplicate"; } }
                    DT = new DataTable();
                    DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select DocType_ID From Template_03_DocumentType Where (DocType_Code = '" + DTC + "')");
                    if (DT.Rows != null) { if (DT.Rows.Count != 0) { Err = true; TEr2 = "Document code field value is duplicate"; } }
                    if (Err == false)
                    {
                        long SID = SQ.Get_New_ID(DataBase_Selector.Administrator, "Template_03_DocumentType", "DocType_ID");
                        SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Template_03_DocumentType Values ('" + SID.ToString() + "','" + DTN + "','" + DTC + "','1','Active','" + PB.Get_Date() + "','" + PB.Get_Time() + "','" + UI + "')");
                        DT = new DataTable();
                        DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Template_03_DocumentType_V Where (DocType_ID = '" + SID + "')");
                        if (DT.Rows != null)
                        {
                            if (DT.Rows.Count == 1)
                            {
                                Res = "OK-" + DT.Rows[0][0].ToString().Trim() + "-" + DT.Rows[0][1].ToString().Trim() + "-" + DT.Rows[0][2].ToString().Trim() + "-" + DT.Rows[0][4].ToString().Trim() + "-" + DT.Rows[0][5].ToString().Trim() + "-" + DT.Rows[0][6].ToString().Trim() + "-" + DT.Rows[0][8].ToString().Trim();
                            }
                            else
                            {
                                Res = "ERR4-Server read data error- ";
                            }
                        }
                        else
                        {
                            Res = "ERR3-Server read data error- ";
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
