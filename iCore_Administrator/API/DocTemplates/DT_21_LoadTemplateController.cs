using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using iCore_Administrator.Modules;

namespace iCore_Administrator.API.DocTemplates
{
    public class DT_21_LoadTemplateController : ApiController
    {
        //--------------------------------------------------------------------------
        SQL_Tranceiver SQ = new SQL_Tranceiver();
        //--------------------------------------------------------------------------
        public string Post(string CID, string SID, string DTID, string DID)
        {
            string LastRes = "";
            try
            {
                CID = CID.ToString().Trim();
                SID = SID.ToString().Trim();
                DTID = DTID.ToString().Trim();
                DID = DID.ToString().Trim();
                DataTable DT = new DataTable();
                DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Ins_User_Fullname,Ins_Date,Ins_Time,Update_User_Fullname,Update_Date,Update_Time From Template_05_BaseData_V Where (Country_ID = '" + CID + "') And (State_ID = '" + SID + "') And (DocType_ID = '" + DTID + "') And (Document_ID = '" + DID + "')");
                if (DT.Rows != null)
                {
                    if (DT.Rows.Count == 1)
                    {
                        LastRes = "OK_" + DT.Rows[0][0].ToString().Trim() + "_" + DT.Rows[0][1].ToString().Trim() + "_" + DT.Rows[0][2].ToString().Trim() + "_" + DT.Rows[0][3].ToString().Trim() + "_" + DT.Rows[0][4].ToString().Trim() + "_" + DT.Rows[0][5].ToString().Trim();
                    }
                    else
                    {
                        LastRes = "ERA"; // Document BaseData not founded.
                    }
                }
                else
                {
                    LastRes = "ERA"; // Sql no response.
                }
            }
            catch (Exception)
            {
                LastRes = "ERA"; // Function error.
            }
            return LastRes;
        }
        //--------------------------------------------------------------------------
    }
}
