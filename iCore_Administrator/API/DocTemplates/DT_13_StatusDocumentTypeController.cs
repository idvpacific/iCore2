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
    public class DT_13_StatusDocumentTypeController : ApiController
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
                DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select DocType_Status_Code From Template_03_DocumentType Where (DocType_ID = '" + DID + "')");
                if (DT.Rows != null)
                {
                    if (DT.Rows.Count == 1)
                    {
                        if (DT.Rows[0][0].ToString().Trim() == "1")
                        {
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Update Template_03_DocumentType Set [DocType_Status_Code] = '0',[DocType_Status_Text] = 'Disable' Where (DocType_ID = '" + DID + "')");
                        }
                        else
                        {
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Update Template_03_DocumentType Set [DocType_Status_Code] = '1',[DocType_Status_Text] = 'Active' Where (DocType_ID = '" + DID + "')");
                        }
                        DT = new DataTable();
                        DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select DocType_Status_Text From Template_03_DocumentType Where (DocType_ID = '" + DID + "')");
                        if (DT.Rows != null)
                        {
                            if (DT.Rows.Count == 1)
                            {
                                Res = "OK-" + DT.Rows[0][0].ToString().Trim();
                            }
                            else
                            {
                                Res = "Document Type after edit not founded";
                            }
                        }
                        else
                        {
                            Res = "Document Type after edit not founded";
                        }
                    }
                    else
                    {
                        Res = "Document Type Not Founded - Country ID not valid";
                    }
                }
                else
                {
                    Res = "Document Type Not Founded - Server Error";
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
