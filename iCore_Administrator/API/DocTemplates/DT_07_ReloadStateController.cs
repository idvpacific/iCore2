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
    public class DT_07_ReloadStateController : ApiController
    {
        //--------------------------------------------------------------------------
        SQL_Tranceiver SQ = new SQL_Tranceiver();
        //--------------------------------------------------------------------------
        public string Post(string CID)
        {
            try
            {
                CID = CID.Trim();
                DataTable DT = new DataTable();
                DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select State_ID,State_Name,State_Code,State_Status_Text,Ins_Date,Ins_Time,Fullname From Template_02_State_V Where (Country_ID = '" + CID + "') Order By State_Name");
                return JsonConvert.SerializeObject(DT);
            }
            catch (Exception)
            { return ""; }
        }
    }
}
