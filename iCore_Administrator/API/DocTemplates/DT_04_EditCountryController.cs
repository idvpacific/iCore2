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
    public class DT_04_EditCountryController : ApiController
    {
        //--------------------------------------------------------------------------
        SQL_Tranceiver SQ = new SQL_Tranceiver();
        //--------------------------------------------------------------------------
        public string Post(string CID, string CN, string CT)
        {
            string Res = "";
            try
            {
                string TEr1 = ""; string TEr2 = ""; bool Err = false;
                CID = CID.Trim();
                CN = CN.Trim();
                CT = CT.Trim();
                if (CN == "") { Err = true; TEr1 = "Country name field is Required"; }
                if (CT == "") { Err = true; TEr2 = "Country code field is Required"; }
                if (Err == false)
                {
                    DataTable DT = new DataTable();
                    DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Country_ID From Template_01_Country Where (Country_ID = '" + CID + "')");
                    if (DT.Rows == null) { Err = true; TEr1 = "Country ID not valid, try again"; }
                    if (Err == false) { if (DT.Rows.Count != 1) { Err = true; TEr1 = "Country ID not valid, try again"; } }
                    if (Err == false)
                    {
                        DT = new DataTable();
                        DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Country_ID From Template_01_Country Where (Country_Name = '" + CN + "') And (Country_ID <> '" + CID + "')");
                        if (DT.Rows != null) { if (DT.Rows.Count != 0) { Err = true; TEr1 = "Country name field value is duplicate"; } }
                        DT = new DataTable();
                        DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Country_ID From Template_01_Country Where (Country_Code = '" + CT + "') And (Country_ID <> '" + CID + "')");
                        if (DT.Rows != null) { if (DT.Rows.Count != 0) { Err = true; TEr2 = "Country code field value is duplicate"; } }
                        if (Err == false)
                        {
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Update Template_01_Country Set [Country_Name] = '" + CN + "',[Country_Code] = '" + CT + "' Where (Country_ID = '" + CID + "')");
                            DT = new DataTable();
                            DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Country_Name,Country_Code From Template_01_Country_V Where (Country_ID = '" + CID + "')");
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
