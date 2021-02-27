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
    public class DT_19_StatusDocumentController : ApiController
    {
        //--------------------------------------------------------------------------
        SQL_Tranceiver SQ = new SQL_Tranceiver();
        //--------------------------------------------------------------------------
        public string Post(string DID)
        {
            string Res = "";
            try
            {
                DID = DID.Trim();
                DataTable DT = new DataTable();
                DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Document_Status_Code From Template_04_Document Where (Document_ID = '" + DID + "')");
                if (DT.Rows != null)
                {
                    if (DT.Rows.Count == 1)
                    {
                        if (DT.Rows[0][0].ToString().Trim() == "1")
                        {
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Update Template_04_Document Set [Document_Status_Code] = '0',[Document_Status_Text] = 'Disable' Where (Document_ID = '" + DID + "')");
                        }
                        else
                        {
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Update Template_04_Document Set [Document_Status_Code] = '1',[Document_Status_Text] = 'Active' Where (Document_ID = '" + DID + "')");
                        }
                        DT = new DataTable();
                        DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Document_Status_Text From Template_04_Document Where (Document_ID = '" + DID + "')");
                        if (DT.Rows != null)
                        {
                            if (DT.Rows.Count == 1)
                            {
                                Res = "OK-" + DT.Rows[0][0].ToString().Trim();
                            }
                            else
                            {
                                Res = "Document after edit not founded";
                            }
                        }
                        else
                        {
                            Res = "Document after edit not founded";
                        }
                    }
                    else
                    {
                        Res = "Document Not Founded - Document ID not valid";
                    }
                }
                else
                {
                    Res = "Document Not Founded - Server Error";
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
