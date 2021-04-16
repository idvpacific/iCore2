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
    [Route("api/Result/RetrieveApplication")]
    public class RetrieveApplicationController : ApiController
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
            string API_Name = "RetrieveApplication";
            string API_Code = "8";
            string API_FullName = "Retrieve Application public function";
            //---------------------------------------------------------------------------------------------------------------------------------------------------------
            string Er_API_Username = "";
            string Er_API_Password = "";
            string Er_TrakingCode = "";
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
                            string TrakingCode = "";
                            string User_API_Post_Username = "";
                            try { TrakingCode = HttpContext.Current.Request.Params["TrakingCode"].Trim(); } catch (Exception) { }
                            try { User_API_Post_Username = HttpContext.Current.Request.Params["Username"].Trim(); } catch (Exception) { }
                            Er_TrakingCode = TrakingCode;
                            Er_User_API_Post_Username = User_API_Post_Username;
                            FunctionResult_Mesaage.Transaction_ID = Er_TrakingCode;
                            if (TrakingCode != "")
                            {
                                if (User_API_Post_Username != "")
                                {
                                    if (User_API_Post_Username.Trim() == DT_Authentication.Rows[0][3].ToString().Trim())
                                    {
                                        DataTable DT_Application = new DataTable();
                                        DT_Application = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Users_08_Hospitality_SingleUser_Application Where (TrakingCode = '" + TrakingCode + "') And (User_ID = '" + DT_Authentication.Rows[0][0].ToString().Trim() + "') And (Removed = '0')");
                                        if ((DT_Application.Rows != null) && (DT_Application.Rows.Count == 1))
                                        {
                                            Err_User_ID = DT_Application.Rows[0][3].ToString().Trim();
                                            Err_User_Type = DT_Authentication.Rows[0][1].ToString().Trim();
                                            User_API_AccessPolicy UAP = new User_API_AccessPolicy();
                                            if (UAP.User_Access(API_Username, API_Password, API_Code) == true)
                                            {
                                                string App_ID = DT_Application.Rows[0][0].ToString().Trim();
                                                string Form_ID = DT_Application.Rows[0][2].ToString().Trim();
                                                string Date_Format = "yyyy-MM-dd";
                                                Application_CLS_Sync AIDC = new Application_CLS_Sync();
                                                // Transaction :
                                                TransactionApplication_CLS Transaction_CLS = new TransactionApplication_CLS();
                                                Transaction_CLS.ID = DT_Application.Rows[0][1].ToString().Trim();
                                                Transaction_CLS.Traking_Code = DT_Application.Rows[0][25].ToString().Trim();
                                                Transaction_CLS.First_Name = DT_Application.Rows[0][6].ToString().Trim();
                                                Transaction_CLS.Last_Name = DT_Application.Rows[0][7].ToString().Trim();
                                                Transaction_CLS.Email = DT_Application.Rows[0][8].ToString().Trim();
                                                Transaction_CLS.Status_Code = int.Parse(DT_Application.Rows[0][9].ToString().Trim());
                                                Transaction_CLS.Status_Result = DT_Application.Rows[0][10].ToString().Trim();
                                                Transaction_CLS.Username = API_Username;
                                                Transaction_CLS.Date_Format = Date_Format;
                                                try { Transaction_CLS.Request_Date = Pb.ConvertDate_Format(DT_Application.Rows[0][11].ToString().Trim().Substring(0, 10), "dd/MM/yyyy", Date_Format); } catch (Exception) { }
                                                Transaction_CLS.Request_Time = DT_Application.Rows[0][12].ToString().Trim();
                                                Transaction_CLS.Request_IP = DT_Application.Rows[0][4].ToString().Trim();
                                                Transaction_CLS.Request_Browser = DT_Application.Rows[0][5].ToString().Trim();
                                                AIDC.Application = Transaction_CLS;
                                                // Data :
                                                AIDC.Data = null;
                                                try
                                                {
                                                    var jsonObject_Data_All = new JObject();
                                                    DataTable DT_Element = new DataTable();
                                                    int ElementCounter = 0;
                                                    DT_Element = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Element_ID,Element_Type_Code,Element_Type_Text,Element_Tag_Name,ATT1 From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + Form_ID + "') And (Status_Code = '1') And (Removed = '0') Order By Section_ID,Element_Row_Index");
                                                    foreach (DataRow RW_Element in DT_Element.Rows)
                                                    {
                                                        ElementCounter++;
                                                        var JO_Element_Data = new JObject();
                                                        try
                                                        {
                                                            JO_Element_Data.Add("Element_ID", RW_Element[0].ToString().Trim());
                                                            JO_Element_Data.Add("Element_Type_Code", RW_Element[1].ToString().Trim());
                                                            JO_Element_Data.Add("Element_Type_Text", RW_Element[2].ToString().Trim());
                                                            JO_Element_Data.Add("Element_Tag", RW_Element[3].ToString().Trim());
                                                            JO_Element_Data.Add("Element_Title", RW_Element[4].ToString().Trim());
                                                            try
                                                            {
                                                                DataTable DT_Value = new DataTable();
                                                                DT_Value = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Element_Value,Element_Text,Element_Other_Value_1,Element_Other_Value_2,Element_Other_Value_3,Element_Other_Value_4,Element_Other_Value_5,Element_Other_Value_6,Element_Other_Value_7 From Users_09_Hospitality_SingleUser_Application_Elements Where (App_ID = '" + App_ID + "') And (Element_ID = '" + RW_Element[0].ToString().Trim() + "')");
                                                                JO_Element_Data.Add("Element_Value", DT_Value.Rows[0][0].ToString().Trim());
                                                                JO_Element_Data.Add("Element_Text", DT_Value.Rows[0][1].ToString().Trim());
                                                                JO_Element_Data.Add("Element_Value_Sub1", DT_Value.Rows[0][2].ToString().Trim());
                                                                JO_Element_Data.Add("Element_Value_Sub2", DT_Value.Rows[0][3].ToString().Trim());
                                                                JO_Element_Data.Add("Element_Value_Sub3", DT_Value.Rows[0][4].ToString().Trim());
                                                                JO_Element_Data.Add("Element_Value_Sub4", DT_Value.Rows[0][5].ToString().Trim());
                                                                JO_Element_Data.Add("Element_Value_Sub5", DT_Value.Rows[0][6].ToString().Trim());
                                                                JO_Element_Data.Add("Element_Value_Sub6", DT_Value.Rows[0][7].ToString().Trim());
                                                                JO_Element_Data.Add("Element_Value_Sub7", DT_Value.Rows[0][8].ToString().Trim());
                                                                try
                                                                {
                                                                    int DTAR = 0;
                                                                    var JO_Element_Data_Acc_Result = new JObject();
                                                                    DataTable DT_Acuant_Result = new DataTable();
                                                                    DT_Acuant_Result = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Document_Key,Document_Value From Users_11_Hospitality_SingleUser_Application_Acuant_Result Where (App_ID = '" + App_ID + "') And (Element_ID = '" + RW_Element[0].ToString().Trim() + "')");
                                                                    foreach (DataRow RW_ACC_Res in DT_Acuant_Result.Rows)
                                                                    {
                                                                        DTAR++;
                                                                        JO_Element_Data_Acc_Result.Add(RW_ACC_Res[0].ToString().Trim(), RW_ACC_Res[1].ToString().Trim());
                                                                    }
                                                                    if (DTAR > 0) { JO_Element_Data.Add("OCR", JO_Element_Data_Acc_Result); }
                                                                }
                                                                catch (Exception) { }
                                                                try
                                                                {
                                                                    int DTAR2 = 0;
                                                                    var JO_Element_Data_Acc_Validation = new JObject();
                                                                    DataTable DT_Acuant_Validation = new DataTable();
                                                                    DT_Acuant_Validation = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Top 1 Result_Code,Result_Text From Users_12_Hospitality_SingleUser_Application_Validation Where (App_ID = '" + App_ID + "') And (Element_ID = '" + RW_Element[0].ToString().Trim() + "')");
                                                                    foreach (DataRow RW_ACC_Res in DT_Acuant_Validation.Rows)
                                                                    {
                                                                        DTAR2++;
                                                                        JO_Element_Data_Acc_Validation.Add("Validation_Code", RW_ACC_Res[0].ToString().Trim());
                                                                        JO_Element_Data_Acc_Validation.Add("Validation_Result", RW_ACC_Res[1].ToString().Trim());
                                                                    }
                                                                    if (DTAR2 > 0) { JO_Element_Data.Add("Validation", JO_Element_Data_Acc_Validation); }
                                                                }
                                                                catch (Exception) { }
                                                                try
                                                                {
                                                                    int DTAR3 = 0;
                                                                    var JO_Element_Data_Acc_Alert = new JObject();
                                                                    DataTable DT_Acuant_Alert = new DataTable();
                                                                    DT_Acuant_Alert = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Alert_Text From Users_10_Hospitality_SingleUser_Application_Acuant_Alert Where (App_ID = '" + App_ID + "') And (Element_ID = '" + RW_Element[0].ToString().Trim() + "')");
                                                                    foreach (DataRow RW_ACC_Res in DT_Acuant_Alert.Rows)
                                                                    {
                                                                        DTAR3++;
                                                                        JO_Element_Data_Acc_Alert.Add("No" + DTAR3.ToString(), RW_ACC_Res[0].ToString().Trim());
                                                                    }
                                                                    if (DTAR3 > 0) { JO_Element_Data.Add("Alert", JO_Element_Data_Acc_Alert); }
                                                                }
                                                                catch (Exception) { }
                                                            }
                                                            catch (Exception) { }
                                                            jsonObject_Data_All.Add("Elements_" + ElementCounter.ToString(), JO_Element_Data);
                                                        }
                                                        catch (Exception) { }
                                                    }
                                                    AIDC.Data = jsonObject_Data_All;
                                                }
                                                catch (Exception) { AIDC.Data = null; }
                                                // Parameters :
                                                AIDC.Parameters = null;
                                                try
                                                {
                                                    DataTable DT_Parameter = new DataTable();
                                                    DT_Parameter = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Parameter_Key_01,Parameter_Value_01,Parameter_Key_02,Parameter_Value_02,Parameter_Key_03,Parameter_Value_03,Parameter_Key_04,Parameter_Value_04,Parameter_Key_05,Parameter_Value_05,Parameter_Key_06,Parameter_Value_06,Parameter_Key_07,Parameter_Value_07,Parameter_Key_08,Parameter_Value_08,Parameter_Key_09,Parameter_Value_09,Parameter_Key_10,Parameter_Value_10 From Users_13_Hospitality_SingleUser_Application_DataInfo Where (App_ID = '" + App_ID + "')");
                                                    var jsonObject_Prm = new JObject();
                                                    if (DT_Parameter.Rows[0][0].ToString().Trim() != "") { jsonObject_Prm.Add(DT_Parameter.Rows[0][0].ToString().Trim(), DT_Parameter.Rows[0][1].ToString().Trim()); }
                                                    if (DT_Parameter.Rows[0][2].ToString().Trim() != "") { jsonObject_Prm.Add(DT_Parameter.Rows[0][2].ToString().Trim(), DT_Parameter.Rows[0][3].ToString().Trim()); }
                                                    if (DT_Parameter.Rows[0][4].ToString().Trim() != "") { jsonObject_Prm.Add(DT_Parameter.Rows[0][4].ToString().Trim(), DT_Parameter.Rows[0][5].ToString().Trim()); }
                                                    if (DT_Parameter.Rows[0][6].ToString().Trim() != "") { jsonObject_Prm.Add(DT_Parameter.Rows[0][6].ToString().Trim(), DT_Parameter.Rows[0][7].ToString().Trim()); }
                                                    if (DT_Parameter.Rows[0][8].ToString().Trim() != "") { jsonObject_Prm.Add(DT_Parameter.Rows[0][8].ToString().Trim(), DT_Parameter.Rows[0][9].ToString().Trim()); }
                                                    if (DT_Parameter.Rows[0][10].ToString().Trim() != "") { jsonObject_Prm.Add(DT_Parameter.Rows[0][10].ToString().Trim(), DT_Parameter.Rows[0][11].ToString().Trim()); }
                                                    if (DT_Parameter.Rows[0][12].ToString().Trim() != "") { jsonObject_Prm.Add(DT_Parameter.Rows[0][12].ToString().Trim(), DT_Parameter.Rows[0][13].ToString().Trim()); }
                                                    if (DT_Parameter.Rows[0][14].ToString().Trim() != "") { jsonObject_Prm.Add(DT_Parameter.Rows[0][14].ToString().Trim(), DT_Parameter.Rows[0][15].ToString().Trim()); }
                                                    if (DT_Parameter.Rows[0][16].ToString().Trim() != "") { jsonObject_Prm.Add(DT_Parameter.Rows[0][16].ToString().Trim(), DT_Parameter.Rows[0][17].ToString().Trim()); }
                                                    if (DT_Parameter.Rows[0][18].ToString().Trim() != "") { jsonObject_Prm.Add(DT_Parameter.Rows[0][18].ToString().Trim(), DT_Parameter.Rows[0][19].ToString().Trim()); }
                                                    AIDC.Parameters = jsonObject_Prm;
                                                }
                                                catch (Exception) { AIDC.Parameters = null; }
                                                // Files :
                                                AIDC.Files = null;
                                                try
                                                {

                                                }
                                                catch (Exception) { AIDC.Files = null; }
                                                Result_API = AIDC;
                                                // Retrieve Info To Sql
                                                try
                                                {
                                                    DataTable DT_APPDTInfo = new DataTable();
                                                    DT_APPDTInfo = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Data_Retrieve_Flag,Data_Retrieve_Count From Users_13_Hospitality_SingleUser_Application_DataInfo Where (App_ID = '" + DT_Application.Rows[0][0].ToString().Trim() + "')");
                                                    string URetIP = Pb.GetUserIP_HttpRequest(HttpContext.Current.Request); ;
                                                    if (DT_Application.Rows[0][0].ToString().Trim().ToUpper() == "FALSE")
                                                    {
                                                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_13_Hospitality_SingleUser_Application_DataInfo Set [Data_Retrieve_Flag] = '1',[Data_Retrieve_Count] = '1',[Data_Retrieve_First_Date] = '" + Sq.Sql_Date() + "',[Data_Retrieve_First_Time] = '" + Sq.Sql_Time() + "',[Data_Retrieve_First_IP] = '" + URetIP + "',[Data_Retrieve_Last_Date] = '" + Sq.Sql_Date() + "',[Data_Retrieve_Last_Time] = '" + Sq.Sql_Time() + "',[Data_Retrieve_Last_IP] = '" + URetIP + "' Where (App_ID = '" + DT_Application.Rows[0][0].ToString().Trim() + "')");
                                                    }
                                                    else
                                                    {
                                                        int RetCount = int.Parse(DT_Application.Rows[0][1].ToString().Trim());
                                                        RetCount = RetCount + 1;
                                                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_13_Hospitality_SingleUser_Application_DataInfo Set [Data_Retrieve_Count] = '" + RetCount + "',[Data_Retrieve_Last_Date] = '" + Sq.Sql_Date() + "',[Data_Retrieve_Last_Time] = '" + Sq.Sql_Time() + "',[Data_Retrieve_Last_IP] = '" + URetIP + "' Where (App_ID = '" + DT_Application.Rows[0][0].ToString().Trim() + "')");
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
                                            FunctionResult_Mesaage.Description = "Traking code is not valid, Please reviewing your request structure";
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
                                FunctionResult_Mesaage.Description = "Your traking code is missing, Please reviewing your request structure";
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
                    Error_Message += "$" + "Application Traking Code : " + Er_TrakingCode;
                    Error_Message += "$" + "Request API Post Username : " + Er_User_API_Post_Username;
                    DT_CreateTransAction = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Insert Into Users_15_API_Transaction OUTPUT Inserted.ID Values ('','" + Err_User_ID + "','" + Err_User_Type + "','" + API_Name + "','" + API_Code + "','" + API_FullName + "','" + Sq.Sql_Date() + "','" + Sq.Sql_Time() + "','" + Pb.GetUserIP_HttpRequest(HttpContext.Current.Request) + "','" + FunctionResult_Mesaage.Code + "','" + ErrFT.ToString() + "','" + FunctionResult_Mesaage.Description + "','3','Failed','0','0','Unknown','0','Unknown','1','0','" + Sq.Sql_Date() + "','" + Sq.Sql_Time() + "','" + Err_User_ID + "','','','0','0','','','0','0','','','0','0','','','','0','0','','','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','','','','','','0','','','','','" + Error_Message.Trim() + "','4')");
                    string Er_App_ID = DT_CreateTransAction.Rows[0][0].ToString().Trim();
                    string Er_Trans_UID = Er_App_ID + Pb.Make_Security_Code(50);
                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [UID] = '" + Er_Trans_UID + "' Where (ID = '" + Er_App_ID + "')");
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
