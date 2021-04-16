using iCore_Administrator.Modules;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace iCore_Administrator.Modules.HSU_Application
{
    public class HSU_Callback
    {
        //====================================================================================================================
        SQL_Tranceiver Sq = new SQL_Tranceiver();
        PublicFunctions Pb = new PublicFunctions();
        //====================================================================================================================
        public void Send_CallBack_Submit(string AppID, string FormID)
        {
            try
            {
                string SBMCB_URL = "";
                DataTable DT_FormConfig = new DataTable();
                DT_FormConfig = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Calback_Submit,Calback_Submit_URL,Calback_Submit_HUsername_Key,Calback_Submit_HUsername_Value,Calback_Submit_HPassword_Key,Calback_Submit_HPassword_Value From Users_07_Hospitality_SingleUser_RegisterForms_Configuration Where (Form_ID = '" + FormID + "')");
                if (DT_FormConfig.Rows.Count == 1)
                {
                    if (DT_FormConfig.Rows[0][0].ToString().Trim() == "1")
                    {
                        if (DT_FormConfig.Rows[0][1].ToString().Trim() != "")
                        {
                            SBMCB_URL = DT_FormConfig.Rows[0][1].ToString().Trim();
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
                if (SBMCB_URL.Trim() == "") { return; }
                DataTable DT_Application = new DataTable();
                DT_Application = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select App_UnicID,TrakingCode,Status_Code,Status_Text,Customer_IP,BrowserName,Firstname,Lastname,Email,Ins_Date,Ins_Time From Users_08_Hospitality_SingleUser_Application Where (ID = '" + AppID + "') And (Removed = '0')");
                if (DT_Application.Rows.Count == 1)
                {
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(SBMCB_URL);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";
                    if(DT_FormConfig.Rows[0][2].ToString().Trim()!="")
                    {
                        if (DT_FormConfig.Rows[0][3].ToString().Trim() != "")
                        {
                            httpWebRequest.Headers.Add(DT_FormConfig.Rows[0][2].ToString().Trim(), DT_FormConfig.Rows[0][3].ToString().Trim());
                        }
                    }
                    if (DT_FormConfig.Rows[0][4].ToString().Trim() != "")
                    {
                        if (DT_FormConfig.Rows[0][5].ToString().Trim() != "")
                        {
                            httpWebRequest.Headers.Add(DT_FormConfig.Rows[0][4].ToString().Trim(), DT_FormConfig.Rows[0][5].ToString().Trim());
                        }
                    }
                    string CB_Date_SQ = Sq.Sql_Date();
                    string CB_Date_Last = "";
                    try { CB_Date_Last = Pb.ConvertDate_Format(CB_Date_SQ, "yyyy-MM-dd", "dd/MM/yyyy"); } catch (Exception) { CB_Date_Last = CB_Date_SQ; }
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        string json = "{\"Application_UniqueID\":\"" + DT_Application.Rows[0][0].ToString().Trim() + "\"," +
                                      "\"Traking_Code\":\"" + DT_Application.Rows[0][1].ToString().Trim() + "\"," +
                                      "\"Status_Code\":" + DT_Application.Rows[0][2].ToString().Trim() + "," +
                                      "\"Status_Result\":\"" + DT_Application.Rows[0][3].ToString().Trim() + "\"," +
                                      "\"Request_IP\":\"" + DT_Application.Rows[0][4].ToString().Trim() + "\"," +
                                      "\"Request_Browser\":\"" + DT_Application.Rows[0][5].ToString().Trim() + "\"," +
                                      "\"Request_Date\":\"" + DT_Application.Rows[0][9].ToString().Trim() + "\"," +
                                      "\"Request_Time\":\"" + DT_Application.Rows[0][10].ToString().Trim() + "\"," +
                                      "\"Callback_Type\":\"" + "Application Submited" + "\"," +
                                      "\"Callback_Date\":\"" + CB_Date_Last + "\"," +
                                      "\"Callback_Time\":\"" + Sq.Sql_Time() + "\"}";
                        streamWriter.Write(json);
                    }
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_13_Hospitality_SingleUser_Application_DataInfo Set [Callback_Submit_Tag] = '1',[Callback_Submit_Send] = '1',[Callback_Submit_URL] = '" + SBMCB_URL + "',[Callback_Submit_Count] = '1',[Callback_Submit_Date_First] = '" + Sq.Sql_Date() + "',[Callback_Submit_Time_First] = '" + Sq.Sql_Time() + "' Where (App_ID = '" + AppID + "')");
                }
            }
            catch (Exception e)
            {
                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_08_Hospitality_SingleUser_Application Set [Status_Code] = '4',[Status_Text] = 'Review',[App_Message] = '" + e.Message.Trim().Replace(",", "").Replace(";", "").Replace("'", "") + "' Where (ID = '" + AppID + "') And (Removed = '0')");
            }
        }
        //====================================================================================================================
        public void Send_CallBack_Submit2(string AppID, string FormID)
        {
            try
            {
                string SBMCB_URL = "";
                DataTable DT_FormConfig = new DataTable();
                DT_FormConfig = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Calback_Submit2,Calback_Submit_URL2,Calback_Submit_HUsername_Key2,Calback_Submit_HUsername_Value2,Calback_Submit_HPassword_Key2,Calback_Submit_HPassword_Value2 From Users_07_Hospitality_SingleUser_RegisterForms_Configuration Where (Form_ID = '" + FormID + "')");
                if (DT_FormConfig.Rows.Count == 1)
                {
                    if (DT_FormConfig.Rows[0][0].ToString().Trim() == "1")
                    {
                        if (DT_FormConfig.Rows[0][1].ToString().Trim() != "")
                        {
                            SBMCB_URL = DT_FormConfig.Rows[0][1].ToString().Trim();
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
                if (SBMCB_URL.Trim() == "") { return; }
                DataTable DT_Application = new DataTable();
                DT_Application = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select App_UnicID,TrakingCode,Status_Code,Status_Text,Customer_IP,BrowserName,Firstname,Lastname,Email,Ins_Date,Ins_Time From Users_08_Hospitality_SingleUser_Application Where (ID = '" + AppID + "') And (Removed = '0')");
                if (DT_Application.Rows.Count == 1)
                {
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(SBMCB_URL);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";
                    if (DT_FormConfig.Rows[0][2].ToString().Trim() != "")
                    {
                        if (DT_FormConfig.Rows[0][3].ToString().Trim() != "")
                        {
                            httpWebRequest.Headers.Add(DT_FormConfig.Rows[0][2].ToString().Trim(), DT_FormConfig.Rows[0][3].ToString().Trim());
                        }
                    }
                    if (DT_FormConfig.Rows[0][4].ToString().Trim() != "")
                    {
                        if (DT_FormConfig.Rows[0][5].ToString().Trim() != "")
                        {
                            httpWebRequest.Headers.Add(DT_FormConfig.Rows[0][4].ToString().Trim(), DT_FormConfig.Rows[0][5].ToString().Trim());
                        }
                    }
                    string CB_Date_SQ = Sq.Sql_Date();
                    string CB_Date_Last = "";
                    try { CB_Date_Last = Pb.ConvertDate_Format(CB_Date_SQ, "yyyy-MM-dd", "dd/MM/yyyy"); } catch (Exception) { CB_Date_Last = CB_Date_SQ; }
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        string json = "{\"Application_UniqueID\":\"" + DT_Application.Rows[0][0].ToString().Trim() + "\"," +
                                      "\"Traking_Code\":\"" + DT_Application.Rows[0][1].ToString().Trim() + "\"," +
                                      "\"Status_Code\":" + DT_Application.Rows[0][2].ToString().Trim() + "," +
                                      "\"Status_Result\":\"" + DT_Application.Rows[0][3].ToString().Trim() + "\"," +
                                      "\"Request_IP\":\"" + DT_Application.Rows[0][4].ToString().Trim() + "\"," +
                                      "\"Request_Browser\":\"" + DT_Application.Rows[0][5].ToString().Trim() + "\"," +
                                      "\"Request_Date\":\"" + DT_Application.Rows[0][9].ToString().Trim() + "\"," +
                                      "\"Request_Time\":\"" + DT_Application.Rows[0][10].ToString().Trim() + "\"," +
                                      "\"Callback_Type\":\"" + "Application Submited" + "\"," +
                                      "\"Callback_Date\":\"" + CB_Date_Last + "\"," +
                                      "\"Callback_Time\":\"" + Sq.Sql_Time() + "\"}";
                        streamWriter.Write(json);
                    }
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_13_Hospitality_SingleUser_Application_DataInfo Set [Callback_Submit_Tag] = '1',[Callback_Submit_Send] = '1',[Callback_Submit_URL] = '" + SBMCB_URL + "',[Callback_Submit_Count] = '2',[Callback_Submit_Date_First] = '" + Sq.Sql_Date() + "',[Callback_Submit_Time_First] = '" + Sq.Sql_Time() + "' Where (App_ID = '" + AppID + "')");
                }
            }
            catch (Exception e)
            {
                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_08_Hospitality_SingleUser_Application Set [Status_Code] = '4',[Status_Text] = 'Review',[App_Message] = '" + e.Message.Trim().Replace(",", "").Replace(";", "").Replace("'", "") + "' Where (ID = '" + AppID + "') And (Removed = '0')");
            }
        }
        //====================================================================================================================
        public void Send_CallBack_Finished(string AppID)
        {
            try
            {
                string FormID = "0";
                try
                {
                    DataTable DT_APForm = new DataTable();
                    DT_APForm = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Form_ID From Users_08_Hospitality_SingleUser_Application Where (ID = '" + AppID + "') And (Removed = '0')");
                    FormID = DT_APForm.Rows[0][0].ToString().Trim();
                }
                catch (Exception) { }
                string SBMCB_URL = "";
                DataTable DT_FormConfig = new DataTable();
                DT_FormConfig = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Callback_Process,Callback_Process_URL,Callback_Process_HUsername_Key,Callback_Process_HUsername_Value,Callback_Process_HPassword_Key,Callback_Process_HPassword_Value From Users_07_Hospitality_SingleUser_RegisterForms_Configuration Where (Form_ID = '" + FormID + "')");
                if (DT_FormConfig.Rows.Count == 1)
                {
                    if (DT_FormConfig.Rows[0][0].ToString().Trim() == "1")
                    {
                        if (DT_FormConfig.Rows[0][1].ToString().Trim() != "")
                        {
                            SBMCB_URL = DT_FormConfig.Rows[0][1].ToString().Trim();
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
                if (SBMCB_URL.Trim() == "") { return; }
                DataTable DT_Application = new DataTable();
                DT_Application = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select App_UnicID,TrakingCode,Status_Code,Status_Text,Customer_IP,BrowserName,Firstname,Lastname,Email,Ins_Date,Ins_Time From Users_08_Hospitality_SingleUser_Application Where (ID = '" + AppID + "') And (Removed = '0')");
                if (DT_Application.Rows.Count == 1)
                {
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(SBMCB_URL);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";
                    if (DT_FormConfig.Rows[0][2].ToString().Trim() != "")
                    {
                        if (DT_FormConfig.Rows[0][3].ToString().Trim() != "")
                        {
                            httpWebRequest.Headers.Add(DT_FormConfig.Rows[0][2].ToString().Trim(), DT_FormConfig.Rows[0][3].ToString().Trim());
                        }
                    }
                    if (DT_FormConfig.Rows[0][4].ToString().Trim() != "")
                    {
                        if (DT_FormConfig.Rows[0][5].ToString().Trim() != "")
                        {
                            httpWebRequest.Headers.Add(DT_FormConfig.Rows[0][4].ToString().Trim(), DT_FormConfig.Rows[0][5].ToString().Trim());
                        }
                    }
                    string CB_Date_SQ = Sq.Sql_Date();
                    string CB_Date_Last = "";
                    try { CB_Date_Last = Pb.ConvertDate_Format(CB_Date_SQ, "yyyy-MM-dd", "dd/MM/yyyy"); } catch (Exception) { CB_Date_Last = CB_Date_SQ; }
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        string json = "{\"Application_UniqueID\":\"" + DT_Application.Rows[0][0].ToString().Trim() + "\"," +
                                      "\"Traking_Code\":\"" + DT_Application.Rows[0][1].ToString().Trim() + "\"," +
                                      "\"Status_Code\":" + DT_Application.Rows[0][2].ToString().Trim() + "," +
                                      "\"Status_Result\":\"" + DT_Application.Rows[0][3].ToString().Trim() + "\"," +
                                      "\"Request_IP\":\"" + DT_Application.Rows[0][4].ToString().Trim() + "\"," +
                                      "\"Request_Browser\":\"" + DT_Application.Rows[0][5].ToString().Trim() + "\"," +
                                      "\"Request_Date\":\"" + DT_Application.Rows[0][9].ToString().Trim() + "\"," +
                                      "\"Request_Time\":\"" + DT_Application.Rows[0][10].ToString().Trim() + "\"," +
                                      "\"Callback_Type\":\"" + "Application Processing Finished" + "\"," +
                                      "\"Callback_Date\":\"" + CB_Date_Last + "\"," +
                                      "\"Callback_Time\":\"" + Sq.Sql_Time() + "\"}";
                        streamWriter.Write(json);
                    }
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_13_Hospitality_SingleUser_Application_DataInfo Set [Callback_Proc_Tag] = '1',[Callback_Proc_Send] = '1',[Callback_Proc_URL] = '" + SBMCB_URL + "',[Callback_Proc_Count] = '1',[Callback_Proc_Date_First] = '" + Sq.Sql_Date() + "',[Callback_Proc_Time_First] = '" + Sq.Sql_Time() + "' Where (App_ID = '" + AppID + "')");
                }
            }
            catch (Exception e)
            {
                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_08_Hospitality_SingleUser_Application Set [Status_Code] = '4',[Status_Text] = 'Review',[App_Message] = '" + e.Message.Trim().Replace(",", "").Replace(";", "").Replace("'", "") + "' Where (ID = '" + AppID + "') And (Removed = '0')");
            }
        }
        //====================================================================================================================
        public void Send_CallBack_Finished2(string AppID)
        {
            try
            {
                string FormID = "0";
                try
                {
                    DataTable DT_APForm = new DataTable();
                    DT_APForm = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Form_ID From Users_08_Hospitality_SingleUser_Application Where (ID = '" + AppID + "') And (Removed = '0')");
                    FormID = DT_APForm.Rows[0][0].ToString().Trim();
                }
                catch (Exception) { }
                string SBMCB_URL = "";
                DataTable DT_FormConfig = new DataTable();
                DT_FormConfig = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Callback_Process2,Callback_Process_URL2,Callback_Process_HUsername_Key2,Callback_Process_HUsername_Value2,Callback_Process_HPassword_Key2,Callback_Process_HPassword_Value2 From Users_07_Hospitality_SingleUser_RegisterForms_Configuration Where (Form_ID = '" + FormID + "')");
                if (DT_FormConfig.Rows.Count == 1)
                {
                    if (DT_FormConfig.Rows[0][0].ToString().Trim() == "1")
                    {
                        if (DT_FormConfig.Rows[0][1].ToString().Trim() != "")
                        {
                            SBMCB_URL = DT_FormConfig.Rows[0][1].ToString().Trim();
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
                if (SBMCB_URL.Trim() == "") { return; }
                DataTable DT_Application = new DataTable();
                DT_Application = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select App_UnicID,TrakingCode,Status_Code,Status_Text,Customer_IP,BrowserName,Firstname,Lastname,Email,Ins_Date,Ins_Time From Users_08_Hospitality_SingleUser_Application Where (ID = '" + AppID + "') And (Removed = '0')");
                if (DT_Application.Rows.Count == 1)
                {
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(SBMCB_URL);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";
                    if (DT_FormConfig.Rows[0][2].ToString().Trim() != "")
                    {
                        if (DT_FormConfig.Rows[0][3].ToString().Trim() != "")
                        {
                            httpWebRequest.Headers.Add(DT_FormConfig.Rows[0][2].ToString().Trim(), DT_FormConfig.Rows[0][3].ToString().Trim());
                        }
                    }
                    if (DT_FormConfig.Rows[0][4].ToString().Trim() != "")
                    {
                        if (DT_FormConfig.Rows[0][5].ToString().Trim() != "")
                        {
                            httpWebRequest.Headers.Add(DT_FormConfig.Rows[0][4].ToString().Trim(), DT_FormConfig.Rows[0][5].ToString().Trim());
                        }
                    }
                    string CB_Date_SQ = Sq.Sql_Date();
                    string CB_Date_Last = "";
                    try { CB_Date_Last = Pb.ConvertDate_Format(CB_Date_SQ, "yyyy-MM-dd", "dd/MM/yyyy"); } catch (Exception) { CB_Date_Last = CB_Date_SQ; }
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        string json = "{\"Application_UniqueID\":\"" + DT_Application.Rows[0][0].ToString().Trim() + "\"," +
                                      "\"Traking_Code\":\"" + DT_Application.Rows[0][1].ToString().Trim() + "\"," +
                                      "\"Status_Code\":" + DT_Application.Rows[0][2].ToString().Trim() + "," +
                                      "\"Status_Result\":\"" + DT_Application.Rows[0][3].ToString().Trim() + "\"," +
                                      "\"Request_IP\":\"" + DT_Application.Rows[0][4].ToString().Trim() + "\"," +
                                      "\"Request_Browser\":\"" + DT_Application.Rows[0][5].ToString().Trim() + "\"," +
                                      "\"Request_Date\":\"" + DT_Application.Rows[0][9].ToString().Trim() + "\"," +
                                      "\"Request_Time\":\"" + DT_Application.Rows[0][10].ToString().Trim() + "\"," +
                                      "\"Callback_Type\":\"" + "Application Processing Finished" + "\"," +
                                      "\"Callback_Date\":\"" + CB_Date_Last + "\"," +
                                      "\"Callback_Time\":\"" + Sq.Sql_Time() + "\"}";
                        streamWriter.Write(json);
                    }
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_13_Hospitality_SingleUser_Application_DataInfo Set [Callback_Proc_Tag] = '1',[Callback_Proc_Send] = '1',[Callback_Proc_URL] = '" + SBMCB_URL + "',[Callback_Proc_Count] = '2',[Callback_Proc_Date_First] = '" + Sq.Sql_Date() + "',[Callback_Proc_Time_First] = '" + Sq.Sql_Time() + "' Where (App_ID = '" + AppID + "')");
                }
            }
            catch (Exception e)
            {
                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_08_Hospitality_SingleUser_Application Set [Status_Code] = '4',[Status_Text] = 'Review',[App_Message] = '" + e.Message.Trim().Replace(",", "").Replace(";", "").Replace("'", "") + "' Where (ID = '" + AppID + "') And (Removed = '0')");
            }
        }
        //====================================================================================================================
    }
}