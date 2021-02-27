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

namespace iCore_Administrator.API.Modules
{
    public class User_API_AccessPolicy
    {
        //====================================================================================================================
        SQL_Tranceiver Sq = new SQL_Tranceiver();
        PublicFunctions Pb = new PublicFunctions();
        //====================================================================================================================
        public bool User_Access(string API_Username, string API_Password, string API_Code)
        {
            try
            {
                bool ResAcc = false;
                DataTable DT_User = new DataTable();
                DT_User = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (((API_Authentication_Username = '" + API_Username + "') And (API_Authentication_Key = '" + API_Password + "')) or ((API_GetRequest_Username = '" + API_Username + "') And (API_GetRequest_Key = '" + API_Password + "')) or ((API_PostRequest_Username = '" + API_Username + "') And (API_PostRequest_Key = '" + API_Password + "'))) And (Status_Code = '1') And (Removed = '0')");
                if (DT_User.Rows != null)
                {
                    if (DT_User.Rows.Count == 1)
                    {
                        DataTable DT_Access = new DataTable();
                        DT_Access = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select IDV_OCR_All,IDV_DrivingLicence,IDV_Passport,Acuant_OCR_IDCard,Acuant_Validation_IDCard,Acuant_OCR_Passport,Acuant_Validation_Passport,TVS_Passport,Vevo_Check,GIC,LNT,FRM From Users_14_APIUser_AccessPolicy Where (User_ID = '" + DT_User.Rows[0][0].ToString().Trim() + "')");
                        if (DT_Access.Rows != null)
                        {
                            if (DT_Access.Rows.Count == 1)
                            {
                                switch(API_Code)
                                {
                                    case "1": { if (DT_Access.Rows[0][3].ToString().Trim() == "1") { ResAcc = true; } break; }
                                    case "2": { if (DT_Access.Rows[0][4].ToString().Trim() == "1") { ResAcc = true; } break; }
                                    case "3": { if (DT_Access.Rows[0][5].ToString().Trim() == "1") { ResAcc = true; } break; }
                                    case "4": { if (DT_Access.Rows[0][6].ToString().Trim() == "1") { ResAcc = true; } break; }
                                    case "7": { if (DT_Access.Rows[0][1].ToString().Trim() == "1") { ResAcc = true; } break; }
                                }
                            }
                        }
                    }
                }
                return ResAcc;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}