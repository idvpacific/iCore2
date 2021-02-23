using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace iCore_Administrator.Modules
{
    public class AuthenticationTester
    {
        //====================================================================================================================
        SQL_Tranceiver Sq = new SQL_Tranceiver();
        PublicFunctions Pb = new PublicFunctions();
        //====================================================================================================================
        // Type Code :
        //------------
        // 1: Hospitality Single User
        //====================================================================================================================
        public bool EmailTester(int TypeCode, string IDIgnore, string EmailAddress)
        {
            try
            {
                EmailAddress = EmailAddress.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                IDIgnore = IDIgnore.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DataTable DT = new DataTable();
                if ((IDIgnore == "") || (IDIgnore == "0"))
                {
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (Email = '" + EmailAddress + "') And (Removed = '0')");
                    if (DT.Rows.Count != 0) { return false; }
                    
                }
                else
                {
                    switch (TypeCode)
                    {
                        case 1:
                            {
                                DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (ID <> '" + IDIgnore + "') And (Email = '" + EmailAddress + "') And (Removed = '0')");
                                if (DT.Rows.Count != 0) { return false; }
                                break;
                            }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        //====================================================================================================================
        public bool UsernameTester(int TypeCode, string IDIgnore, string Username)
        {
            try
            {
                Username = Username.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                IDIgnore = IDIgnore.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DataTable DT = new DataTable();
                if ((IDIgnore == "") || (IDIgnore == "0"))
                {
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (Account_Login_Username = '" + Username + "') And (Removed = '0')");
                    if (DT.Rows.Count != 0) { return false; }



                }
                else
                {
                    switch (TypeCode)
                    {
                        case 1:
                            {
                                DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (ID <> '" + IDIgnore + "') And (Account_Login_Username = '" + Username + "') And (Removed = '0')");
                                if (DT.Rows.Count != 0) { return false; }
                                break;
                            }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        //====================================================================================================================
        public bool APIUsernameTester(int TypeCode, string IDIgnore, string APIUsername)
        {
            try
            {
                APIUsername = APIUsername.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                IDIgnore = IDIgnore.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DataTable DT = new DataTable();
                if ((IDIgnore == "") || (IDIgnore == "0"))
                {
                    DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where ((API_Authentication_Username = '" + APIUsername + "') Or (API_GetRequest_Username = '" + APIUsername + "') Or (API_PostRequest_Username = '" + APIUsername + "')) And (Removed = '0')");
                    if (DT.Rows.Count != 0) { return false; }



                }
                else
                {
                    switch (TypeCode)
                    {
                        case 1:
                            {
                                DT = Sq.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (ID <> '" + IDIgnore + "') And ((API_Authentication_Username = '" + APIUsername + "') Or (API_GetRequest_Username = '" + APIUsername + "') Or (API_PostRequest_Username = '" + APIUsername + "')) And (Removed = '0')");
                                if (DT.Rows.Count != 0) { return false; }
                                break;
                            }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        //====================================================================================================================
        public bool User_Authentication_Action(int ActionCode)
        {
            try
            {



                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        //====================================================================================================================
        public bool Public_Email_IsUnic(string EmailAddress)
        {
            try
            {


                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        //====================================================================================================================
        public bool Public_Username_IsUnic(string EmailAddress)
        {
            try
            {


                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        //====================================================================================================================
        public bool Public_UnicID_IsUnic(string EmailAddress)
        {
            try
            {


                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        //====================================================================================================================
    }
}