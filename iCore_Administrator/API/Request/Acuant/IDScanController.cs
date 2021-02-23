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

namespace iCore_Administrator.API.Request.Acuant
{
    [Route("api/Request/ID-Scan")]
    public class IDScanController : ApiController
    {
        //=========================================================================================================================================
        SQL_Tranceiver Sq = new SQL_Tranceiver();
        PublicFunctions Pb = new PublicFunctions();
        //=========================================================================================================================================
        // Error Code :
        // 1: Exception Error
        // 2: API Username or password missing in header request
        // 3: API username or password not valid
        // 4: User status code in Users_02_SingleUser sql table is deactive
        // 5: User API access failed
        // 6: Front image not uploaded
        // 7: Front image file format not valid
        // 8: Back image file format not valid
        // 9: Capture patameter value not valid
        // 10: Cripping patameter value not valid
        //=========================================================================================================================================
        // Status :
        // 1 : Pending
        // 2 : Review
        // 3 : Failed
        // 4 : Completed
        //=========================================================================================================================================
        public async Task<API_Main_Result> PostAsync()
        {
            //---------------------------------------------------------------------------------------------------------------------------------------------------------
            // Result Structure :
            API_Mesaage FunctionResult_Mesaage = new API_Mesaage();
            object Result_API = null;
            string Trans_ID = "0"; string Trans_UID = "";
            string API_Name = "ID-Scan";
            string API_Code = "1";
            string API_FullName = "Acuant ID Card OCR";
            bool Tran_Add = false;
            //---------------------------------------------------------------------------------------------------------------------------------------------------------
            string Error_Message = "";
            string Err_API_Username = "";
            string Err_API_Password = "";
            string Err_User_ID = "";
            string Err_User_Type = "";
            string Err_Callback_URL = "";
            string Err_Date_Format = "";
            string Err_Capture_Type = "";
            string Err_Cropping_Mode = "";
            //---------------------------------------------------------------------------------------------------------------------------------------------------------
            try
            {
                FunctionResult_Mesaage.Code = 0; FunctionResult_Mesaage.Error = false; FunctionResult_Mesaage.Description = "";
                string API_Username = ""; string API_Password = "";
                var H_Request = Request; var H_Headers = H_Request.Headers;
                if (H_Headers.Contains("API-Username")) { API_Username = H_Headers.GetValues("API-Username").First(); }
                if (H_Headers.Contains("API-Password")) { API_Password = H_Headers.GetValues("API-Password").First(); }
                API_Username = API_Username.Trim(); API_Password = API_Password.Trim();
                Err_API_Username = API_Username;
                Err_API_Password = API_Password;
                if ((API_Username != "") && (API_Password != ""))
                {
                    DataTable DT_Authentication = new DataTable();
                    DT_Authentication = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,User_GroupType_Code,Status_Code From Users_02_SingleUser Where (API_PostRequest_Username = '" + API_Username + "') And (API_PostRequest_Key = '" + API_Password + "') And (Removed = '0')");
                    if (DT_Authentication.Rows.Count == 1)
                    {
                        if (DT_Authentication.Rows[0][2].ToString().Trim() == "1")
                        {
                            User_API_AccessPolicy UAP = new User_API_AccessPolicy();
                            if (UAP.User_Access(API_Username, API_Password, API_Code) == true)
                            {
                                bool Is_Async = false;
                                string User_ID = DT_Authentication.Rows[0][0].ToString().Trim();
                                string User_Type = DT_Authentication.Rows[0][1].ToString().Trim();
                                string Callback_URL = HttpContext.Current.Request.Params["Callback_URL"];
                                string Date_Format = HttpContext.Current.Request.Params["Date_Format"];
                                string Capture_Type = HttpContext.Current.Request.Params["Capture_Type"];
                                string Cropping_Mode = HttpContext.Current.Request.Params["Cropping_Mode"];
                                Err_User_ID = User_ID;
                                Err_User_Type = User_Type;
                                Err_Callback_URL = Callback_URL;
                                Err_Date_Format = Date_Format;
                                Err_Capture_Type = Capture_Type;
                                Err_Cropping_Mode = Cropping_Mode;
                                var FilesReadToProvider = await Request.Content.ReadAsMultipartAsync();
                                byte[] ID_Front_Image = null;
                                string ID_Front_Name = "";
                                string ID_Front_Type = "";
                                byte[] ID_Back_Image = null;
                                string ID_Back_Name = "";
                                string ID_Back_Type = "";
                                foreach (var FileStream in FilesReadToProvider.Contents)
                                {
                                    try
                                    {
                                        var FileKey = FileStream.Headers.ContentDisposition.Name.Trim('"');
                                        if (FileKey.Trim().ToUpper() == "ID_FRONT_IMAGE")
                                        {
                                            ID_Front_Image = await FileStream.ReadAsByteArrayAsync();
                                            ID_Front_Name = FileStream.Headers.ContentDisposition.FileName.Trim('"');
                                            ID_Front_Type = FileStream.Headers.ContentType.MediaType;
                                        }
                                        if (FileKey.Trim().ToUpper() == "ID_BACK_IMAGE")
                                        {
                                            ID_Back_Image = await FileStream.ReadAsByteArrayAsync();
                                            ID_Back_Name = FileStream.Headers.ContentDisposition.FileName.Trim('"');
                                            ID_Back_Type = FileStream.Headers.ContentType.MediaType;
                                        }
                                    }
                                    catch (Exception)
                                    { }
                                }
                                if (ID_Front_Name != "")
                                {
                                    string FIFormat = Pb.GetFile_Type(ID_Front_Name).ToLower().Replace(".", "").Trim();
                                    if ((FIFormat == "jpg") || (FIFormat == "jpeg") || (FIFormat == "png") || (FIFormat == "gif"))
                                    {
                                        bool Backimageformat_Error = false;
                                        if (ID_Back_Name != "")
                                        {
                                            string BIFormat = Pb.GetFile_Type(ID_Back_Name).ToLower().Replace(".", "").Trim();
                                            if ((FIFormat != "jpg") && (FIFormat != "jpeg") && (FIFormat != "png") && (FIFormat != "gif"))
                                            {
                                                Backimageformat_Error = true;
                                            }
                                        }
                                        if (Backimageformat_Error == false)
                                        {
                                            try { Callback_URL = Callback_URL.Trim(); } catch (Exception) { Callback_URL = ""; }
                                            try { Date_Format = Date_Format.Trim(); } catch (Exception) { Date_Format = ""; }
                                            try { Capture_Type = Capture_Type.Trim(); } catch (Exception) { Capture_Type = ""; }
                                            try { Cropping_Mode = Cropping_Mode.Trim(); } catch (Exception) { Cropping_Mode = ""; }
                                            if (Callback_URL != "") { Is_Async = true; }
                                            if (Date_Format == "") { Date_Format = "dd/MM/yyyy"; }
                                            if (Capture_Type == "") { Capture_Type = "0"; }
                                            if (Cropping_Mode == "") { Cropping_Mode = "1"; }
                                            Date_Format = Date_Format.Replace("Y", "y").Replace("m", "M").Replace("D", "d").Trim();
                                            if ((Capture_Type == "0") || (Capture_Type == "1") || (Capture_Type == "2") || (Capture_Type == "3"))
                                            {
                                                if ((Cropping_Mode == "0") || (Cropping_Mode == "1") || (Cropping_Mode == "2") || (Cropping_Mode == "3"))
                                                {
                                                    string Ins_Date = Sq.Sql_Date(); string Ins_Time = Sq.Sql_Time();
                                                    string Client_Request_IP = "";
                                                    string Capture_Type_Text = ""; string Cropping_Mode_Text = "";
                                                    // Get User Request IP :
                                                    Client_Request_IP = Pb.GetUserIP_HttpRequest(HttpContext.Current.Request);
                                                    // Get Capture / Cropping Text :
                                                    switch (Capture_Type)
                                                    {
                                                        case "0": { Capture_Type_Text = "Unknown"; break; }
                                                        case "1": { Capture_Type_Text = "Camera"; break; }
                                                        case "2": { Capture_Type_Text = "Scanner"; break; }
                                                        case "3": { Capture_Type_Text = "Mobile"; break; }
                                                    }
                                                    switch (Cropping_Mode)
                                                    {
                                                        case "0": { Cropping_Mode_Text = "None"; break; }
                                                        case "1": { Cropping_Mode_Text = "Automatic"; break; }
                                                        case "2": { Cropping_Mode_Text = "Interactive"; break; }
                                                        case "3": { Cropping_Mode_Text = "Always"; break; }
                                                    }
                                                    // Create Transaction :
                                                    DataTable DT_CreateTransAction = new DataTable();
                                                    DT_CreateTransAction = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Insert Into Users_15_API_Transaction OUTPUT Inserted.ID Values ('','" + User_ID + "','" + User_Type + "','" + API_Name + "','" + API_Code + "','" + API_FullName + "','" + Ins_Date + "','" + Ins_Time + "','" + Client_Request_IP + "','0','0','','1','Pending','" + Date_Format + "','" + Capture_Type + "','" + Capture_Type_Text + "','" + Cropping_Mode + "','" + Cropping_Mode_Text + "','1','0','" + Ins_Date + "','" + Ins_Time + "','" + User_ID + "','','','0','0','','','0','0','','','0','0','" + Callback_URL + "','','','0','0','','','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','1')");
                                                    Trans_ID = DT_CreateTransAction.Rows[0][0].ToString().Trim();
                                                    Tran_Add = true;
                                                    Trans_UID = Trans_ID + Pb.Make_Security_Code(50);
                                                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [UID] = '" + Trans_UID + "' Where (ID = '" + Trans_ID + "')");
                                                    // Upload Image Files :
                                                    string Path_Forlder_Transaction = HttpContext.Current.Server.MapPath("~/Drive/Users/API/" + Trans_ID);
                                                    string Path_Forlder_UploadImage = HttpContext.Current.Server.MapPath("~/Drive/Users/API/" + Trans_ID + "/Upload");
                                                    try { if (!Directory.Exists(Path_Forlder_Transaction)) { Directory.CreateDirectory(Path_Forlder_Transaction); } } catch (Exception) { }
                                                    try { if (!Directory.Exists(Path_Forlder_UploadImage)) { Directory.CreateDirectory(Path_Forlder_UploadImage); } } catch (Exception) { }
                                                    string Image1_Name = "";
                                                    string Image2_Name = "";
                                                    int Image_Count = 0;
                                                    if (ID_Front_Name != "")
                                                    {
                                                        try
                                                        {
                                                            Image1_Name = Trans_ID + "I1" + Pb.Make_Security_Code(20);
                                                            using (FileStream stream = new FileStream(Path_Forlder_UploadImage + "/" + Image1_Name + "." + Pb.GetFile_Type(ID_Front_Name), FileMode.Create, FileAccess.Write, FileShare.Read))
                                                            {
                                                                stream.Write(ID_Front_Image, 0, ID_Front_Image.Length);
                                                            }
                                                            Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Image_1_Title] = 'Front image',[Image_1_File_Name] = '" + ID_Front_Name + "',[Image_1_File_Format] = '" + Pb.GetFile_Type(ID_Front_Name) + "',[Image_1_File_Type] = '" + ID_Front_Type + "',[Image_1_Download_ID] = '" + Image1_Name + "',[Image_1_Download_Count] = '0' Where (ID = '" + Trans_ID + "')");
                                                            Image_Count++;
                                                        }
                                                        catch (Exception)
                                                        { }
                                                    }
                                                    if (ID_Back_Name != "")
                                                    {
                                                        try
                                                        {
                                                            Image2_Name = Trans_ID + "I2" + Pb.Make_Security_Code(20);
                                                            using (FileStream stream = new FileStream(Path_Forlder_UploadImage + "/" + Image2_Name + "." + Pb.GetFile_Type(ID_Back_Name), FileMode.Create, FileAccess.Write, FileShare.Read))
                                                            {
                                                                stream.Write(ID_Back_Image, 0, ID_Back_Image.Length);
                                                            }
                                                            Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Image_2_Title] = 'Back image',[Image_2_File_Name] = '" + ID_Back_Name + "',[Image_2_File_Format] = '" + Pb.GetFile_Type(ID_Back_Name) + "',[Image_2_File_Type] = '" + ID_Back_Type + "',[Image_2_Download_ID] = '" + Image2_Name + "',[Image_2_Download_Count] = '0' Where (ID = '" + Trans_ID + "')");
                                                            Image_Count++;
                                                        }
                                                        catch (Exception)
                                                        { }
                                                    }
                                                    // Call Acuant Function :
                                                    if (Is_Async == true)
                                                    {
                                                        Acuant_CLS_Async AIDC = new Acuant_CLS_Async();
                                                        Transaction_CLS Transaction_CLS = new Transaction_CLS();
                                                        Transaction_CLS.Name = API_Name;
                                                        Transaction_CLS.ID = Trans_UID;
                                                        Transaction_CLS.Username = API_Username;
                                                        Transaction_CLS.Date_Format = Date_Format;
                                                        try { Transaction_CLS.Request_Date = Pb.ConvertDate_Format(Ins_Date, "yyyy-MM-dd", Date_Format); } catch (Exception) { Transaction_CLS.Request_Date = Ins_Date; }
                                                        Transaction_CLS.Request_Time = Ins_Time;
                                                        Transaction_CLS.Request_IP = Client_Request_IP;
                                                        Transaction_CLS.Attached_File = Image_Count;
                                                        Transaction_CLS.Async = Is_Async;
                                                        Transaction_CLS.Callback_URL = Callback_URL;
                                                        Transaction_CLS.Status_Code = 1;
                                                        Transaction_CLS.Status_Result = "Pending";
                                                        AIDC.Transaction = Transaction_CLS;
                                                        Result_API = AIDC;
                                                        _ = Task.Run(() =>
                                                           {
                                                               Acuant_ModuleFunction AMF = new Acuant_ModuleFunction();
                                                               AMF.GetData(Trans_ID);
                                                               CallBackFunction CBU = new CallBackFunction();
                                                               CBU.Send_CallBack(Trans_ID, API_Username, Image_Count.ToString(), Transaction_CLS.Request_Date, Ins_Time, Date_Format);
                                                               try
                                                               {
                                                                   DataTable DT_STS = new DataTable();
                                                                   DT_STS = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Status_Code From Users_15_API_Transaction Where (ID = '" + Trans_ID + "') And (Removed = '0')");
                                                                   if (DT_STS.Rows[0][0].ToString().Trim() == "1")
                                                                   {
                                                                       Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '4',[Status_Text] = 'Completed' Where (ID = '" + Trans_ID + "') And (Removed = '0')");
                                                                   }
                                                               }
                                                               catch (Exception) { }
                                                           });
                                                    }
                                                    else
                                                    {
                                                        Acuant_CLS_OCR_Sync AIDC = new Acuant_CLS_OCR_Sync();
                                                        Transaction_CLS Transaction_CLS = new Transaction_CLS();
                                                        Transaction_CLS.Name = API_Name;
                                                        Transaction_CLS.ID = Trans_UID;
                                                        Transaction_CLS.Username = API_Username;
                                                        Transaction_CLS.Date_Format = Date_Format;
                                                        try { Transaction_CLS.Request_Date = Pb.ConvertDate_Format(Ins_Date, "yyyy-MM-dd", Date_Format); } catch (Exception) { Transaction_CLS.Request_Date = Ins_Date; }
                                                        Transaction_CLS.Request_Time = Ins_Time;
                                                        Transaction_CLS.Request_IP = Client_Request_IP;
                                                        Transaction_CLS.Attached_File = Image_Count;
                                                        Transaction_CLS.Async = Is_Async;
                                                        Transaction_CLS.Callback_URL = Callback_URL;
                                                        Transaction_CLS.Status_Code = 1;
                                                        Transaction_CLS.Status_Result = "Pending";
                                                        AIDC.Transaction = Transaction_CLS;
                                                        Acuant_ModuleFunction AMF = new Acuant_ModuleFunction();
                                                        AMF.GetData(Trans_ID);
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
                                                        catch (Exception)
                                                        { }
                                                        try
                                                        {
                                                            DataTable DT_STS = new DataTable();
                                                            DT_STS = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Status_Code From Users_15_API_Transaction Where (ID = '" + Trans_ID + "') And (Removed = '0')");
                                                            if (DT_STS.Rows[0][0].ToString().Trim() == "1")
                                                            {
                                                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '4',[Status_Text] = 'Completed' Where (ID = '" + Trans_ID + "') And (Removed = '0')");
                                                                AIDC.Transaction.Status_Code = 4;
                                                                AIDC.Transaction.Status_Result = "Completed";
                                                            }
                                                        }
                                                        catch (Exception) { }
                                                        Result_API = AIDC;
                                                    }
                                                }
                                                else
                                                {
                                                    FunctionResult_Mesaage.Code = 10;
                                                    FunctionResult_Mesaage.Error = true;
                                                    FunctionResult_Mesaage.Description = "Cropping mode not valid, Please reviewing your request structure";
                                                }
                                            }
                                            else
                                            {
                                                FunctionResult_Mesaage.Code = 9;
                                                FunctionResult_Mesaage.Error = true;
                                                FunctionResult_Mesaage.Description = "Capture type not valid, Please reviewing your request structure";
                                            }
                                        }
                                        else
                                        {
                                            FunctionResult_Mesaage.Code = 8;
                                            FunctionResult_Mesaage.Error = true;
                                            FunctionResult_Mesaage.Description = "ID back image file format not valid, Authorized formats : JPG ,JPEG ,PNG, GIF";
                                        }
                                    }
                                    else
                                    {
                                        FunctionResult_Mesaage.Code = 7;
                                        FunctionResult_Mesaage.Error = true;
                                        FunctionResult_Mesaage.Description = "ID front image file format not valid, Authorized formats : JPG ,JPEG ,PNG, GIF";
                                    }
                                }
                                else
                                {
                                    FunctionResult_Mesaage.Code = 6;
                                    FunctionResult_Mesaage.Error = true;
                                    FunctionResult_Mesaage.Description = "ID front image not founded, Please reviewing your request structure";
                                }
                            }
                            else
                            {
                                FunctionResult_Mesaage.Code = 5;
                                FunctionResult_Mesaage.Error = true;
                                FunctionResult_Mesaage.Description = "You do not have permission to use this service, Please contact the IDV support team";
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
            catch (Exception e)
            {
                FunctionResult_Mesaage.Code = 1;
                FunctionResult_Mesaage.Error = true;
                FunctionResult_Mesaage.Description = "IDV server returns an unknown error, Please contact the IDV support team if it is not resolved after reviewing your request";
                if (Tran_Add == true)
                {
                    try
                    {
                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '2',[Status_Text] = 'Review',[Error_Message] = '" + e.Message.Trim().Replace(",", "").Replace(";", "").Replace("'", "") + "' Where (ID = '" + Trans_ID + "') And (Removed = '0')");
                        Error_Message = "Exception : " + e.Message.Trim().Replace(",", "").Replace(";", "").Replace("'", "");
                    }
                    catch (Exception) { }
                }
            }
            //---------------------------------------------------------------------------------------------------------------------------------------------------------
            // Log bad request :
            try
            {
                if (FunctionResult_Mesaage.Error == true)
                {
                    if (Tran_Add == true)
                    {
                        FunctionResult_Mesaage.Transaction_ID = Trans_ID;
                        Error_Message += "$" + "Request API Username : " + Err_API_Username;
                        Error_Message += "$" + "Request API Password : " + Err_API_Password;
                        Error_Message += "$" + "Request User ID : " + Err_User_ID;
                        Error_Message += "$" + "Request User IP : " + Err_User_Type;
                        Error_Message += "$" + "Request Callback URL : " + Err_Callback_URL;
                        Error_Message += "$" + "Request Date Format : " + Err_Date_Format;
                        Error_Message += "$" + "Request Capture Type : " + Err_Capture_Type;
                        Error_Message += "$" + "Request Cropping Mode : " + Err_Cropping_Mode;
                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Insert_API_Code] = '2',[Error_Message] = '" + Error_Message.Trim() + "',[Message_Code] = '" + FunctionResult_Mesaage.Code.ToString() + "',[Message_Error] = '" + FunctionResult_Mesaage.Error.ToString() + "',[Message_Description] = '" + FunctionResult_Mesaage.Description + "' Where (ID = '" + Trans_ID + "') And (Removed = '0')");
                    }
                    else
                    {
                        DataTable DT_CreateTransAction = new DataTable();
                        int ErrFT = 0;
                        if (FunctionResult_Mesaage.Error == true) { ErrFT = 1; }
                        Error_Message += "$" + "Request API Username : " + Err_API_Username;
                        Error_Message += "$" + "Request API Password : " + Err_API_Password;
                        Error_Message += "$" + "Request User ID : " + Err_User_ID;
                        Error_Message += "$" + "Request User IP : " + Err_User_Type;
                        Error_Message += "$" + "Request Callback URL : " + Err_Callback_URL;
                        Error_Message += "$" + "Request Date Format : " + Err_Date_Format;
                        Error_Message += "$" + "Request Capture Type : " + Err_Capture_Type;
                        Error_Message += "$" + "Request Cropping Mode : " + Err_Cropping_Mode;
                        DT_CreateTransAction = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Insert Into Users_15_API_Transaction OUTPUT Inserted.ID Values ('','" + Err_User_ID + "','" + Err_User_Type + "','" + API_Name + "','" + API_Code + "','" + API_FullName + "','" + Sq.Sql_Date() + "','" + Sq.Sql_Time() + "','" + Pb.GetUserIP_HttpRequest(HttpContext.Current.Request) + "','" + FunctionResult_Mesaage.Code + "','" + ErrFT.ToString() + "','" + FunctionResult_Mesaage.Description + "','3','Failed','" + Err_Date_Format + "','" + Err_Capture_Type + "','Unknown','" + Err_Cropping_Mode + "','Unknown','1','0','" + Sq.Sql_Date() + "','" + Sq.Sql_Time() + "','" + Err_User_ID + "','','','0','0','','','0','0','','','0','0','" + Err_Callback_URL + "','','','0','0','','','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','" + Error_Message.Trim() + "','2')");
                        string Er_Trans_ID = DT_CreateTransAction.Rows[0][0].ToString().Trim();
                        string Er_Trans_UID = Er_Trans_ID + Pb.Make_Security_Code(50);
                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [UID] = '" + Er_Trans_UID + "' Where (ID = '" + Er_Trans_ID + "')");
                        FunctionResult_Mesaage.Transaction_ID = Er_Trans_UID;
                    }
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
