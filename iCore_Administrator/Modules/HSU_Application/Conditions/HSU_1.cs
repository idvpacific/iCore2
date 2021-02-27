using AssureTec.AssureID.Web.SDK;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace iCore_Administrator.Modules.HSU_Application.Conditions
{
    public class HSU_1
    {
        const string User_ID = "1";
        //====================================================================================================================
        SQL_Tranceiver Sq = new SQL_Tranceiver();
        PublicFunctions Pb = new PublicFunctions();
        Application_Guesty Gusty_Func = new Application_Guesty();
        //====================================================================================================================
        private string App_Guesty_Address(string App_ID)
        {
            string G_Result = "";
            try
            {
                DataTable DT_Application = new DataTable();
                DT_Application = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Form_ID,User_ID From Users_08_Hospitality_SingleUser_Application Where (ID = '" + App_ID + "')");
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
                                    Application_Guesty.Guesty_Address Gadd = new Application_Guesty.Guesty_Address();
                                    Gadd.searchable = ""; Gadd.lng = 0; Gadd.lat = 0; Gadd.apt = ""; Gadd.street = ""; Gadd.zipcode = ""; Gadd.country = ""; Gadd.state = ""; Gadd.city = ""; Gadd.full = ""; Gadd.Error_Code = 0; Gadd.Error_Text = "";
                                    Gadd = Gusty_Func.Get_Address(DT_Value.Rows[0][0].ToString().Trim());
                                    if (Gadd.Error_Code == 0)
                                    {
                                        G_Result = Gadd.full + " | Map : Lat=" + Gadd.lat.ToString() + " - Lng:" + Gadd.lng.ToString();
                                    }
                                    else
                                    {
                                        G_Result = Gadd.Error_Code + " - " + Gadd.Error_Text;
                                    }

                                }
                                else
                                {
                                    G_Result = "You[User] have not entered guesty account keys in the settings page of your[User] panel";
                                }
                            }
                            else
                            {
                                G_Result = "IDV technical support does not define guesty server information";
                            }
                        }
                        else
                        {
                            G_Result = "The customer did not enter the confirmation code";
                        }
                    }
                    else
                    {
                        G_Result = "Guesty confirmation code not existed";
                    }
                }
                else
                {
                    if (DT_Element.Rows.Count > 1)
                    {
                        G_Result = "Guesty confirmation code element not properly defined";
                    }
                }
            }
            catch (Exception)
            { G_Result = "Guesty system exception error"; }
            return G_Result.Trim();
        }
        //====================================================================================================================
        public void Form_6(string App_ID)
        {
            try
            {
                //------------------------------------------------------------------------------------------------------------
                // Function Defines :
                //const string Form_ID = "6";
                //------------------------------------------------------------------------------------------------------------
                // Guesty Home Address : Element ID : 34
                string GHA_EID = "34";
                string Ins_Date = Pb.Get_Date();
                string Ins_Time = Pb.Get_Time();
                try
                {
                    string GHA_Address = App_Guesty_Address(App_ID);
                    GHA_Address = GHA_Address.Replace(";", "").Replace("'", "").Replace(",", "").Trim();
                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_13_Hospitality_SingleUser_Application_ConditionMessage Values ('" + App_ID + "','" + GHA_EID + "','" + Ins_Date + "','" + Ins_Time + "','Guesty Address','" + GHA_Address + "','1','0','0')");
                }
                catch (Exception)
                {
                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_13_Hospitality_SingleUser_Application_ConditionMessage Values ('" + App_ID + "','" + GHA_EID + "','" + Ins_Date + "','" + Ins_Time + "','Guesty Address','Server return an exception error','1','0','0')");
                }
                //------------------------------------------------------------------------------------------------------------





            }
            catch (Exception) { }
        }
        //====================================================================================================================
    }
}



