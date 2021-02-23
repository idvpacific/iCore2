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
    public class CallBackFunction
    {
        //====================================================================================================================
        SQL_Tranceiver Sq = new SQL_Tranceiver();
        PublicFunctions Pb = new PublicFunctions();
        //====================================================================================================================
        public void Send_CallBack(string Transaction_ID, string Username, string AttachFile, string Ins_Date, string Ins_Time, string Date_Format)
        {
            try
            {
                DataTable DT_Authentication = new DataTable();
                DT_Authentication = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Callback_URL,API_Name,UID,Request_IP,Image_1_Title,Image_2_Title,Image_3_Title,Image_4_Title,Image_5_Title,Image_6_Title,API_Type_Code,Status_Code,Status_Text From Users_15_API_Transaction Where (ID = '" + Transaction_ID + "') And (Removed = '0')");
                if (DT_Authentication.Rows.Count == 1)
                {
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(DT_Authentication.Rows[0][0].ToString().Trim());
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";

                    string CB_Date_SQ = Sq.Sql_Date();
                    string CB_Date_Last = "";
                    try { CB_Date_Last = Pb.ConvertDate_Format(CB_Date_SQ, "yyyy-MM-dd", Date_Format); } catch (Exception) { CB_Date_Last = CB_Date_SQ; }
                    int FileCounter = 0;
                    if (DT_Authentication.Rows[0][4].ToString().Trim() != "") { FileCounter++; }
                    if (DT_Authentication.Rows[0][5].ToString().Trim() != "") { FileCounter++; }
                    if (DT_Authentication.Rows[0][6].ToString().Trim() != "") { FileCounter++; }
                    if (DT_Authentication.Rows[0][7].ToString().Trim() != "") { FileCounter++; }
                    if (DT_Authentication.Rows[0][8].ToString().Trim() != "") { FileCounter++; }
                    if (DT_Authentication.Rows[0][9].ToString().Trim() != "") { FileCounter++; }
                    string STS_C = "1"; string STS_T = "Pending";
                    if (DT_Authentication.Rows[0][11].ToString().Trim() == "1")
                    {
                        STS_C = "4";
                        STS_T = "Completed";
                    }
                    else
                    {
                        STS_C = DT_Authentication.Rows[0][11].ToString().Trim();
                        STS_T = DT_Authentication.Rows[0][12].ToString().Trim();
                    }
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        string json = "{\"Request_Code\":\"" + DT_Authentication.Rows[0][10].ToString().Trim() + "\"," +
                                      "\"Request_Name\":\"" + DT_Authentication.Rows[0][1].ToString().Trim() + "\"," +
                                      "\"Transaction_ID\":\"" + DT_Authentication.Rows[0][2].ToString().Trim() + "\"," +
                                      "\"Status_Code\":" + STS_C + "," +
                                      "\"Status_Result\":\"" + STS_T + "\"," +
                                      "\"Username\":\"" + Username + "\"," +
                                      "\"Date_Format\":\"" + Date_Format + "\"," +
                                      "\"Request_Date\":\"" + Ins_Date + "\"," +
                                      "\"Request_Time\":\"" + Ins_Time + "\"," +
                                      "\"Callback_Date\":\"" + CB_Date_Last + "\"," +
                                      "\"Callback_Time\":\"" + Sq.Sql_Time() + "\"," +
                                      "\"Attached_File\":\"" + AttachFile + "\"," +
                                      "\"Processed_File\":\"" + FileCounter.ToString() + "\"," +
                                      "\"Request_IP\":\"" + DT_Authentication.Rows[0][3].ToString().Trim() + "\"}";
                        streamWriter.Write(json);
                    }
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Callback_Send] = '1',[Callback_Date] = '" + Sq.Sql_Date() + "',[Callback_Time] = '" + Sq.Sql_Time() + "' Where (ID = '" + Transaction_ID + "') And (Removed = '0')");
                }
            }
            catch (Exception e)
            {
                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '2',[Status_Text] = 'Review',[Error_Message] = '" + e.Message.Trim().Replace(",", "").Replace(";", "").Replace("'", "") + "' Where (ID = '" + Transaction_ID + "') And (Removed = '0')");
            }
        }
    }
}