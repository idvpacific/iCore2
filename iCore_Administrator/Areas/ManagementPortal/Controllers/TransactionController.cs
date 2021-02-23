using iCore_Administrator.Modules;
using iCore_Administrator.Modules.SecurityAuthentication;
using NPOI.POIFS.Crypt.Standard;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace iCore_Administrator.Areas.ManagementPortal.Controllers
{
    public class TransactionController : Controller
    {
        //====================================================================================================================
        AuthenticationTester AAuth = new AuthenticationTester();
        SQL_Tranceiver Sq = new SQL_Tranceiver();
        PublicFunctions Pb = new PublicFunctions();
        EmailSender Email = new EmailSender();
        MimeTypeMap MTM = new MimeTypeMap();
        Crypto Cry = new Crypto();
        //====================================================================================================================
        public ActionResult Index() { return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" }); }
        //====================================================================================================================
        [HttpGet]
        public ActionResult APIU()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 128;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" }); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" });
            }
        }

        [HttpPost]
        public JsonResult APIU_Grid()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 128;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select API_UID,User_Fullname,User_Status_Text,API_Name,API_Type_Name,Ins_Date,Ins_Time,Request_IP,API_Status_Text,User_Status_Code,API_Status_Code,API_Type_Code From Users_15_API_Transaction_V Where (User_Type_Code = '3') And (Message_Error = '0') And (Removed = '0') And (Insert_API_Code = '1')");
                    if (DT.Rows != null)
                    {
                        foreach (DataRow RW in DT.Rows)
                        {
                            ResSTR += "<tr>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[0].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[1].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[3].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[4].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[5].ToString().Trim().Substring(0, 10).Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[6].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[7].ToString().Trim() + "</td>";
                            switch (RW[10].ToString().Trim())
                            {
                                case "1":
                                    {
                                        ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">";
                                        ResSTR += "<div class=\"badge badge-pill badge-light-warning\" style=\"width:80px\">" + RW[8].ToString().Trim() + "</div>";
                                        ResSTR += "</td>";
                                        break;
                                    }
                                case "2":
                                    {
                                        ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">";
                                        ResSTR += "<div class=\"badge badge-pill badge-light-info\" style=\"width:80px\">" + RW[8].ToString().Trim() + "</div>";
                                        ResSTR += "</td>";
                                        break;
                                    }
                                case "3":
                                    {
                                        ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">";
                                        ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:80px\">" + RW[8].ToString().Trim() + "</div>";
                                        ResSTR += "</td>";
                                        break;
                                    }
                                case "4":
                                    {
                                        ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">";
                                        ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:80px\">" + RW[8].ToString().Trim() + "</div>";
                                        ResSTR += "</td>";
                                        break;
                                    }
                            }
                            ResSTR += "</tr>";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading API user transactions information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult APIU_New()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 129;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" }); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" });
            }
        }

        [HttpPost]
        public JsonResult APIU_New_Grid()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 129;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select API_UID,User_Fullname,User_Status_Text,API_Name,API_Type_Name,Ins_Date,Ins_Time,Request_IP,API_Status_Text,User_Status_Code,API_Status_Code,API_Type_Code From Users_15_API_Transaction_V Where (User_Type_Code = '3') And (Seen_Admin_Flag = '0') And (Message_Error = '0') And (Removed = '0') And (Insert_API_Code = '1')");
                    if (DT.Rows != null)
                    {
                        foreach (DataRow RW in DT.Rows)
                        {
                            ResSTR += "<tr>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[0].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[1].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[3].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[4].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[5].ToString().Trim().Substring(0, 10).Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[6].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[7].ToString().Trim() + "</td>";
                            switch (RW[10].ToString().Trim())
                            {
                                case "1":
                                    {
                                        ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">";
                                        ResSTR += "<div class=\"badge badge-pill badge-light-warning\" style=\"width:80px\">" + RW[8].ToString().Trim() + "</div>";
                                        ResSTR += "</td>";
                                        break;
                                    }
                                case "2":
                                    {
                                        ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">";
                                        ResSTR += "<div class=\"badge badge-pill badge-light-info\" style=\"width:80px\">" + RW[8].ToString().Trim() + "</div>";
                                        ResSTR += "</td>";
                                        break;
                                    }
                                case "3":
                                    {
                                        ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">";
                                        ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:80px\">" + RW[8].ToString().Trim() + "</div>";
                                        ResSTR += "</td>";
                                        break;
                                    }
                                case "4":
                                    {
                                        ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">";
                                        ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:80px\">" + RW[8].ToString().Trim() + "</div>";
                                        ResSTR += "</td>";
                                        break;
                                    }
                            }
                            ResSTR += "</tr>";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading API user transactions information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        //====================================================================================================================
        [HttpGet]
        public ActionResult APIU_Transaction()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 130;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" }); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" }); }
                string TranID = Url.RequestContext.RouteData.Values["id"].ToString().Trim();
                DataTable DT_Tran = new DataTable();
                DT_Tran = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Users_15_API_Transaction Where (UID = '" + TranID + "') And (Removed = '0') And (User_Type_Code = '3') And (Message_Error = '0') And (Insert_API_Code = '1')");
                if (DT_Tran.Rows != null)
                {
                    if (DT_Tran.Rows.Count == 1)
                    {
                        if (DT_Tran.Rows[0][31].ToString().Trim().ToLower() == "false")
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Seen_Admin_Flag] = '1',[Seen_Admin_ID] = '" + UID + "',[Seen_Admin_Date] = '" + InsDate + "',[Seen_Admin_Time] = '" + InsTime + "' Where (ID = '" + DT_Tran.Rows[0][0].ToString().Trim() + "')");
                        }
                        ViewBag.DT_Tran = DT_Tran.Rows[0];
                        DataTable DT_User = new DataTable();
                        DT_User = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Users_02_SingleUser Where (ID = '" + DT_Tran.Rows[0][2].ToString().Trim() + "') And (User_GroupType_Code = '3')");
                        ViewBag.DT_User = DT_User.Rows[0];
                        ViewBag.TID = DT_Tran.Rows[0][0].ToString().Trim();
                        return View();
                    }
                    else
                    {
                        return RedirectToAction("APIU", "Transaction", new { id = "", area = "ManagementPortal" });
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" });
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" });
            }
        }

        //====================================================================================================================
        [HttpGet]
        public ActionResult APIUErr()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 131;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" }); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" });
            }
        }

        [HttpPost]
        public JsonResult APIUErr_Grid()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 131;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select API_UID,User_Fullname,User_Type_Code,API_Name,API_Type_Name,Ins_Date,Ins_Time,Request_IP,Message_Code From Users_15_API_Transaction_V Where (Message_Error = '1') And (Removed = '0') And (Insert_API_Code <> '1')");
                    if (DT.Rows != null)
                    {
                        foreach (DataRow RW in DT.Rows)
                        {
                            ResSTR += "<tr>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[0].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[1].ToString().Trim() + "</td>";

                            switch (RW[2].ToString().Trim())
                            {
                                case "":
                                    {
                                        ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">Unknown</td>";
                                        break;
                                    }
                                case "0":
                                    {
                                        ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">Unknown</td>";
                                        break;
                                    }
                                case "1":
                                    {
                                        ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">Hospitalitu</td>";
                                        break;
                                    }
                                case "2":
                                    {
                                        ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">Automative</td>";
                                        break;
                                    }
                                case "3":
                                    {
                                        ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">API User</td>";
                                        break;
                                    }
                            }
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[3].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[4].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[5].ToString().Trim().Substring(0, 10).Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[6].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[7].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[8].ToString().Trim() + "</td>";
                            ResSTR += "</tr>";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading API error transactions information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult APIUErr_New()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 132;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" }); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" });
            }
        }

        [HttpPost]
        public JsonResult APIUErr_New_Grid()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 132;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select API_UID,User_Fullname,User_Type_Code,API_Name,API_Type_Name,Ins_Date,Ins_Time,Request_IP,Message_Code From Users_15_API_Transaction_V Where (Message_Error = '1') And (Removed = '0') And (Insert_API_Code <> '1') And (Seen_Admin_Flag = '0')");
                    if (DT.Rows != null)
                    {
                        foreach (DataRow RW in DT.Rows)
                        {
                            ResSTR += "<tr>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[0].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[1].ToString().Trim() + "</td>";

                            switch (RW[2].ToString().Trim())
                            {
                                case "":
                                    {
                                        ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">Unknown</td>";
                                        break;
                                    }
                                case "0":
                                    {
                                        ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">Unknown</td>";
                                        break;
                                    }
                                case "1":
                                    {
                                        ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">Hospitalitu</td>";
                                        break;
                                    }
                                case "2":
                                    {
                                        ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">Automative</td>";
                                        break;
                                    }
                                case "3":
                                    {
                                        ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">API User</td>";
                                        break;
                                    }
                            }
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[3].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[4].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[5].ToString().Trim().Substring(0, 10).Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[6].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[7].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIU_Application_Show('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer;text-align:center\">" + RW[8].ToString().Trim() + "</td>";
                            ResSTR += "</tr>";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading API error new transactions information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult APIUErr_Information(string TID)
        {
            try
            {
                string ResVal = "0"; string ResSTR = ""; string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    string UType = "";
                    string APIType = "";
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Users_15_API_Transaction_V2 Where (UID = '" + TID.Trim() + "') And (Message_Error = '1') And (Removed = '0') And (Insert_API_Code <> '1')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            if (DT.Rows[0][27].ToString().Trim().ToLower() == "false")
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Seen_Admin_Flag] = '1',[Seen_Admin_ID] = '" + UID + "',[Seen_Admin_Date] = '" + Sq.Sql_Date() + "',[Seen_Admin_Time] = '" + Sq.Sql_Time() + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "')");
                            }
                            ResSTR = "";
                            ResSTR += "<div class=\"alert alert-dark\" role=\"alert\">";
                            ResSTR += "<i class=\"fa fa-arrow-circle-o-right mr-1 align-middle\"></i>";
                            ResSTR += "<span>";
                            ResSTR += "Transaction ID : " + DT.Rows[0][1].ToString().Trim();
                            ResSTR += "</span>";
                            ResSTR += "</div>";
                            ResSTR += "<div class=\"col-lg-12 form-inline\">";
                            ResSTR += "<div class=\"col-lg-6\"><span class=\"text-bold-500 text-dark\">User : </span>" + DT.Rows[0][3].ToString().Trim() + "</div>";
                            UType = "Unknown";
                            if (DT.Rows[0][4].ToString().Trim() == "1") { UType = "Hospitality"; }
                            if (DT.Rows[0][4].ToString().Trim() == "2") { UType = "Automative"; }
                            if (DT.Rows[0][4].ToString().Trim() == "3") { UType = "API User"; }
                            ResSTR += "<div class=\"col-lg-6\"><span class=\"text-bold-500 text-dark\">User Type : </span>" + UType + "</div>";
                            ResSTR += "<div class=\"col-lg-6\"><span class=\"text-bold-500 text-dark\">API Name : </span>" + DT.Rows[0][5].ToString().Trim() + "</div>";
                            ResSTR += "<div class=\"col-lg-6\"><span class=\"text-bold-500 text-dark\">API Type : </span>" + DT.Rows[0][7].ToString().Trim() + "</div>";
                            ResSTR += "<hr style=\"width:100%\"/>";
                            try
                            {
                                ResSTR += "<div class=\"col-lg-3\"><span class=\"text-bold-500 text-dark\">Date : </span>" + DT.Rows[0][8].ToString().Trim().Substring(0, 10) + "</div>";
                            }
                            catch (Exception)
                            {
                                ResSTR += "<div class=\"col-lg-3\"><span class=\"text-bold-500 text-dark\">Date : </span>Unknown</div>";
                            }
                            ResSTR += "<div class=\"col-lg-3\"><span class=\"text-bold-500 text-dark\">Time : </span>" + DT.Rows[0][9].ToString().Trim() + "</div>";
                            ResSTR += "<div class=\"col-lg-6\"><span class=\"text-bold-500 text-dark\">IP : </span>" + DT.Rows[0][10].ToString().Trim() + "</div>";
                            ResSTR += "<hr style=\"width:100%\"/>";
                            ResSTR += "<div class=\"col-lg-12\"><span class=\"text-bold-500 text-dark\">Error Code : </span>" + DT.Rows[0][11].ToString().Trim() + "</div>";
                            ResSTR += "<div class=\"col-lg-12\"><span class=\"text-bold-500 text-dark\">Description : </span>" + DT.Rows[0][13].ToString().Trim() + "</div>";
                            ResSTR += "<hr style=\"width:100%\"/>";
                            APIType = "Unknown";
                            if (DT.Rows[0][26].ToString().Trim() == "1") { APIType = "Not valid"; }
                            if (DT.Rows[0][26].ToString().Trim() == "2") { APIType = "Request API"; }
                            if (DT.Rows[0][26].ToString().Trim() == "3") { APIType = "Result API"; }
                            if (DT.Rows[0][26].ToString().Trim() == "4") { APIType = "Result Auto"; }
                            if (DT.Rows[0][26].ToString().Trim() == "5") { APIType = "Files"; }
                            ResSTR += "<div class=\"col-lg-4\"><span class=\"text-bold-500 text-dark\">Activity : </span>" + APIType + "</div>";
                            ResSTR += "<div class=\"col-lg-4\"><span class=\"text-bold-500 text-dark\">Date Format : </span>" + DT.Rows[0][14].ToString().Trim() + "</div>";
                            ResSTR += "<div class=\"col-lg-4\"><span class=\"text-bold-500 text-dark\">Capture Code : </span>" + DT.Rows[0][15].ToString().Trim() + "</div>";
                            ResSTR += "<div class=\"col-lg-4\"><span class=\"text-bold-500 text-dark\">Capture Name : </span>" + DT.Rows[0][16].ToString().Trim() + "</div>";
                            ResSTR += "<div class=\"col-lg-4\"><span class=\"text-bold-500 text-dark\">Cropping Code : </span>" + DT.Rows[0][17].ToString().Trim() + "</div>";
                            ResSTR += "<div class=\"col-lg-4\"><span class=\"text-bold-500 text-dark\">Cropping Name : </span>" + DT.Rows[0][18].ToString().Trim() + "</div>";
                            if (DT.Rows[0][19].ToString().Trim().ToLower() == "true")
                            {
                                ResSTR += "<div class=\"col-lg-4\"><span class=\"text-bold-500 text-dark\">OCR : </span>Yes</div>";
                            }
                            else
                            {
                                ResSTR += "<div class=\"col-lg-4\"><span class=\"text-bold-500 text-dark\">OCR : </span>No</div>";
                            }
                            if (DT.Rows[0][20].ToString().Trim().ToLower() == "true")
                            {
                                ResSTR += "<div class=\"col-lg-4\"><span class=\"text-bold-500 text-dark\">Validation : </span>Yes</div>";
                            }
                            else
                            {
                                ResSTR += "<div class=\"col-lg-4\"><span class=\"text-bold-500 text-dark\">Validation : </span>No</div>";
                            }
                            if (DT.Rows[0][21].ToString().Trim().ToLower() == "true")
                            {
                                ResSTR += "<div class=\"col-lg-4\"><span class=\"text-bold-500 text-dark\">Callback : </span>Yes</div>";
                            }
                            else
                            {
                                ResSTR += "<div class=\"col-lg-4\"><span class=\"text-bold-500 text-dark\">Callback : </span>No</div>";
                            }
                            ResSTR += "<div class=\"col-lg-12\"><span class=\"text-bold-500 text-dark\">Callback URL : </span>" + DT.Rows[0][22].ToString().Trim() + "</div>";
                            ResSTR += "<hr style=\"width:100%\"/>";
                            ResSTR += "<div class=\"col-lg-12\"><span class=\"text-bold-500 text-dark\">Message : </span></div>";
                            ResSTR += "<div class=\"col-lg-12\">";
                            try
                            {
                                string[] ResMes = DT.Rows[0][25].ToString().Trim().Split('$');
                                foreach(string RMS in ResMes)
                                {
                                    if(RMS.Trim()!="")
                                    {
                                        ResSTR += "- " + RMS.Trim() + "<br />";
                                    }
                                }
                            }
                            catch (Exception) { }
                            ResSTR += "</div>";
                            ResSTR += "</div>";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The server encountered an error while get API user error transactions information";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while get API user error transactions information";
                    }
                }
                IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                IList<SelectListItem> FeedBack = new List<SelectListItem>
                { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
                return Json(FeedBack, JsonRequestBehavior.AllowGet);
            }
        }
        //====================================================================================================================
    }
}