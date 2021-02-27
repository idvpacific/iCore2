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
    public class DT_03_ReloadCountryController : ApiController
    {
        //--------------------------------------------------------------------------
        SQL_Tranceiver SQ = new SQL_Tranceiver();
        //--------------------------------------------------------------------------
        public string Post(string TP)
        {
            try
            {
                DataTable DT = new DataTable();
                DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Country_ID,Country_Name,Country_Code,Country_Status_Text,Ins_Date,Ins_Time,Fullname From Template_01_Country_V Order By Country_Name");
                return JsonConvert.SerializeObject(DT);
            }
            catch (Exception)
            { return ""; }
        }
    }
}
