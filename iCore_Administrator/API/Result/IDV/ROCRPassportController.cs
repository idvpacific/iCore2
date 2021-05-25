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

namespace iCore_Administrator.API.Result.IDV
{
    [Route("api/Result/OCR-Passport")]
    public class ROCRPassportController : ApiController
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
            string API_Name = "OCR-Passport";
            string API_Code = "11";
            string API_Code_Validation = "12";
            string API_FullName = "IDV Passport OCR and Validation";
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
                                        User_API_AccessPolicy UAP = new User_API_AccessPolicy();
                                        if ((UAP.User_Access(API_Username, API_Password, API_Code) == true) || (UAP.User_Access(API_Username, API_Password, API_Code_Validation) == true))
                                        {
                                            DataTable DT_Transaction = new DataTable();
                                            DT_Transaction = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Users_15_API_Transaction Where (UID = '" + Transaction_Unic_ID + "') And (User_ID = '" + DT_Authentication.Rows[0][0].ToString().Trim() + "') And (User_Type_Code = '" + DT_Authentication.Rows[0][1].ToString().Trim() + "') And (API_Type_Code = '" + API_Code + "') And (Removed = '0')");
                                            if ((DT_Transaction.Rows != null) && (DT_Transaction.Rows.Count == 1))
                                            {
                                                Err_User_ID = DT_Transaction.Rows[0][2].ToString().Trim();
                                                Err_User_Type = DT_Transaction.Rows[0][3].ToString().Trim();
                                                string Trans_ID = DT_Transaction.Rows[0][0].ToString().Trim();
                                                string Date_Format = DT_Transaction.Rows[0][15].ToString().Trim();
                                                Passport_CLS_OCR_Sync AIDC = new Passport_CLS_OCR_Sync();
                                                Transaction_CLS_Passport Transaction_CLS = new Transaction_CLS_Passport();
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
                                                Transaction_CLS.Status_Code = int.Parse(DT_Transaction.Rows[0][13].ToString().Trim());
                                                Transaction_CLS.Status_Result = DT_Transaction.Rows[0][14].ToString().Trim();
                                                AIDC.Transaction = Transaction_CLS;
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
                                                FunctionResult_Mesaage.Code = 9;
                                                FunctionResult_Mesaage.Error = true;
                                                FunctionResult_Mesaage.Description = "Transaction ID is not valid, Please reviewing your request structure";
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
                    DT_CreateTransAction = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Insert Into Users_15_API_Transaction OUTPUT Inserted.ID Values ('','" + Err_User_ID + "','" + Err_User_Type + "','" + API_Name + "','" + API_Code + "','" + API_FullName + "','" + Sq.Sql_Date() + "','" + Sq.Sql_Time() + "','" + Pb.GetUserIP_HttpRequest(HttpContext.Current.Request) + "','" + FunctionResult_Mesaage.Code + "','" + ErrFT.ToString() + "','" + FunctionResult_Mesaage.Description + "','3','Failed','0','0','Unknown','0','Unknown','1','0','" + Sq.Sql_Date() + "','" + Sq.Sql_Time() + "','" + Err_User_ID + "','','','0','0','','','0','0','','','0','0','','','','0','0','','','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','" + Error_Message.Trim() + "','3')");
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
