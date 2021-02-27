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
    public class DT_05_StatusCountryController : ApiController
    {
        //--------------------------------------------------------------------------
        SQL_Tranceiver SQ = new SQL_Tranceiver();
        //--------------------------------------------------------------------------
        public string Post(string CID)
        {
            string Res = "";
            try
            {
                CID = CID.Trim();
                DataTable DT = new DataTable();
                DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Country_Status_Code From Template_01_Country Where (Country_ID = '" + CID + "')");
                if (DT.Rows != null)
                {
                    if (DT.Rows.Count == 1)
                    {
                        if(DT.Rows[0][0].ToString().Trim()=="1")
                        {
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Update Template_01_Country Set [Country_Status_Code] = '0',[Country_Status_Text] = 'Disable' Where (Country_ID = '" + CID + "')");
                        }
                        else
                        {
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Update Template_01_Country Set [Country_Status_Code] = '1',[Country_Status_Text] = 'Active' Where (Country_ID = '" + CID + "')");
                        }
                        DT = new DataTable();
                        DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Country_Status_Text From Template_01_Country Where (Country_ID = '" + CID + "')");
                        if (DT.Rows != null)
                        {
                            if (DT.Rows.Count == 1)
                            {
                                Res = "OK-" + DT.Rows[0][0].ToString().Trim();
                            }
                            else
                            {
                                Res = "Country after edit not founded";
                            }
                        }
                        else
                        {
                            Res = "Country after edit not founded";
                        }
                    }
                    else
                    {
                        Res = "Country Not Founded - Country ID not valid";
                    }
                }
                else
                {
                    Res = "Country Not Founded - Server Error";
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
