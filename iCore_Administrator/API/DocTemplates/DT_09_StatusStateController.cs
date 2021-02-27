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
    public class DT_09_StatusStateController : ApiController
    {
        //--------------------------------------------------------------------------
        SQL_Tranceiver SQ = new SQL_Tranceiver();
        //--------------------------------------------------------------------------
        public string Post(string SID)
        {
            string Res = "";
            try
            {
                SID = SID.Trim();
                DataTable DT = new DataTable();
                DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select State_Status_Code From Template_02_State Where (State_ID = '" + SID + "')");
                if (DT.Rows != null)
                {
                    if (DT.Rows.Count == 1)
                    {
                        if (DT.Rows[0][0].ToString().Trim() == "1")
                        {
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Update Template_02_State Set [State_Status_Code] = '0',[State_Status_Text] = 'Disable' Where (State_ID = '" + SID + "')");
                        }
                        else
                        {
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Update Template_02_State Set [State_Status_Code] = '1',[State_Status_Text] = 'Active' Where (State_ID = '" + SID + "')");
                        }
                        DT = new DataTable();
                        DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select State_Status_Text From Template_02_State Where (State_ID = '" + SID + "')");
                        if (DT.Rows != null)
                        {
                            if (DT.Rows.Count == 1)
                            {
                                Res = "OK-" + DT.Rows[0][0].ToString().Trim();
                            }
                            else
                            {
                                Res = "State after edit not founded";
                            }
                        }
                        else
                        {
                            Res = "State after edit not founded";
                        }
                    }
                    else
                    {
                        Res = "State Not Founded - State ID not valid";
                    }
                }
                else
                {
                    Res = "State Not Founded - Server Error";
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
