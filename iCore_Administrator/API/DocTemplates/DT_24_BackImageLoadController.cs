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
    public class DT_24_BackImageLoadController : ApiController
    {
        //--------------------------------------------------------------------------
        SQL_Tranceiver SQ = new SQL_Tranceiver();
        //--------------------------------------------------------------------------
        public HttpResponseMessage Post(string CID, string SID, string DTID, string DID)
        {
            HttpResponseMessage res = new HttpResponseMessage(HttpStatusCode.OK);
            try
            {
                CID = CID.ToString().Trim();
                SID = SID.ToString().Trim();
                DTID = DTID.ToString().Trim();
                DID = DID.ToString().Trim();
                DataTable DT = new DataTable();
                DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ImageData From Template_07_Images Where (CID = '" + CID + "') And (SID = '" + SID + "') And (DTID = '" + DTID + "') And (DID = '" + DID + "') And (ImageID = '2')");
                if (DT.Rows != null)
                {
                    if (DT.Rows.Count == 1)
                    {
                        res = new HttpResponseMessage(HttpStatusCode.OK);
                        Byte[] data = new Byte[0];
                        data = (Byte[])(DT.Rows[0][0]);
                        MemoryStream mem = new MemoryStream(data);
                        res.Content = new StreamContent(mem);
                        res.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = "BI.jpg" };
                        res.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    }
                    else
                    {
                        res = new HttpResponseMessage(HttpStatusCode.NotFound);
                        res.Content = new StringContent("Server return null image - Err3", Encoding.UTF8, "text/plain");
                    }
                }
                else
                {
                    res = new HttpResponseMessage(HttpStatusCode.NotFound);
                    res.Content = new StringContent("Server return null image - Err2", Encoding.UTF8, "text/plain");
                }
            }
            catch (Exception)
            {
                res = new HttpResponseMessage(HttpStatusCode.NotFound);
                res.Content = new StringContent("Server return null image - Err1", Encoding.UTF8, "text/plain");
            }
            return res;
        }
        //--------------------------------------------------------------------------
    }
}
