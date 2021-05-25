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

namespace iCore_Administrator.API.Request.IDV
{
    [Route("api/Request/OCR-Passport")]
    public class OCRPassportController : ApiController
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
        // 5: User API access failed - OCR
        // 6: User API access failed - Validation
        // 7: Passport image not uploaded
        // 8: Front image file format not valid
        // 9: Processing type not valid, Authorized code : 1, 2, 3, 4, 5, 6
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
            string API_Name = "OCR-Passport";
            string API_Code = "11";
            string API_Code_Validation = "12";
            string API_FullName = "IDV Passport OCR and Validation";
            bool Tran_Add = false;
            string Req_Processing_Type = "1";
            string Req_Document_Validation = "False";
            string Req_Overlay_Required = "False";
            string Req_Detect_Orientation = "False";
            string Req_Image_Scale = "False";
            string Req_Cropping_Mode = "False";
            //---------------------------------------------------------------------------------------------------------------------------------------------------------
            string Error_Message = "";
            string Err_API_Username = "";
            string Err_API_Password = "";
            string Err_User_ID = "";
            string Err_User_Type = "";
            string Err_Callback_URL = "";
            string Err_Date_Format = "";
            string Err_Processing_Type = "Unknown";
            string Err_Document_Validation = "Unknown";
            string Err_Overlay_Required = "Unknown";
            string Err_Detect_Orientation = "Unknown";
            string Err_Image_Scale = "Unknown";
            string Err_Cropping_Mode = "Unknown";
            //---------------------------------------------------------------------------------------------------------------------------------------------------------
            try
            {
                bool RequestValidation = false; bool RequestValidationGoNext = true;
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
                    // Test Validation request :
                    try
                    {
                        Req_Document_Validation = HttpContext.Current.Request.Params["Document_Validation"];
                        if ((Req_Document_Validation.Trim().ToUpper() == "TRUE") || (Req_Document_Validation.Trim().ToUpper() == "1")) { RequestValidation = true; Req_Document_Validation = "True"; Err_Document_Validation = "True"; }
                    }
                    catch (Exception) { RequestValidation = false; Req_Document_Validation = "False"; Err_Document_Validation = "False"; }
                    DataTable DT_Authentication = new DataTable();
                    DT_Authentication = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,User_GroupType_Code,Status_Code From Users_02_SingleUser Where (API_PostRequest_Username = '" + API_Username + "') And (API_PostRequest_Key = '" + API_Password + "') And (Removed = '0')");
                    if (DT_Authentication.Rows.Count == 1)
                    {
                        if (DT_Authentication.Rows[0][2].ToString().Trim() == "1")
                        {
                            User_API_AccessPolicy UAP = new User_API_AccessPolicy();
                            if (UAP.User_Access(API_Username, API_Password, API_Code) == true)
                            {
                                RequestValidationGoNext = true;
                                if (RequestValidation == true)
                                {
                                    if (UAP.User_Access(API_Username, API_Password, API_Code_Validation) == false)
                                    {
                                        RequestValidationGoNext = false;
                                    }
                                }
                                if (RequestValidationGoNext == true)
                                {
                                    bool Is_Async = false;
                                    string User_ID = DT_Authentication.Rows[0][0].ToString().Trim();
                                    string User_Type = DT_Authentication.Rows[0][1].ToString().Trim();
                                    string Callback_URL = HttpContext.Current.Request.Params["Callback_URL"];
                                    string Date_Format = HttpContext.Current.Request.Params["Date_Format"];
                                    Req_Processing_Type = HttpContext.Current.Request.Params["Processing_Type"];
                                    Req_Overlay_Required = HttpContext.Current.Request.Params["Overlay_Required"];
                                    Req_Detect_Orientation = HttpContext.Current.Request.Params["Detect_Orientation"];
                                    Req_Image_Scale = HttpContext.Current.Request.Params["Image_Scale"];
                                    Req_Cropping_Mode = HttpContext.Current.Request.Params["Cropping_Mode"];
                                    Err_User_ID = User_ID;
                                    Err_User_Type = User_Type;
                                    Err_Callback_URL = Callback_URL;
                                    Err_Date_Format = Date_Format;
                                    Err_Processing_Type = Req_Processing_Type;
                                    Err_Overlay_Required = Req_Overlay_Required;
                                    Err_Detect_Orientation = Req_Detect_Orientation;
                                    Err_Image_Scale = Req_Image_Scale;
                                    Err_Cropping_Mode = Req_Cropping_Mode;
                                    var FilesReadToProvider = await Request.Content.ReadAsMultipartAsync();
                                    byte[] ID_Front_Image = null;
                                    string ID_Front_Name = "";
                                    string ID_Front_Type = "";
                                    foreach (var FileStream in FilesReadToProvider.Contents)
                                    {
                                        try
                                        {
                                            var FileKey = FileStream.Headers.ContentDisposition.Name.Trim('"');
                                            if (FileKey.Trim().ToUpper() == "PASSPORT_IMAGE")
                                            {
                                                ID_Front_Image = await FileStream.ReadAsByteArrayAsync();
                                                ID_Front_Name = FileStream.Headers.ContentDisposition.FileName.Trim('"');
                                                ID_Front_Type = FileStream.Headers.ContentType.MediaType;
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
                                            bool Processing_Type_Error = false;
                                            try { Callback_URL = Callback_URL.Trim(); } catch (Exception) { Callback_URL = ""; }
                                            try { Date_Format = Date_Format.Trim(); } catch (Exception) { Date_Format = ""; }
                                            try { Req_Processing_Type = Req_Processing_Type.Trim(); } catch (Exception) { Req_Processing_Type = ""; }
                                            try { Req_Overlay_Required = Req_Overlay_Required.Trim(); } catch (Exception) { Req_Overlay_Required = ""; }
                                            try { Req_Detect_Orientation = Req_Detect_Orientation.Trim(); } catch (Exception) { Req_Detect_Orientation = ""; }
                                            try { Req_Image_Scale = Req_Image_Scale.Trim(); } catch (Exception) { Req_Image_Scale = ""; }
                                            try { Req_Cropping_Mode = Req_Cropping_Mode.Trim(); } catch (Exception) { Req_Cropping_Mode = ""; }
                                            if (Callback_URL != "") { Is_Async = true; }
                                            if (Date_Format == "") { Date_Format = "dd/MM/yyyy"; }
                                            if (Req_Processing_Type == "") { Req_Processing_Type = "1"; }
                                            if ((Req_Processing_Type != "1") && (Req_Processing_Type != "2") && (Req_Processing_Type != "3") && (Req_Processing_Type != "4") && (Req_Processing_Type != "5") && (Req_Processing_Type != "6")) { Processing_Type_Error = true; }
                                            if (Req_Document_Validation == "") { Req_Document_Validation = "0"; }
                                            if (Req_Document_Validation.ToLower() == "true") { Req_Document_Validation = "1"; } else { Req_Document_Validation = "0"; }
                                            if (Req_Overlay_Required == "") { Req_Overlay_Required = "0"; }
                                            if (Req_Overlay_Required.ToLower() == "true") { Req_Overlay_Required = "1"; } else { Req_Overlay_Required = "0"; }
                                            if (Req_Detect_Orientation == "") { Req_Detect_Orientation = "0"; }
                                            if (Req_Detect_Orientation.ToLower() == "true") { Req_Detect_Orientation = "1"; } else { Req_Detect_Orientation = "0"; }
                                            if (Req_Image_Scale == "") { Req_Image_Scale = "0"; }
                                            if (Req_Image_Scale.ToLower() == "true") { Req_Image_Scale = "1"; } else { Req_Image_Scale = "0"; }
                                            if (Req_Cropping_Mode == "") { Req_Cropping_Mode = "0"; }
                                            if (Req_Cropping_Mode.ToLower() == "true") { Req_Cropping_Mode = "1"; } else { Req_Cropping_Mode = "0"; }
                                            if (Processing_Type_Error == false)
                                            {
                                                Date_Format = Date_Format.Replace("Y", "y").Replace("m", "M").Replace("D", "d").Trim();
                                                string Ins_Date = Sq.Sql_Date(); string Ins_Time = Sq.Sql_Time();
                                                // Get User Request IP :
                                                string Client_Request_IP = "";
                                                Client_Request_IP = Pb.GetUserIP_HttpRequest(HttpContext.Current.Request);
                                                // Create Transaction :
                                                DataTable DT_CreateTransAction = new DataTable();
                                                DT_CreateTransAction = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Insert Into Users_15_API_Transaction OUTPUT Inserted.ID Values ('','" + User_ID + "','" + User_Type + "','" + API_Name + "','" + API_Code + "','" + API_FullName + "','" + Ins_Date + "','" + Ins_Time + "','" + Client_Request_IP + "','0','0','','1','Pending','" + Date_Format + "','0','Undefine','0','Undefine','1','0','" + Ins_Date + "','" + Ins_Time + "','" + User_ID + "','','','0','0','','','0','0','','','0','0','" + Callback_URL + "','','','0','0','','','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','1')");
                                                Trans_ID = DT_CreateTransAction.Rows[0][0].ToString().Trim();
                                                Tran_Add = true;
                                                Trans_UID = Trans_ID + Pb.Make_Security_Code(50);
                                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [UID] = '" + Trans_UID + "' Where (ID = '" + Trans_ID + "')");
                                                // Transaction request input argument :
                                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Delete From Users_15_API_Transaction_RequestArgument Where (Transaction_ID = '" + Trans_ID + "')");
                                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert into Users_15_API_Transaction_RequestArgument Values ('" + Trans_ID + "','" + Trans_UID + "','" + Callback_URL + "','" + Date_Format + "','" + Req_Processing_Type + "','" + Req_Document_Validation + "','" + Req_Overlay_Required + "','" + Req_Detect_Orientation + "','" + Req_Image_Scale + "','" + Req_Cropping_Mode + "','','','','','','','','','','','','','','','','','','','','')");
                                                // Upload Image Files :
                                                string Path_Forlder_Transaction = HttpContext.Current.Server.MapPath("~/Drive/Users/API/" + Trans_ID);
                                                string Path_Forlder_UploadImage = HttpContext.Current.Server.MapPath("~/Drive/Users/API/" + Trans_ID + "/Upload");
                                                try { if (!Directory.Exists(Path_Forlder_Transaction)) { Directory.CreateDirectory(Path_Forlder_Transaction); } } catch (Exception) { }
                                                try { if (!Directory.Exists(Path_Forlder_UploadImage)) { Directory.CreateDirectory(Path_Forlder_UploadImage); } } catch (Exception) { }
                                                string Image1_Name = "";
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
                                                        IDV_Passport AMF = new IDV_Passport();
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
                                                    Passport_CLS_OCR_Sync AIDC = new Passport_CLS_OCR_Sync();
                                                    Transaction_CLS_Passport Transaction_CLS = new Transaction_CLS_Passport();
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
                                                    Transaction_CLS.Processing_Type = "1";
                                                    Transaction_CLS.Document_Validation = "False";
                                                    Transaction_CLS.Overlay_Required = "False";
                                                    Transaction_CLS.Detect_Orientation = "False";
                                                    Transaction_CLS.Image_Scale = "False";
                                                    Transaction_CLS.Cropping_Mode = "False";
                                                    try
                                                    {
                                                        DataTable DT_Get_Pass_Argument = new DataTable();
                                                        DT_Get_Pass_Argument = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Processing_Type,Document_Validation,Overlay_Required,Detect_Orientation,Image_Scale,Cropping_Mode From Users_15_API_Transaction_RequestArgument Where (Transaction_ID = '" + Trans_ID + "')");
                                                        if (DT_Get_Pass_Argument.Rows != null)
                                                        {
                                                            if (DT_Get_Pass_Argument.Rows.Count == 1)
                                                            {
                                                                Transaction_CLS.Processing_Type = DT_Get_Pass_Argument.Rows[0][0].ToString().Trim();
                                                                if (DT_Get_Pass_Argument.Rows[0][1].ToString().Trim() == "1") { Transaction_CLS.Document_Validation = "True"; }
                                                                if (DT_Get_Pass_Argument.Rows[0][2].ToString().Trim() == "1") { Transaction_CLS.Overlay_Required = "True"; }
                                                                if (DT_Get_Pass_Argument.Rows[0][3].ToString().Trim() == "1") { Transaction_CLS.Detect_Orientation = "True"; }
                                                                if (DT_Get_Pass_Argument.Rows[0][4].ToString().Trim() == "1") { Transaction_CLS.Image_Scale = "True"; }
                                                                if (DT_Get_Pass_Argument.Rows[0][5].ToString().Trim() == "1") { Transaction_CLS.Cropping_Mode = "True"; }
                                                            }
                                                        }
                                                    }
                                                    catch (Exception) { }
                                                    Transaction_CLS.Status_Code = 1;
                                                    Transaction_CLS.Status_Result = "Pending";
                                                    AIDC.Transaction = Transaction_CLS;
                                                    IDV_Passport AMF = new IDV_Passport();
                                                    AMF.GetData(Trans_ID);
                                                    var Json_Data = new JObject();
                                                    // MRZ :
                                                    try
                                                    {
                                                        int BMRZ1 = 0; int BMRZ2 = 0;
                                                        var Json_MRZLines = new JObject();
                                                        var Json_MRZClassification = new JObject();
                                                        try
                                                        {
                                                            DataTable DT_Get_MRZ_Lines = new DataTable();
                                                            DT_Get_MRZ_Lines = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Users_19_API_Passport_MRZ Where (Transaction_ID = '" + Trans_ID + "')");
                                                            if (DT_Get_MRZ_Lines.Rows != null)
                                                            {
                                                                if (DT_Get_MRZ_Lines.Rows.Count == 1)
                                                                {
                                                                    try
                                                                    {
                                                                        Json_MRZLines.Add("Line_1", DT_Get_MRZ_Lines.Rows[0][1].ToString().Trim());
                                                                        Json_MRZLines.Add("Line_2", DT_Get_MRZ_Lines.Rows[0][2].ToString().Trim());
                                                                        Json_MRZLines.Add("Message", DT_Get_MRZ_Lines.Rows[0][3].ToString().Trim());
                                                                        BMRZ1 = 1;
                                                                    }
                                                                    catch (Exception) { }
                                                                }
                                                            }
                                                        }
                                                        catch (Exception) { }
                                                        try
                                                        {
                                                            DataTable DT_Get_MRZ_Classification = new DataTable();
                                                            DT_Get_MRZ_Classification = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Users_20_API_Passport_MRZ_Decoded Where (Transaction_ID = '" + Trans_ID + "') Order By Field_ID");
                                                            if (DT_Get_MRZ_Classification.Rows != null)
                                                            {
                                                                if (DT_Get_MRZ_Classification.Rows.Count != 0)
                                                                {
                                                                    foreach (DataRow RW in DT_Get_MRZ_Classification.Rows)
                                                                    {
                                                                        try
                                                                        {
                                                                            if ((RW[1].ToString().Trim() == "10") || (RW[1].ToString().Trim() == "13"))
                                                                            {
                                                                                Json_MRZClassification.Add(RW[2].ToString().Trim().Replace(" ", "_"), Pb.ConvertDate_Format(RW[3].ToString().Trim(), "yyMMdd", Date_Format));
                                                                            }
                                                                            else
                                                                            {
                                                                                Json_MRZClassification.Add(RW[2].ToString().Trim().Replace(" ", "_"), RW[3].ToString().Trim());
                                                                            }
                                                                        }
                                                                        catch (Exception) { }
                                                                        BMRZ2 = 1;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        catch (Exception) { }
                                                        if ((BMRZ1 == 1) || (BMRZ2 == 1))
                                                        {
                                                            var Json_MRZ = new JObject();
                                                            if (BMRZ1 == 1) { Json_MRZ.Add("Lines", Json_MRZLines); }
                                                            if (BMRZ2 == 1) { Json_MRZ.Add("Classification", Json_MRZClassification); }
                                                            Json_Data.Add("MRZ", Json_MRZ);
                                                        }
                                                    }
                                                    catch (Exception) { }
                                                    // Passport :
                                                    try
                                                    {
                                                        int BPass1 = 0; int BPass2 = 0; int BPass3 = 0;
                                                        string Json_Pass_Lines = "";
                                                        string Json_Pass_ParsedText = "";
                                                        var Json_Pass_Classification = new JObject();
                                                        try
                                                        {
                                                            DataTable DT_Get_Passport_LinesText = new DataTable();
                                                            DT_Get_Passport_LinesText = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Users_21_API_Passport_OCR Where (Transaction_ID = '" + Trans_ID + "')");
                                                            if (DT_Get_Passport_LinesText.Rows != null)
                                                            {
                                                                if (DT_Get_Passport_LinesText.Rows.Count == 1)
                                                                {
                                                                    try
                                                                    {
                                                                        if (DT_Get_Passport_LinesText.Rows[0][1].ToString().Trim() != "")
                                                                        {
                                                                            string FullOCRResult = "";
                                                                            FullOCRResult = DT_Get_Passport_LinesText.Rows[0][1].ToString().Trim();
                                                                            FullOCRResult = FullOCRResult.Replace('#', ',');
                                                                            FullOCRResult = FullOCRResult.Replace('$', '\'');
                                                                            FullOCRResult = FullOCRResult.Replace('%', '\"');
                                                                            FullOCRResult = FullOCRResult.Trim();
                                                                            OCR_Result OCR_Orientation_Res = JsonConvert.DeserializeObject<OCR_Result>(FullOCRResult);
                                                                            Json_Pass_Lines = JsonConvert.SerializeObject(OCR_Orientation_Res.ParsedResults[0].TextOverlay);
                                                                            BPass1 = 1;
                                                                        }
                                                                    }
                                                                    catch (Exception) { BPass1 = 0; }
                                                                    try
                                                                    {
                                                                        if (DT_Get_Passport_LinesText.Rows[0][2].ToString().Trim() != "")
                                                                        {
                                                                            Json_Pass_ParsedText = DT_Get_Passport_LinesText.Rows[0][2].ToString().Trim();
                                                                            Json_Pass_ParsedText = Json_Pass_ParsedText.Replace('#', ',');
                                                                            Json_Pass_ParsedText = Json_Pass_ParsedText.Replace('$', '\'');
                                                                            Json_Pass_ParsedText = Json_Pass_ParsedText.Replace('%', '\"');
                                                                            Json_Pass_ParsedText = Json_Pass_ParsedText.Trim();
                                                                            BPass2 = 1;
                                                                        }
                                                                    }
                                                                    catch (Exception) { BPass2 = 0; }
                                                                }
                                                            }
                                                        }
                                                        catch (Exception) { }
                                                        try
                                                        {
                                                            DataTable DT_Get_MRZ_Classification = new DataTable();
                                                            DT_Get_MRZ_Classification = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Users_22_API_Passport_OCR_Decoded Where (Transaction_ID = '" + Trans_ID + "') Order By Field_ID");
                                                            if (DT_Get_MRZ_Classification.Rows != null)
                                                            {
                                                                if (DT_Get_MRZ_Classification.Rows.Count != 0)
                                                                {
                                                                    foreach (DataRow RW in DT_Get_MRZ_Classification.Rows)
                                                                    {
                                                                        try { Json_Pass_Classification.Add(RW[2].ToString().Trim().Replace(" ", "_"), RW[3].ToString().Trim()); } catch (Exception) { }
                                                                        BPass2 = 1;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        catch (Exception) { }
                                                        if ((BPass1 == 1) || (BPass2 == 1) || (BPass3 == 1))
                                                        {
                                                            var Json_Passport = new JObject();
                                                            if (BPass1 == 1) { Json_Passport.Add("Lines", Json_Pass_Lines); }
                                                            if (BPass2 == 1) { Json_Passport.Add("ParsedText", Json_Pass_ParsedText); }
                                                            if (BPass3 == 1) { Json_Passport.Add("Classification", Json_Pass_Classification); }
                                                            Json_Data.Add("Passport", Json_Passport);
                                                        }
                                                    }
                                                    catch (Exception) { }
                                                    // Validation :
                                                    try
                                                    {
                                                        int BValid1 = 0; int BValid2 = 0;
                                                        string Validation_Code = "0";
                                                        string Validation_Status = "Unknown";
                                                        var Json_Validation_Details = new JObject();
                                                        try
                                                        {
                                                            DataTable DT_Get_Validation_Result = new DataTable();
                                                            DT_Get_Validation_Result = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Users_24_API_Passport_Validation_Result Where (Transaction_ID = '" + Trans_ID + "')");
                                                            if (DT_Get_Validation_Result.Rows != null)
                                                            {
                                                                if (DT_Get_Validation_Result.Rows.Count == 1)
                                                                {
                                                                    Validation_Code = DT_Get_Validation_Result.Rows[0][1].ToString().Trim();
                                                                    Validation_Status = DT_Get_Validation_Result.Rows[0][2].ToString().Trim();
                                                                    BValid1 = 1;
                                                                }
                                                            }
                                                        }
                                                        catch (Exception) { }
                                                        try
                                                        {
                                                            DataTable DT_Get_ValidationDetails = new DataTable();
                                                            DT_Get_ValidationDetails = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Users_23_API_Passport_Validation Where (Transaction_ID = '" + Trans_ID + "') Order By Field_ID");
                                                            if (DT_Get_ValidationDetails.Rows != null)
                                                            {
                                                                if (DT_Get_ValidationDetails.Rows.Count != 0)
                                                                {
                                                                    foreach (DataRow RW in DT_Get_ValidationDetails.Rows)
                                                                    {
                                                                        try { Json_Validation_Details.Add(RW[2].ToString().Trim().Replace(" ", "_"), RW[3].ToString().Trim()); } catch (Exception) { }
                                                                        BValid2 = 1;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        catch (Exception) { }
                                                        if ((BValid1 == 1) || (BValid2 == 1))
                                                        {
                                                            var Json_Passport = new JObject();
                                                            if (BValid1 == 1) { Json_Passport.Add("Code", Validation_Code); Json_Passport.Add("Status", Validation_Status); }
                                                            if (BValid2 == 1) { Json_Passport.Add("Details", Json_Validation_Details); }
                                                            Json_Data.Add("Validation", Json_Passport);
                                                        }
                                                    }
                                                    catch (Exception) { }
                                                    // Setup Result :
                                                    AIDC.Data = Json_Data;
                                                    // Image Files :
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
                                                FunctionResult_Mesaage.Code = 9;
                                                FunctionResult_Mesaage.Error = true;
                                                FunctionResult_Mesaage.Description = "Processing type not valid, Authorized code : 1, 2, 3, 4, 5, 6";
                                            }
                                        }
                                        else
                                        {
                                            FunctionResult_Mesaage.Code = 8;
                                            FunctionResult_Mesaage.Error = true;
                                            FunctionResult_Mesaage.Description = "Passport image file format not valid, Authorized formats : JPG ,JPEG ,PNG, GIF";
                                        }
                                    }
                                    else
                                    {
                                        FunctionResult_Mesaage.Code = 7;
                                        FunctionResult_Mesaage.Error = true;
                                        FunctionResult_Mesaage.Description = "Passport image not founded, Please reviewing your request structure";
                                    }
                                }
                                else
                                {
                                    FunctionResult_Mesaage.Code = 6;
                                    FunctionResult_Mesaage.Error = true;
                                    FunctionResult_Mesaage.Description = "You do not have permission to use this service, Please contact the IDV support team";
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
                        Error_Message += "$" + "Request Processing Type : " + Err_Processing_Type;
                        Error_Message += "$" + "Request Document Validation : " + Err_Document_Validation;
                        Error_Message += "$" + "Request Overlay Required : " + Err_Overlay_Required;
                        Error_Message += "$" + "Request Detect Orientation : " + Err_Detect_Orientation;
                        Error_Message += "$" + "Request Image Scale : " + Err_Image_Scale;
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
                        Error_Message += "$" + "Request Callback URL : " + Err_Callback_URL;
                        Error_Message += "$" + "Request Date Format : " + Err_Date_Format;
                        Error_Message += "$" + "Request Processing Type : " + Err_Processing_Type;
                        Error_Message += "$" + "Request Document Validation : " + Err_Document_Validation;
                        Error_Message += "$" + "Request Overlay Required : " + Err_Overlay_Required;
                        Error_Message += "$" + "Request Detect Orientation : " + Err_Detect_Orientation;
                        Error_Message += "$" + "Request Image Scale : " + Err_Image_Scale;
                        Error_Message += "$" + "Request Cropping Mode : " + Err_Cropping_Mode;
                        DT_CreateTransAction = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Insert Into Users_15_API_Transaction OUTPUT Inserted.ID Values ('','" + Err_User_ID + "','" + Err_User_Type + "','" + API_Name + "','" + API_Code + "','" + API_FullName + "','" + Sq.Sql_Date() + "','" + Sq.Sql_Time() + "','" + Pb.GetUserIP_HttpRequest(HttpContext.Current.Request) + "','" + FunctionResult_Mesaage.Code + "','" + ErrFT.ToString() + "','" + FunctionResult_Mesaage.Description + "','3','Failed','" + Err_Date_Format + "','0','Unknown','0','Unknown','1','0','" + Sq.Sql_Date() + "','" + Sq.Sql_Time() + "','" + Err_User_ID + "','','','0','0','','','0','0','','','0','0','" + Err_Callback_URL + "','','','0','0','','','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','" + Error_Message.Trim() + "','2')");
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
