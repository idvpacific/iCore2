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
    public class DT_20_GetDocumentInformationController : ApiController
    {
        //--------------------------------------------------------------------------
        SQL_Tranceiver SQ = new SQL_Tranceiver();
        //--------------------------------------------------------------------------
        public string Post(string DID)
        {
            string LastRes = "";
            try
            {
                DID = DID.ToString().Trim();
                DataTable DT = new DataTable();
                DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Country_ID,Country_Name,State_ID,State_Name,DocType_ID,DocType_Name,Document_ID,Document_Name From Template_04_Document_V Where (Document_ID = '" + DID + "')");
                if (DT.Rows != null)
                {
                    if (DT.Rows.Count == 1)
                    {
                        LastRes = "OK_" + DT.Rows[0][0].ToString().Trim() + "_" + DT.Rows[0][1].ToString().Trim() + "_" + DT.Rows[0][2].ToString().Trim() + "_" + DT.Rows[0][3].ToString().Trim() + "_" + DT.Rows[0][4].ToString().Trim() + "_" + DT.Rows[0][5].ToString().Trim() + "_" + DT.Rows[0][6].ToString().Trim() + "_" + DT.Rows[0][7].ToString().Trim();
                    }
                    else
                    {
                        LastRes = "ERA"; // Document not founded.
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
