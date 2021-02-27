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

namespace iCore_Administrator.API.Result
{
    [Route("api/Result/RetrieveTransaction")]
    public class RetrieveTransactionController : ApiController
    {
        //=========================================================================================================================================
        SQL_Tranceiver Sq = new SQL_Tranceiver();
        PublicFunctions Pb = new PublicFunctions();
        //=========================================================================================================================================
        public API_Main_Result Get()
        {
            //---------------------------------------------------------------------------------------------------------------------------------------------------------
            // Result Structure :
            API_Mesaage FunctionResult_Mesaage = new API_Mesaage();
            object Result_API = null;
            string API_Name = "RetrieveTransaction";
            string API_Code = "5";
            string API_FullName = "Retrieve Transaction public function";
            //---------------------------------------------------------------------------------------------------------------------------------------------------------
            string Er_API_Username = "";
            string Er_API_Password = "";
            string Er_Transaction_Unic_ID = "";
            string Er_User_API_Post_Username = "";
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
                            try { Transaction_Unic_ID = HttpContext.Current.Request.Params["Transaction_ID"].Trim(); } catch (Exception) { }
                            try { User_API_Post_Username = HttpContext.Current.Request.Params["Username"].Trim(); } catch (Exception) { }
                            Er_Transaction_Unic_ID = Transaction_Unic_ID;
                            Er_User_API_Post_Username = User_API_Post_Username;
                            FunctionResult_Mesaage.Transaction_ID = Er_Transaction_Unic_ID;
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
                                            if (UAP.User_Access(API_Username, API_Password, DT_Transaction.Rows[0][5].ToString().Trim()) == true)
                                            {
                                                switch (DT_Transaction.Rows[0][5].ToString().Trim())
                                                {
                                                    case "1":
                                                        {
                                                            string Trans_ID = DT_Transaction.Rows[0][0].ToString().Trim();
                                                            string Date_Format = DT_Transaction.Rows[0][15].ToString().Trim();
                                                            Acuant_CLS_OCR_Sync AIDC = new Acuant_CLS_OCR_Sync();
                                                            Transaction_CLS Transaction_CLS = new Transaction_CLS();
                                                            Transaction_CLS.Name = DT_Transaction.Rows[0][4].ToString().Trim();
                                                            Transaction_CLS.ID = DT_Transaction.Rows[0][1].ToString().Trim();
                                                            Transaction_CLS.Username = API_Username;
                                                            Transaction_CLS.Date_Format = Date_Format;
                                                            Transaction_CLS.Request_Date = Pb.ConvertDate_Format(DT_Transaction.Rows[0][7].ToString().Trim(), "yyyy-MM-dd", Date_Format);
                                                            Transaction_CLS.Request_Time = DT_Transaction.Rows[0][8].ToString().Trim();
                                                            Transaction_CLS.Request_IP = DT_Transaction.Rows[0][9].ToString().Trim();
                                                            Transaction_CLS.Attached_File = 0;
                                                            if (DT_Transaction.Rows[0][37].ToString().Trim() == "") { Transaction_CLS.Async = false; } else { Transaction_CLS.Async = true; }
                                                            Transaction_CLS.Callback_URL = DT_Transaction.Rows[0][37].ToString().Trim();
                                                            Transaction_CLS.Status_Code = int.Parse(DT_Transaction.Rows[0][13].ToString().Trim());
                                                            Transaction_CLS.Status_Result = DT_Transaction.Rows[0][14].ToString().Trim();
                                                            AIDC.Transaction = Transaction_CLS;
                                                            var jsonObject_CD = new JObject();
                                                            DataTable DT_CaptureData = new DataTable();
                                                            DT_CaptureData = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Document_Key,Document_Value From Users_16_API_Acuant_Result Where (Transaction_ID = '" + Trans_ID + "') Order By Document_ID");
                                                            if (DT_CaptureData.Rows != null)
                                                            {
                                                                foreach (DataRow RW in DT_CaptureData.Rows)
                                                                {
                                                                    try
                                                                    {
                                                                        string DataValue = RW[1].ToString().Trim();
                                                                        try
                                                                        {
                                                                            string CheckDate = DataValue.Substring(0, 10);
                                                                            if ((CheckDate[2] == '/') && (CheckDate[5] == '/'))
                                                                            {
                                                                                DataValue = Pb.ConvertDate_Format(CheckDate, "dd/MM/yyyy", Date_Format);
                                                                            }
                                                                        }
                                                                        catch (Exception)
                                                                        {
                                                                            DataValue = RW[1].ToString().Trim();
                                                                        }
                                                                        jsonObject_CD.Add(RW[0].ToString().Trim().Replace("  ", " ").Replace(" ", "_"), DataValue);
                                                                    }
                                                                    catch (Exception) { }
                                                                }
                                                            }
                                                            AIDC.Data = jsonObject_CD;
                                                            int Img_Cnt = 0;
                                                            try
                                                            {
                                                                Transaction_File TFL = new Transaction_File();
                                                                DataTable DT_Files = new DataTable();
                                                                DT_Files = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Image_1_Title,Image_1_Download_ID,Image_1_File_Format,Image_2_Title,Image_2_Download_ID,Image_2_File_Format,Image_3_Title,Image_3_Download_ID,Image_3_File_Format,Image_4_Title,Image_4_Download_ID,Image_4_File_Format,Image_5_Title,Image_5_Download_ID,Image_5_File_Format,Image_6_Title,Image_6_Download_ID,Image_6_File_Format From Users_15_API_Transaction Where (ID = '" + Trans_ID + "')");
                                                                string FilePath = "";
                                                                var jsonObject_Upload = new JObject();
                                                                for (int i = 0; i < 18; i += 3)
                                                                {
                                                                    try
                                                                    {
                                                                        FilePath = "";
                                                                        if (DT_Files.Rows[0][i + 1].ToString().Trim() != "")
                                                                        {
                                                                            FilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Trans_ID + "/Upload/" + DT_Files.Rows[0][i + 1].ToString().Trim() + "." + DT_Files.Rows[0][i + 2].ToString().Trim());
                                                                            if (File.Exists(FilePath) == true)
                                                                            {
                                                                                jsonObject_Upload.Add(DT_Files.Rows[0][i].ToString().Trim().Replace("  ", " ").Replace(" ", "_"), "U" + DT_Files.Rows[0][i + 1].ToString().Trim());
                                                                                Img_Cnt++;
                                                                            }
                                                                        }
                                                                    }
                                                                    catch (Exception) { }
                                                                }
                                                                TFL.Upload = jsonObject_Upload;
                                                                var jsonObject_Processed = new JObject();
                                                                for (int i = 0; i < 18; i += 3)
                                                                {
                                                                    try
                                                                    {
                                                                        FilePath = "";
                                                                        if (DT_Files.Rows[0][i + 1].ToString().Trim() != "")
                                                                        {
                                                                            FilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Trans_ID + "/Result/" + DT_Files.Rows[0][i + 1].ToString().Trim() + "." + DT_Files.Rows[0][i + 2].ToString().Trim());
                                                                            if (File.Exists(FilePath) == true)
                                                                            {
                                                                                jsonObject_Processed.Add(DT_Files.Rows[0][i].ToString().Trim().Replace("  ", " ").Replace(" ", "_"), "P" + DT_Files.Rows[0][i + 1].ToString().Trim());
                                                                            }
                                                                        }
                                                                    }
                                                                    catch (Exception) { }
                                                                }
                                                                TFL.Processed = jsonObject_Processed;
                                                                AIDC.Files = TFL;
                                                            }
                                                            catch (Exception) { }
                                                            AIDC.Transaction.Attached_File = Img_Cnt;
                                                            Result_API = AIDC;
                                                            break;
                                                        }
                                                    case "2":
                                                        {
                                                            string Trans_ID = DT_Transaction.Rows[0][0].ToString().Trim();
                                                            string Date_Format = DT_Transaction.Rows[0][15].ToString().Trim();
                                                            Acuant_CLS_Validation_Sync AIDC = new Acuant_CLS_Validation_Sync();
                                                            Transaction_CLS Transaction_CLS = new Transaction_CLS();
                                                            Transaction_CLS.Name = DT_Transaction.Rows[0][4].ToString().Trim();
                                                            Transaction_CLS.ID = DT_Transaction.Rows[0][1].ToString().Trim();
                                                            Transaction_CLS.Username = API_Username;
                                                            Transaction_CLS.Date_Format = Date_Format;
                                                            Transaction_CLS.Request_Date = Pb.ConvertDate_Format(DT_Transaction.Rows[0][7].ToString().Trim(), "yyyy-MM-dd", Date_Format);
                                                            Transaction_CLS.Request_Time = DT_Transaction.Rows[0][8].ToString().Trim();
                                                            Transaction_CLS.Request_IP = DT_Transaction.Rows[0][9].ToString().Trim();
                                                            Transaction_CLS.Attached_File = 0;
                                                            if (DT_Transaction.Rows[0][37].ToString().Trim() == "") { Transaction_CLS.Async = false; } else { Transaction_CLS.Async = true; }
                                                            Transaction_CLS.Callback_URL = DT_Transaction.Rows[0][37].ToString().Trim();
                                                            Transaction_CLS.Status_Code = int.Parse(DT_Transaction.Rows[0][13].ToString().Trim());
                                                            Transaction_CLS.Status_Result = DT_Transaction.Rows[0][14].ToString().Trim();
                                                            AIDC.Transaction = Transaction_CLS;
                                                            var jsonObject_CD = new JObject();
                                                            DataTable DT_CaptureData = new DataTable();
                                                            DT_CaptureData = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Document_Key,Document_Value From Users_16_API_Acuant_Result Where (Transaction_ID = '" + Trans_ID + "') Order By Document_ID");
                                                            if (DT_CaptureData.Rows != null)
                                                            {
                                                                foreach (DataRow RW in DT_CaptureData.Rows)
                                                                {
                                                                    try
                                                                    {
                                                                        string DataValue = RW[1].ToString().Trim();
                                                                        try
                                                                        {
                                                                            string CheckDate = DataValue.Substring(0, 10);
                                                                            if ((CheckDate[2] == '/') && (CheckDate[5] == '/'))
                                                                            {
                                                                                DataValue = Pb.ConvertDate_Format(CheckDate, "dd/MM/yyyy", Date_Format);
                                                                            }
                                                                        }
                                                                        catch (Exception)
                                                                        {
                                                                            DataValue = RW[1].ToString().Trim();
                                                                        }
                                                                        jsonObject_CD.Add(RW[0].ToString().Trim().Replace("  ", " ").Replace(" ", "_"), DataValue);
                                                                    }
                                                                    catch (Exception) { }
                                                                }
                                                            }
                                                            AIDC.Data = jsonObject_CD;
                                                            jsonObject_CD = new JObject();
                                                            DataTable DT_Alert = new DataTable();
                                                            DT_Alert = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Alert_Text From Users_17_API_Acuant_Alert Where (Transaction_ID = '" + Trans_ID + "') Order By Alert_Text");
                                                            if (DT_Alert.Rows != null)
                                                            {
                                                                int NoCounter = 0;
                                                                foreach (DataRow RW in DT_Alert.Rows)
                                                                {
                                                                    try
                                                                    {
                                                                        NoCounter++;
                                                                        jsonObject_CD.Add("N" + NoCounter.ToString(), RW[0].ToString().Trim());
                                                                    }
                                                                    catch (Exception) { }
                                                                }
                                                            }
                                                            AIDC.Alert = jsonObject_CD;
                                                            jsonObject_CD = new JObject();
                                                            DataTable DT_Authtication = new DataTable();
                                                            DT_Authtication = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Top 1 Result_Code,Result_Text From Users_18_API_Acuant_Authentication Where (Transaction_ID = '" + Trans_ID + "')");
                                                            if (DT_Authtication.Rows != null)
                                                            {
                                                                try
                                                                {
                                                                    jsonObject_CD.Add("Code", DT_Authtication.Rows[0][0].ToString().Trim());
                                                                    jsonObject_CD.Add("Result", DT_Authtication.Rows[0][1].ToString().Trim());
                                                                }
                                                                catch (Exception) { }
                                                            }
                                                            AIDC.Authentication = jsonObject_CD;
                                                            int Img_Cnt = 0;
                                                            try
                                                            {
                                                                Transaction_File TFL = new Transaction_File();
                                                                DataTable DT_Files = new DataTable();
                                                                DT_Files = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Image_1_Title,Image_1_Download_ID,Image_1_File_Format,Image_2_Title,Image_2_Download_ID,Image_2_File_Format,Image_3_Title,Image_3_Download_ID,Image_3_File_Format,Image_4_Title,Image_4_Download_ID,Image_4_File_Format,Image_5_Title,Image_5_Download_ID,Image_5_File_Format,Image_6_Title,Image_6_Download_ID,Image_6_File_Format From Users_15_API_Transaction Where (ID = '" + Trans_ID + "')");
                                                                string FilePath = "";
                                                                var jsonObject_Upload = new JObject();
                                                                for (int i = 0; i < 18; i += 3)
                                                                {
                                                                    try
                                                                    {
                                                                        FilePath = "";
                                                                        if (DT_Files.Rows[0][i + 1].ToString().Trim() != "")
                                                                        {
                                                                            FilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Trans_ID + "/Upload/" + DT_Files.Rows[0][i + 1].ToString().Trim() + "." + DT_Files.Rows[0][i + 2].ToString().Trim());
                                                                            if (File.Exists(FilePath) == true)
                                                                            {
                                                                                jsonObject_Upload.Add(DT_Files.Rows[0][i].ToString().Trim().Replace("  ", " ").Replace(" ", "_"), "U" + DT_Files.Rows[0][i + 1].ToString().Trim());
                                                                                Img_Cnt++;
                                                                            }
                                                                        }
                                                                    }
                                                                    catch (Exception) { }
                                                                }
                                                                TFL.Upload = jsonObject_Upload;
                                                                var jsonObject_Processed = new JObject();
                                                                for (int i = 0; i < 18; i += 3)
                                                                {
                                                                    try
                                                                    {
                                                                        FilePath = "";
                                                                        if (DT_Files.Rows[0][i + 1].ToString().Trim() != "")
                                                                        {
                                                                            FilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Trans_ID + "/Result/" + DT_Files.Rows[0][i + 1].ToString().Trim() + "." + DT_Files.Rows[0][i + 2].ToString().Trim());
                                                                            if (File.Exists(FilePath) == true)
                                                                            {
                                                                                jsonObject_Processed.Add(DT_Files.Rows[0][i].ToString().Trim().Replace("  ", " ").Replace(" ", "_"), "P" + DT_Files.Rows[0][i + 1].ToString().Trim());
                                                                            }
                                                                        }
                                                                    }
                                                                    catch (Exception) { }
                                                                }
                                                                TFL.Processed = jsonObject_Processed;
                                                                AIDC.Files = TFL;
                                                            }
                                                            catch (Exception) { }
                                                            AIDC.Transaction.Attached_File = Img_Cnt;
                                                            Result_API = AIDC;
                                                            break;
                                                        }
                                                    case "3":
                                                        {
                                                            string Trans_ID = DT_Transaction.Rows[0][0].ToString().Trim();
                                                            string Date_Format = DT_Transaction.Rows[0][15].ToString().Trim();
                                                            Acuant_CLS_OCR_Sync AIDC = new Acuant_CLS_OCR_Sync();
                                                            Transaction_CLS Transaction_CLS = new Transaction_CLS();
                                                            Transaction_CLS.Name = DT_Transaction.Rows[0][4].ToString().Trim();
                                                            Transaction_CLS.ID = DT_Transaction.Rows[0][1].ToString().Trim();
                                                            Transaction_CLS.Username = API_Username;
                                                            Transaction_CLS.Date_Format = Date_Format;
                                                            Transaction_CLS.Request_Date = Pb.ConvertDate_Format(DT_Transaction.Rows[0][7].ToString().Trim(), "yyyy-MM-dd", Date_Format);
                                                            Transaction_CLS.Request_Time = DT_Transaction.Rows[0][8].ToString().Trim();
                                                            Transaction_CLS.Request_IP = DT_Transaction.Rows[0][9].ToString().Trim();
                                                            Transaction_CLS.Attached_File = 0;
                                                            if (DT_Transaction.Rows[0][37].ToString().Trim() == "") { Transaction_CLS.Async = false; } else { Transaction_CLS.Async = true; }
                                                            Transaction_CLS.Callback_URL = DT_Transaction.Rows[0][37].ToString().Trim();
                                                            Transaction_CLS.Status_Code = int.Parse(DT_Transaction.Rows[0][13].ToString().Trim());
                                                            Transaction_CLS.Status_Result = DT_Transaction.Rows[0][14].ToString().Trim();
                                                            AIDC.Transaction = Transaction_CLS;
                                                            var jsonObject_CD = new JObject();
                                                            DataTable DT_CaptureData = new DataTable();
                                                            DT_CaptureData = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Document_Key,Document_Value From Users_16_API_Acuant_Result Where (Transaction_ID = '" + Trans_ID + "') Order By Document_ID");
                                                            if (DT_CaptureData.Rows != null)
                                                            {
                                                                foreach (DataRow RW in DT_CaptureData.Rows)
                                                                {
                                                                    try
                                                                    {
                                                                        string DataValue = RW[1].ToString().Trim();
                                                                        try
                                                                        {
                                                                            string CheckDate = DataValue.Substring(0, 10);
                                                                            if ((CheckDate[2] == '/') && (CheckDate[5] == '/'))
                                                                            {
                                                                                DataValue = Pb.ConvertDate_Format(CheckDate, "dd/MM/yyyy", Date_Format);
                                                                            }
                                                                        }
                                                                        catch (Exception)
                                                                        {
                                                                            DataValue = RW[1].ToString().Trim();
                                                                        }
                                                                        jsonObject_CD.Add(RW[0].ToString().Trim().Replace("  ", " ").Replace(" ", "_"), DataValue);
                                                                    }
                                                                    catch (Exception) { }
                                                                }
                                                            }
                                                            AIDC.Data = jsonObject_CD;
                                                            int Img_Cnt = 0;
                                                            try
                                                            {
                                                                Transaction_File TFL = new Transaction_File();
                                                                DataTable DT_Files = new DataTable();
                                                                DT_Files = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Image_1_Title,Image_1_Download_ID,Image_1_File_Format,Image_2_Title,Image_2_Download_ID,Image_2_File_Format,Image_3_Title,Image_3_Download_ID,Image_3_File_Format,Image_4_Title,Image_4_Download_ID,Image_4_File_Format,Image_5_Title,Image_5_Download_ID,Image_5_File_Format,Image_6_Title,Image_6_Download_ID,Image_6_File_Format From Users_15_API_Transaction Where (ID = '" + Trans_ID + "')");
                                                                string FilePath = "";
                                                                var jsonObject_Upload = new JObject();
                                                                for (int i = 0; i < 18; i += 3)
                                                                {
                                                                    try
                                                                    {
                                                                        FilePath = "";
                                                                        if (DT_Files.Rows[0][i + 1].ToString().Trim() != "")
                                                                        {
                                                                            FilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Trans_ID + "/Upload/" + DT_Files.Rows[0][i + 1].ToString().Trim() + "." + DT_Files.Rows[0][i + 2].ToString().Trim());
                                                                            if (File.Exists(FilePath) == true)
                                                                            {
                                                                                jsonObject_Upload.Add(DT_Files.Rows[0][i].ToString().Trim().Replace("  ", " ").Replace(" ", "_"), "U" + DT_Files.Rows[0][i + 1].ToString().Trim());
                                                                                Img_Cnt++;
                                                                            }
                                                                        }
                                                                    }
                                                                    catch (Exception) { }
                                                                }
                                                                TFL.Upload = jsonObject_Upload;
                                                                var jsonObject_Processed = new JObject();
                                                                for (int i = 0; i < 18; i += 3)
                                                                {
                                                                    try
                                                                    {
                                                                        FilePath = "";
                                                                        if (DT_Files.Rows[0][i + 1].ToString().Trim() != "")
                                                                        {
                                                                            FilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Trans_ID + "/Result/" + DT_Files.Rows[0][i + 1].ToString().Trim() + "." + DT_Files.Rows[0][i + 2].ToString().Trim());
                                                                            if (File.Exists(FilePath) == true)
                                                                            {
                                                                                jsonObject_Processed.Add(DT_Files.Rows[0][i].ToString().Trim().Replace("  ", " ").Replace(" ", "_"), "P" + DT_Files.Rows[0][i + 1].ToString().Trim());
                                                                            }
                                                                        }
                                                                    }
                                                                    catch (Exception) { }
                                                                }
                                                                TFL.Processed = jsonObject_Processed;
                                                                AIDC.Files = TFL;
                                                            }
                                                            catch (Exception) { }
                                                            AIDC.Transaction.Attached_File = Img_Cnt;
                                                            Result_API = AIDC;
                                                            break;
                                                        }
                                                    case "4":
                                                        {
                                                            string Trans_ID = DT_Transaction.Rows[0][0].ToString().Trim();
                                                            string Date_Format = DT_Transaction.Rows[0][15].ToString().Trim();
                                                            Acuant_CLS_Validation_Sync AIDC = new Acuant_CLS_Validation_Sync();
                                                            Transaction_CLS Transaction_CLS = new Transaction_CLS();
                                                            Transaction_CLS.Name = DT_Transaction.Rows[0][4].ToString().Trim();
                                                            Transaction_CLS.ID = DT_Transaction.Rows[0][1].ToString().Trim();
                                                            Transaction_CLS.Username = API_Username;
                                                            Transaction_CLS.Date_Format = Date_Format;
                                                            Transaction_CLS.Request_Date = Pb.ConvertDate_Format(DT_Transaction.Rows[0][7].ToString().Trim(), "yyyy-MM-dd", Date_Format);
                                                            Transaction_CLS.Request_Time = DT_Transaction.Rows[0][8].ToString().Trim();
                                                            Transaction_CLS.Request_IP = DT_Transaction.Rows[0][9].ToString().Trim();
                                                            Transaction_CLS.Attached_File = 0;
                                                            if (DT_Transaction.Rows[0][37].ToString().Trim() == "") { Transaction_CLS.Async = false; } else { Transaction_CLS.Async = true; }
                                                            Transaction_CLS.Callback_URL = DT_Transaction.Rows[0][37].ToString().Trim();
                                                            Transaction_CLS.Status_Code = int.Parse(DT_Transaction.Rows[0][13].ToString().Trim());
                                                            Transaction_CLS.Status_Result = DT_Transaction.Rows[0][14].ToString().Trim();
                                                            AIDC.Transaction = Transaction_CLS;
                                                            var jsonObject_CD = new JObject();
                                                            DataTable DT_CaptureData = new DataTable();
                                                            DT_CaptureData = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Document_Key,Document_Value From Users_16_API_Acuant_Result Where (Transaction_ID = '" + Trans_ID + "') Order By Document_ID");
                                                            if (DT_CaptureData.Rows != null)
                                                            {
                                                                foreach (DataRow RW in DT_CaptureData.Rows)
                                                                {
                                                                    try
                                                                    {
                                                                        string DataValue = RW[1].ToString().Trim();
                                                                        try
                                                                        {
                                                                            string CheckDate = DataValue.Substring(0, 10);
                                                                            if ((CheckDate[2] == '/') && (CheckDate[5] == '/'))
                                                                            {
                                                                                DataValue = Pb.ConvertDate_Format(CheckDate, "dd/MM/yyyy", Date_Format);
                                                                            }
                                                                        }
                                                                        catch (Exception)
                                                                        {
                                                                            DataValue = RW[1].ToString().Trim();
                                                                        }
                                                                        jsonObject_CD.Add(RW[0].ToString().Trim().Replace("  ", " ").Replace(" ", "_"), DataValue);
                                                                    }
                                                                    catch (Exception) { }
                                                                }
                                                            }
                                                            AIDC.Data = jsonObject_CD;
                                                            jsonObject_CD = new JObject();
                                                            DataTable DT_Alert = new DataTable();
                                                            DT_Alert = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Alert_Text From Users_17_API_Acuant_Alert Where (Transaction_ID = '" + Trans_ID + "') Order By Alert_Text");
                                                            if (DT_Alert.Rows != null)
                                                            {
                                                                int NoCounter = 0;
                                                                foreach (DataRow RW in DT_Alert.Rows)
                                                                {
                                                                    try
                                                                    {
                                                                        NoCounter++;
                                                                        jsonObject_CD.Add("N" + NoCounter.ToString(), RW[0].ToString().Trim());
                                                                    }
                                                                    catch (Exception) { }
                                                                }
                                                            }
                                                            AIDC.Alert = jsonObject_CD;
                                                            jsonObject_CD = new JObject();
                                                            DataTable DT_Authtication = new DataTable();
                                                            DT_Authtication = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Top 1 Result_Code,Result_Text From Users_18_API_Acuant_Authentication Where (Transaction_ID = '" + Trans_ID + "')");
                                                            if (DT_Authtication.Rows != null)
                                                            {
                                                                try
                                                                {
                                                                    jsonObject_CD.Add("Code", DT_Authtication.Rows[0][0].ToString().Trim());
                                                                    jsonObject_CD.Add("Result", DT_Authtication.Rows[0][1].ToString().Trim());
                                                                }
                                                                catch (Exception) { }
                                                            }
                                                            AIDC.Authentication = jsonObject_CD;
                                                            int Img_Cnt = 0;
                                                            try
                                                            {
                                                                Transaction_File TFL = new Transaction_File();
                                                                DataTable DT_Files = new DataTable();
                                                                DT_Files = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Image_1_Title,Image_1_Download_ID,Image_1_File_Format,Image_2_Title,Image_2_Download_ID,Image_2_File_Format,Image_3_Title,Image_3_Download_ID,Image_3_File_Format,Image_4_Title,Image_4_Download_ID,Image_4_File_Format,Image_5_Title,Image_5_Download_ID,Image_5_File_Format,Image_6_Title,Image_6_Download_ID,Image_6_File_Format From Users_15_API_Transaction Where (ID = '" + Trans_ID + "')");
                                                                string FilePath = "";
                                                                var jsonObject_Upload = new JObject();
                                                                for (int i = 0; i < 18; i += 3)
                                                                {
                                                                    try
                                                                    {
                                                                        FilePath = "";
                                                                        if (DT_Files.Rows[0][i + 1].ToString().Trim() != "")
                                                                        {
                                                                            FilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Trans_ID + "/Upload/" + DT_Files.Rows[0][i + 1].ToString().Trim() + "." + DT_Files.Rows[0][i + 2].ToString().Trim());
                                                                            if (File.Exists(FilePath) == true)
                                                                            {
                                                                                jsonObject_Upload.Add(DT_Files.Rows[0][i].ToString().Trim().Replace("  ", " ").Replace(" ", "_"), "U" + DT_Files.Rows[0][i + 1].ToString().Trim());
                                                                                Img_Cnt++;
                                                                            }
                                                                        }
                                                                    }
                                                                    catch (Exception) { }
                                                                }
                                                                TFL.Upload = jsonObject_Upload;
                                                                var jsonObject_Processed = new JObject();
                                                                for (int i = 0; i < 18; i += 3)
                                                                {
                                                                    try
                                                                    {
                                                                        FilePath = "";
                                                                        if (DT_Files.Rows[0][i + 1].ToString().Trim() != "")
                                                                        {
                                                                            FilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Trans_ID + "/Result/" + DT_Files.Rows[0][i + 1].ToString().Trim() + "." + DT_Files.Rows[0][i + 2].ToString().Trim());
                                                                            if (File.Exists(FilePath) == true)
                                                                            {
                                                                                jsonObject_Processed.Add(DT_Files.Rows[0][i].ToString().Trim().Replace("  ", " ").Replace(" ", "_"), "P" + DT_Files.Rows[0][i + 1].ToString().Trim());
                                                                            }
                                                                        }
                                                                    }
                                                                    catch (Exception) { }
                                                                }
                                                                TFL.Processed = jsonObject_Processed;
                                                                AIDC.Files = TFL;
                                                            }
                                                            catch (Exception) { }
                                                            AIDC.Transaction.Attached_File = Img_Cnt;
                                                            Result_API = AIDC;
                                                            break;
                                                        }
                                                    case "7":
                                                        {
                                                            string Trans_ID = DT_Transaction.Rows[0][0].ToString().Trim();
                                                            string Date_Format = DT_Transaction.Rows[0][15].ToString().Trim();
                                                            Acuant_CLS_OCR_Sync AIDC = new Acuant_CLS_OCR_Sync();
                                                            Transaction_CLS Transaction_CLS = new Transaction_CLS();
                                                            Transaction_CLS.Name = DT_Transaction.Rows[0][4].ToString().Trim();
                                                            Transaction_CLS.ID = DT_Transaction.Rows[0][1].ToString().Trim();
                                                            Transaction_CLS.Username = API_Username;
                                                            Transaction_CLS.Date_Format = Date_Format;
                                                            Transaction_CLS.Request_Date = Pb.ConvertDate_Format(DT_Transaction.Rows[0][7].ToString().Trim(), "yyyy-MM-dd", Date_Format);
                                                            Transaction_CLS.Request_Time = DT_Transaction.Rows[0][8].ToString().Trim();
                                                            Transaction_CLS.Request_IP = DT_Transaction.Rows[0][9].ToString().Trim();
                                                            Transaction_CLS.Attached_File = 0;
                                                            if (DT_Transaction.Rows[0][37].ToString().Trim() == "") { Transaction_CLS.Async = false; } else { Transaction_CLS.Async = true; }
                                                            Transaction_CLS.Callback_URL = DT_Transaction.Rows[0][37].ToString().Trim();
                                                            Transaction_CLS.Status_Code = int.Parse(DT_Transaction.Rows[0][13].ToString().Trim());
                                                            Transaction_CLS.Status_Result = DT_Transaction.Rows[0][14].ToString().Trim();
                                                            AIDC.Transaction = Transaction_CLS;
                                                            var jsonObject_CD = new JObject();
                                                            DataTable DT_CaptureData = new DataTable();
                                                            DT_CaptureData = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Document_Key,Document_Value From Users_16_API_Acuant_Result Where (Transaction_ID = '" + Trans_ID + "') Order By Document_ID");
                                                            if (DT_CaptureData.Rows != null)
                                                            {
                                                                foreach (DataRow RW in DT_CaptureData.Rows)
                                                                {
                                                                    try
                                                                    {
                                                                        string DataValue = RW[1].ToString().Trim();
                                                                        try
                                                                        {
                                                                            string CheckDate = DataValue.Substring(0, 10);
                                                                            if ((CheckDate[2] == '/') && (CheckDate[5] == '/'))
                                                                            {
                                                                                DataValue = Pb.ConvertDate_Format(CheckDate, "dd/MM/yyyy", Date_Format);
                                                                            }
                                                                        }
                                                                        catch (Exception)
                                                                        {
                                                                            DataValue = RW[1].ToString().Trim();
                                                                        }
                                                                        jsonObject_CD.Add(RW[0].ToString().Trim().Replace("  ", " ").Replace(" ", "_"), DataValue);
                                                                    }
                                                                    catch (Exception) { }
                                                                }
                                                            }
                                                            AIDC.Data = jsonObject_CD;
                                                            int Img_Cnt = 0;
                                                            try
                                                            {
                                                                Transaction_File TFL = new Transaction_File();
                                                                DataTable DT_Files = new DataTable();
                                                                DT_Files = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Image_1_Title,Image_1_Download_ID,Image_1_File_Format,Image_2_Title,Image_2_Download_ID,Image_2_File_Format,Image_3_Title,Image_3_Download_ID,Image_3_File_Format,Image_4_Title,Image_4_Download_ID,Image_4_File_Format,Image_5_Title,Image_5_Download_ID,Image_5_File_Format,Image_6_Title,Image_6_Download_ID,Image_6_File_Format From Users_15_API_Transaction Where (ID = '" + Trans_ID + "')");
                                                                string FilePath = "";
                                                                var jsonObject_Upload = new JObject();
                                                                for (int i = 0; i < 18; i += 3)
                                                                {
                                                                    try
                                                                    {
                                                                        FilePath = "";
                                                                        if (DT_Files.Rows[0][i + 1].ToString().Trim() != "")
                                                                        {
                                                                            FilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Trans_ID + "/Upload/" + DT_Files.Rows[0][i + 1].ToString().Trim() + "." + DT_Files.Rows[0][i + 2].ToString().Trim());
                                                                            if (File.Exists(FilePath) == true)
                                                                            {
                                                                                jsonObject_Upload.Add(DT_Files.Rows[0][i].ToString().Trim().Replace("  ", " ").Replace(" ", "_"), "U" + DT_Files.Rows[0][i + 1].ToString().Trim());
                                                                                Img_Cnt++;
                                                                            }
                                                                        }
                                                                    }
                                                                    catch (Exception) { }
                                                                }
                                                                TFL.Upload = jsonObject_Upload;
                                                                var jsonObject_Processed = new JObject();
                                                                for (int i = 0; i < 18; i += 3)
                                                                {
                                                                    try
                                                                    {
                                                                        FilePath = "";
                                                                        if (DT_Files.Rows[0][i + 1].ToString().Trim() != "")
                                                                        {
                                                                            FilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Trans_ID + "/Result/" + DT_Files.Rows[0][i + 1].ToString().Trim() + "." + DT_Files.Rows[0][i + 2].ToString().Trim());
                                                                            if (File.Exists(FilePath) == true)
                                                                            {
                                                                                jsonObject_Processed.Add(DT_Files.Rows[0][i].ToString().Trim().Replace("  ", " ").Replace(" ", "_"), "P" + DT_Files.Rows[0][i + 1].ToString().Trim());
                                                                            }
                                                                        }
                                                                    }
                                                                    catch (Exception) { }
                                                                }
                                                                TFL.Processed = jsonObject_Processed;
                                                                AIDC.Files = TFL;
                                                            }
                                                            catch (Exception) { }
                                                            AIDC.Transaction.Attached_File = Img_Cnt;
                                                            Result_API = AIDC;
                                                            break;
                                                        }
                                                }
                                                try
                                                {
                                                    string URetIP = Pb.GetUserIP_HttpRequest(HttpContext.Current.Request); ;
                                                    if (DT_Transaction.Rows[0][40].ToString().Trim() == "0")
                                                    {
                                                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Retrieve_Flag] = '1',[Retrieve_Count] = '1',[Retrieve_First_Date] = '" + Sq.Sql_Date() + "',[Retrieve_First_Time] = '" + Sq.Sql_Time() + "',[Retrieve_First_IP] = '" + URetIP + "',[Retrieve_Last_Date] = '" + Sq.Sql_Date() + "',[Retrieve_Last_Time] = '" + Sq.Sql_Time() + "',[Retrieve_Last_IP] = '" + URetIP + "' Where (ID = '" + DT_Transaction.Rows[0][0].ToString().Trim() + "')");
                                                    }
                                                    else
                                                    {
                                                        int RetCount = int.Parse(DT_Transaction.Rows[0][41].ToString().Trim());
                                                        RetCount = RetCount + 1;
                                                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Retrieve_Count] = '" + RetCount + "',[Retrieve_Last_Date] = '" + Sq.Sql_Date() + "',[Retrieve_Last_Time] = '" + Sq.Sql_Time() + "',[Retrieve_Last_IP] = '" + URetIP + "' Where (ID = '" + DT_Transaction.Rows[0][0].ToString().Trim() + "')");
                                                    }
                                                }
                                                catch (Exception) { }
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
                    DT_CreateTransAction = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Insert Into Users_15_API_Transaction OUTPUT Inserted.ID Values ('','" + Err_User_ID + "','" + Err_User_Type + "','" + API_Name + "','" + API_Code + "','" + API_FullName + "','" + Sq.Sql_Date() + "','" + Sq.Sql_Time() + "','" + Pb.GetUserIP_HttpRequest(HttpContext.Current.Request) + "','" + FunctionResult_Mesaage.Code + "','" + ErrFT.ToString() + "','" + FunctionResult_Mesaage.Description + "','3','Failed','0','0','Unknown','0','Unknown','1','0','" + Sq.Sql_Date() + "','" + Sq.Sql_Time() + "','" + Err_User_ID + "','','','0','0','','','0','0','','','0','0','','','','0','0','','','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','" + Error_Message.Trim() + "','4')");
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
            API_Main_Result FunctionResult = new API_Main_Result();
            FunctionResult.Result = Result_API;
            FunctionResult.Message = FunctionResult_Mesaage;
            return FunctionResult;
            //---------------------------------------------------------------------------------------------------------------------------------------------------------
        }
    }
}
