using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Script.Serialization;
using iCore_Administrator.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace iCore_Administrator.API.DocTemplates
{

    public class DocumentRelationInfoBI
    {
        public string CountryID { get; set; }
        public string StateID { get; set; }
        public string DocumentTypeID { get; set; }
        public string DocumentID { get; set; }
        public string UserID { get; set; }
    }

    public class DT_31_BackImageSaveController : ApiController
    {
        //--------------------------------------------------------------------------
        SQL_Tranceiver SQ = new SQL_Tranceiver();
        PublicFunctions PB = new PublicFunctions();
        //--------------------------------------------------------------------------
        public string Post(string W, string H)
        {
            string LastRes = "";
            try
            {
                W = W.Trim();
                H = H.Trim();
                DocumentRelationInfoBI DBI = new DocumentRelationInfoBI();
                var H_Request = Request;
                var H_Headers = H_Request.Headers;
                if (H_Headers.Contains("CID")) { DBI.CountryID = H_Headers.GetValues("CID").First(); }
                if (H_Headers.Contains("SID")) { DBI.StateID = H_Headers.GetValues("SID").First(); }
                if (H_Headers.Contains("DTID")) { DBI.DocumentTypeID = H_Headers.GetValues("DTID").First(); }
                if (H_Headers.Contains("DID")) { DBI.DocumentID = H_Headers.GetValues("DID").First(); }
                if (H_Headers.Contains("UID")) { DBI.UserID = H_Headers.GetValues("UID").First(); }
                DataTable DT = new DataTable();
                DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Document_Name From Template_04_Document Where (Country_ID = '" + DBI.CountryID + "') And (State_ID = '" + DBI.StateID + "') And (DocType_ID = '" + DBI.DocumentTypeID + "') And (Document_ID = '" + DBI.DocumentID + "')");
                if (DT.Rows != null)
                {
                    if (DT.Rows.Count == 1)
                    {
                        SQ.Execute_TSql(DataBase_Selector.Administrator, "Delete From Template_07_Images Where (CID = '" + DBI.CountryID + "') And (SID = '" + DBI.StateID + "') And (DTID = '" + DBI.DocumentTypeID + "') And (DID = '" + DBI.DocumentID + "') And (ImageID = '2')");
                        var httpRequest = HttpContext.Current.Request;
                        foreach (string file in httpRequest.Files)
                        {
                            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);
                            var postedFile = httpRequest.Files[file];
                            if (postedFile != null && postedFile.ContentLength > 0)
                            {
                                Stream fs = postedFile.InputStream;
                                BinaryReader br = new BinaryReader(fs);
                                byte[] PIMG = br.ReadBytes((Int32)fs.Length);
                                SqlConnection CN = new SqlConnection();
                                CN = SQ.Get_Sql_Connection(DataBase_Selector.Administrator);
                                string qry = "insert into Template_07_Images values (@CID,@SID,@DTID,@DID,@ImageID,@Img_W,@Img_H,@ImageData)";
                                SqlCommand SqlCom = new SqlCommand(qry, CN);
                                SqlCom.Parameters.Add(new SqlParameter("@CID", DBI.CountryID));
                                SqlCom.Parameters.Add(new SqlParameter("@SID", DBI.StateID));
                                SqlCom.Parameters.Add(new SqlParameter("@DTID", DBI.DocumentTypeID));
                                SqlCom.Parameters.Add(new SqlParameter("@DID", DBI.DocumentID));
                                SqlCom.Parameters.Add(new SqlParameter("@ImageID", "2"));
                                SqlCom.Parameters.Add(new SqlParameter("@Img_W", W));
                                SqlCom.Parameters.Add(new SqlParameter("@Img_H", H));
                                SqlCom.Parameters.Add(new SqlParameter("@ImageData", (object)PIMG));
                                CN.Open();
                                try
                                {
                                    SqlCom.ExecuteNonQuery();
                                    LastRes = "OK";
                                }
                                catch (Exception)
                                {
                                    LastRes = "ER5"; // Execute Sql Error
                                }
                                CN.Close();
                            }
                            else
                            {
                                LastRes = "ER4"; // No File Posted
                            }
                        }

                    }
                    else
                    {
                        LastRes = "ER3"; // DT Return Multi or Zero Row
                    }
                }
                else
                {
                    LastRes = "ER2"; // DT Return Null Row
                }
            }
            catch (Exception)
            {
                LastRes = "ER1"; // Function error.
            }
            return LastRes;
        }
    }
}
