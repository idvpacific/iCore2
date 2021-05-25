using iCore_Administrator.API.Modules;
using iCore_Administrator.API.ResultStructure;
using iCore_Administrator.Modules;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace iCore_Administrator.API.Result
{
    [Route("api/Result/RetrieveFile")]
    public class RetrieveFileController : ApiController
    {
        //=========================================================================================================================================
        SQL_Tranceiver Sq = new SQL_Tranceiver();
        PublicFunctions Pb = new PublicFunctions();
        //=========================================================================================================================================
        public object Get()
        {
            //---------------------------------------------------------------------------------------------------------------------------------------------------------
            // Result Structure :
            API_Mesaage FunctionResult_Mesaage = new API_Mesaage();
            string API_Name = "RetrieveFile";
            string API_Code = "6";
            string API_FullName = "Retrieve Upload And Prossecced File";
            //---------------------------------------------------------------------------------------------------------------------------------------------------------
            string Er_API_Username = "";
            string Er_API_Password = "";
            string Er_Transaction_Unic_ID = "";
            string Er_User_API_Post_Username = "";
            string Er_Image_ID = "";
            string Err_User_ID = "";
            string Err_User_Type = "";
            //---------------------------------------------------------------------------------------------------------------------------------------------------------
            try
            {
                FunctionResult_Mesaage.Code = 0; FunctionResult_Mesaage.Error = false; FunctionResult_Mesaage.Description = "";
                string API_Username = ""; string API_Password = "";
                var H_Request = Request; var H_Headers = H_Request.Headers;
                if (H_Headers.Contains("API-Username")) { API_Username = H_Headers.GetValues("API-Username").First(); }
                if (H_Headers.Contains("API-Password")) { API_Password = H_Headers.GetValues("API-Password").First(); }
                API_Username = API_Username.Trim(); API_Password = API_Password.Trim();
                if ((API_Username != "") && (API_Password != ""))
                {
                    Er_API_Username = API_Username;
                    Er_API_Password = API_Password;
                    DataTable DT_Authentication = new DataTable();
                    DT_Authentication = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,User_GroupType_Code,Status_Code,API_PostRequest_Username From Users_02_SingleUser Where (API_GetRequest_Username = '" + API_Username + "') And (API_GetRequest_Key = '" + API_Password + "') And (Removed = '0')");
                    if (DT_Authentication.Rows.Count == 1)
                    {
                        if (DT_Authentication.Rows[0][2].ToString().Trim() == "1")
                        {
                            string Transaction_Unic_ID = "";
                            string User_API_Post_Username = "";
                            string Image_ID = "";
                            try { Transaction_Unic_ID = HttpContext.Current.Request.Params["Transaction_ID"].Trim(); } catch (Exception) { }
                            try { User_API_Post_Username = HttpContext.Current.Request.Params["Username"].Trim(); } catch (Exception) { }
                            try { Image_ID = HttpContext.Current.Request.Params["Image_ID"].Trim(); } catch (Exception) { }
                            Er_Transaction_Unic_ID = Transaction_Unic_ID;
                            Er_User_API_Post_Username = User_API_Post_Username;
                            FunctionResult_Mesaage.Transaction_ID = Er_Transaction_Unic_ID;
                            Er_Image_ID = Image_ID;
                            if (Transaction_Unic_ID != "")
                            {
                                if (User_API_Post_Username != "")
                                {
                                    if (User_API_Post_Username.Trim() == DT_Authentication.Rows[0][3].ToString().Trim())
                                    {
                                        DataTable DT_Transaction = new DataTable();
                                        DT_Transaction = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Users_15_API_Transaction Where (UID = '" + Transaction_Unic_ID + "') And (User_ID = '" + DT_Authentication.Rows[0][0].ToString().Trim() + "') And (User_Type_Code = '" + DT_Authentication.Rows[0][1].ToString().Trim() + "') And (Removed = '0')");
                                        if ((DT_Transaction.Rows != null) && (DT_Transaction.Rows.Count == 1))
                                        {
                                            Err_User_ID = DT_Transaction.Rows[0][2].ToString().Trim();
                                            Err_User_Type = DT_Transaction.Rows[0][3].ToString().Trim();
                                            User_API_AccessPolicy UAP = new User_API_AccessPolicy();

                                            if (UAP.User_Access(API_Username, API_Password, API_Code) == true)
                                            {
                                                if (UAP.User_Access(API_Username, API_Password, DT_Transaction.Rows[0][5].ToString().Trim()) == true)
                                                {
                                                    if (Image_ID != "")
                                                    {
                                                        HttpResponseMessage res = new HttpResponseMessage(HttpStatusCode.OK);
                                                        string BasicFileFolder = "";
                                                        string ImageFileName = "";
                                                        string ImageFileTitle = "";
                                                        string ImageFileFormat = "";
                                                        if (Image_ID[0].ToString().ToUpper() == "U") { BasicFileFolder = "Upload"; }
                                                        if (Image_ID[0].ToString().ToUpper() == "P") { BasicFileFolder = "Result"; }
                                                        try { ImageFileName = Image_ID.Substring(1, Image_ID.Length - 1).Trim(); } catch (Exception) { ImageFileName = "ArashMasihi843"; }
                                                        if (ImageFileName == DT_Transaction.Rows[0][52].ToString().Trim())
                                                        {
                                                            ImageFileTitle = DT_Transaction.Rows[0][48].ToString().Trim();
                                                            ImageFileFormat = DT_Transaction.Rows[0][50].ToString().Trim();
                                                            if (DT_Transaction.Rows[0][53].ToString().Trim() == "0")
                                                            {
                                                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Image_1_Download_Count] = '1',[Image_1_Download_FIrst_Date] = '" + Sq.Sql_Date() + "',[Image_1_Download_FIrst_Time] = '" + Sq.Sql_Time() + "' Where (ID = '" + DT_Transaction.Rows[0][0].ToString().Trim() + "')");
                                                            }
                                                            else
                                                            {
                                                                int DCnt = 1;
                                                                DCnt = int.Parse(DT_Transaction.Rows[0][53].ToString().Trim());
                                                                DCnt++;
                                                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Image_1_Download_Count] = '" + DCnt + "',[Image_1_Download_Last_Date] = '" + Sq.Sql_Date() + "',[Image_1_Download_Last_Time] = '" + Sq.Sql_Time() + "' Where (ID = '" + DT_Transaction.Rows[0][0].ToString().Trim() + "')");
                                                            }
                                                        }
                                                        if (ImageFileName == DT_Transaction.Rows[0][62].ToString().Trim())
                                                        {
                                                            ImageFileTitle = DT_Transaction.Rows[0][58].ToString().Trim();
                                                            ImageFileFormat = DT_Transaction.Rows[0][60].ToString().Trim();
                                                            if (DT_Transaction.Rows[0][63].ToString().Trim() == "0")
                                                            {
                                                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Image_2_Download_Count] = '1',[Image_2_Download_FIrst_Date] = '" + Sq.Sql_Date() + "',[Image_2_Download_FIrst_Time] = '" + Sq.Sql_Time() + "' Where (ID = '" + DT_Transaction.Rows[0][0].ToString().Trim() + "')");
                                                            }
                                                            else
                                                            {
                                                                int DCnt = 1;
                                                                DCnt = int.Parse(DT_Transaction.Rows[0][63].ToString().Trim());
                                                                DCnt++;
                                                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Image_2_Download_Count] = '" + DCnt + "',[Image_2_Download_Last_Date] = '" + Sq.Sql_Date() + "',[Image_2_Download_Last_Time] = '" + Sq.Sql_Time() + "' Where (ID = '" + DT_Transaction.Rows[0][0].ToString().Trim() + "')");
                                                            }
                                                        }
                                                        if (ImageFileName == DT_Transaction.Rows[0][72].ToString().Trim())
                                                        {
                                                            ImageFileTitle = DT_Transaction.Rows[0][68].ToString().Trim();
                                                            ImageFileFormat = DT_Transaction.Rows[0][70].ToString().Trim();
                                                            if (DT_Transaction.Rows[0][73].ToString().Trim() == "0")
                                                            {
                                                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Image_3_Download_Count] = '1',[Image_3_Download_FIrst_Date] = '" + Sq.Sql_Date() + "',[Image_3_Download_FIrst_Time] = '" + Sq.Sql_Time() + "' Where (ID = '" + DT_Transaction.Rows[0][0].ToString().Trim() + "')");
                                                            }
                                                            else
                                                            {
                                                                int DCnt = 1;
                                                                DCnt = int.Parse(DT_Transaction.Rows[0][73].ToString().Trim());
                                                                DCnt++;
                                                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Image_3_Download_Count] = '" + DCnt + "',[Image_3_Download_Last_Date] = '" + Sq.Sql_Date() + "',[Image_3_Download_Last_Time] = '" + Sq.Sql_Time() + "' Where (ID = '" + DT_Transaction.Rows[0][0].ToString().Trim() + "')");
                                                            }
                                                        }
                                                        if (ImageFileName == DT_Transaction.Rows[0][82].ToString().Trim())
                                                        {
                                                            ImageFileTitle = DT_Transaction.Rows[0][78].ToString().Trim();
                                                            ImageFileFormat = DT_Transaction.Rows[0][80].ToString().Trim();
                                                            if (DT_Transaction.Rows[0][83].ToString().Trim() == "0")
                                                            {
                                                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Image_4_Download_Count] = '1',[Image_4_Download_FIrst_Date] = '" + Sq.Sql_Date() + "',[Image_4_Download_FIrst_Time] = '" + Sq.Sql_Time() + "' Where (ID = '" + DT_Transaction.Rows[0][0].ToString().Trim() + "')");
                                                            }
                                                            else
                                                            {
                                                                int DCnt = 1;
                                                                DCnt = int.Parse(DT_Transaction.Rows[0][83].ToString().Trim());
                                                                DCnt++;
                                                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Image_4_Download_Count] = '" + DCnt + "',[Image_4_Download_Last_Date] = '" + Sq.Sql_Date() + "',[Image_4_Download_Last_Time] = '" + Sq.Sql_Time() + "' Where (ID = '" + DT_Transaction.Rows[0][0].ToString().Trim() + "')");
                                                            }
                                                        }
                                                        if (ImageFileName == DT_Transaction.Rows[0][92].ToString().Trim())
                                                        {
                                                            ImageFileTitle = DT_Transaction.Rows[0][88].ToString().Trim();
                                                            ImageFileFormat = DT_Transaction.Rows[0][90].ToString().Trim();
                                                            if (DT_Transaction.Rows[0][93].ToString().Trim() == "0")
                                                            {
                                                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Image_5_Download_Count] = '1',[Image_5_Download_FIrst_Date] = '" + Sq.Sql_Date() + "',[Image_5_Download_FIrst_Time] = '" + Sq.Sql_Time() + "' Where (ID = '" + DT_Transaction.Rows[0][0].ToString().Trim() + "')");
                                                            }
                                                            else
                                                            {
                                                                int DCnt = 1;
                                                                DCnt = int.Parse(DT_Transaction.Rows[0][93].ToString().Trim());
                                                                DCnt++;
                                                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Image_5_Download_Count] = '" + DCnt + "',[Image_5_Download_Last_Date] = '" + Sq.Sql_Date() + "',[Image_5_Download_Last_Time] = '" + Sq.Sql_Time() + "' Where (ID = '" + DT_Transaction.Rows[0][0].ToString().Trim() + "')");
                                                            }
                                                        }
                                                        if (ImageFileName == DT_Transaction.Rows[0][102].ToString().Trim())
                                                        {
                                                            ImageFileTitle = DT_Transaction.Rows[0][98].ToString().Trim();
                                                            ImageFileFormat = DT_Transaction.Rows[0][100].ToString().Trim();
                                                            if (DT_Transaction.Rows[0][103].ToString().Trim() == "0")
                                                            {
                                                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Image_6_Download_Count] = '1',[Image_6_Download_FIrst_Date] = '" + Sq.Sql_Date() + "',[Image_6_Download_FIrst_Time] = '" + Sq.Sql_Time() + "' Where (ID = '" + DT_Transaction.Rows[0][0].ToString().Trim() + "')");
                                                            }
                                                            else
                                                            {
                                                                int DCnt = 1;
                                                                DCnt = int.Parse(DT_Transaction.Rows[0][103].ToString().Trim());
                                                                DCnt++;
                                                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Image_6_Download_Count] = '" + DCnt + "',[Image_6_Download_Last_Date] = '" + Sq.Sql_Date() + "',[Image_6_Download_Last_Time] = '" + Sq.Sql_Time() + "' Where (ID = '" + DT_Transaction.Rows[0][0].ToString().Trim() + "')");
                                                            }
                                                        }
                                                        try
                                                        {
                                                            var ImgPath = HttpContext.Current.Server.MapPath("~/Drive/Users/API/" + DT_Transaction.Rows[0][0].ToString().Trim() + "/" + BasicFileFolder + "/" + ImageFileName + "." + ImageFileFormat);
                                                            if (File.Exists(ImgPath) == true)
                                                            {
                                                                res = new HttpResponseMessage(HttpStatusCode.OK);
                                                                var stream = new FileStream(ImgPath, FileMode.Open, FileAccess.Read);
                                                                res.Content = new StreamContent(stream);

                                                                res.Content.Headers.ContentDisposition =
                                                                    new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                                                                    {
                                                                        FileName = ImageFileTitle + "-" + ImageFileName + "-" + DT_Transaction.Rows[0][1].ToString().Trim() + "." + ImageFileFormat
                                                                    };
                                                                res.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                                                                return res;
                                                            }
                                                            else
                                                            {
                                                                FunctionResult_Mesaage.Code = 13;
                                                                FunctionResult_Mesaage.Error = true;
                                                                FunctionResult_Mesaage.Description = "Image ID is not valid, Please reviewing your request structure";
                                                            }
                                                        }
                                                        catch (Exception)
                                                        {
                                                            FunctionResult_Mesaage.Code = 12;
                                                            FunctionResult_Mesaage.Error = true;
                                                            FunctionResult_Mesaage.Description = "Image ID is not valid, Please reviewing your request structure";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        FunctionResult_Mesaage.Code = 11;
                                                        FunctionResult_Mesaage.Error = true;
                                                        FunctionResult_Mesaage.Description = "Image ID is not founded, Please reviewing your request structure";
                                                    }
                                                }
                                                else
                                                {
                                                    FunctionResult_Mesaage.Code = 8;
                                                    FunctionResult_Mesaage.Error = true;
                                                    FunctionResult_Mesaage.Description = "You do not have permission to use this service, Please contact the IDV support team";
                                                }
                                            }
                                            else
                                            {
                                                FunctionResult_Mesaage.Code = 10;
                                                FunctionResult_Mesaage.Error = true;
                                                FunctionResult_Mesaage.Description = "You do not have permission to use this service, Please contact the IDV support team";
                                            }
                                        }
                                        else
                                        {
                                            FunctionResult_Mesaage.Code = 9;
                                            FunctionResult_Mesaage.Error = true;
                                            FunctionResult_Mesaage.Description = "Transaction ID is not valid, Please reviewing your request structure";
                                        }
                                    }
                                    else
                                    {
                                        FunctionResult_Mesaage.Code = 7;
                                        FunctionResult_Mesaage.Error = true;
                                        FunctionResult_Mesaage.Description = "Your post API username is not valid, Please reviewing your request structure";
                                    }
                                }
                                else
                                {
                                    FunctionResult_Mesaage.Code = 6;
                                    FunctionResult_Mesaage.Error = true;
                                    FunctionResult_Mesaage.Description = "Your post API username is missing, Please reviewing your request structure";
                                }
                            }
                            else
                            {
                                FunctionResult_Mesaage.Code = 5;
                                FunctionResult_Mesaage.Error = true;
                                FunctionResult_Mesaage.Description = "Your transaction ID is missing, Please reviewing your request structure";
                            }

                        }
                        else
                        {
                            FunctionResult_Mesaage.Code = 4;
                            FunctionResult_Mesaage.Error = true;
                            FunctionResult_Mesaage.Description = "Your account has been blocked. To follow up or resolve this issue, please contact the IDV support team";
                        }
                    }
                    else
                    {
                        FunctionResult_Mesaage.Code = 3;
                        FunctionResult_Mesaage.Error = true;
                        FunctionResult_Mesaage.Description = "Your API username or password is not valid, Please reviewing your API_Username and API_Password values";
                    }
                }
                else
                {
                    FunctionResult_Mesaage.Code = 2;
                    FunctionResult_Mesaage.Error = true;
                    FunctionResult_Mesaage.Description = "Your API username or password is missing, Please reviewing your request structure";
                }
            }
            catch (Exception)
            {
                FunctionResult_Mesaage.Code = 1;
                FunctionResult_Mesaage.Error = true;
                FunctionResult_Mesaage.Description = "IDV server returns an unknown error, Please contact the IDV support team if it is not resolved after reviewing your request";
            }
            //---------------------------------------------------------------------------------------------------------------------------------------------------------
            // Log bad request :
            try
            {
                if (FunctionResult_Mesaage.Error == true)
                {
                    DataTable DT_CreateTransAction = new DataTable();
                    int ErrFT = 0;
                    if (FunctionResult_Mesaage.Error == true) { ErrFT = 1; }
                    string Error_Message = "";
                    Error_Message = "Request API Username : " + Er_API_Username;
                    Error_Message += "$" + "Request API Password : " + Er_API_Password;
                    Error_Message += "$" + "Transaction Unic ID : " + Er_Transaction_Unic_ID;
                    Error_Message += "$" + "Request API Post Username : " + Er_User_API_Post_Username;
                    DT_CreateTransAction = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Insert Into Users_15_API_Transaction OUTPUT Inserted.ID Values ('','" + Err_User_ID + "','" + Err_User_Type + "','" + API_Name + "','" + API_Code + "','" + API_FullName + "','" + Sq.Sql_Date() + "','" + Sq.Sql_Time() + "','" + Pb.GetUserIP_HttpRequest(HttpContext.Current.Request) + "','" + FunctionResult_Mesaage.Code + "','" + ErrFT.ToString() + "','" + FunctionResult_Mesaage.Description + "','3','Failed','0','0','Unknown','0','Unknown','1','0','" + Sq.Sql_Date() + "','" + Sq.Sql_Time() + "','" + Err_User_ID + "','','','0','0','','','0','0','','','0','0','','','','0','0','','','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','" + Error_Message.Trim() + "','5')");
                    string Er_Trans_ID = DT_CreateTransAction.Rows[0][0].ToString().Trim();
                    string Er_Trans_UID = Er_Trans_ID + Pb.Make_Security_Code(50);
                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [UID] = '" + Er_Trans_UID + "' Where (ID = '" + Er_Trans_ID + "')");
                    FunctionResult_Mesaage.Transaction_ID = Er_Trans_UID;
                }
            }
            catch (Exception)
            { }
            //---------------------------------------------------------------------------------------------------------------------------------------------------------
            // Last API Result :
            return FunctionResult_Mesaage;
            //---------------------------------------------------------------------------------------------------------------------------------------------------------
        }
    }
}
