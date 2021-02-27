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
    public class DT_16_AddDocumentController : ApiController
    {
        //--------------------------------------------------------------------------
        SQL_Tranceiver SQ = new SQL_Tranceiver();
        PublicFunctions PB = new PublicFunctions();
        //--------------------------------------------------------------------------
        public string Post(string CID, string SID, string DTID,string DN,string DC,string UI)
        {
            string Res = "";
            try
            {
                string TEr1 = ""; string TEr2 = ""; bool Err = false;
                CID = CID.Trim();
                SID = SID.Trim();
                DTID = DTID.Trim();
                DN = DN.Trim();
                DC = DC.Trim();
                UI = UI.Trim();
                if ((DTID == "") || (DTID == "0")) { Err = true; TEr1 = "Document Type not valid"; }
                if ((SID == "") || (SID == "0")) { Err = true; TEr1 = "State not valid"; }
                if ((CID == "") || (CID == "0")) { Err = true; TEr1 = "Country not valid"; }
                if (DN == "") { Err = true; TEr1 = "Document name field is required"; }
                if (DC == "") { Err = true; TEr2 = "Document code field is required"; }
                if (Err == false)
                {
                    DataTable DT = new DataTable();
                    DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Document_ID From Template_04_Document Where (Document_Name = '" + DN + "') And (Country_ID = '" + CID + "') And (State_ID = '" + SID + "') And (DocType_ID = '" + DTID + "')");
                    if (DT.Rows != null) { if (DT.Rows.Count != 0) { Err = true; TEr1 = "Document name field value is duplicate"; } }
                    DT = new DataTable();
                    DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Document_ID From Template_04_Document Where (Document_Code = '" + DC + "') And (Country_ID = '" + CID + "') And (State_ID = '" + SID + "') And (DocType_ID = '" + DTID + "')");
                    if (DT.Rows != null) { if (DT.Rows.Count != 0) { Err = true; TEr2 = "Document code field value is duplicate"; } }
                    if (Err == false)
                    {
                        long CSID = SQ.Get_New_ID(DataBase_Selector.Administrator, "Template_04_Document", "Document_ID");
                        SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Template_04_Document Values ('" + CID +"','" + SID + "','" + DTID + "','" + CSID.ToString() + "','" + DN + "','" + DC + "','1','Active','" + PB.Get_Date() + "','" + PB.Get_Time() + "','" + UI + "')");
                        DT = new DataTable();
                        DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select DocType_Name,Document_ID,Document_Name,Document_Code,Document_Status_Text,Ins_Date,Ins_Time,Fullname From Template_04_Document_V Where (Document_ID = '" + CSID + "')");
                        if (DT.Rows != null)
                        {
                            if (DT.Rows.Count == 1)
                            {
                                Res = "OK-" + DT.Rows[0][0].ToString().Trim() + "-" + DT.Rows[0][1].ToString().Trim() + "-" + DT.Rows[0][2].ToString().Trim() + "-" + DT.Rows[0][3].ToString().Trim() + "-" + DT.Rows[0][4].ToString().Trim() + "-" + DT.Rows[0][5].ToString().Trim() + "-" + DT.Rows[0][6].ToString().Trim() + "-" + DT.Rows[0][7].ToString().Trim();
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
