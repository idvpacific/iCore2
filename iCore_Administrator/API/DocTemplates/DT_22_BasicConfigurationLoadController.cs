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
    public class DT_22_BasicConfigurationLoadController : ApiController
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
                DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Template_06_BasicConfiguration Where (CID = '" + CID + "') And (SID = '" + SID + "') And (DTID = '" + DTID + "') And (DID = '" + DID + "')");
                if (DT.Rows != null)
                {
                    if (DT.Rows.Count == 1)
                    {
                        string JsonBody = "";
                        JsonBody += "{";
                        JsonBody += "\"" + "V1" + "\": " + "\"" + DT.Rows[0][4].ToString().Trim() + "\",";
                        JsonBody += "\"" + "V2" + "\": " + "\"" + DT.Rows[0][5].ToString().Trim() + "\",";
                        JsonBody += "\"" + "V3" + "\": " + "\"" + DT.Rows[0][6].ToString().Trim() + "\",";
                        JsonBody += "\"" + "V4" + "\": " + "\"" + DT.Rows[0][7].ToString().Trim() + "\",";
                        JsonBody += "\"" + "V5" + "\": " + "\"" + DT.Rows[0][8].ToString().Trim() + "\",";
                        JsonBody += "\"" + "V6" + "\": " + "\"" + DT.Rows[0][9].ToString().Trim() + "\",";
                        JsonBody += "\"" + "V7" + "\": " + "\"" + DT.Rows[0][10].ToString().Trim() + "\",";
                        JsonBody += "\"" + "V8" + "\": " + "\"" + DT.Rows[0][11].ToString().Trim() + "\",";
                        JsonBody += "\"" + "V9" + "\": " + "\"" + DT.Rows[0][12].ToString().Trim() + "\",";
                        JsonBody += "\"" + "V10" + "\": " + "\"" + DT.Rows[0][13].ToString().Trim() + "\",";
                        JsonBody += "\"" + "V11" + "\": " + "\"" + DT.Rows[0][14].ToString().Trim() + "\",";
                        JsonBody += "\"" + "V12" + "\": " + "\"" + DT.Rows[0][15].ToString().Trim() + "\",";
                        JsonBody += "\"" + "V13" + "\": " + "\"" + DT.Rows[0][16].ToString().Trim() + "\",";
                        JsonBody += "\"" + "V14" + "\": " + "\"" + DT.Rows[0][17].ToString().Trim() + "\",";
                        JsonBody += "\"" + "V15" + "\": " + "\"" + DT.Rows[0][18].ToString().Trim() + "\",";
                        JsonBody += "\"" + "V16" + "\": " + "\"" + DT.Rows[0][19].ToString().Trim() + "\",";
                        JsonBody += "\"" + "V17" + "\": " + "\"" + DT.Rows[0][20].ToString().Trim() + "\",";
                        JsonBody += "\"" + "V18" + "\": " + "\"" + DT.Rows[0][21].ToString().Trim() + "\",";
                        JsonBody += "\"" + "V19" + "\": " + "\"" + DT.Rows[0][22].ToString().Trim() + "\"";
                        JsonBody += "}";
                        LastRes = JsonBody.Trim();
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
