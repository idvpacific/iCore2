using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace iCore_Administrator.Modules.HSU_Application
{
    public class Application_MasterFunction
    {
        //====================================================================================================================
        SQL_Tranceiver Sq = new SQL_Tranceiver();
        PublicFunctions Pb = new PublicFunctions();
        Application_Acuant Acuant_Func = new Application_Acuant();
        Application_Guesty Gusty_Func = new Application_Guesty();
        //====================================================================================================================
        // Application Ststus Code :
        // 1 : Pending
        // 2 : Review
        // 3 : Failed
        // 4 : Passed
        //====================================================================================================================
        public void Application_StartCheck(string App_ID)
        {
            try { Application_GetFieldValue(App_ID); } catch (Exception) { }
            try { Application_CheckAccuant(App_ID); } catch (Exception) { }
            try { Application_DoTask(App_ID); } catch (Exception) { }
            try { Application_EditStatus(App_ID); } catch (Exception) { }
            try { Application_GuestyDo(App_ID); } catch (Exception) { }
        }
        //====================================================================================================================
        public void Application_GetFieldValue(string App_ID)
        {
            try
            {
                string V_Firstname = "";
                string V_Lastname = "";
                string V_Email = "";
                DataTable DT_Application = new DataTable();
                DT_Application = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Form_ID From Users_08_Hospitality_SingleUser_Application Where (ID = '" + App_ID + "') And (Removed = '0')");
                if (DT_Application.Rows != null)
                {
                    if (DT_Application.Rows.Count == 1)
                    {
                        string Form_ID = DT_Application.Rows[0][0].ToString().Trim();
                        DataTable DT_Section = new DataTable();
                        DT_Section = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Section_ID From Users_05_Hospitality_SingleUser_RegisterForms_Section Where (Group_ID = '" + Form_ID + "') And (Status_Code = '1') And (Removed = '0') Order By Row_Index,Name");
                        DataTable DT_Element = null;
                        DataTable DT_Value = null;
                        foreach (DataRow RW_Sec in DT_Section.Rows)
                        {
                            DT_Element = new DataTable();
                            DT_Element = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Element_ID,ATT19 From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + Form_ID + "') And (Section_ID = '" + RW_Sec[0].ToString().Trim() + "') And (Element_Type_Code = '1') And (ATT19 In ('2','3','7','8')) And (Status_Code = '1') And (Removed = '0')");
                            foreach (DataRow RW_Ele in DT_Element.Rows)
                            {
                                DT_Value = new DataTable();
                                DT_Value = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Element_Text From Users_09_Hospitality_SingleUser_Application_Elements Where (App_ID = '" + App_ID + "') And (Element_ID = '" + RW_Ele[0].ToString().Trim() + "')");
                                if (DT_Value.Rows != null)
                                {
                                    if (DT_Value.Rows.Count != 0)
                                    {
                                        switch (RW_Ele[1].ToString().Trim())
                                        {
                                            case "2":
                                                {
                                                    V_Firstname = DT_Value.Rows[0][0].ToString().Trim();
                                                    break;
                                                }
                                            case "3":
                                                {
                                                    V_Lastname = DT_Value.Rows[0][0].ToString().Trim();
                                                    break;
                                                }
                                            case "7":
                                                {
                                                    V_Email = DT_Value.Rows[0][0].ToString().Trim();
                                                    break;
                                                }
                                            case "8":
                                                {
                                                    V_Email = DT_Value.Rows[0][0].ToString().Trim();
                                                    break;
                                                }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_08_Hospitality_SingleUser_Application Set [Firstname] = '" + V_Firstname + "',[Lastname] = '" + V_Lastname + "',[Email] = '" + V_Email + "' Where (ID = '" + App_ID + "')");
            }
            catch (Exception)
            { }
        }
        //====================================================================================================================
        public void Application_CheckAccuant(string App_ID)
        {
            try
            {
                List<string> EList = new List<string>();
                EList.Add("0");
                string NOTIDHERE = "";
                DataTable DT_Application = new DataTable();
                DT_Application = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Form_ID From Users_08_Hospitality_SingleUser_Application Where (ID = '" + App_ID + "') And (Removed = '0')");
                if (DT_Application.Rows != null)
                {
                    if (DT_Application.Rows.Count == 1)
                    {
                        string Form_ID = DT_Application.Rows[0][0].ToString().Trim();
                        DataTable DT_Section = new DataTable();
                        DT_Section = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Section_ID From Users_05_Hospitality_SingleUser_RegisterForms_Section Where (Group_ID = '" + Form_ID + "') And (Status_Code = '1') And (Removed = '0') Order By Row_Index,Name");
                        DataTable DT_Element = null;
                        // Driving Licence - Validation Elements :
                        foreach (DataRow RW_Sec in DT_Section.Rows)
                        {
                            NOTIDHERE = ""; foreach (string StrID in EList) { NOTIDHERE += StrID + ","; }
                            NOTIDHERE = NOTIDHERE.Trim(); if (NOTIDHERE == "") { NOTIDHERE = "0"; }
                            if (NOTIDHERE.Substring(NOTIDHERE.Length - 1, 1) == ",") { NOTIDHERE = NOTIDHERE.Substring(0, NOTIDHERE.Length - 1); }
                            DT_Element = new DataTable();
                            DT_Element = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Element_ID From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + Form_ID + "') And (Section_ID = '" + RW_Sec[0].ToString().Trim() + "') And (Element_ID Not In (" + NOTIDHERE + ")) And (Element_Type_Code = '6') And (ATT12 = '5') And (Status_Code = '1') And (Removed = '0')");
                            foreach (DataRow RW_Ele in DT_Element.Rows)
                            {
                                string FontElement_ID = "0";
                                string BackElement_ID = "0";
                                EList.Add(RW_Ele[0].ToString().Trim());
                                FontElement_ID = RW_Ele[0].ToString().Trim();
                                DataTable DT_GetBackImage = new DataTable();
                                NOTIDHERE = ""; foreach (string StrID in EList) { NOTIDHERE += StrID + ","; }
                                NOTIDHERE = NOTIDHERE.Trim(); if (NOTIDHERE == "") { NOTIDHERE = "0"; }
                                if (NOTIDHERE.Substring(NOTIDHERE.Length - 1, 1) == ",") { NOTIDHERE = NOTIDHERE.Substring(0, NOTIDHERE.Length - 1); }
                                DT_GetBackImage = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Top 1 Element_ID From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + Form_ID + "') And (Section_ID = '" + RW_Sec[0].ToString().Trim() + "') And (Element_ID Not In (" + NOTIDHERE + ")) And (Element_Type_Code = '6') And (ATT12 = '7') And (Status_Code = '1') And (Removed = '0')");
                                if (DT_GetBackImage.Rows != null) { if (DT_GetBackImage.Rows.Count == 1) { EList.Add(DT_GetBackImage.Rows[0][0].ToString().Trim()); BackElement_ID = DT_GetBackImage.Rows[0][0].ToString().Trim(); } }
                                FontElement_ID = FontElement_ID.Trim();
                                BackElement_ID = BackElement_ID.Trim();
                                if ((FontElement_ID != "0") || (BackElement_ID != "0"))
                                {
                                    Acuant_Func.GetData(App_ID, FontElement_ID, FontElement_ID, BackElement_ID, true, AssureTec.AssureID.Web.SDK.CroppingExpectedSize.ID1);
                                }
                            }
                        }
                        // Driving Licence - OCR Elementns :
                        foreach (DataRow RW_Sec in DT_Section.Rows)
                        {
                            NOTIDHERE = ""; foreach (string StrID in EList) { NOTIDHERE += StrID + ","; }
                            NOTIDHERE = NOTIDHERE.Trim(); if (NOTIDHERE == "") { NOTIDHERE = "0"; }
                            if (NOTIDHERE.Substring(NOTIDHERE.Length - 1, 1) == ",") { NOTIDHERE = NOTIDHERE.Substring(0, NOTIDHERE.Length - 1); }
                            DT_Element = new DataTable();
                            DT_Element = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Element_ID From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + Form_ID + "') And (Section_ID = '" + RW_Sec[0].ToString().Trim() + "') And (Element_ID Not In (" + NOTIDHERE + ")) And (Element_Type_Code = '6') And (ATT12 = '4') And (Status_Code = '1') And (Removed = '0')");
                            foreach (DataRow RW_Ele in DT_Element.Rows)
                            {
                                string FontElement_ID = "0";
                                string BackElement_ID = "0";
                                EList.Add(RW_Ele[0].ToString().Trim());
                                FontElement_ID = RW_Ele[0].ToString().Trim();
                                DataTable DT_GetBackImage = new DataTable();
                                NOTIDHERE = ""; foreach (string StrID in EList) { NOTIDHERE += StrID + ","; }
                                NOTIDHERE = NOTIDHERE.Trim(); if (NOTIDHERE == "") { NOTIDHERE = "0"; }
                                if (NOTIDHERE.Substring(NOTIDHERE.Length - 1, 1) == ",") { NOTIDHERE = NOTIDHERE.Substring(0, NOTIDHERE.Length - 1); }
                                DT_GetBackImage = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Top 1 Element_ID From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + Form_ID + "') And (Section_ID = '" + RW_Sec[0].ToString().Trim() + "') And (Element_ID Not In (" + NOTIDHERE + ")) And (Element_Type_Code = '6') And (ATT12 = '6') And (Status_Code = '1') And (Removed = '0')");
                                if (DT_GetBackImage.Rows != null) { if (DT_GetBackImage.Rows.Count == 1) { EList.Add(DT_GetBackImage.Rows[0][0].ToString().Trim()); BackElement_ID = DT_GetBackImage.Rows[0][0].ToString().Trim(); } }
                                FontElement_ID = FontElement_ID.Trim();
                                BackElement_ID = BackElement_ID.Trim();
                                if ((FontElement_ID != "0") || (BackElement_ID != "0"))
                                {
                                    Acuant_Func.GetData(App_ID, FontElement_ID, FontElement_ID, BackElement_ID, false, AssureTec.AssureID.Web.SDK.CroppingExpectedSize.ID1);
                                }
                            }
                        }
                        // Passport - Validation Elements :
                        foreach (DataRow RW_Sec in DT_Section.Rows)
                        {
                            NOTIDHERE = ""; foreach (string StrID in EList) { NOTIDHERE += StrID + ","; }
                            NOTIDHERE = NOTIDHERE.Trim(); if (NOTIDHERE == "") { NOTIDHERE = "0"; }
                            if (NOTIDHERE.Substring(NOTIDHERE.Length - 1, 1) == ",") { NOTIDHERE = NOTIDHERE.Substring(0, NOTIDHERE.Length - 1); }
                            DT_Element = new DataTable();
                            DT_Element = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Element_ID From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + Form_ID + "') And (Section_ID = '" + RW_Sec[0].ToString().Trim() + "') And (Element_ID Not In (" + NOTIDHERE + ")) And (Element_Type_Code = '6') And (ATT12 = '3') And (Status_Code = '1') And (Removed = '0')");
                            foreach (DataRow RW_Ele in DT_Element.Rows)
                            {
                                string FontElement_ID = "0";
                                EList.Add(RW_Ele[0].ToString().Trim());
                                FontElement_ID = RW_Ele[0].ToString().Trim();
                                if ((FontElement_ID != "0"))
                                {
                                    Acuant_Func.GetData(App_ID, FontElement_ID, FontElement_ID, "0", true, AssureTec.AssureID.Web.SDK.CroppingExpectedSize.ID3);
                                }
                            }
                        }
                        // Passport - OCR Elements :
                        foreach (DataRow RW_Sec in DT_Section.Rows)
                        {
                            NOTIDHERE = ""; foreach (string StrID in EList) { NOTIDHERE += StrID + ","; }
                            NOTIDHERE = NOTIDHERE.Trim(); if (NOTIDHERE == "") { NOTIDHERE = "0"; }
                            if (NOTIDHERE.Substring(NOTIDHERE.Length - 1, 1) == ",") { NOTIDHERE = NOTIDHERE.Substring(0, NOTIDHERE.Length - 1); }
                            DT_Element = new DataTable();
                            DT_Element = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Element_ID From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + Form_ID + "') And (Section_ID = '" + RW_Sec[0].ToString().Trim() + "') And (Element_ID Not In (" + NOTIDHERE + ")) And (Element_Type_Code = '6') And (ATT12 = '2') And (Status_Code = '1') And (Removed = '0')");
                            foreach (DataRow RW_Ele in DT_Element.Rows)
                            {
                                string FontElement_ID = "0";
                                EList.Add(RW_Ele[0].ToString().Trim());
                                FontElement_ID = RW_Ele[0].ToString().Trim();
                                if ((FontElement_ID != "0"))
                                {
                                    Acuant_Func.GetData(App_ID, FontElement_ID, FontElement_ID, "0", false, AssureTec.AssureID.Web.SDK.CroppingExpectedSize.ID3);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        }
        //====================================================================================================================
        public void Application_DoTask(string App_ID)
        {
            try
            {
                string HSU_User_ID = "0";
                string HSU_Form_ID = "0";
                DataTable DT_App = new DataTable();
                DT_App = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select User_ID,Form_ID From Users_08_Hospitality_SingleUser_Application Where (ID = '" + App_ID + "')");
                HSU_User_ID = DT_App.Rows[0][0].ToString().Trim();
                HSU_Form_ID = DT_App.Rows[0][1].ToString().Trim();
                var type = Type.GetType("iCore_Administrator.Modules.HSU_Application.Conditions.HSU_" + HSU_User_ID);
                var inst = Activator.CreateInstance(type);
                var method = type.GetMethod("Form_" + HSU_Form_ID);
                method.Invoke(inst, new object[] { App_ID });
            }
            catch (Exception)
            { }
        }
        //====================================================================================================================
        public void Application_EditStatus(string App_ID)
        {
            string Status_Code = "1";
            string Status_Text = "Pending";
            string Err_Text = "No error occurred";
            try
            {
                DataTable DT = new DataTable();
                DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Element_Type_Text,Element_Tag_Name,Result_Text,Result_Code From Users_12_Hospitality_SingleUser_Application_Validation_V Where (App_ID = '" + App_ID + "')");
                if (DT.Rows != null)
                {
                    if (DT.Rows.Count > 0)
                    {
                        Err_Text = "";
                        bool App_Passed = true;
                        bool App_Failed = true;
                        foreach (DataRow RW in DT.Rows)
                        {
                            if (RW[3].ToString().Trim() != "5") { App_Passed = false; }
                            if (RW[3].ToString().Trim() != "6") { App_Failed = false; }
                            Err_Text += (RW[0].ToString().Trim() + " - " + RW[1].ToString().Trim() + " - " + RW[2].ToString().Trim()).Trim() + "$";
                        }
                        Err_Text = Err_Text.Replace("$$", "$").Trim();
                        if (Err_Text.Substring(Err_Text.Length - 1, 1) == "$") { Err_Text = Err_Text.Substring(0, Err_Text.Length - 1); }
                        if ((App_Passed == true) && (App_Failed == false))
                        {
                            Status_Code = "4";
                            Status_Text = "Passed";
                        }
                        else
                        {
                            if ((App_Passed == false) && (App_Failed == true))
                            {
                                Status_Code = "3";
                                Status_Text = "Failed";
                            }
                            else
                            {
                                Status_Code = "2";
                                Status_Text = "Review";

                            }
                        }
                    }
                    else
                    {
                        Status_Code = "2";
                        Status_Text = "Review";
                        Err_Text = "No auto check conditions are set";
                    }
                }
                else
                {
                    Status_Code = "2";
                    Status_Text = "Review";
                    Err_Text = "Error while checking application conditions";
                }
            }
            catch (Exception)
            {
                Status_Code = "2";
                Status_Text = "Review";
                Err_Text = "System exception error while checking application conditions";
            }
            try
            {
                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_08_Hospitality_SingleUser_Application Set [Status_Code] = '" + Status_Code + "',[Status_Text] = '" + Status_Text + "',[App_Message] = '" + Err_Text + "' Where (ID = '" + App_ID + "')");
            }
            catch (Exception)
            {
                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_08_Hospitality_SingleUser_Application Set [Status_Code] = '1',[Status_Text] = 'Pending',[App_Message] = 'System exception error while updating application status' Where (ID = '" + App_ID + "')");
            }
        }
        //====================================================================================================================
        public void Application_GuestyDo(string App_ID)
        {
            string GError = "";
            DataTable DT_Application = new DataTable();
            try
            {
                DT_Application = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Form_ID,User_ID,Status_Code,Status_Text,App_Message From Users_08_Hospitality_SingleUser_Application Where (ID = '" + App_ID + "')");
                DataTable DT_Element = new DataTable();
                DT_Element = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Element_ID From Users_06_Hospitality_SingleUser_RegisterForms_Elements Where (Group_ID = '" + DT_Application.Rows[0][0].ToString().Trim() + "') And (Element_Type_Code = '1') And (Status_Code = '1') And (Removed = '0') And (ATT19 = '10')");
                if (DT_Element.Rows.Count == 1)
                {
                    DataTable DT_Value = new DataTable();
                    DT_Value = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Element_Text From Users_09_Hospitality_SingleUser_Application_Elements Where (App_ID = '" + App_ID + "') And (Element_ID = '" + DT_Element.Rows[0][0].ToString().Trim() + "')");
                    if (DT_Value.Rows.Count == 1)
                    {
                        if (DT_Value.Rows[0][0].ToString().Trim() != "")
                        {
                            string GURL = "";
                            try
                            {
                                DataTable DT_GuestyURL = new DataTable();
                                DT_GuestyURL = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Top 1 Endpoint_URL From Setting_Basic_04_Guesty");
                                GURL = DT_GuestyURL.Rows[0][0].ToString().Trim();
                            }
                            catch (Exception) { GURL = ""; }
                            GURL = GURL.Trim();
                            if (GURL != "")
                            {
                                string Guesty_Key = "";
                                string Guesty_Secret = "";
                                try
                                {
                                    DataTable DT_GuestyKeys = new DataTable();
                                    DT_GuestyKeys = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Top 1 GuestyAPI_Key,GuestyAPI_Secret From Users_03_Hospitality_SingleUser_BasicSetting Where (User_ID = '" + DT_Application.Rows[0][1].ToString().Trim() + "')");
                                    Guesty_Key = DT_GuestyKeys.Rows[0][0].ToString().Trim();
                                    Guesty_Secret = DT_GuestyKeys.Rows[0][1].ToString().Trim();
                                }
                                catch (Exception) { Guesty_Key = ""; Guesty_Secret = ""; }
                                Guesty_Key = Guesty_Key.Trim();
                                Guesty_Secret = Guesty_Secret.Trim();
                                if ((Guesty_Key != "") && (Guesty_Secret != ""))
                                {
                                    Gusty_Func.Set_ConnectionKey(GURL, Guesty_Key, Guesty_Secret);
                                    Application_Guesty.Guesty_BookingStatus GBS = new Application_Guesty.Guesty_BookingStatus();
                                    GBS.Error_Code = 0; GBS.Error_Text = ""; GBS._id = ""; GBS.status = "";
                                    bool DoGus = false;
                                    string BSres = "Unknown";
                                    if (DT_Application.Rows[0][2].ToString().Trim() == "4")
                                    {
                                        DataTable DT_GuestyURL = new DataTable();

                                        GBS = Gusty_Func.Set_Booking_Status(DT_Value.Rows[0][0].ToString().Trim(), Application_Guesty.BookingStatus.Approve);
                                        DoGus = true;
                                        BSres = "approve";
                                    }
                                    else
                                    {
                                        if (DT_Application.Rows[0][2].ToString().Trim() == "3")
                                        {
                                            GBS = Gusty_Func.Set_Booking_Status(DT_Value.Rows[0][0].ToString().Trim(), Application_Guesty.BookingStatus.Decline);
                                            DoGus = true;
                                            BSres = "decline";
                                        }
                                        else
                                        {
                                            GError = "Guesty wait for determine the status";
                                        }
                                    }
                                    if (DoGus == true)
                                    {
                                        if (GBS.Error_Code == 0)
                                        {
                                            GError = "Guesty booking status successfully set to " + BSres;
                                        }
                                        else
                                        {
                                            GError = GBS.Error_Code + " - " + GBS.Error_Text;
                                        }
                                    }
                                }
                                else
                                {
                                    GError = "You have not entered your guesty account keys in the settings page of your panel";
                                }
                            }
                            else
                            {
                                GError = "IDV technical support does not define guesty server information";
                            }
                        }
                        else
                        {
                            GError = "The customer did not enter the confirmation code";
                        }
                    }
                    else
                    {
                        GError = "Guesty confirmation code not existed";
                    }
                }
                else
                {
                    if (DT_Element.Rows.Count > 1)
                    {
                        GError = "Guesty confirmation code element not properly defined";
                    }
                }
            }
            catch (Exception)
            { GError = "Guesty system exception error"; }
            try
            {
                string DTM = "[" + Pb.Get_Date() + " " + Pb.Get_Time() + "] ";
                GError = "[Guesty] " + DTM + GError;
                string BeforeError = DT_Application.Rows[0][4].ToString().Trim();
                if (BeforeError.Trim() != "")
                {
                    BeforeError = BeforeError + "$" + GError.Trim();
                }
                else
                {
                    BeforeError = GError.Trim();
                }
                BeforeError = BeforeError.Replace("'", "").Replace(",", "");
                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_08_Hospitality_SingleUser_Application Set [App_Message] = '" + BeforeError + "' Where (ID = '" + App_ID + "')");
            }
            catch (Exception)
            { }
        }
        //====================================================================================================================
    }
}