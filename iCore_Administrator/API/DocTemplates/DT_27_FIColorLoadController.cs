using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using iCore_Administrator.Modules;

namespace iCore_Administrator.API.DocTemplates
{
    public class DT_27_FIColorLoadController : ApiController
    {
        //--------------------------------------------------------------------------
        SQL_Tranceiver SQ = new SQL_Tranceiver();
        //--------------------------------------------------------------------------
        public DataTable Post(string CID, string SID, string DTID, string DID)
        {
            try
            {
                CID = CID.ToString().Trim();
                SID = SID.ToString().Trim();
                DTID = DTID.ToString().Trim();
                DID = DID.ToString().Trim();
                DataTable DT = new DataTable();
                DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select X,Y,R,G,B,Similarity From Template_10_FrontImage_ColorPicker Where (CID = '" + CID + "') And (SID = '" + SID + "') And (DTID = '" + DTID + "') And (DID = '" + DID + "')");
                if (DT.Rows != null)
                {
                    if (DT.Rows.Count != 0)
                    {
                        return DT;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        //--------------------------------------------------------------------------
    }
}
