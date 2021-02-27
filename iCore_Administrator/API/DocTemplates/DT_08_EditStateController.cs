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
    public class DT_08_EditStateController : ApiController
    {
        //--------------------------------------------------------------------------
        SQL_Tranceiver SQ = new SQL_Tranceiver();
        //--------------------------------------------------------------------------
        public string Post(string SID, string CN, string CT)
        {
            string Res = "";
            try
            {
                string TEr1 = ""; string TEr2 = ""; bool Err = false;
                SID = SID.Trim();
                CN = CN.Trim();
                CT = CT.Trim();
                if (CN == "") { Err = true; TEr1 = "State name field is Required"; }
                if (CT == "") { Err = true; TEr2 = "State code field is Required"; }
                if (Err == false)
                {
                    DataTable DT = new DataTable();
                    DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select State_ID From Template_02_State Where (State_ID = '" + SID + "')");
                    if (DT.Rows == null) { Err = true; TEr1 = "State ID not valid, try again"; }
                    if (Err == false) { if (DT.Rows.Count != 1) { Err = true; TEr1 = "State ID not valid, try again"; } }
                    if (Err == false)
                    {
                        DT = new DataTable();
                        DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select State_ID From Template_02_State Where (State_Name = '" + CN + "') And (State_ID <> '" + SID + "')");
                        if (DT.Rows != null) { if (DT.Rows.Count != 0) { Err = true; TEr1 = "State name field value is duplicate"; } }
                        DT = new DataTable();
                        DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select State_ID From Template_02_State Where (State_Code = '" + CT + "') And (State_ID <> '" + SID + "')");
                        if (DT.Rows != null) { if (DT.Rows.Count != 0) { Err = true; TEr2 = "State code field value is duplicate"; } }
                        if (Err == false)
                        {
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Update Template_02_State Set [State_Name] = '" + CN + "',[State_Code] = '" + CT + "' Where (State_ID = '" + SID + "')");
                            DT = new DataTable();
                            DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select State_Name,State_Code From Template_02_State_V Where (State_ID = '" + SID + "')");
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
