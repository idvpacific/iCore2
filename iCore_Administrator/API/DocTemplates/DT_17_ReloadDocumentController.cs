using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Web.Http;
using System.Web.Mvc;
using iCore_Administrator.Modules;
using Newtonsoft.Json;

namespace iCore_Administrator.API.DocTemplates
{
    public class DT_17_ReloadDocumentController : ApiController
    {
        //--------------------------------------------------------------------------
        SQL_Tranceiver SQ = new SQL_Tranceiver();
        //--------------------------------------------------------------------------
        public string Post(string CID, string SID, string DTID)
        {
            try
            {
                CID = CID.Trim();
                SID = SID.Trim();
                DTID = DTID.Trim();
                DataTable DT = new DataTable();
                DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select DocType_Name,Document_ID,Document_Name,Document_Code,Document_Status_Text,Ins_Date,Ins_Time,Fullname From Template_04_Document_V Where (Country_ID = '" + CID + "') And (State_ID = '" + SID + "') And (DocType_ID = '" + DTID + "') Order By Document_Name");
                return JsonConvert.SerializeObject(DT);
            }
            catch (Exception)
            { return ""; }
        }
    }
}
