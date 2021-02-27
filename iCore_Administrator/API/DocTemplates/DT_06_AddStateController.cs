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
    public class DT_06_AddStateController : ApiController
    {
        //--------------------------------------------------------------------------
        SQL_Tranceiver SQ = new SQL_Tranceiver();
        PublicFunctions PB = new PublicFunctions();
        //--------------------------------------------------------------------------
        public string Post(string CID, string SN,string ST, string UI)
        {
            string Res = "";
            try
            {
                string TEr1 = ""; string TEr2 = ""; bool Err = false;
                CID = CID.Trim();
                SN = SN.Trim();
                ST = ST.Trim();
                UI = UI.Trim();
                if (SN == "") { Err = true; TEr1 = "State name field is required"; }
                if (ST == "") { Err = true; TEr2 = "State code field is required"; }
                if (Err == false)
                {
                    DataTable DT = new DataTable();
                    DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select State_ID From Template_02_State Where (State_Name = '" + SN + "') And (Country_ID = '" + CID + "')");
                    if (DT.Rows != null) { if (DT.Rows.Count != 0) { Err = true; TEr1 = "State name field value is duplicate"; } }
                    DT = new DataTable();
                    DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select State_ID From Template_02_State Where (State_Code = '" + ST + "') And (Country_ID = '" + CID + "')");
                    if (DT.Rows != null) { if (DT.Rows.Count != 0) { Err = true; TEr2 = "State code field value is duplicate"; } }
                    if (Err == false)
                    {
                        long SID = SQ.Get_New_ID(DataBase_Selector.Administrator, "Template_02_State", "State_ID");
                        SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Template_02_State Values ('" + CID + "','" + SID.ToString() + "','" + SN + "','" + ST + "','1','Active','" + PB.Get_Date() + "','" + PB.Get_Time() + "','" + UI + "')");
                        DT = new DataTable();
                        DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select State_ID,State_Name,State_Code,State_Status_Text,Ins_Date,Ins_Time,Fullname From Template_02_State_V Where (State_ID = '" + SID + "')");
                        if (DT.Rows != null)
                        {
                            if (DT.Rows.Count == 1)
                            {
                                Res = "OK-" + DT.Rows[0][0].ToString().Trim() + "-" + DT.Rows[0][1].ToString().Trim() + "-" + DT.Rows[0][2].ToString().Trim() + "-" + DT.Rows[0][3].ToString().Trim() + "-" + DT.Rows[0][4].ToString().Trim() + "-" + DT.Rows[0][5].ToString().Trim() + "-" + DT.Rows[0][6].ToString().Trim();
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
