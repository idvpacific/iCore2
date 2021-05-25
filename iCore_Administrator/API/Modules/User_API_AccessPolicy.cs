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
                        DT_Access = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Users_14_APIUser_AccessPolicy Where (User_ID = '" + DT_User.Rows[0][0].ToString().Trim() + "')");
                        if (DT_Access.Rows != null)
                        {
                            if (DT_Access.Rows.Count == 1)
                            {
                                switch (API_Code)
                                {
                                    // -------------------------------------------------------------------------------------------------------------------------------------------------------------------
                                    // IDV Local Api : ---------------------------------------------------------------------------------------------------------------------------------------------------

                                    //case "??": { if (DT_Access.Rows[0][5].ToString().Trim() == "1") { ResAcc = true; } break; }   //  IDV OCR document - ????? ( IDV Folder )

                                    case "7": { if (DT_Access.Rows[0][6].ToString().Trim() == "1") { ResAcc = true; } break; }      //  IDV driving licence OCR - OCRID ( IDV Folder )
                                    //case "??": { if (DT_Access.Rows[0][7].ToString().Trim() == "1") { ResAcc = true; } break; }   //  IDV driving licence Validation - ????? ( IDV Folder )

                                    case "11": { if (DT_Access.Rows[0][8].ToString().Trim() == "1") { ResAcc = true; } break; }     //  IDV Passport OCR - OCRPassport ( IDV Folder )
                                    case "12": { if (DT_Access.Rows[0][9].ToString().Trim() == "1") { ResAcc = true; } break; }     //  IDV Passport Validation - OCRPassport ( IDV Folder )



                                    // -------------------------------------------------------------------------------------------------------------------------------------------------------------------
                                    // Acuant Api : ------------------------------------------------------------------------------------------------------------------------------------------------------
                                    case "1": { if (DT_Access.Rows[0][10].ToString().Trim() == "1") { ResAcc = true; } break; }     //  IDScan ID cards - IDScan ( Acuant Folder )
                                    case "2": { if (DT_Access.Rows[0][11].ToString().Trim() == "1") { ResAcc = true; } break; }     //  AssureID ID cards - IDValidation ( Acuant Folder )
                                    case "3": { if (DT_Access.Rows[0][12].ToString().Trim() == "1") { ResAcc = true; } break; }     //  IDScan passport - PasportScan ( Acuant Folder )
                                    case "4": { if (DT_Access.Rows[0][13].ToString().Trim() == "1") { ResAcc = true; } break; }     //  AssureID passport - PassportValidation ( Acuant Folder )



                                    // -------------------------------------------------------------------------------------------------------------------------------------------------------------------
                                    // TVS Api : ---------------------------------------------------------------------------------------------------------------------------------------------------------

                                    //case "??": { if (DT_Access.Rows[0][14].ToString().Trim() == "1") { ResAcc = true; } break; }  //  TVS Passport validation - ????? ( TVS Folder )



                                    // -------------------------------------------------------------------------------------------------------------------------------------------------------------------
                                    // Vevo Visa Api : ---------------------------------------------------------------------------------------------------------------------------------------------------

                                    //case "10": { if (DT_Access.Rows[0][15].ToString().Trim() == "1") { ResAcc = true; } break; }  //  Vevo visa check ( Vevo Folder )



                                    // -------------------------------------------------------------------------------------------------------------------------------------------------------------------
                                    // Image Prosseccing : -----------------------------------------------------------------------------------------------------------------------------------------------------

                                    //case "??": { if (DT_Access.Rows[0][16].ToString().Trim() == "1") { ResAcc = true; } break; }  //  Guided image capture ( ?? )

                                    //case "??": { if (DT_Access.Rows[0][17].ToString().Trim() == "1") { ResAcc = true; } break; }  // Liveness test ( ?? )

                                    //case "??": { if (DT_Access.Rows[0][18].ToString().Trim() == "1") { ResAcc = true; } break; }  //  Face machting ( ?? )



                                    // -------------------------------------------------------------------------------------------------------------------------------------------------------------------
                                    // Retrive Api : -----------------------------------------------------------------------------------------------------------------------------------------------------
                                    case "5": { if (DT_Access.Rows[0][19].ToString().Trim() == "1") { ResAcc = true; } break; }     //  Transaction Retrive Data ( Result )
                                    case "6": { if (DT_Access.Rows[0][20].ToString().Trim() == "1") { ResAcc = true; } break; }     //  Transaction Retrive File ( Result )

                                    case "8": { if (DT_Access.Rows[0][21].ToString().Trim() == "1") { ResAcc = true; } break; }     //  Application Retrive Data ( Result )
                                    case "9": { if (DT_Access.Rows[0][22].ToString().Trim() == "1") { ResAcc = true; } break; }     //  Application Retrive File ( Result )
                                    // -------------------------------------------------------------------------------------------------------------------------------------------------------------------
                                    // -------------------------------------------------------------------------------------------------------------------------------------------------------------------
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