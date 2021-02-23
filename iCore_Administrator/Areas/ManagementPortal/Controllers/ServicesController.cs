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
    public class ServicesController : Controller
    {
        //====================================================================================================================
        AuthenticationTester AAuth = new AuthenticationTester();
        SQL_Tranceiver Sq = new SQL_Tranceiver();
        PublicFunctions Pb = new PublicFunctions();
        Crypto Cry = new Crypto();
        //====================================================================================================================
        public ActionResult Index() { return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" }); }
        //====================================================================================================================
        // Device Management :
        // Type :
        // 1 : Device Master  
        // 2 : Device Group
        //-------------------------------------
        // Status :
        // 0 : Disabled
        // 1 : Active
        //-------------------------------------
        // Removed :
        // 0 : No and show to table
        // 1 : Yes and dont show
        //-------------------------------------
        // Device :
        public ActionResult Devices_Management()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 52;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" }); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                bool GroupTreeRemoved = false;
                string[] Breadcrumb_List_Name = new string[6];
                string[] Breadcrumb_List_Code = new string[6];
                int Breadcrumb_Count = 0;
                for (int i = 0; i < 6; i++) { Breadcrumb_List_Name[i] = ""; Breadcrumb_List_Code[i] = ""; }
                ViewBag.Breadcrumb_Count = 0;
                ViewBag.Breadcrumb_ListName = Breadcrumb_List_Name;
                ViewBag.Breadcrumb_ListCode = Breadcrumb_List_Code;
                string Parent_ID = "0";
                string Parent_IDUC = "0";
                try { Parent_IDUC = Url.RequestContext.RouteData.Values["id"].ToString().Trim(); } catch (Exception) { }
                if (Parent_IDUC.Trim() == "") { Parent_IDUC = "0"; }
                Parent_IDUC = Parent_IDUC.Trim();
                if (Parent_IDUC != "0")
                {
                    try
                    {
                        DataTable DTConvert = new DataTable();
                        DTConvert = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,ID_UnicCode,Name,Parent_ID From Services_01_Devices_Group Where (ID_UnicCode = '" + Parent_IDUC + "') And (Removed = '0')");
                        if (DTConvert.Rows != null)
                        {
                            if (DTConvert.Rows.Count == 1)
                            {
                                Parent_ID = DTConvert.Rows[0][0].ToString().Trim();
                                Parent_IDUC = DTConvert.Rows[0][1].ToString().Trim();
                                bool ExitWhile = false;
                                Breadcrumb_Count++;
                                Breadcrumb_List_Code[Breadcrumb_Count - 1] = DTConvert.Rows[0][1].ToString().Trim();
                                Breadcrumb_List_Name[Breadcrumb_Count - 1] = DTConvert.Rows[0][2].ToString().Trim();
                                string PrntID = DTConvert.Rows[0][3].ToString().Trim();
                                if (PrntID == "0") { ExitWhile = true; }
                                while (ExitWhile == false)
                                {
                                    if (PrntID.Trim() != "0")
                                    {
                                        DataTable DT_Breadcrumb = new DataTable();
                                        DT_Breadcrumb = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID_UnicCode,Name,Parent_ID,Removed From Services_01_Devices_Group Where (ID = '" + PrntID + "') And (Removed = '0')");
                                        if (DT_Breadcrumb.Rows[0][3].ToString().Trim().ToLower() == "false")
                                        {
                                            PrntID = DT_Breadcrumb.Rows[0][2].ToString().Trim();
                                            Breadcrumb_Count++;
                                            if (Breadcrumb_Count > 6) { Breadcrumb_Count = 6; }
                                            Breadcrumb_List_Code[Breadcrumb_Count - 1] = DT_Breadcrumb.Rows[0][0].ToString().Trim();
                                            Breadcrumb_List_Name[Breadcrumb_Count - 1] = DT_Breadcrumb.Rows[0][1].ToString().Trim();
                                        }
                                        else
                                        {
                                            GroupTreeRemoved = true;
                                        }
                                    }
                                    if (GroupTreeRemoved == true) { ExitWhile = true; }
                                    if (PrntID == "0") { ExitWhile = true; }
                                }
                                if (Breadcrumb_Count > 6) { Breadcrumb_Count = 6; }
                                ViewBag.Breadcrumb_Count = Breadcrumb_Count;
                            }
                            else
                            {
                                return RedirectToAction("Devices_Management", "Services", new { id = "", area = "ManagementPortal" });
                            }
                        }
                        else
                        {
                            return RedirectToAction("Devices_Management", "Services", new { id = "", area = "ManagementPortal" });
                        }
                    }
                    catch (Exception)
                    {
                        return RedirectToAction("Devices_Management", "Services", new { id = "", area = "ManagementPortal" });
                    }
                }
                if (GroupTreeRemoved == true) { return RedirectToAction("Devices_Management", "Services", new { id = "", area = "ManagementPortal" }); }
                DataTable DT = new DataTable();
                DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,ID_UnicCode,Name,Description,Status_Code,UnicID_Forced,Inventories_Forced From Services_01_Devices_Group Where (Parent_ID = '" + Parent_ID + "') And (Removed = '0') Order By Name,Ins_Date,Ins_Time");
                ViewBag.DT = DT.Rows;
                DataTable DT_DeviceList = new DataTable();
                DT_DeviceList = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Name,Master_Unic_ID,Model,Product_ID,Serial_Number,Build_Series_Number,Version,Situation_Code,Description,Extra_Unic_ID_1,Extra_Unic_ID_2,Extra_Unic_ID_3,Extra_Unic_ID_4,Status_Code,Situation_Text From Services_03_Devices_List Where (Group_ID = '" + Parent_ID + "') And (Removed = '0') Order By Name,Master_Unic_ID,Product_ID,Serial_Number,Build_Series_Number,Version");
                ViewBag.DTDevice = DT_DeviceList.Rows;
                ViewBag.Parent_ID = Parent_ID;
                ViewBag.Parent_IDUC = Parent_IDUC;
                ViewBag.Breadcrumb_ListName = Breadcrumb_List_Name;
                ViewBag.Breadcrumb_ListCode = Breadcrumb_List_Code;
                ViewBag.Breadcrumb_Count = Breadcrumb_Count;
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" });
            }
        }

        [HttpPost]
        public JsonResult Devices_Management_Grid(string PID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 52;
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
                    PID = PID.Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,ID_UnicCode,Name,Description,Status_Code,UnicID_Forced,Inventories_Forced From Services_01_Devices_Group Where (Parent_ID = '" + PID + "') And (Removed = '0') And (Type_Code = '1') Order By Name,Ins_Date,Ins_Time");
                    if (DT.Rows != null)
                    {
                        foreach (DataRow RW in DT.Rows)
                        {
                            ResSTR += "<tr>";
                            ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\"><i class=\"fa fa-hdd-o text-primary\" style=\"font-size:25px\"></i></td>";
                            ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[2].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[3].ToString().Trim() + "</td>";
                            if (RW[5].ToString().Trim() == "0")
                            {
                                ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:35px\">No</div>";
                                ResSTR += "</td>";
                            }
                            else
                            {
                                ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:35px\">Yes</div>";
                                ResSTR += "</td>";
                            }
                            if (RW[6].ToString().Trim() == "0")
                            {
                                ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:35px\">No</div>";
                                ResSTR += "</td>";
                            }
                            else
                            {
                                ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:35px\">Yes</div>";
                                ResSTR += "</td>";
                            }
                            if (RW[4].ToString().Trim() == "0")
                            {
                                ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:70px\">Disable</div>";
                                ResSTR += "</td>";
                            }
                            else
                            {
                                ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:70px\">Active</div>";
                                ResSTR += "</td>";
                            }
                            ResSTR += "<td style=\"text-align:center\">";
                            ResSTR += "<div class=\"btn-group dropleft\">";
                            ResSTR += "<button type=\"button\" class=\"btn\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                            ResSTR += "<div class=\"MenuToolbox text-primary\"/>";
                            ResSTR += "</button>";
                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-search text-primary\" style=\"width:24px;font-size:14px\"></i>Device's group</a>";
                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Device_Remove('" + RW[1].ToString().Trim() + "','" + RW[2].ToString().Trim() + "')\"><i class=\"fa fa-trash-o text-primary\" style=\"width:24px;font-size:14px\"></i>Remove device</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Device_ChangeStatus('" + RW[1].ToString().Trim() + "','" + RW[2].ToString().Trim() + "')\"><i class=\"fa fa-refresh text-primary\" style=\"width:24px;font-size:14px\"></i>Change status</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Device_Edit('" + RW[1].ToString().Trim() + "','" + RW[2].ToString().Trim() + "','" + RW[3].ToString().Trim() + "','" + RW[5].ToString().Trim() + "','" + RW[6].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit device</a>";
                            ResSTR += "</div>";
                            ResSTR += "</div>";
                            ResSTR += "</td>";
                            ResSTR += "</tr>";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading devices information";
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
        public JsonResult Device_Addnew(string DName, string DDesc, string DUni, string DInve)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 56;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                string ParentID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                DName = DName.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DDesc = DDesc.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DUni = DUni.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DInve = DInve.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    if (DName == "")
                    {
                        ResVal = "1"; ResSTR = "It is necessary to enter the name to add a new device";
                    }
                    else
                    {
                        DName = Pb.Text_UpperCase_AfterSpase(DName);
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_01_Devices_Group Where (Parent_ID = '" + ParentID + "') And (Name = '" + DName + "') And (Removed = '0')");
                    if (DT.Rows.Count == 0)
                    {
                        string RndKey = Pb.Make_Security_CodeFake(15);
                        long InsCode = Sq.Get_New_ID(DataBase_Selector.Administrator, "Services_01_Devices_Group", "ID_Counter");
                        string InsDate = Sq.Sql_Date();
                        string InsTime = Sq.Sql_Time();
                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Services_01_Devices_Group Values ('" + InsCode.ToString() + "','" + InsCode.ToString() + RndKey + "','" + ParentID + "','1','Device','" + DName + "','" + DDesc + "','" + DUni + "','" + DInve + "','1','Active','" + InsDate + "','" + InsTime + "','" + UID + "','" + InsDate + "','" + InsTime + "','" + UID + "','0')");
                        ResVal = "0"; ResSTR = "Device named " + DName + " was successfully added";
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The entered name of the device is duplicate, so it is not possible for you to add new device with this name";
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
        public JsonResult Device_Remove(string DID, string DName)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 57;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                DID = DID.Trim();
                DName = DName.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_01_Devices_Group Where (ID_UnicCode = '" + DID + "') And (Type_Code = '1') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_01_Devices_Group Set [Removed] = '1',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '1')");
                            ResSTR = "The " + DName + " device was successfully deleted";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified device is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving device information";
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
        public JsonResult Device_ChangeStatus(string DID, string DName)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 58;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                DID = DID.Trim();
                DName = DName.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Status_Code From Services_01_Devices_Group Where (ID_UnicCode = '" + DID + "') And (Type_Code = '1') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            if (DT.Rows[0][1].ToString().Trim() == "1")
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_01_Devices_Group Set [Status_Code] = '0',[Status_Text] = 'Disabled',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '1')");
                                ResSTR = "The " + DName + " device was successfully change status to disabled";
                            }
                            else
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_01_Devices_Group Set [Status_Code] = '1',[Status_Text] = 'Active',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '1')");
                                ResSTR = "The " + DName + " device was successfully change status to active";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified device is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving device information";
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
        public JsonResult Device_SaveEdit(string DID, string DName, string DDesc, string DUni, string DInve)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 59;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                DID = DID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DName = DName.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DDesc = DDesc.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DUni = DUni.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DInve = DInve.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    if (DName == "")
                    {
                        ResVal = "1"; ResSTR = "It is necessary to enter the name to edit device";
                    }
                    else
                    {
                        DName = Pb.Text_UpperCase_AfterSpase(DName);
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Parent_ID From Services_01_Devices_Group Where (ID_UnicCode = '" + DID + "') And (Removed = '0') And (Type_Code = '1')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            DataTable DT2 = new DataTable();
                            DT2 = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_01_Devices_Group Where (ID <> '" + DT.Rows[0][0].ToString().Trim() + "') And (Parent_ID = '" + DT.Rows[0][1].ToString().Trim() + "') And (Name = '" + DName + "') And (Removed = '0')");
                            if (DT2.Rows != null)
                            {
                                if (DT2.Rows.Count == 0)
                                {
                                    string InsDate = Sq.Sql_Date();
                                    string InsTime = Sq.Sql_Time();
                                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_01_Devices_Group Set [Name] = '" + DName + "',[Description] = '" + DDesc + "',UnicID_Forced = '" + DUni + "',Inventories_Forced = '" + DInve + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '1')");
                                    ResVal = "0"; ResSTR = "Device named " + DName + " was successfully edited";
                                }
                                else
                                {
                                    ResVal = "1"; ResSTR = "The entered name of the device is duplicate, so it is not possible for you to edit device with this name";
                                }
                            }
                            else
                            {
                                ResVal = "1"; ResSTR = "The server encountered an error while receiving device information";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified device is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving device information";
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
        public JsonResult Management_Grid(string PID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 52;
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
                    PID = PID.Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,ID_UnicCode,Name,Description,Status_Code,UnicID_Forced,Inventories_Forced From Services_01_Devices_Group Where (Parent_ID = '" + PID + "') And (Removed = '0') And (Type_Code = '2') Order By Name,Ins_Date,Ins_Time");
                    if (DT.Rows != null)
                    {
                        foreach (DataRow RW in DT.Rows)
                        {
                            ResSTR += "<tr>";
                            ResSTR += "<td onclick=\"Group_SubGroupShow('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\"><i class=\"fa fa-object-group text-primary\" style=\"font-size:25px\"></i></td>";
                            ResSTR += "<td onclick=\"Group_SubGroupShow('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[2].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"Group_SubGroupShow('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[3].ToString().Trim() + "</td>";
                            if (RW[4].ToString().Trim() == "0")
                            {
                                ResSTR += "<td onclick=\"Group_SubGroupShow('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:70px\">Disabled</div>";
                                ResSTR += "</td>";
                            }
                            else
                            {
                                ResSTR += "<td onclick=\"Group_SubGroupShow('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:70px\">Active</div>";
                                ResSTR += "</td>";
                            }
                            ResSTR += "<td style=\"text-align:center\">";
                            ResSTR += "<div class=\"btn-group dropleft\">";
                            ResSTR += "<button type=\"button\" class=\"btn\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                            ResSTR += "<div class=\"MenuToolbox text-primary\"/>";
                            ResSTR += "</button>";
                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Group_AddDeviceProperties('" + RW[0].ToString().Trim() + "','" + RW[2].ToString().Trim() + "')\"><i class=\"fa fa-info-circle text-primary\" style=\"width:24px;font-size:14px\"></i>Groups's device Properties</a>";
                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Group_SubGroupShow('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-search text-primary\" style=\"width:24px;font-size:14px\"></i>Show subgroup</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Group_Remove('" + RW[1].ToString().Trim() + "','" + RW[2].ToString().Trim() + "')\"><i class=\"fa fa-trash-o text-primary\" style=\"width:24px;font-size:14px\"></i>Remove group</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Group_ChangeStatus('" + RW[1].ToString().Trim() + "','" + RW[2].ToString().Trim() + "')\"><i class=\"fa fa-refresh text-primary\" style=\"width:24px;font-size:14px\"></i>Change status</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Group_Edit('" + RW[1].ToString().Trim() + "','" + RW[2].ToString().Trim() + "','" + RW[3].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit group</a>";
                            ResSTR += "</div>";
                            ResSTR += "</div>";
                            ResSTR += "</td>";
                            ResSTR += "</tr>";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading group information";
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
        public JsonResult Group_Addnew(string ParentIDUC, string GroupName, string GroupDescription)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 60;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                string ParentID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                ParentIDUC = ParentIDUC.Trim();
                GroupName = GroupName.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GroupDescription = GroupDescription.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    if (ParentIDUC != "0")
                    {
                        DataTable DT_PID = new DataTable();
                        DT_PID = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_01_Devices_Group Where (ID_UnicCode = '" + ParentIDUC + "') And (Removed = '0')");
                        if (DT_PID.Rows != null)
                        {
                            if (DT_PID.Rows.Count == 1)
                            {
                                ParentID = DT_PID.Rows[0][0].ToString().Trim();
                            }
                            else
                            {
                                ResVal = "1"; ResSTR = "Parent group information not identified";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "Parent group information not identified";
                        }
                    }
                    else
                    {
                        ParentID = "0";
                    }
                }
                if (ResVal == "0")
                {
                    if (GroupName == "")
                    {
                        ResVal = "1"; ResSTR = "It is necessary to enter the name to add a new group";
                    }
                    else
                    {
                        GroupName = Pb.Text_UpperCase_AfterSpase(GroupName);
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_01_Devices_Group Where (Parent_ID = '" + ParentID + "') And (Name = '" + GroupName + "') And (Removed = '0')");
                    if (DT.Rows.Count == 0)
                    {
                        string RndKey = Pb.Make_Security_CodeFake(15);
                        long InsCode = Sq.Get_New_ID(DataBase_Selector.Administrator, "Services_01_Devices_Group", "ID_Counter");
                        string InsDate = Sq.Sql_Date();
                        string InsTime = Sq.Sql_Time();
                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Services_01_Devices_Group Values ('" + InsCode.ToString() + "','" + InsCode.ToString() + RndKey + "','" + ParentID + "','2','Group','" + GroupName + "','" + GroupDescription + "','0','0','1','Active','" + InsDate + "','" + InsTime + "','" + UID + "','" + InsDate + "','" + InsTime + "','" + UID + "','0')");
                        ResVal = "0"; ResSTR = "Group named " + GroupName + " was successfully added";
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The entered name of the group is duplicate, so it is not possible for you to add new group with this name";
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
        public JsonResult Group_Remove(string GroupIDU, string GroupName)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 61;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GroupIDU = GroupIDU.Trim();
                GroupName = GroupName.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_01_Devices_Group Where (ID_UnicCode = '" + GroupIDU + "') And (Type_Code = '2') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_01_Devices_Group Set [Removed] = '1',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '2')");
                            ResSTR = "The " + GroupName + " group was successfully deleted";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified group is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving group information";
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
        public JsonResult Group_ChangeStatus(string GroupIDU, string GroupName)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 62;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GroupIDU = GroupIDU.Trim();
                GroupName = GroupName.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Status_Code From Services_01_Devices_Group Where (ID_UnicCode = '" + GroupIDU + "') And (Type_Code = '2') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            if (DT.Rows[0][1].ToString().Trim() == "1")
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_01_Devices_Group Set [Status_Code] = '0',[Status_Text] = 'Disabled',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '2')");
                                ResSTR = "The " + GroupName + " group was successfully change status to disabled";
                            }
                            else
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_01_Devices_Group Set [Status_Code] = '1',[Status_Text] = 'Active',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '2')");
                                ResSTR = "The " + GroupName + " group was successfully change status to active";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified group is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving group information";
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
        public JsonResult Group_SaveEdit(string GroupIDUC, string GroupName, string GroupDescription)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 63;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GroupIDUC = GroupIDUC.Trim();
                GroupName = GroupName.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GroupDescription = GroupDescription.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    if (GroupName == "")
                    {
                        ResVal = "1"; ResSTR = "It is necessary to enter the name to edit group";
                    }
                    else
                    {
                        GroupName = Pb.Text_UpperCase_AfterSpase(GroupName);
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Parent_ID From Services_01_Devices_Group Where (ID_UnicCode = '" + GroupIDUC + "') And (Removed = '0') And (Type_Code = '2')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            DataTable DT2 = new DataTable();
                            DT2 = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_01_Devices_Group Where (ID <> '" + DT.Rows[0][0].ToString().Trim() + "') And (Parent_ID = '" + DT.Rows[0][1].ToString().Trim() + "') And (Name = '" + GroupName + "') And (Removed = '0')");
                            if (DT2.Rows != null)
                            {
                                if (DT2.Rows.Count == 0)
                                {
                                    string InsDate = Sq.Sql_Date();
                                    string InsTime = Sq.Sql_Time();
                                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_01_Devices_Group Set [Name] = '" + GroupName + "',[Description] = '" + GroupDescription + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '2')");
                                    ResVal = "0"; ResSTR = "Group named " + GroupName + " was successfully edited";
                                }
                                else
                                {
                                    ResVal = "1"; ResSTR = "The entered name of the group is duplicate, so it is not possible for you to edit group with this name";
                                }
                            }
                            else
                            {
                                ResVal = "1"; ResSTR = "The server encountered an error while receiving group information";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified group is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving group information";
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
        public JsonResult Group_Properties_Index(string GID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 52;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GID = GID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Max(RowIndex) From Services_02_Devices_GroupProperties Where (Group_ID = '" + GID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            long NowIndex = 0;
                            try { NowIndex = long.Parse(DT.Rows[0][0].ToString().Trim()); } catch (Exception) { }
                            NowIndex++;
                            ResSTR = NowIndex.ToString();
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The server encountered an error while reloading information";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading information";
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
        public JsonResult Group_Properties_Grid(string GID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 52;
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
                    GID = GID.Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Tag,Unit,Description,RowIndex,Width,Status_Code From Services_02_Devices_GroupProperties Where (Group_ID = '" + GID + "') And (Removed = '0') Order By Tag,Ins_Date,Ins_Time");
                    if (DT.Rows != null)
                    {
                        foreach (DataRow RW in DT.Rows)
                        {
                            ResSTR += "<tr>";
                            ResSTR += "<td>" + RW[1].ToString().Trim() + "</td>";
                            ResSTR += "<td>" + RW[2].ToString().Trim() + "</td>";
                            ResSTR += "<td>" + RW[3].ToString().Trim() + "</td>";
                            ResSTR += "<td style=\"width:70px;text-align:center\">" + RW[4].ToString().Trim() + "</td>";
                            ResSTR += "<td style=\"width:70px;text-align:center\">" + RW[5].ToString().Trim() + "</td>";
                            if (RW[6].ToString().Trim() == "0")
                            {
                                ResSTR += "<td style=\"text-align:center\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:70px\">Disabled</div>";
                                ResSTR += "</td>";
                            }
                            else
                            {
                                ResSTR += "<td style=\"text-align:center\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:70px\">Active</div>";
                                ResSTR += "</td>";
                            }
                            ResSTR += "<td style=\"text-align:center\">";
                            ResSTR += "<div class=\"btn-group dropleft\">";
                            ResSTR += "<button type=\"button\" class=\"btn\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                            ResSTR += "<div class=\"MenuToolbox text-primary\"/>";
                            ResSTR += "</button>";
                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"GroupProperties_Remove('" + RW[0].ToString().Trim() + "','" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-trash-o text-primary\" style=\"width:24px;font-size:14px\"></i>Remove properties</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"GroupProperties_ChangeStatus('" + RW[0].ToString().Trim() + "','" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-refresh text-primary\" style=\"width:24px;font-size:14px\"></i>Change properties status</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"GroupProperties_Edit('" + RW[0].ToString().Trim() + "','" + RW[1].ToString().Trim() + "','" + RW[2].ToString().Trim() + "','" + RW[3].ToString().Trim() + "','" + RW[4].ToString().Trim() + "','" + RW[5].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit properties</a>";
                            ResSTR += "</div>";
                            ResSTR += "</div>";
                            ResSTR += "</td>";
                            ResSTR += "</tr>";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading group properties information";
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
        public JsonResult Group_Properties_Addnew(string GID, string GTA, string GUN, string GDC, string GIN, string GWD)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 64;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GID = GID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GTA = GTA.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GUN = GUN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GDC = GDC.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GIN = GIN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GWD = GWD.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT_PID = new DataTable();
                    DT_PID = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_01_Devices_Group Where (ID = '" + GID + "') And (Removed = '0')");
                    if (DT_PID.Rows != null)
                    {
                        if (DT_PID.Rows.Count != 1)
                        {
                            ResVal = "1"; ResSTR = "Base group information not identified";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "Base group information not identified";
                    }
                }
                if (ResVal == "0")
                {
                    if (GTA == "")
                    {
                        ResVal = "1"; ResSTR = "It is necessary to enter the tag to add a new properties";
                    }
                    else
                    {
                        GTA = Pb.Text_UpperCase_AfterSpase(GTA);
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_02_Devices_GroupProperties Where (Group_ID = '" + GID + "') And (Tag = '" + GTA + "') And (Removed = '0')");
                    if (DT.Rows.Count == 0)
                    {
                        string InsDate = Sq.Sql_Date();
                        string InsTime = Sq.Sql_Time();
                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Services_02_Devices_GroupProperties Values ('" + GID.ToString() + "','" + GTA + "','" + GUN + "','" + GDC + "','" + GIN + "','" + GWD + "','1','Active','" + InsDate + "','" + InsTime + "','" + UID + "','" + InsDate + "','" + InsTime + "','" + UID + "','0')");
                        ResVal = "0"; ResSTR = "Properties tag named " + GTA + " was successfully added";
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The entered tag of the properties is duplicate, so it is not possible for you to add new properties with this tag";
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
        public JsonResult Group_Properties_Remove(string GID, string PID, string PTA)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 65;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GID = GID.Trim();
                PID = PID.Trim();
                PTA = PTA.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_02_Devices_GroupProperties Where (Group_ID = '" + GID + "') And (ID = '" + PID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_02_Devices_GroupProperties Set [Removed] = '1',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GID + "') And (ID = '" + PID + "')");
                            ResSTR = "The " + PTA + " properties was successfully deleted";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified properties is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving properties information";
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
        public JsonResult Group_Properties_ChangeStatus(string GID, string PID, string PTA)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 66;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GID = GID.Trim();
                PID = PID.Trim();
                PTA = PTA.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Status_Code From Services_02_Devices_GroupProperties Where (Group_ID = '" + GID + "') And (ID = '" + PID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            if (DT.Rows[0][1].ToString().Trim() == "1")
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_02_Devices_GroupProperties Set [Status_Code] = '0',[Status_Text] = 'Disabled',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GID + "') And (ID = '" + PID + "')");
                                ResSTR = "The " + PTA + " properties was successfully change status to disabled";
                            }
                            else
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_02_Devices_GroupProperties Set [Status_Code] = '1',[Status_Text] = 'Active',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GID + "') And (ID = '" + PID + "')");
                                ResSTR = "The " + PTA + " properties was successfully change status to active";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified properties is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving properties information";
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
        public JsonResult Group_Properties_SaveEdit(string GID, string PID, string GTA, string GUN, string GDC, string GIN, string GWD)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 67;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GID = GID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                PID = PID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GTA = GTA.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GUN = GUN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GDC = GDC.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GIN = GIN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GWD = GWD.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT_PID = new DataTable();
                    DT_PID = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_01_Devices_Group Where (ID = '" + GID + "') And (Removed = '0')");
                    if (DT_PID.Rows != null)
                    {
                        if (DT_PID.Rows.Count != 1)
                        {
                            ResVal = "1"; ResSTR = "Base group information not identified";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "Base group information not identified";
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT_PID2 = new DataTable();
                    DT_PID2 = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_02_Devices_GroupProperties Where (Group_ID = '" + GID + "') And (ID = '" + PID + "') And (Removed = '0')");
                    if (DT_PID2.Rows != null)
                    {
                        if (DT_PID2.Rows.Count != 1)
                        {
                            ResVal = "1"; ResSTR = "Properties information not identified";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "Properties information not identified";
                    }
                }
                if (ResVal == "0")
                {
                    if (GTA == "")
                    {
                        ResVal = "1"; ResSTR = "It is necessary to enter the tag to save properties changes";
                    }
                    else
                    {
                        GTA = Pb.Text_UpperCase_AfterSpase(GTA);
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_02_Devices_GroupProperties Where (Group_ID = '" + GID + "') And (ID <> '" + PID + "') And (Tag = '" + GTA + "') And (Removed = '0')");
                    if (DT.Rows.Count == 0)
                    {
                        string InsDate = Sq.Sql_Date();
                        string InsTime = Sq.Sql_Time();
                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_02_Devices_GroupProperties Set [Tag] = '" + GTA + "',[Unit] = '" + GUN + "',[Description] = '" + GDC + "',[RowIndex] = '" + GIN + "',[Width] = '" + GWD + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GID + "') And (ID = '" + PID + "') And (Removed = '0')");
                        ResVal = "0"; ResSTR = "Properties tag named " + GTA + " was successfully edited";
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The entered tag of the properties is duplicate, so it is not possible for you to save properties changes with this tag";
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
        public JsonResult DeviceAdd_Grid(string GID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 52;
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
                    GID = GID.Trim();
                    DataTable DTUIDTID = new DataTable();
                    DTUIDTID = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_01_Devices_Group Where (ID_UnicCode = '" + GID + "') And (Removed = '0')");
                    GID = DTUIDTID.Rows[0][0].ToString().Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Name,Master_Unic_ID,Model,Product_ID,Serial_Number,Build_Series_Number,Version,Situation_Code,Description,Extra_Unic_ID_1,Extra_Unic_ID_2,Extra_Unic_ID_3,Extra_Unic_ID_4,Status_Code,Situation_Text From Services_03_Devices_List Where (Group_ID = '" + GID + "') And (Removed = '0') Order By Name,Master_Unic_ID,Product_ID,Serial_Number,Build_Series_Number,Version");
                    if (DT.Rows != null)
                    {
                        string STT = "badge-light-danger";
                        foreach (DataRow RW in DT.Rows)
                        {
                            ResSTR += "<tr>";
                            ResSTR += "<td onclick=\"DeviceAdd_ShowInfo('" + RW[0].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\"><i class=\"fa fa-desktop text-primary\" style=\"font-size:25px\"></i></td>";
                            ResSTR += "<td onclick=\"DeviceAdd_ShowInfo('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[1].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"DeviceAdd_ShowInfo('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[2].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"DeviceAdd_ShowInfo('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[3].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"DeviceAdd_ShowInfo('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[4].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"DeviceAdd_ShowInfo('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[5].ToString().Trim() + "</td>";
                            switch (RW[8].ToString().Trim())
                            {
                                case "1": { STT = "badge-light-primary"; break; }
                                case "2": { STT = "badge-light-danger"; break; }
                                case "3": { STT = "badge-light-success"; break; }
                                case "4": { STT = "badge-light-warning"; break; }
                                case "5": { STT = "badge-light-warning"; break; }
                                case "6": { STT = "badge-light-primary"; break; }
                                case "7": { STT = "badge-light-info"; break; }
                                case "8": { STT = "badge-light-danger"; break; }
                                case "9": { STT = "badge-light-success"; break; }
                                case "10": { STT = "badge-light-danger"; break; }
                                case "11": { STT = "badge-light-info"; break; }
                                case "12": { STT = "badge-light-warning"; break; }
                                case "13": { STT = "badge-light-warning"; break; }
                                case "14": { STT = "badge-light-warning"; break; }
                                case "15": { STT = "badge-light-danger"; break; }
                                case "16": { STT = "badge-light-danger"; break; }
                                case "17": { STT = "badge-light-danger"; break; }
                                case "18": { STT = "badge-light-info"; break; }
                                case "19": { STT = "badge-light-warning"; break; }
                                case "20": { STT = "badge-light-warning"; break; }
                                case "21": { STT = "badge-light-info"; break; }
                                case "22": { STT = "badge-light-warning"; break; }
                                case "23": { STT = "badge-light-warning"; break; }
                                case "24": { STT = "badge-light-info"; break; }
                                case "25": { STT = "badge-light-danger"; break; }
                                case "26": { STT = "badge-light-info"; break; }
                                case "27": { STT = "badge-light-info"; break; }
                                case "28": { STT = "badge-light-danger"; break; }
                            };
                            ResSTR += "<td onclick=\"DeviceAdd_ShowInfo('" + RW[0].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                            ResSTR += "<div class=\"badge badge-pill " + STT + "\">" + RW[15].ToString().Trim() + "</div>";
                            ResSTR += "</td>";
                            if (RW[14].ToString().Trim() == "0")
                            {
                                ResSTR += "<td onclick=\"DeviceAdd_ShowInfo('" + RW[0].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:70px\">Disabled</div>";
                                ResSTR += "</td>";
                            }
                            else
                            {
                                ResSTR += "<td onclick=\"DeviceAdd_ShowInfo('" + RW[0].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:70px\">Active</div>";
                                ResSTR += "</td>";
                            }
                            ResSTR += "<td style=\"text-align:center\">";
                            ResSTR += "<div class=\"btn-group dropleft\">";
                            ResSTR += "<button type=\"button\" class=\"btn\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                            ResSTR += "<div class=\"MenuToolbox text-primary\"/>";
                            ResSTR += "</button>";
                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"DeviceAdd_Properties('" + RW[0].ToString().Trim() + "','" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-info-circle text-primary\" style=\"width:24px;font-size:14px\"></i>Device Properties</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"DeviceAdd_Comments('" + RW[0].ToString().Trim() + "','" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-sticky-note text-primary\" style=\"width:24px;font-size:14px\"></i>Activity Comments</a>";
                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"DeviceAdd_Remove('" + RW[0].ToString().Trim() + "','" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-trash-o text-primary\" style=\"width:24px;font-size:14px\"></i>Remove device</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"DeviceAdd_ChangeStatus('" + RW[0].ToString().Trim() + "','" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-refresh text-primary\" style=\"width:24px;font-size:14px\"></i>Change status</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"DeviceAdd_Edit('" + RW[0].ToString().Trim() + "','" + RW[1].ToString().Trim() + "','" + RW[2].ToString().Trim() + "','" + RW[3].ToString().Trim() + "','" + RW[4].ToString().Trim() + "','" + RW[5].ToString().Trim() + "','" + RW[6].ToString().Trim() + "','" + RW[7].ToString().Trim() + "','" + RW[8].ToString().Trim() + "','" + RW[9].ToString().Trim() + "','" + RW[10].ToString().Trim() + "','" + RW[11].ToString().Trim() + "','" + RW[12].ToString().Trim() + "','" + RW[13].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit device</a>";
                            ResSTR += "</div>";
                            ResSTR += "</div>";
                            ResSTR += "</td>";
                            ResSTR += "</tr>";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading group properties information";
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
        public JsonResult DeviceAdd_AddNew(string A1, string A2, string A3, string A4, string A5, string A6, string A7, string A8, string A9, string A10, string A11, string A12, string A13, string A14, string A15)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 68;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                string Group_ID = "0";
                string Header_ID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                A1 = A1.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A2 = A2.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A3 = A3.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A4 = A4.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A5 = A5.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A6 = A6.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A7 = A7.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A8 = A8.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A9 = A9.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A10 = A10.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A11 = A11.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A12 = A12.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A13 = A13.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A14 = A14.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A15 = A15.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    if (A2 == "")
                    {
                        ResVal = "1"; ResSTR = "It is necessary to enter the tag name / unic key to add a new device";
                    }
                    else
                    {
                        A2 = Pb.Text_UpperCase_AfterSpase(A2);
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT_PID = new DataTable();
                    DT_PID = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_01_Devices_Group Where (ID_UnicCode = '" + A1 + "') And (Removed = '0')");
                    if (DT_PID.Rows != null)
                    {
                        if (DT_PID.Rows.Count != 1)
                        {
                            ResVal = "1"; ResSTR = "Base group information not identified";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "Base group information not identified";
                    }
                    if (ResVal == "0") { Group_ID = DT_PID.Rows[0][0].ToString().Trim(); }
                }
                // Header ID :
                if (ResVal == "0")
                {
                    Header_ID = Group_ID;
                    bool ExiitWN = false;
                    while (ExiitWN == false)
                    {
                        DataTable DT_HDG = new DataTable();
                        DT_HDG = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Parent_ID,ID From Services_01_Devices_Group Where (ID = '" + Header_ID + "') And (Removed = '0')");
                        if (DT_HDG.Rows != null)
                        {
                            if (DT_HDG.Rows.Count == 1)
                            {
                                Header_ID = DT_HDG.Rows[0][0].ToString().Trim();
                                if (Header_ID.Trim() == "0")
                                {
                                    Header_ID = DT_HDG.Rows[0][1].ToString().Trim();
                                    ExiitWN = true;
                                }
                            }
                            else
                            {
                                ResVal = "1"; ResSTR = "Header device group information not identified"; ExiitWN = true;
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "Header device group information not identified"; ExiitWN = true;
                        }
                    }
                }
                // ID Duplicated Values :
                if (ResVal == "0")
                {
                    if (A3 != "")
                    {
                        if (A3 == A12) { ResVal = "1"; ResSTR = "The value of master unic ID overlaps with ext unic ID #1"; }
                        if (A3 == A13) { ResVal = "1"; ResSTR = "The value of master unic ID overlaps with ext unic ID #2"; }
                        if (A3 == A14) { ResVal = "1"; ResSTR = "The value of master unic ID overlaps with ext unic ID #3"; }
                        if (A3 == A15) { ResVal = "1"; ResSTR = "The value of master unic ID overlaps with ext unic ID #4"; }
                    }
                    if (ResVal == "0")
                    {
                        if (A12 != "")
                        {
                            if (A12 == A3) { ResVal = "1"; ResSTR = "The value of ext unic ID #1 overlaps with master unic ID"; }
                            if (A12 == A13) { ResVal = "1"; ResSTR = "The value of ext unic ID #1  overlaps with ext unic ID #2"; }
                            if (A12 == A14) { ResVal = "1"; ResSTR = "The value of ext unic ID #1 overlaps with ext unic ID #3"; }
                            if (A12 == A15) { ResVal = "1"; ResSTR = "The value of ext unic ID #1 overlaps with ext unic ID #4"; }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A13 != "")
                        {
                            if (A13 == A3) { ResVal = "1"; ResSTR = "The value of ext unic ID #2 overlaps with master unic ID"; }
                            if (A13 == A12) { ResVal = "1"; ResSTR = "The value of ext unic ID #2  overlaps with ext unic ID #1"; }
                            if (A13 == A14) { ResVal = "1"; ResSTR = "The value of ext unic ID #2 overlaps with ext unic ID #3"; }
                            if (A13 == A15) { ResVal = "1"; ResSTR = "The value of ext unic ID #2 overlaps with ext unic ID #4"; }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A14 != "")
                        {
                            if (A14 == A3) { ResVal = "1"; ResSTR = "The value of ext unic ID #3 overlaps with master unic ID"; }
                            if (A14 == A12) { ResVal = "1"; ResSTR = "The value of ext unic ID #3  overlaps with ext unic ID #1"; }
                            if (A14 == A13) { ResVal = "1"; ResSTR = "The value of ext unic ID #3 overlaps with ext unic ID #2"; }
                            if (A14 == A15) { ResVal = "1"; ResSTR = "The value of ext unic ID #3 overlaps with ext unic ID #4"; }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A15 != "")
                        {
                            if (A15 == A3) { ResVal = "1"; ResSTR = "The value of ext unic ID #4 overlaps with master unic ID"; }
                            if (A15 == A12) { ResVal = "1"; ResSTR = "The value of ext unic ID #4  overlaps with ext unic ID #1"; }
                            if (A15 == A13) { ResVal = "1"; ResSTR = "The value of ext unic ID #4 overlaps with ext unic ID #2"; }
                            if (A15 == A14) { ResVal = "1"; ResSTR = "The value of ext unic ID #4 overlaps with ext unic ID #3"; }
                        }
                    }
                }
                // ID Duplicated SQL :
                if (ResVal == "0")
                {
                    if (A3 != "")
                    {
                        try
                        {
                            DataTable DTUM = new DataTable();
                            DTUM = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_03_Devices_List Where (Master_Unic_ID = '" + A3 + "') or (Extra_Unic_ID_1 = '" + A3 + "') or (Extra_Unic_ID_2 = '" + A3 + "') or (Extra_Unic_ID_3 = '" + A3 + "') or (Extra_Unic_ID_4 = '" + A3 + "') And (Removed = '0')");
                            if (DTUM.Rows.Count != 0) { ResVal = "1"; ResSTR = "The master unic ID value is duplicated"; }
                        }
                        catch (Exception) { }
                    }
                    if (ResVal == "0")
                    {
                        if (A12 != "")
                        {
                            try
                            {
                                DataTable DTUM = new DataTable();
                                DTUM = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_03_Devices_List Where (Master_Unic_ID = '" + A12 + "') or (Extra_Unic_ID_1 = '" + A12 + "') or (Extra_Unic_ID_2 = '" + A12 + "') or (Extra_Unic_ID_3 = '" + A12 + "') or (Extra_Unic_ID_4 = '" + A12 + "') And (Removed = '0')");
                                if (DTUM.Rows.Count != 0) { ResVal = "1"; ResSTR = "The ext unic ID #1 value is duplicated"; }
                            }
                            catch (Exception) { }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A13 != "")
                        {
                            try
                            {
                                DataTable DTUM = new DataTable();
                                DTUM = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_03_Devices_List Where (Master_Unic_ID = '" + A13 + "') or (Extra_Unic_ID_1 = '" + A13 + "') or (Extra_Unic_ID_2 = '" + A13 + "') or (Extra_Unic_ID_3 = '" + A13 + "') or (Extra_Unic_ID_4 = '" + A13 + "') And (Removed = '0')");
                                if (DTUM.Rows.Count != 0) { ResVal = "1"; ResSTR = "The ext unic ID #2 value is duplicated"; }
                            }
                            catch (Exception) { }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A14 != "")
                        {
                            try
                            {
                                DataTable DTUM = new DataTable();
                                DTUM = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_03_Devices_List Where (Master_Unic_ID = '" + A14 + "') or (Extra_Unic_ID_1 = '" + A14 + "') or (Extra_Unic_ID_2 = '" + A14 + "') or (Extra_Unic_ID_3 = '" + A14 + "') or (Extra_Unic_ID_4 = '" + A14 + "') And (Removed = '0')");
                                if (DTUM.Rows.Count != 0) { ResVal = "1"; ResSTR = "The ext unic ID #3 value is duplicated"; }
                            }
                            catch (Exception) { }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A15 != "")
                        {
                            try
                            {
                                DataTable DTUM = new DataTable();
                                DTUM = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_03_Devices_List Where (Master_Unic_ID = '" + A15 + "') or (Extra_Unic_ID_1 = '" + A15 + "') or (Extra_Unic_ID_2 = '" + A15 + "') or (Extra_Unic_ID_3 = '" + A15 + "') or (Extra_Unic_ID_4 = '" + A15 + "') And (Removed = '0')");
                                if (DTUM.Rows.Count != 0) { ResVal = "1"; ResSTR = "The ext unic ID #4 value is duplicated"; }
                            }
                            catch (Exception) { }
                        }
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_03_Devices_List Where (Group_ID = '" + Group_ID + "') And (Header_ID = '" + Header_ID + "') And (Name = '" + A2 + "') And (Removed = '0')");
                    if (DT.Rows.Count == 0)
                    {
                        string InsDate = Sq.Sql_Date();
                        string InsTime = Sq.Sql_Time();
                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Services_03_Devices_List Values ('" + Group_ID.ToString() + "','" + Header_ID.ToString() + "','" + A2 + "','" + A3 + "','" + A4 + "','" + A5 + "','" + A6 + "','" + A7 + "','" + A8 + "','" + A9 + "','" + A10 + "','" + A11 + "','" + A12 + "','" + A13 + "','" + A14 + "','" + A15 + "','1','Active','" + InsDate + "','" + InsTime + "','" + UID + "','" + InsDate + "','" + InsTime + "','" + UID + "','0')");
                        ResVal = "0"; ResSTR = "Device " + A2 + " was successfully added";
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The entered tag name / unic key of the device is duplicate, so it is not possible for you to add new device with this tag name/ unic key";
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
        public JsonResult DeviceAdd_Remove(string DID, string DNM)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 69;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                DID = DID.Trim();
                DNM = DNM.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_03_Devices_List Where (ID = '" + DID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_03_Devices_List Set [Removed] = '1',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DID + "')");
                            ResSTR = "The " + DNM + " device was successfully deleted";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified device is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving device information";
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
        public JsonResult DeviceAdd_ChangeStatus(string DID, string DNM)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 70;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                DID = DID.Trim();
                DNM = DNM.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Status_Code From Services_03_Devices_List Where (ID = '" + DID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            if (DT.Rows[0][1].ToString().Trim() == "1")
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_03_Devices_List Set [Status_Code] = '0',[Status_Text] = 'Disabled',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DID + "')");
                                ResSTR = "The " + DNM + " device was successfully change status to disabled";
                            }
                            else
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_03_Devices_List Set [Status_Code] = '1',[Status_Text] = 'Active',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DID + "')");
                                ResSTR = "The " + DNM + " device was successfully change status to active";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified device is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving device information";
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
        public JsonResult DeviceAdd_SaveEdit(string A1, string A2, string A3, string A4, string A5, string A6, string A7, string A8, string A9, string A10, string A11, string A12, string A13, string A14, string A15)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 71;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                A1 = A1.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A2 = A2.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A3 = A3.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A4 = A4.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A5 = A5.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A6 = A6.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A7 = A7.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A8 = A8.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A9 = A9.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A10 = A10.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A11 = A11.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A12 = A12.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A13 = A13.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A14 = A14.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A15 = A15.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT_PID = new DataTable();
                    DT_PID = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_03_Devices_List Where (ID = '" + A1 + "') And (Removed = '0')");
                    if (DT_PID.Rows != null)
                    {
                        if (DT_PID.Rows.Count != 1)
                        {
                            ResVal = "1"; ResSTR = "Base device information not identified";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "Base device information not identified";
                    }
                }
                if (ResVal == "0")
                {
                    if (A2 == "")
                    {
                        ResVal = "1"; ResSTR = "It is necessary to enter the tag name / unic key to save device changes";
                    }
                    else
                    {
                        A2 = Pb.Text_UpperCase_AfterSpase(A2);
                    }
                }
                // ID Duplicated Values :
                if (ResVal == "0")
                {
                    if (A3 != "")
                    {
                        if (A3 == A12) { ResVal = "1"; ResSTR = "The value of master unic ID overlaps with ext unic ID #1"; }
                        if (A3 == A13) { ResVal = "1"; ResSTR = "The value of master unic ID overlaps with ext unic ID #2"; }
                        if (A3 == A14) { ResVal = "1"; ResSTR = "The value of master unic ID overlaps with ext unic ID #3"; }
                        if (A3 == A15) { ResVal = "1"; ResSTR = "The value of master unic ID overlaps with ext unic ID #4"; }
                    }
                    if (ResVal == "0")
                    {
                        if (A12 != "")
                        {
                            if (A12 == A3) { ResVal = "1"; ResSTR = "The value of ext unic ID #1 overlaps with master unic ID"; }
                            if (A12 == A13) { ResVal = "1"; ResSTR = "The value of ext unic ID #1  overlaps with ext unic ID #2"; }
                            if (A12 == A14) { ResVal = "1"; ResSTR = "The value of ext unic ID #1 overlaps with ext unic ID #3"; }
                            if (A12 == A15) { ResVal = "1"; ResSTR = "The value of ext unic ID #1 overlaps with ext unic ID #4"; }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A13 != "")
                        {
                            if (A13 == A3) { ResVal = "1"; ResSTR = "The value of ext unic ID #2 overlaps with master unic ID"; }
                            if (A13 == A12) { ResVal = "1"; ResSTR = "The value of ext unic ID #2  overlaps with ext unic ID #1"; }
                            if (A13 == A14) { ResVal = "1"; ResSTR = "The value of ext unic ID #2 overlaps with ext unic ID #3"; }
                            if (A13 == A15) { ResVal = "1"; ResSTR = "The value of ext unic ID #2 overlaps with ext unic ID #4"; }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A14 != "")
                        {
                            if (A14 == A3) { ResVal = "1"; ResSTR = "The value of ext unic ID #3 overlaps with master unic ID"; }
                            if (A14 == A12) { ResVal = "1"; ResSTR = "The value of ext unic ID #3  overlaps with ext unic ID #1"; }
                            if (A14 == A13) { ResVal = "1"; ResSTR = "The value of ext unic ID #3 overlaps with ext unic ID #2"; }
                            if (A14 == A15) { ResVal = "1"; ResSTR = "The value of ext unic ID #3 overlaps with ext unic ID #4"; }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A15 != "")
                        {
                            if (A15 == A3) { ResVal = "1"; ResSTR = "The value of ext unic ID #4 overlaps with master unic ID"; }
                            if (A15 == A12) { ResVal = "1"; ResSTR = "The value of ext unic ID #4  overlaps with ext unic ID #1"; }
                            if (A15 == A13) { ResVal = "1"; ResSTR = "The value of ext unic ID #4 overlaps with ext unic ID #2"; }
                            if (A15 == A14) { ResVal = "1"; ResSTR = "The value of ext unic ID #4 overlaps with ext unic ID #3"; }
                        }
                    }
                }
                // ID Duplicated SQL :
                if (ResVal == "0")
                {
                    if (A3 != "")
                    {
                        try
                        {
                            DataTable DTUM = new DataTable();
                            DTUM = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_03_Devices_List Where (ID <> '" + A1 + "') And ((Master_Unic_ID = '" + A3 + "') or (Extra_Unic_ID_1 = '" + A3 + "') or (Extra_Unic_ID_2 = '" + A3 + "') or (Extra_Unic_ID_3 = '" + A3 + "') or (Extra_Unic_ID_4 = '" + A3 + "')) And (Removed = '0')");
                            if (DTUM.Rows.Count != 0) { ResVal = "1"; ResSTR = "The master unic ID value is duplicated"; }
                        }
                        catch (Exception) { }
                    }
                    if (ResVal == "0")
                    {
                        if (A12 != "")
                        {
                            try
                            {
                                DataTable DTUM = new DataTable();
                                DTUM = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_03_Devices_List Where (ID <> '" + A1 + "') And ((Master_Unic_ID = '" + A12 + "') or (Extra_Unic_ID_1 = '" + A12 + "') or (Extra_Unic_ID_2 = '" + A12 + "') or (Extra_Unic_ID_3 = '" + A12 + "') or (Extra_Unic_ID_4 = '" + A12 + "')) And (Removed = '0')");
                                if (DTUM.Rows.Count != 0) { ResVal = "1"; ResSTR = "The ext unic ID #1 value is duplicated"; }
                            }
                            catch (Exception) { }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A13 != "")
                        {
                            try
                            {
                                DataTable DTUM = new DataTable();
                                DTUM = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_03_Devices_List Where (ID <> '" + A1 + "') And ((Master_Unic_ID = '" + A13 + "') or (Extra_Unic_ID_1 = '" + A13 + "') or (Extra_Unic_ID_2 = '" + A13 + "') or (Extra_Unic_ID_3 = '" + A13 + "') or (Extra_Unic_ID_4 = '" + A13 + "')) And (Removed = '0')");
                                if (DTUM.Rows.Count != 0) { ResVal = "1"; ResSTR = "The ext unic ID #2 value is duplicated"; }
                            }
                            catch (Exception) { }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A14 != "")
                        {
                            try
                            {
                                DataTable DTUM = new DataTable();
                                DTUM = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_03_Devices_List Where (ID <> '" + A1 + "') And ((Master_Unic_ID = '" + A14 + "') or (Extra_Unic_ID_1 = '" + A14 + "') or (Extra_Unic_ID_2 = '" + A14 + "') or (Extra_Unic_ID_3 = '" + A14 + "') or (Extra_Unic_ID_4 = '" + A14 + "')) And (Removed = '0')");
                                if (DTUM.Rows.Count != 0) { ResVal = "1"; ResSTR = "The ext unic ID #3 value is duplicated"; }
                            }
                            catch (Exception) { }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A15 != "")
                        {
                            try
                            {
                                DataTable DTUM = new DataTable();
                                DTUM = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_03_Devices_List Where (ID <> '" + A1 + "') And ((Master_Unic_ID = '" + A15 + "') or (Extra_Unic_ID_1 = '" + A15 + "') or (Extra_Unic_ID_2 = '" + A15 + "') or (Extra_Unic_ID_3 = '" + A15 + "') or (Extra_Unic_ID_4 = '" + A15 + "')) And (Removed = '0')");
                                if (DTUM.Rows.Count != 0) { ResVal = "1"; ResSTR = "The ext unic ID #4 value is duplicated"; }
                            }
                            catch (Exception) { }
                        }
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_03_Devices_List Where (ID <> '" + A1 + "') And (Name = '" + A2 + "') And (Removed = '0')");
                    if (DT.Rows.Count == 0)
                    {
                        string InsDate = Sq.Sql_Date();
                        string InsTime = Sq.Sql_Time();
                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_03_Devices_List Set [Name] = '" + A2 + "',[Master_Unic_ID] = '" + A3 + "',[Model] = '" + A4 + "',[Product_ID] = '" + A5 + "',[Serial_Number] = '" + A6 + "',[Build_Series_Number] = '" + A7 + "',[Version] = '" + A8 + "',[Situation_Code] = '" + A9 + "',[Situation_Text] = '" + A10 + "',[Description] = '" + A11 + "',[Extra_Unic_ID_1] = '" + A12 + "',[Extra_Unic_ID_2] = '" + A13 + "',[Extra_Unic_ID_3] = '" + A14 + "',[Extra_Unic_ID_4] = '" + A15 + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + A1 + "') And (Removed = '0')");
                        ResVal = "0"; ResSTR = "Device named " + A2 + " was successfully edited";
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The entered tag name / unic key of the device is duplicate, so it is not possible for you to save device changes with this tag name / unic key";
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
        public JsonResult DeviceAdd_Information(string DID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 72;
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
                    DID = DID.Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Top 1 * From Services_03_Devices_List_V Where (ID = '" + DID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            ResSTR = DT.Rows[0][5].ToString().Trim() + "#";
                            ResSTR += "<b class=\"text-primary\">Device Type : </b>" + DT.Rows[0][4].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Device Group : </b>" + DT.Rows[0][2].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Device Name : </b>" + DT.Rows[0][5].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Master Unic ID : </b>" + DT.Rows[0][6].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Model : </b>" + DT.Rows[0][7].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Product ID : </b>" + DT.Rows[0][8].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Serial Number : </b>" + DT.Rows[0][9].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Build Series Number : </b>" + DT.Rows[0][10].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Version : </b>" + DT.Rows[0][11].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Situation : </b>" + DT.Rows[0][13].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Extra Unic ID 1 : </b>" + DT.Rows[0][15].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Extra Unic ID 2 : </b>" + DT.Rows[0][16].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Extra Unic ID 3 : </b>" + DT.Rows[0][17].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Extra Unic ID 4 : </b>" + DT.Rows[0][18].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Description : </b>" + DT.Rows[0][14].ToString().Trim() + "<br>";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The server encountered an error while reloading device information";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading device information";
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
        public JsonResult DeviceAdd_GetPropertiesData(string DID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 73;
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
                    int ObjectCount = 0;
                    DID = DID.Trim();
                    DataTable DT = new DataTable();
                    bool ExitNow = false;
                    List<string> GroupID = new List<string>();
                    DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Group_ID From Services_03_Devices_List Where (ID = '" + DID + "') And (Removed = '0')");
                    string PDID = DT.Rows[0][0].ToString().Trim();
                    while (ExitNow == false)
                    {
                        DT = new DataTable();
                        DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Parent_ID,Name From Services_01_Devices_Group Where (ID = '" + PDID + "') And (Removed = '0')");
                        if (DT.Rows.Count == 1)
                        {
                            if (DT.Rows[0][0].ToString().Trim() == "0") { ExitNow = true; }
                            GroupID.Add(PDID + "#" + DT.Rows[0][1].ToString().Trim());
                            PDID = DT.Rows[0][0].ToString().Trim();
                        }
                        else
                        {
                            ExitNow = true;
                        }
                    }
                    ResSTR = "";
                    if (GroupID.Count > 0)
                    {
                        DataTable DTVal = new DataTable();
                        ResSTR += "<div class=\"col-lg-12 form-inline\">";
                        for (int i = (GroupID.Count - 1); i >= 0; i--)
                        {
                            string[] GroupInfo = GroupID[i].Split('#');
                            DT = new DataTable();
                            DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Tag,Unit,Description,Width,ID From Services_02_Devices_GroupProperties Where (Group_ID = '" + GroupInfo[0] + "') And (Status_Code = '1') And (Removed = '0') Order By RowIndex,Tag,ID");
                            if (DT.Rows.Count > 0)
                            {
                                ObjectCount++;
                                ResSTR += "<div class=\"divider divider-primary col-lg-12\">";
                                ResSTR += "<div class=\"divider-text\">" + GroupInfo[1].ToString().Trim() + "</div>";
                                ResSTR += "</div>";
                                foreach (DataRow RW in DT.Rows)
                                {
                                    string TxtValue = "";
                                    try
                                    {
                                        DTVal = new DataTable();
                                        DTVal = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Value From Services_04_Devices_Properties Where (Device_ID = '" + DID + "') And (Properties_ID = '" + RW[4].ToString().Trim() + "')");
                                        if (DTVal.Rows.Count == 1) { TxtValue = DTVal.Rows[0][0].ToString().Trim(); }
                                    }
                                    catch (Exception) { }
                                    ResSTR += "<div class=\"col-lg-" + RW[3].ToString().Trim() + "\" style=\"opacity:1;pointer-events:auto\">";
                                    ResSTR += "<div class=\"text-bold-300 font-medium-1\" style=\"margin-top:10px;margin-left:5px\">";
                                    ResSTR += RW[0].ToString().Trim();
                                    if (RW[1].ToString().Trim() != "") { ResSTR += " [ " + RW[1].ToString().Trim() + " ]"; }
                                    ResSTR += "</div>";
                                    ResSTR += "<fieldset class=\"form-group position-relative\" style=\"width:100%\">";
                                    ResSTR += "<input type=\"text\" class=\"form-control GSelTXTAM\" id=\"DP_" + RW[4].ToString().Trim() + "\" placeholder=\"" + RW[2].ToString().Trim() + "\" style=\"width:100%\" maxlength=\"200\" value=\"" + TxtValue + "\">";
                                    ResSTR += "</fieldset>";
                                    ResSTR += "</div>";
                                }
                            }
                        }
                        ResSTR += "</div>";
                    }
                    if (ObjectCount == 0) { ResSTR = "<b>Dear admin ...</b><br>No properties have been defined for this device in the past"; }
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
        public void DeviceAdd_PropertiesAdd(string DID, string PID, string PVal)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 74;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return; }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                DID = DID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                PID = PID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                PVal = PVal.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                Sq.Execute_TSql(DataBase_Selector.Administrator, "Delete From Services_04_Devices_Properties Where (Device_ID = '" + DID + "') And (Properties_ID = '" + PID + "')");
                Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Services_04_Devices_Properties Values ('" + DID + "','" + PID + "','" + PVal + "')");
            }
            catch (Exception) { }
        }

        [HttpPost]
        public JsonResult DeviceAdd_GetCommentsData(string DID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 75;
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
                    DID = DID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    int ObjectCount = 0;
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Ins_Date,Ins_Time,Ins_ID,Ins_ID_Name,Degree_Code,Comment,Status_Code,ID From Services_05_Devices_Comment Where (Device_ID = '" + DID + "') And (Removed = '0') Order By Ins_Date desc,Ins_Time desc");
                    if (DT.Rows.Count > 0)
                    {
                        ResSTR += "<div class=\"accordion\" id=\"EmasColApp\">";
                        foreach (DataRow RW in DT.Rows)
                        {
                            ObjectCount++;
                            ResSTR += "<div class=\"collapse-margin\">";
                            ResSTR += "<div class=\"card-header\" id=\"heading" + ObjectCount.ToString() + "\" data-toggle=\"collapse\" role=\"button\" data-target=\"#ColappN" + ObjectCount.ToString() + "\" aria-expanded=\"false\" aria-controls=\"ColappN" + ObjectCount.ToString() + "\">";
                            ResSTR += "<span class=\"lead collapse-title\">";
                            ResSTR += "<i class=\"fa fa-calendar\" style=\"margin-right:10px\"></i>" + RW[0].ToString().Substring(0, 10).Trim() + "<i class=\"fa fa-clock-o\" style=\"margin-right:10px;margin-left:30px\"></i>" + RW[1].ToString().Trim() + "<i class=\"fa fa-user\" style=\"margin-right:10px;margin-left:30px\"></i>" + RW[2].ToString().Trim() + " - " + RW[3].ToString().Trim();
                            ResSTR += "</span>";
                            switch (RW[4].ToString().Trim())
                            {
                                case "1": { ResSTR += "<div class=\"badge badge-pill badge-glow badge-primary\" style=\"width:95px;margin-left:30px\">Normal</div>"; break; }
                                case "2": { ResSTR += "<div class=\"badge badge-pill badge-glow badge-info\" style=\"width:95px;margin-left:30px\">information</div>"; break; }
                                case "3": { ResSTR += "<div class=\"badge badge-pill badge-glow badge-warning\" style=\"width:95px;margin-left:30px\">Warning</div>"; break; }
                                case "4": { ResSTR += "<div class=\"badge badge-pill badge-glow badge-danger\" style=\"width:95px;margin-left:30px\">Important</div>"; break; }
                                case "5": { ResSTR += "<div class=\"badge badge-pill badge-glow badge-success\" style=\"width:95px;margin-left:30px\">Successfull</div>"; break; }
                            }
                            if (RW[6].ToString().Trim() == "1")
                            {
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:70px;margin-left:30px\">Active</div>";
                            }
                            else
                            {
                                ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:70px;margin-left:30px\">Disabled</div>";
                            }
                            ResSTR += "</div>";
                            ResSTR += "<div id=\"ColappN" + ObjectCount.ToString() + "\" class=\"collapse\" aria-labelledby=\"heading" + ObjectCount.ToString() + "\" data-parent=\"#EmasColApp\">";
                            ResSTR += "<div class=\"card-body\">";
                            ResSTR += RW[5].ToString().Trim();
                            ResSTR += "</div>";
                            ResSTR += "<div class=\"card-footer col-lg-12\">";
                            ResSTR += "<a class=\"text-danger col-lg-2\" href=\"javascript:void(0)\" onclick=\"DeviceAdd_CommentRemove('" + DID + "','" + RW[7].ToString().Trim() + "')\"><i class=\"fa fa-trash\" style=\"margin-right:10px\"></i>Remove this comment</a>";
                            ResSTR += "<a class=\"text-info col-lg-2\" href=\"javascript:void(0)\" onclick=\"DeviceAdd_CommentChangeStatus('" + DID + "','" + RW[7].ToString().Trim() + "')\"><i class=\"fa fa-refresh\" style=\"margin-right:10px\"></i>Change status</a>";
                            ResSTR += "</div>";
                            ResSTR += "</div>";
                            ResSTR += "</div>";
                        }
                        ResSTR += "</div>";
                    }
                    if (ObjectCount == 0) { ResSTR = "<b>Dear admin ...</b><br>No comment have been added for this device in the past"; }
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
        public JsonResult DeviceAdd_CommentADD(string DID, string CMT, string DVL, string DTX)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 76;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                string UNM = "";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                try { UNM = Session["Admin_UNM"].ToString().Trim(); } catch (Exception) { UNM = "0"; }
                UID = UID.Trim();
                UNM = UNM.Trim();
                DID = DID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                CMT = CMT.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DVL = DVL.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DTX = DTX.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_03_Devices_List Where (ID = '" + DID + "') And (Removed = '0')");
                    if (DT.Rows.Count == 1)
                    {
                        string InsDate = Sq.Sql_Date();
                        string InsTime = Sq.Sql_Time();
                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Services_05_Devices_Comment Values ('" + DID + "','" + CMT + "','" + DVL + "','" + DTX + "','1','Active','" + InsDate + "','" + InsTime + "','" + UID + "','" + UNM + "','" + InsDate + "','" + InsTime + "','" + UID + "','0','','','0','')");
                        ResVal = "0"; ResSTR = "Device's new comment was successfully added";
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The device could not be identified";
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
        public JsonResult DeviceAdd_CommentRemove(string DID, string CID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 77;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                string UNM = "";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                try { UNM = Session["Admin_UNM"].ToString().Trim(); } catch (Exception) { UNM = "0"; }
                UID = UID.Trim();
                UNM = UNM.Trim();
                DID = DID.Trim();
                CID = CID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_05_Devices_Comment Where (ID = '" + CID + "') And (Device_ID = '" + DID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_05_Devices_Comment Set [Removed] = '1',[Removed_Date] = '" + InsDate + "',[Removed_Time] = '" + InsTime + "',[Removed_ID] = '" + UID + "',[Removed_ID_Name] = '" + UNM + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + CID + "') And (Device_ID = '" + DID + "')");
                            ResSTR = "The comment device was successfully deleted";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified comment is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving comment information";
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
        public JsonResult DeviceAdd_CommentChangeStatus(string DID, string CID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 78;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                DID = DID.Trim();
                CID = CID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Status_Code From Services_05_Devices_Comment Where (ID = '" + CID + "') And (Device_ID = '" + DID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            if (DT.Rows[0][1].ToString().Trim() == "1")
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_05_Devices_Comment Set [Status_Code] = '0',[Status_Text] = 'Disabled',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + CID + "') And (Device_ID = '" + DID + "')");
                                ResSTR = "The comment was successfully change status to disabled";
                            }
                            else
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_05_Devices_Comment Set [Status_Code] = '1',[Status_Text] = 'Active',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + CID + "') And (Device_ID = '" + DID + "')");
                                ResSTR = "The comment was successfully change status to active";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified comment is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving comment information";
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
        // Device List Search :
        public ActionResult Devices_List()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 53;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" }); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------




                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" });
            }
        }

        //====================================================================================================================
        // Software Management :
        public ActionResult Softwares_Management()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 54;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" }); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                bool GroupTreeRemoved = false;
                string[] Breadcrumb_List_Name = new string[6];
                string[] Breadcrumb_List_Code = new string[6];
                int Breadcrumb_Count = 0;
                for (int i = 0; i < 6; i++) { Breadcrumb_List_Name[i] = ""; Breadcrumb_List_Code[i] = ""; }
                ViewBag.Breadcrumb_Count = 0;
                ViewBag.Breadcrumb_ListName = Breadcrumb_List_Name;
                ViewBag.Breadcrumb_ListCode = Breadcrumb_List_Code;
                string Parent_ID = "0";
                string Parent_IDUC = "0";
                try { Parent_IDUC = Url.RequestContext.RouteData.Values["id"].ToString().Trim(); } catch (Exception) { }
                if (Parent_IDUC.Trim() == "") { Parent_IDUC = "0"; }
                Parent_IDUC = Parent_IDUC.Trim();
                if (Parent_IDUC != "0")
                {
                    try
                    {
                        DataTable DTConvert = new DataTable();
                        DTConvert = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,ID_UnicCode,Name,Parent_ID From Services_06_Softwares_Group Where (ID_UnicCode = '" + Parent_IDUC + "') And (Removed = '0')");
                        if (DTConvert.Rows != null)
                        {
                            if (DTConvert.Rows.Count == 1)
                            {
                                Parent_ID = DTConvert.Rows[0][0].ToString().Trim();
                                Parent_IDUC = DTConvert.Rows[0][1].ToString().Trim();
                                bool ExitWhile = false;
                                Breadcrumb_Count++;
                                Breadcrumb_List_Code[Breadcrumb_Count - 1] = DTConvert.Rows[0][1].ToString().Trim();
                                Breadcrumb_List_Name[Breadcrumb_Count - 1] = DTConvert.Rows[0][2].ToString().Trim();
                                string PrntID = DTConvert.Rows[0][3].ToString().Trim();
                                if (PrntID == "0") { ExitWhile = true; }
                                while (ExitWhile == false)
                                {
                                    if (PrntID.Trim() != "0")
                                    {
                                        DataTable DT_Breadcrumb = new DataTable();
                                        DT_Breadcrumb = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID_UnicCode,Name,Parent_ID,Removed From Services_06_Softwares_Group Where (ID = '" + PrntID + "') And (Removed = '0')");
                                        if (DT_Breadcrumb.Rows[0][3].ToString().Trim().ToLower() == "false")
                                        {
                                            PrntID = DT_Breadcrumb.Rows[0][2].ToString().Trim();
                                            Breadcrumb_Count++;
                                            if (Breadcrumb_Count > 6) { Breadcrumb_Count = 6; }
                                            Breadcrumb_List_Code[Breadcrumb_Count - 1] = DT_Breadcrumb.Rows[0][0].ToString().Trim();
                                            Breadcrumb_List_Name[Breadcrumb_Count - 1] = DT_Breadcrumb.Rows[0][1].ToString().Trim();
                                        }
                                        else
                                        {
                                            GroupTreeRemoved = true;
                                        }
                                    }
                                    if (GroupTreeRemoved == true) { ExitWhile = true; }
                                    if (PrntID == "0") { ExitWhile = true; }
                                }
                                if (Breadcrumb_Count > 6) { Breadcrumb_Count = 6; }
                                ViewBag.Breadcrumb_Count = Breadcrumb_Count;
                            }
                            else
                            {
                                return RedirectToAction("Softwares_Management", "Services", new { id = "", area = "ManagementPortal" });
                            }
                        }
                        else
                        {
                            return RedirectToAction("Softwares_Management", "Services", new { id = "", area = "ManagementPortal" });
                        }
                    }
                    catch (Exception)
                    {
                        return RedirectToAction("Softwares_Management", "Services", new { id = "", area = "ManagementPortal" });
                    }
                }
                if (GroupTreeRemoved == true) { return RedirectToAction("Softwares_Management", "Services", new { id = "", area = "ManagementPortal" }); }
                DataTable DT = new DataTable();
                DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,ID_UnicCode,Name,Description,Status_Code,UnicID_Forced,Inventories_Forced From Services_06_Softwares_Group Where (Parent_ID = '" + Parent_ID + "') And (Removed = '0') Order By Name,Ins_Date,Ins_Time");
                ViewBag.DT = DT.Rows;
                DataTable DT_DeviceList = new DataTable();
                DT_DeviceList = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Name,Master_Unic_ID,Model,Product_ID,Serial_Number,Build_Series_Number,Version,Situation_Code,Description,Extra_Unic_ID_1,Extra_Unic_ID_2,Extra_Unic_ID_3,Extra_Unic_ID_4,Status_Code,Situation_Text From Services_08_Softwares_List Where (Group_ID = '" + Parent_ID + "') And (Removed = '0') Order By Name,Master_Unic_ID,Product_ID,Serial_Number,Build_Series_Number,Version");
                ViewBag.DTDevice = DT_DeviceList.Rows;
                ViewBag.Parent_ID = Parent_ID;
                ViewBag.Parent_IDUC = Parent_IDUC;
                ViewBag.Breadcrumb_ListName = Breadcrumb_List_Name;
                ViewBag.Breadcrumb_ListCode = Breadcrumb_List_Code;
                ViewBag.Breadcrumb_Count = Breadcrumb_Count;
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" });
            }
        }
        [HttpPost]
        public JsonResult Softwares_Management_Grid(string PID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 54;
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
                    PID = PID.Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,ID_UnicCode,Name,Description,Status_Code,UnicID_Forced,Inventories_Forced From Services_06_Softwares_Group Where (Parent_ID = '" + PID + "') And (Removed = '0') And (Type_Code = '1') Order By Name,Ins_Date,Ins_Time");
                    if (DT.Rows != null)
                    {
                        foreach (DataRow RW in DT.Rows)
                        {
                            ResSTR += "<tr>";
                            ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\"><i class=\"fa fa-cubes text-primary\" style=\"font-size:25px\"></i></td>";
                            ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[2].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[3].ToString().Trim() + "</td>";
                            if (RW[5].ToString().Trim() == "0")
                            {
                                ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:35px\">No</div>";
                                ResSTR += "</td>";
                            }
                            else
                            {
                                ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:35px\">Yes</div>";
                                ResSTR += "</td>";
                            }
                            if (RW[6].ToString().Trim() == "0")
                            {
                                ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:35px\">No</div>";
                                ResSTR += "</td>";
                            }
                            else
                            {
                                ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:35px\">Yes</div>";
                                ResSTR += "</td>";
                            }
                            if (RW[4].ToString().Trim() == "0")
                            {
                                ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:70px\">Disable</div>";
                                ResSTR += "</td>";
                            }
                            else
                            {
                                ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:70px\">Active</div>";
                                ResSTR += "</td>";
                            }
                            ResSTR += "<td style=\"text-align:center\">";
                            ResSTR += "<div class=\"btn-group dropleft\">";
                            ResSTR += "<button type=\"button\" class=\"btn\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                            ResSTR += "<div class=\"MenuToolbox text-primary\"/>";
                            ResSTR += "</button>";
                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-search text-primary\" style=\"width:24px;font-size:14px\"></i>Software's group</a>";
                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Device_Remove('" + RW[1].ToString().Trim() + "','" + RW[2].ToString().Trim() + "')\"><i class=\"fa fa-trash-o text-primary\" style=\"width:24px;font-size:14px\"></i>Remove software</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Device_ChangeStatus('" + RW[1].ToString().Trim() + "','" + RW[2].ToString().Trim() + "')\"><i class=\"fa fa-refresh text-primary\" style=\"width:24px;font-size:14px\"></i>Change status</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Device_Edit('" + RW[1].ToString().Trim() + "','" + RW[2].ToString().Trim() + "','" + RW[3].ToString().Trim() + "','" + RW[5].ToString().Trim() + "','" + RW[6].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit software</a>";
                            ResSTR += "</div>";
                            ResSTR += "</div>";
                            ResSTR += "</td>";
                            ResSTR += "</tr>";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading softwares information";
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
        public JsonResult Software_Addnew(string DName, string DDesc, string DUni, string DInve)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 79;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                string ParentID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                DName = DName.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DDesc = DDesc.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DUni = DUni.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DInve = DInve.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    if (DName == "")
                    {
                        ResVal = "1"; ResSTR = "It is necessary to enter the name to add a new software";
                    }
                    else
                    {
                        DName = Pb.Text_UpperCase_AfterSpase(DName);
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_06_Softwares_Group Where (Parent_ID = '" + ParentID + "') And (Name = '" + DName + "') And (Removed = '0')");
                    if (DT.Rows.Count == 0)
                    {
                        string RndKey = Pb.Make_Security_CodeFake(15);
                        long InsCode = Sq.Get_New_ID(DataBase_Selector.Administrator, "Services_06_Softwares_Group", "ID_Counter");
                        string InsDate = Sq.Sql_Date();
                        string InsTime = Sq.Sql_Time();
                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Services_06_Softwares_Group Values ('" + InsCode.ToString() + "','" + InsCode.ToString() + RndKey + "','" + ParentID + "','1','Software','" + DName + "','" + DDesc + "','" + DUni + "','" + DInve + "','1','Active','" + InsDate + "','" + InsTime + "','" + UID + "','" + InsDate + "','" + InsTime + "','" + UID + "','0')");
                        ResVal = "0"; ResSTR = "Software named " + DName + " was successfully added";
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The entered name of the software is duplicate, so it is not possible for you to add new software with this name";
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
        public JsonResult Software_Remove(string DID, string DName)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 80;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                DID = DID.Trim();
                DName = DName.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_06_Softwares_Group Where (ID_UnicCode = '" + DID + "') And (Type_Code = '1') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_06_Softwares_Group Set [Removed] = '1',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '1')");
                            ResSTR = "The " + DName + " software was successfully deleted";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified software is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving software information";
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
        public JsonResult Software_ChangeStatus(string DID, string DName)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 81;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                DID = DID.Trim();
                DName = DName.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Status_Code From Services_06_Softwares_Group Where (ID_UnicCode = '" + DID + "') And (Type_Code = '1') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            if (DT.Rows[0][1].ToString().Trim() == "1")
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_06_Softwares_Group Set [Status_Code] = '0',[Status_Text] = 'Disabled',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '1')");
                                ResSTR = "The " + DName + " software was successfully change status to disabled";
                            }
                            else
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_06_Softwares_Group Set [Status_Code] = '1',[Status_Text] = 'Active',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '1')");
                                ResSTR = "The " + DName + " software was successfully change status to active";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified software is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving software information";
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
        public JsonResult Software_SaveEdit(string DID, string DName, string DDesc, string DUni, string DInve)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 82;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                DID = DID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DName = DName.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DDesc = DDesc.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DUni = DUni.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DInve = DInve.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    if (DName == "")
                    {
                        ResVal = "1"; ResSTR = "It is necessary to enter the name to edit software";
                    }
                    else
                    {
                        DName = Pb.Text_UpperCase_AfterSpase(DName);
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Parent_ID From Services_06_Softwares_Group Where (ID_UnicCode = '" + DID + "') And (Removed = '0') And (Type_Code = '1')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            DataTable DT2 = new DataTable();
                            DT2 = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_06_Softwares_Group Where (ID <> '" + DT.Rows[0][0].ToString().Trim() + "') And (Parent_ID = '" + DT.Rows[0][1].ToString().Trim() + "') And (Name = '" + DName + "') And (Removed = '0')");
                            if (DT2.Rows != null)
                            {
                                if (DT2.Rows.Count == 0)
                                {
                                    string InsDate = Sq.Sql_Date();
                                    string InsTime = Sq.Sql_Time();
                                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_06_Softwares_Group Set [Name] = '" + DName + "',[Description] = '" + DDesc + "',UnicID_Forced = '" + DUni + "',Inventories_Forced = '" + DInve + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '1')");
                                    ResVal = "0"; ResSTR = "Software named " + DName + " was successfully edited";
                                }
                                else
                                {
                                    ResVal = "1"; ResSTR = "The entered name of the software is duplicate, so it is not possible for you to edit software with this name";
                                }
                            }
                            else
                            {
                                ResVal = "1"; ResSTR = "The server encountered an error while receiving software information";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified software is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving software information";
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
        public JsonResult Software_Management_Grid(string PID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 54;
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
                    PID = PID.Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,ID_UnicCode,Name,Description,Status_Code,UnicID_Forced,Inventories_Forced From Services_06_Softwares_Group Where (Parent_ID = '" + PID + "') And (Removed = '0') And (Type_Code = '2') Order By Name,Ins_Date,Ins_Time");
                    if (DT.Rows != null)
                    {
                        foreach (DataRow RW in DT.Rows)
                        {
                            ResSTR += "<tr>";
                            ResSTR += "<td onclick=\"Group_SubGroupShow('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\"><i class=\"fa fa-object-group text-primary\" style=\"font-size:25px\"></i></td>";
                            ResSTR += "<td onclick=\"Group_SubGroupShow('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[2].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"Group_SubGroupShow('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[3].ToString().Trim() + "</td>";
                            if (RW[4].ToString().Trim() == "0")
                            {
                                ResSTR += "<td onclick=\"Group_SubGroupShow('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:70px\">Disabled</div>";
                                ResSTR += "</td>";
                            }
                            else
                            {
                                ResSTR += "<td onclick=\"Group_SubGroupShow('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:70px\">Active</div>";
                                ResSTR += "</td>";
                            }
                            ResSTR += "<td style=\"text-align:center\">";
                            ResSTR += "<div class=\"btn-group dropleft\">";
                            ResSTR += "<button type=\"button\" class=\"btn\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                            ResSTR += "<div class=\"MenuToolbox text-primary\"/>";
                            ResSTR += "</button>";
                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Group_AddDeviceProperties('" + RW[0].ToString().Trim() + "','" + RW[2].ToString().Trim() + "')\"><i class=\"fa fa-info-circle text-primary\" style=\"width:24px;font-size:14px\"></i>Groups's spftware Properties</a>";
                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Group_SubGroupShow('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-search text-primary\" style=\"width:24px;font-size:14px\"></i>Show subgroup</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Group_Remove('" + RW[1].ToString().Trim() + "','" + RW[2].ToString().Trim() + "')\"><i class=\"fa fa-trash-o text-primary\" style=\"width:24px;font-size:14px\"></i>Remove group</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Group_ChangeStatus('" + RW[1].ToString().Trim() + "','" + RW[2].ToString().Trim() + "')\"><i class=\"fa fa-refresh text-primary\" style=\"width:24px;font-size:14px\"></i>Change status</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Group_Edit('" + RW[1].ToString().Trim() + "','" + RW[2].ToString().Trim() + "','" + RW[3].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit group</a>";
                            ResSTR += "</div>";
                            ResSTR += "</div>";
                            ResSTR += "</td>";
                            ResSTR += "</tr>";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading group information";
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
        public JsonResult Software_Group_Addnew(string ParentIDUC, string GroupName, string GroupDescription)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 83;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                string ParentID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                ParentIDUC = ParentIDUC.Trim();
                GroupName = GroupName.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GroupDescription = GroupDescription.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    if (ParentIDUC != "0")
                    {
                        DataTable DT_PID = new DataTable();
                        DT_PID = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_06_Softwares_Group Where (ID_UnicCode = '" + ParentIDUC + "') And (Removed = '0')");
                        if (DT_PID.Rows != null)
                        {
                            if (DT_PID.Rows.Count == 1)
                            {
                                ParentID = DT_PID.Rows[0][0].ToString().Trim();
                            }
                            else
                            {
                                ResVal = "1"; ResSTR = "Parent group information not identified";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "Parent group information not identified";
                        }
                    }
                    else
                    {
                        ParentID = "0";
                    }
                }
                if (ResVal == "0")
                {
                    if (GroupName == "")
                    {
                        ResVal = "1"; ResSTR = "It is necessary to enter the name to add a new group";
                    }
                    else
                    {
                        GroupName = Pb.Text_UpperCase_AfterSpase(GroupName);
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_06_Softwares_Group Where (Parent_ID = '" + ParentID + "') And (Name = '" + GroupName + "') And (Removed = '0')");
                    if (DT.Rows.Count == 0)
                    {
                        string RndKey = Pb.Make_Security_CodeFake(15);
                        long InsCode = Sq.Get_New_ID(DataBase_Selector.Administrator, "Services_06_Softwares_Group", "ID_Counter");
                        string InsDate = Sq.Sql_Date();
                        string InsTime = Sq.Sql_Time();
                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Services_06_Softwares_Group Values ('" + InsCode.ToString() + "','" + InsCode.ToString() + RndKey + "','" + ParentID + "','2','Group','" + GroupName + "','" + GroupDescription + "','0','0','1','Active','" + InsDate + "','" + InsTime + "','" + UID + "','" + InsDate + "','" + InsTime + "','" + UID + "','0')");
                        ResVal = "0"; ResSTR = "Group named " + GroupName + " was successfully added";
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The entered name of the group is duplicate, so it is not possible for you to add new group with this name";
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
        public JsonResult Software_Group_Remove(string GroupIDU, string GroupName)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 84;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GroupIDU = GroupIDU.Trim();
                GroupName = GroupName.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_06_Softwares_Group Where (ID_UnicCode = '" + GroupIDU + "') And (Type_Code = '2') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_06_Softwares_Group Set [Removed] = '1',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '2')");
                            ResSTR = "The " + GroupName + " group was successfully deleted";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified group is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving group information";
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
        public JsonResult Software_Group_ChangeStatus(string GroupIDU, string GroupName)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 85;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GroupIDU = GroupIDU.Trim();
                GroupName = GroupName.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Status_Code From Services_06_Softwares_Group Where (ID_UnicCode = '" + GroupIDU + "') And (Type_Code = '2') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            if (DT.Rows[0][1].ToString().Trim() == "1")
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_06_Softwares_Group Set [Status_Code] = '0',[Status_Text] = 'Disabled',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '2')");
                                ResSTR = "The " + GroupName + " group was successfully change status to disabled";
                            }
                            else
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_06_Softwares_Group Set [Status_Code] = '1',[Status_Text] = 'Active',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '2')");
                                ResSTR = "The " + GroupName + " group was successfully change status to active";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified group is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving group information";
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
        public JsonResult Software_Group_SaveEdit(string GroupIDUC, string GroupName, string GroupDescription)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 86;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GroupIDUC = GroupIDUC.Trim();
                GroupName = GroupName.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GroupDescription = GroupDescription.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    if (GroupName == "")
                    {
                        ResVal = "1"; ResSTR = "It is necessary to enter the name to edit group";
                    }
                    else
                    {
                        GroupName = Pb.Text_UpperCase_AfterSpase(GroupName);
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Parent_ID From Services_06_Softwares_Group Where (ID_UnicCode = '" + GroupIDUC + "') And (Removed = '0') And (Type_Code = '2')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            DataTable DT2 = new DataTable();
                            DT2 = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_06_Softwares_Group Where (ID <> '" + DT.Rows[0][0].ToString().Trim() + "') And (Parent_ID = '" + DT.Rows[0][1].ToString().Trim() + "') And (Name = '" + GroupName + "') And (Removed = '0')");
                            if (DT2.Rows != null)
                            {
                                if (DT2.Rows.Count == 0)
                                {
                                    string InsDate = Sq.Sql_Date();
                                    string InsTime = Sq.Sql_Time();
                                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_06_Softwares_Group Set [Name] = '" + GroupName + "',[Description] = '" + GroupDescription + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '2')");
                                    ResVal = "0"; ResSTR = "Group named " + GroupName + " was successfully edited";
                                }
                                else
                                {
                                    ResVal = "1"; ResSTR = "The entered name of the group is duplicate, so it is not possible for you to edit group with this name";
                                }
                            }
                            else
                            {
                                ResVal = "1"; ResSTR = "The server encountered an error while receiving group information";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified group is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving group information";
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
        public JsonResult Software_Group_Properties_Index(string GID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 54;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GID = GID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Max(RowIndex) From Services_07_Softwares_GroupProperties Where (Group_ID = '" + GID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            long NowIndex = 0;
                            try { NowIndex = long.Parse(DT.Rows[0][0].ToString().Trim()); } catch (Exception) { }
                            NowIndex++;
                            ResSTR = NowIndex.ToString();
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The server encountered an error while reloading information";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading information";
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
        public JsonResult Software_Group_Properties_Grid(string GID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 54;
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
                    GID = GID.Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Tag,Unit,Description,RowIndex,Width,Status_Code From Services_07_Softwares_GroupProperties Where (Group_ID = '" + GID + "') And (Removed = '0') Order By Tag,Ins_Date,Ins_Time");
                    if (DT.Rows != null)
                    {
                        foreach (DataRow RW in DT.Rows)
                        {
                            ResSTR += "<tr>";
                            ResSTR += "<td>" + RW[1].ToString().Trim() + "</td>";
                            ResSTR += "<td>" + RW[2].ToString().Trim() + "</td>";
                            ResSTR += "<td>" + RW[3].ToString().Trim() + "</td>";
                            ResSTR += "<td style=\"width:70px;text-align:center\">" + RW[4].ToString().Trim() + "</td>";
                            ResSTR += "<td style=\"width:70px;text-align:center\">" + RW[5].ToString().Trim() + "</td>";
                            if (RW[6].ToString().Trim() == "0")
                            {
                                ResSTR += "<td style=\"text-align:center\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:70px\">Disabled</div>";
                                ResSTR += "</td>";
                            }
                            else
                            {
                                ResSTR += "<td style=\"text-align:center\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:70px\">Active</div>";
                                ResSTR += "</td>";
                            }
                            ResSTR += "<td style=\"text-align:center\">";
                            ResSTR += "<div class=\"btn-group dropleft\">";
                            ResSTR += "<button type=\"button\" class=\"btn\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                            ResSTR += "<div class=\"MenuToolbox text-primary\"/>";
                            ResSTR += "</button>";
                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"GroupProperties_Remove('" + RW[0].ToString().Trim() + "','" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-trash-o text-primary\" style=\"width:24px;font-size:14px\"></i>Remove properties</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"GroupProperties_ChangeStatus('" + RW[0].ToString().Trim() + "','" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-refresh text-primary\" style=\"width:24px;font-size:14px\"></i>Change properties status</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"GroupProperties_Edit('" + RW[0].ToString().Trim() + "','" + RW[1].ToString().Trim() + "','" + RW[2].ToString().Trim() + "','" + RW[3].ToString().Trim() + "','" + RW[4].ToString().Trim() + "','" + RW[5].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit properties</a>";
                            ResSTR += "</div>";
                            ResSTR += "</div>";
                            ResSTR += "</td>";
                            ResSTR += "</tr>";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading group properties information";
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
        public JsonResult Software_Group_Properties_Addnew(string GID, string GTA, string GUN, string GDC, string GIN, string GWD)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 87;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GID = GID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GTA = GTA.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GUN = GUN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GDC = GDC.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GIN = GIN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GWD = GWD.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT_PID = new DataTable();
                    DT_PID = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_06_Softwares_Group Where (ID = '" + GID + "') And (Removed = '0')");
                    if (DT_PID.Rows != null)
                    {
                        if (DT_PID.Rows.Count != 1)
                        {
                            ResVal = "1"; ResSTR = "Base group information not identified";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "Base group information not identified";
                    }
                }
                if (ResVal == "0")
                {
                    if (GTA == "")
                    {
                        ResVal = "1"; ResSTR = "It is necessary to enter the tag to add a new properties";
                    }
                    else
                    {
                        GTA = Pb.Text_UpperCase_AfterSpase(GTA);
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_07_Softwares_GroupProperties Where (Group_ID = '" + GID + "') And (Tag = '" + GTA + "') And (Removed = '0')");
                    if (DT.Rows.Count == 0)
                    {
                        string InsDate = Sq.Sql_Date();
                        string InsTime = Sq.Sql_Time();
                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Services_07_Softwares_GroupProperties Values ('" + GID.ToString() + "','" + GTA + "','" + GUN + "','" + GDC + "','" + GIN + "','" + GWD + "','1','Active','" + InsDate + "','" + InsTime + "','" + UID + "','" + InsDate + "','" + InsTime + "','" + UID + "','0')");
                        ResVal = "0"; ResSTR = "Properties tag named " + GTA + " was successfully added";
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The entered tag of the properties is duplicate, so it is not possible for you to add new properties with this tag";
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
        public JsonResult Software_Group_Properties_Remove(string GID, string PID, string PTA)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 88;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GID = GID.Trim();
                PID = PID.Trim();
                PTA = PTA.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_07_Softwares_GroupProperties Where (Group_ID = '" + GID + "') And (ID = '" + PID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_07_Softwares_GroupProperties Set [Removed] = '1',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GID + "') And (ID = '" + PID + "')");
                            ResSTR = "The " + PTA + " properties was successfully deleted";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified properties is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving properties information";
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
        public JsonResult Software_Group_Properties_ChangeStatus(string GID, string PID, string PTA)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 89;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GID = GID.Trim();
                PID = PID.Trim();
                PTA = PTA.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Status_Code From Services_07_Softwares_GroupProperties Where (Group_ID = '" + GID + "') And (ID = '" + PID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            if (DT.Rows[0][1].ToString().Trim() == "1")
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_07_Softwares_GroupProperties Set [Status_Code] = '0',[Status_Text] = 'Disabled',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GID + "') And (ID = '" + PID + "')");
                                ResSTR = "The " + PTA + " properties was successfully change status to disabled";
                            }
                            else
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_07_Softwares_GroupProperties Set [Status_Code] = '1',[Status_Text] = 'Active',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GID + "') And (ID = '" + PID + "')");
                                ResSTR = "The " + PTA + " properties was successfully change status to active";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified properties is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving properties information";
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
        public JsonResult Software_Group_Properties_SaveEdit(string GID, string PID, string GTA, string GUN, string GDC, string GIN, string GWD)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 90;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                GID = GID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                PID = PID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GTA = GTA.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GUN = GUN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GDC = GDC.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GIN = GIN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                GWD = GWD.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT_PID = new DataTable();
                    DT_PID = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_06_Softwares_Group Where (ID = '" + GID + "') And (Removed = '0')");
                    if (DT_PID.Rows != null)
                    {
                        if (DT_PID.Rows.Count != 1)
                        {
                            ResVal = "1"; ResSTR = "Base group information not identified";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "Base group information not identified";
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT_PID2 = new DataTable();
                    DT_PID2 = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_07_Softwares_GroupProperties Where (Group_ID = '" + GID + "') And (ID = '" + PID + "') And (Removed = '0')");
                    if (DT_PID2.Rows != null)
                    {
                        if (DT_PID2.Rows.Count != 1)
                        {
                            ResVal = "1"; ResSTR = "Properties information not identified";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "Properties information not identified";
                    }
                }
                if (ResVal == "0")
                {
                    if (GTA == "")
                    {
                        ResVal = "1"; ResSTR = "It is necessary to enter the tag to save properties changes";
                    }
                    else
                    {
                        GTA = Pb.Text_UpperCase_AfterSpase(GTA);
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_07_Softwares_GroupProperties Where (Group_ID = '" + GID + "') And (ID <> '" + PID + "') And (Tag = '" + GTA + "') And (Removed = '0')");
                    if (DT.Rows.Count == 0)
                    {
                        string InsDate = Sq.Sql_Date();
                        string InsTime = Sq.Sql_Time();
                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_07_Softwares_GroupProperties Set [Tag] = '" + GTA + "',[Unit] = '" + GUN + "',[Description] = '" + GDC + "',[RowIndex] = '" + GIN + "',[Width] = '" + GWD + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (Group_ID = '" + GID + "') And (ID = '" + PID + "') And (Removed = '0')");
                        ResVal = "0"; ResSTR = "Properties tag named " + GTA + " was successfully edited";
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The entered tag of the properties is duplicate, so it is not possible for you to save properties changes with this tag";
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
        public JsonResult Software_DeviceAdd_Grid(string GID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 54;
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
                    GID = GID.Trim();
                    DataTable DTUIDTID = new DataTable();
                    DTUIDTID = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_06_Softwares_Group Where (ID_UnicCode = '" + GID + "') And (Removed = '0')");
                    GID = DTUIDTID.Rows[0][0].ToString().Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Name,Master_Unic_ID,Model,Product_ID,Serial_Number,Build_Series_Number,Version,Situation_Code,Description,Extra_Unic_ID_1,Extra_Unic_ID_2,Extra_Unic_ID_3,Extra_Unic_ID_4,Status_Code,Situation_Text From Services_08_Softwares_List Where (Group_ID = '" + GID + "') And (Removed = '0') Order By Name,Master_Unic_ID,Product_ID,Serial_Number,Build_Series_Number,Version");
                    if (DT.Rows != null)
                    {
                        string STT = "badge-light-danger";
                        foreach (DataRow RW in DT.Rows)
                        {
                            ResSTR += "<tr>";
                            ResSTR += "<td onclick=\"DeviceAdd_ShowInfo('" + RW[0].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\"><i class=\"fa fa-desktop text-primary\" style=\"font-size:25px\"></i></td>";
                            ResSTR += "<td onclick=\"DeviceAdd_ShowInfo('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[1].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"DeviceAdd_ShowInfo('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[2].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"DeviceAdd_ShowInfo('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[3].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"DeviceAdd_ShowInfo('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[4].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"DeviceAdd_ShowInfo('" + RW[0].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[5].ToString().Trim() + "</td>";
                            switch (RW[8].ToString().Trim())
                            {
                                case "1": { STT = "badge-light-primary"; break; }
                                case "2": { STT = "badge-light-danger"; break; }
                                case "3": { STT = "badge-light-success"; break; }
                                case "4": { STT = "badge-light-warning"; break; }
                                case "5": { STT = "badge-light-warning"; break; }
                                case "6": { STT = "badge-light-primary"; break; }
                                case "7": { STT = "badge-light-info"; break; }
                                case "8": { STT = "badge-light-danger"; break; }
                                case "9": { STT = "badge-light-success"; break; }
                                case "10": { STT = "badge-light-danger"; break; }
                                case "11": { STT = "badge-light-info"; break; }
                                case "12": { STT = "badge-light-warning"; break; }
                                case "13": { STT = "badge-light-warning"; break; }
                                case "14": { STT = "badge-light-warning"; break; }
                                case "15": { STT = "badge-light-danger"; break; }
                                case "16": { STT = "badge-light-danger"; break; }
                                case "17": { STT = "badge-light-danger"; break; }
                                case "18": { STT = "badge-light-info"; break; }
                                case "19": { STT = "badge-light-warning"; break; }
                                case "20": { STT = "badge-light-warning"; break; }
                                case "21": { STT = "badge-light-info"; break; }
                                case "22": { STT = "badge-light-warning"; break; }
                                case "23": { STT = "badge-light-warning"; break; }
                                case "24": { STT = "badge-light-info"; break; }
                                case "25": { STT = "badge-light-danger"; break; }
                                case "26": { STT = "badge-light-info"; break; }
                                case "27": { STT = "badge-light-info"; break; }
                                case "28": { STT = "badge-light-danger"; break; }
                            };
                            ResSTR += "<td onclick=\"DeviceAdd_ShowInfo('" + RW[0].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                            ResSTR += "<div class=\"badge badge-pill " + STT + "\">" + RW[15].ToString().Trim() + "</div>";
                            ResSTR += "</td>";
                            if (RW[14].ToString().Trim() == "0")
                            {
                                ResSTR += "<td onclick=\"DeviceAdd_ShowInfo('" + RW[0].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:70px\">Disabled</div>";
                                ResSTR += "</td>";
                            }
                            else
                            {
                                ResSTR += "<td onclick=\"DeviceAdd_ShowInfo('" + RW[0].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:70px\">Active</div>";
                                ResSTR += "</td>";
                            }
                            ResSTR += "<td style=\"text-align:center\">";
                            ResSTR += "<div class=\"btn-group dropleft\">";
                            ResSTR += "<button type=\"button\" class=\"btn\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                            ResSTR += "<div class=\"MenuToolbox text-primary\"/>";
                            ResSTR += "</button>";
                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"DeviceAdd_Properties('" + RW[0].ToString().Trim() + "','" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-info-circle text-primary\" style=\"width:24px;font-size:14px\"></i>Software Properties</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"DeviceAdd_Comments('" + RW[0].ToString().Trim() + "','" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-sticky-note text-primary\" style=\"width:24px;font-size:14px\"></i>Activity Comments</a>";
                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"DeviceAdd_Remove('" + RW[0].ToString().Trim() + "','" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-trash-o text-primary\" style=\"width:24px;font-size:14px\"></i>Remove software</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"DeviceAdd_ChangeStatus('" + RW[0].ToString().Trim() + "','" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-refresh text-primary\" style=\"width:24px;font-size:14px\"></i>Change status</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"DeviceAdd_Edit('" + RW[0].ToString().Trim() + "','" + RW[1].ToString().Trim() + "','" + RW[2].ToString().Trim() + "','" + RW[3].ToString().Trim() + "','" + RW[4].ToString().Trim() + "','" + RW[5].ToString().Trim() + "','" + RW[6].ToString().Trim() + "','" + RW[7].ToString().Trim() + "','" + RW[8].ToString().Trim() + "','" + RW[9].ToString().Trim() + "','" + RW[10].ToString().Trim() + "','" + RW[11].ToString().Trim() + "','" + RW[12].ToString().Trim() + "','" + RW[13].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit software</a>";
                            ResSTR += "</div>";
                            ResSTR += "</div>";
                            ResSTR += "</td>";
                            ResSTR += "</tr>";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading group properties information";
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
        public JsonResult Software_DeviceAdd_AddNew(string A1, string A2, string A3, string A4, string A5, string A6, string A7, string A8, string A9, string A10, string A11, string A12, string A13, string A14, string A15)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 91;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                string Group_ID = "0";
                string Header_ID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                A1 = A1.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A2 = A2.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A3 = A3.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A4 = A4.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A5 = A5.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A6 = A6.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A7 = A7.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A8 = A8.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A9 = A9.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A10 = A10.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A11 = A11.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A12 = A12.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A13 = A13.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A14 = A14.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A15 = A15.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    if (A2 == "")
                    {
                        ResVal = "1"; ResSTR = "It is necessary to enter the tag name / unic key to add a new software";
                    }
                    else
                    {
                        A2 = Pb.Text_UpperCase_AfterSpase(A2);
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT_PID = new DataTable();
                    DT_PID = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_06_Softwares_Group Where (ID_UnicCode = '" + A1 + "') And (Removed = '0')");
                    if (DT_PID.Rows != null)
                    {
                        if (DT_PID.Rows.Count != 1)
                        {
                            ResVal = "1"; ResSTR = "Base group information not identified";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "Base group information not identified";
                    }
                    if (ResVal == "0") { Group_ID = DT_PID.Rows[0][0].ToString().Trim(); }
                }
                // Header ID :
                if (ResVal == "0")
                {
                    Header_ID = Group_ID;
                    bool ExiitWN = false;
                    while (ExiitWN == false)
                    {
                        DataTable DT_HDG = new DataTable();
                        DT_HDG = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Parent_ID,ID From Services_06_Softwares_Group Where (ID = '" + Header_ID + "') And (Removed = '0')");
                        if (DT_HDG.Rows != null)
                        {
                            if (DT_HDG.Rows.Count == 1)
                            {
                                Header_ID = DT_HDG.Rows[0][0].ToString().Trim();
                                if (Header_ID.Trim() == "0")
                                {
                                    Header_ID = DT_HDG.Rows[0][1].ToString().Trim();
                                    ExiitWN = true;
                                }
                            }
                            else
                            {
                                ResVal = "1"; ResSTR = "Header software group information not identified"; ExiitWN = true;
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "Header software group information not identified"; ExiitWN = true;
                        }
                    }
                }
                // ID Duplicated Values :
                if (ResVal == "0")
                {
                    if (A3 != "")
                    {
                        if (A3 == A12) { ResVal = "1"; ResSTR = "The value of master unic ID overlaps with ext unic ID #1"; }
                        if (A3 == A13) { ResVal = "1"; ResSTR = "The value of master unic ID overlaps with ext unic ID #2"; }
                        if (A3 == A14) { ResVal = "1"; ResSTR = "The value of master unic ID overlaps with ext unic ID #3"; }
                        if (A3 == A15) { ResVal = "1"; ResSTR = "The value of master unic ID overlaps with ext unic ID #4"; }
                    }
                    if (ResVal == "0")
                    {
                        if (A12 != "")
                        {
                            if (A12 == A3) { ResVal = "1"; ResSTR = "The value of ext unic ID #1 overlaps with master unic ID"; }
                            if (A12 == A13) { ResVal = "1"; ResSTR = "The value of ext unic ID #1  overlaps with ext unic ID #2"; }
                            if (A12 == A14) { ResVal = "1"; ResSTR = "The value of ext unic ID #1 overlaps with ext unic ID #3"; }
                            if (A12 == A15) { ResVal = "1"; ResSTR = "The value of ext unic ID #1 overlaps with ext unic ID #4"; }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A13 != "")
                        {
                            if (A13 == A3) { ResVal = "1"; ResSTR = "The value of ext unic ID #2 overlaps with master unic ID"; }
                            if (A13 == A12) { ResVal = "1"; ResSTR = "The value of ext unic ID #2  overlaps with ext unic ID #1"; }
                            if (A13 == A14) { ResVal = "1"; ResSTR = "The value of ext unic ID #2 overlaps with ext unic ID #3"; }
                            if (A13 == A15) { ResVal = "1"; ResSTR = "The value of ext unic ID #2 overlaps with ext unic ID #4"; }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A14 != "")
                        {
                            if (A14 == A3) { ResVal = "1"; ResSTR = "The value of ext unic ID #3 overlaps with master unic ID"; }
                            if (A14 == A12) { ResVal = "1"; ResSTR = "The value of ext unic ID #3  overlaps with ext unic ID #1"; }
                            if (A14 == A13) { ResVal = "1"; ResSTR = "The value of ext unic ID #3 overlaps with ext unic ID #2"; }
                            if (A14 == A15) { ResVal = "1"; ResSTR = "The value of ext unic ID #3 overlaps with ext unic ID #4"; }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A15 != "")
                        {
                            if (A15 == A3) { ResVal = "1"; ResSTR = "The value of ext unic ID #4 overlaps with master unic ID"; }
                            if (A15 == A12) { ResVal = "1"; ResSTR = "The value of ext unic ID #4  overlaps with ext unic ID #1"; }
                            if (A15 == A13) { ResVal = "1"; ResSTR = "The value of ext unic ID #4 overlaps with ext unic ID #2"; }
                            if (A15 == A14) { ResVal = "1"; ResSTR = "The value of ext unic ID #4 overlaps with ext unic ID #3"; }
                        }
                    }
                }
                // ID Duplicated SQL :
                if (ResVal == "0")
                {
                    if (A3 != "")
                    {
                        try
                        {
                            DataTable DTUM = new DataTable();
                            DTUM = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_08_Softwares_List Where (Master_Unic_ID = '" + A3 + "') or (Extra_Unic_ID_1 = '" + A3 + "') or (Extra_Unic_ID_2 = '" + A3 + "') or (Extra_Unic_ID_3 = '" + A3 + "') or (Extra_Unic_ID_4 = '" + A3 + "') And (Removed = '0')");
                            if (DTUM.Rows.Count != 0) { ResVal = "1"; ResSTR = "The master unic ID value is duplicated"; }
                        }
                        catch (Exception) { }
                    }
                    if (ResVal == "0")
                    {
                        if (A12 != "")
                        {
                            try
                            {
                                DataTable DTUM = new DataTable();
                                DTUM = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_08_Softwares_List Where (Master_Unic_ID = '" + A12 + "') or (Extra_Unic_ID_1 = '" + A12 + "') or (Extra_Unic_ID_2 = '" + A12 + "') or (Extra_Unic_ID_3 = '" + A12 + "') or (Extra_Unic_ID_4 = '" + A12 + "') And (Removed = '0')");
                                if (DTUM.Rows.Count != 0) { ResVal = "1"; ResSTR = "The ext unic ID #1 value is duplicated"; }
                            }
                            catch (Exception) { }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A13 != "")
                        {
                            try
                            {
                                DataTable DTUM = new DataTable();
                                DTUM = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_08_Softwares_List Where (Master_Unic_ID = '" + A13 + "') or (Extra_Unic_ID_1 = '" + A13 + "') or (Extra_Unic_ID_2 = '" + A13 + "') or (Extra_Unic_ID_3 = '" + A13 + "') or (Extra_Unic_ID_4 = '" + A13 + "') And (Removed = '0')");
                                if (DTUM.Rows.Count != 0) { ResVal = "1"; ResSTR = "The ext unic ID #2 value is duplicated"; }
                            }
                            catch (Exception) { }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A14 != "")
                        {
                            try
                            {
                                DataTable DTUM = new DataTable();
                                DTUM = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_08_Softwares_List Where (Master_Unic_ID = '" + A14 + "') or (Extra_Unic_ID_1 = '" + A14 + "') or (Extra_Unic_ID_2 = '" + A14 + "') or (Extra_Unic_ID_3 = '" + A14 + "') or (Extra_Unic_ID_4 = '" + A14 + "') And (Removed = '0')");
                                if (DTUM.Rows.Count != 0) { ResVal = "1"; ResSTR = "The ext unic ID #3 value is duplicated"; }
                            }
                            catch (Exception) { }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A15 != "")
                        {
                            try
                            {
                                DataTable DTUM = new DataTable();
                                DTUM = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_08_Softwares_List Where (Master_Unic_ID = '" + A15 + "') or (Extra_Unic_ID_1 = '" + A15 + "') or (Extra_Unic_ID_2 = '" + A15 + "') or (Extra_Unic_ID_3 = '" + A15 + "') or (Extra_Unic_ID_4 = '" + A15 + "') And (Removed = '0')");
                                if (DTUM.Rows.Count != 0) { ResVal = "1"; ResSTR = "The ext unic ID #4 value is duplicated"; }
                            }
                            catch (Exception) { }
                        }
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_08_Softwares_List Where (Group_ID = '" + Group_ID + "') And (Header_ID = '" + Header_ID + "') And (Name = '" + A2 + "') And (Removed = '0')");
                    if (DT.Rows.Count == 0)
                    {
                        string InsDate = Sq.Sql_Date();
                        string InsTime = Sq.Sql_Time();
                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Services_08_Softwares_List Values ('" + Group_ID.ToString() + "','" + Header_ID.ToString() + "','" + A2 + "','" + A3 + "','" + A4 + "','" + A5 + "','" + A6 + "','" + A7 + "','" + A8 + "','" + A9 + "','" + A10 + "','" + A11 + "','" + A12 + "','" + A13 + "','" + A14 + "','" + A15 + "','1','Active','" + InsDate + "','" + InsTime + "','" + UID + "','" + InsDate + "','" + InsTime + "','" + UID + "','0')");
                        ResVal = "0"; ResSTR = "Software " + A2 + " was successfully added";
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The entered tag name / unic key of the software is duplicate, so it is not possible for you to add new software with this tag name/ unic key";
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
        public JsonResult Software_DeviceAdd_Remove(string DID, string DNM)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 92;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                DID = DID.Trim();
                DNM = DNM.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_08_Softwares_List Where (ID = '" + DID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_08_Softwares_List Set [Removed] = '1',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DID + "')");
                            ResSTR = "The " + DNM + " Software was successfully deleted";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified software is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving software information";
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
        public JsonResult Software_DeviceAdd_ChangeStatus(string DID, string DNM)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 93;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                DID = DID.Trim();
                DNM = DNM.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Status_Code From Services_08_Softwares_List Where (ID = '" + DID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            if (DT.Rows[0][1].ToString().Trim() == "1")
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_08_Softwares_List Set [Status_Code] = '0',[Status_Text] = 'Disabled',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DID + "')");
                                ResSTR = "The " + DNM + " software was successfully change status to disabled";
                            }
                            else
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_08_Softwares_List Set [Status_Code] = '1',[Status_Text] = 'Active',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DID + "')");
                                ResSTR = "The " + DNM + " software was successfully change status to active";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified software is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving software information";
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
        public JsonResult Software_DeviceAdd_SaveEdit(string A1, string A2, string A3, string A4, string A5, string A6, string A7, string A8, string A9, string A10, string A11, string A12, string A13, string A14, string A15)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 94;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                A1 = A1.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A2 = A2.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A3 = A3.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A4 = A4.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A5 = A5.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A6 = A6.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A7 = A7.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A8 = A8.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A9 = A9.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A10 = A10.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A11 = A11.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A12 = A12.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A13 = A13.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A14 = A14.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                A15 = A15.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT_PID = new DataTable();
                    DT_PID = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_08_Softwares_List Where (ID = '" + A1 + "') And (Removed = '0')");
                    if (DT_PID.Rows != null)
                    {
                        if (DT_PID.Rows.Count != 1)
                        {
                            ResVal = "1"; ResSTR = "Base software information not identified";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "Base software information not identified";
                    }
                }
                if (ResVal == "0")
                {
                    if (A2 == "")
                    {
                        ResVal = "1"; ResSTR = "It is necessary to enter the tag name / unic key to save software changes";
                    }
                    else
                    {
                        A2 = Pb.Text_UpperCase_AfterSpase(A2);
                    }
                }
                // ID Duplicated Values :
                if (ResVal == "0")
                {
                    if (A3 != "")
                    {
                        if (A3 == A12) { ResVal = "1"; ResSTR = "The value of master unic ID overlaps with ext unic ID #1"; }
                        if (A3 == A13) { ResVal = "1"; ResSTR = "The value of master unic ID overlaps with ext unic ID #2"; }
                        if (A3 == A14) { ResVal = "1"; ResSTR = "The value of master unic ID overlaps with ext unic ID #3"; }
                        if (A3 == A15) { ResVal = "1"; ResSTR = "The value of master unic ID overlaps with ext unic ID #4"; }
                    }
                    if (ResVal == "0")
                    {
                        if (A12 != "")
                        {
                            if (A12 == A3) { ResVal = "1"; ResSTR = "The value of ext unic ID #1 overlaps with master unic ID"; }
                            if (A12 == A13) { ResVal = "1"; ResSTR = "The value of ext unic ID #1  overlaps with ext unic ID #2"; }
                            if (A12 == A14) { ResVal = "1"; ResSTR = "The value of ext unic ID #1 overlaps with ext unic ID #3"; }
                            if (A12 == A15) { ResVal = "1"; ResSTR = "The value of ext unic ID #1 overlaps with ext unic ID #4"; }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A13 != "")
                        {
                            if (A13 == A3) { ResVal = "1"; ResSTR = "The value of ext unic ID #2 overlaps with master unic ID"; }
                            if (A13 == A12) { ResVal = "1"; ResSTR = "The value of ext unic ID #2  overlaps with ext unic ID #1"; }
                            if (A13 == A14) { ResVal = "1"; ResSTR = "The value of ext unic ID #2 overlaps with ext unic ID #3"; }
                            if (A13 == A15) { ResVal = "1"; ResSTR = "The value of ext unic ID #2 overlaps with ext unic ID #4"; }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A14 != "")
                        {
                            if (A14 == A3) { ResVal = "1"; ResSTR = "The value of ext unic ID #3 overlaps with master unic ID"; }
                            if (A14 == A12) { ResVal = "1"; ResSTR = "The value of ext unic ID #3  overlaps with ext unic ID #1"; }
                            if (A14 == A13) { ResVal = "1"; ResSTR = "The value of ext unic ID #3 overlaps with ext unic ID #2"; }
                            if (A14 == A15) { ResVal = "1"; ResSTR = "The value of ext unic ID #3 overlaps with ext unic ID #4"; }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A15 != "")
                        {
                            if (A15 == A3) { ResVal = "1"; ResSTR = "The value of ext unic ID #4 overlaps with master unic ID"; }
                            if (A15 == A12) { ResVal = "1"; ResSTR = "The value of ext unic ID #4  overlaps with ext unic ID #1"; }
                            if (A15 == A13) { ResVal = "1"; ResSTR = "The value of ext unic ID #4 overlaps with ext unic ID #2"; }
                            if (A15 == A14) { ResVal = "1"; ResSTR = "The value of ext unic ID #4 overlaps with ext unic ID #3"; }
                        }
                    }
                }
                // ID Duplicated SQL :
                if (ResVal == "0")
                {
                    if (A3 != "")
                    {
                        try
                        {
                            DataTable DTUM = new DataTable();
                            DTUM = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_08_Softwares_List Where (ID <> '" + A1 + "') And ((Master_Unic_ID = '" + A3 + "') or (Extra_Unic_ID_1 = '" + A3 + "') or (Extra_Unic_ID_2 = '" + A3 + "') or (Extra_Unic_ID_3 = '" + A3 + "') or (Extra_Unic_ID_4 = '" + A3 + "')) And (Removed = '0')");
                            if (DTUM.Rows.Count != 0) { ResVal = "1"; ResSTR = "The master unic ID value is duplicated"; }
                        }
                        catch (Exception) { }
                    }
                    if (ResVal == "0")
                    {
                        if (A12 != "")
                        {
                            try
                            {
                                DataTable DTUM = new DataTable();
                                DTUM = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_08_Softwares_List Where (ID <> '" + A1 + "') And ((Master_Unic_ID = '" + A12 + "') or (Extra_Unic_ID_1 = '" + A12 + "') or (Extra_Unic_ID_2 = '" + A12 + "') or (Extra_Unic_ID_3 = '" + A12 + "') or (Extra_Unic_ID_4 = '" + A12 + "')) And (Removed = '0')");
                                if (DTUM.Rows.Count != 0) { ResVal = "1"; ResSTR = "The ext unic ID #1 value is duplicated"; }
                            }
                            catch (Exception) { }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A13 != "")
                        {
                            try
                            {
                                DataTable DTUM = new DataTable();
                                DTUM = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_08_Softwares_List Where (ID <> '" + A1 + "') And ((Master_Unic_ID = '" + A13 + "') or (Extra_Unic_ID_1 = '" + A13 + "') or (Extra_Unic_ID_2 = '" + A13 + "') or (Extra_Unic_ID_3 = '" + A13 + "') or (Extra_Unic_ID_4 = '" + A13 + "')) And (Removed = '0')");
                                if (DTUM.Rows.Count != 0) { ResVal = "1"; ResSTR = "The ext unic ID #2 value is duplicated"; }
                            }
                            catch (Exception) { }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A14 != "")
                        {
                            try
                            {
                                DataTable DTUM = new DataTable();
                                DTUM = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_08_Softwares_List Where (ID <> '" + A1 + "') And ((Master_Unic_ID = '" + A14 + "') or (Extra_Unic_ID_1 = '" + A14 + "') or (Extra_Unic_ID_2 = '" + A14 + "') or (Extra_Unic_ID_3 = '" + A14 + "') or (Extra_Unic_ID_4 = '" + A14 + "')) And (Removed = '0')");
                                if (DTUM.Rows.Count != 0) { ResVal = "1"; ResSTR = "The ext unic ID #3 value is duplicated"; }
                            }
                            catch (Exception) { }
                        }
                    }
                    if (ResVal == "0")
                    {
                        if (A15 != "")
                        {
                            try
                            {
                                DataTable DTUM = new DataTable();
                                DTUM = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_08_Softwares_List Where (ID <> '" + A1 + "') And ((Master_Unic_ID = '" + A15 + "') or (Extra_Unic_ID_1 = '" + A15 + "') or (Extra_Unic_ID_2 = '" + A15 + "') or (Extra_Unic_ID_3 = '" + A15 + "') or (Extra_Unic_ID_4 = '" + A15 + "')) And (Removed = '0')");
                                if (DTUM.Rows.Count != 0) { ResVal = "1"; ResSTR = "The ext unic ID #4 value is duplicated"; }
                            }
                            catch (Exception) { }
                        }
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_08_Softwares_List Where (ID <> '" + A1 + "') And (Name = '" + A2 + "') And (Removed = '0')");
                    if (DT.Rows.Count == 0)
                    {
                        string InsDate = Sq.Sql_Date();
                        string InsTime = Sq.Sql_Time();
                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_08_Softwares_List Set [Name] = '" + A2 + "',[Master_Unic_ID] = '" + A3 + "',[Model] = '" + A4 + "',[Product_ID] = '" + A5 + "',[Serial_Number] = '" + A6 + "',[Build_Series_Number] = '" + A7 + "',[Version] = '" + A8 + "',[Situation_Code] = '" + A9 + "',[Situation_Text] = '" + A10 + "',[Description] = '" + A11 + "',[Extra_Unic_ID_1] = '" + A12 + "',[Extra_Unic_ID_2] = '" + A13 + "',[Extra_Unic_ID_3] = '" + A14 + "',[Extra_Unic_ID_4] = '" + A15 + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + A1 + "') And (Removed = '0')");
                        ResVal = "0"; ResSTR = "Software named " + A2 + " was successfully edited";
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The entered tag name / unic key of the software is duplicate, so it is not possible for you to save software changes with this tag name / unic key";
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
        public JsonResult Software_DeviceAdd_Information(string DID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 95;
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
                    DID = DID.Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Top 1 * From Services_08_Softwares_List_V Where (ID = '" + DID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            ResSTR = DT.Rows[0][5].ToString().Trim() + "#";
                            ResSTR += "<b class=\"text-primary\">Software Type : </b>" + DT.Rows[0][4].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Software Group : </b>" + DT.Rows[0][2].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Software Name : </b>" + DT.Rows[0][5].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Master Unic ID : </b>" + DT.Rows[0][6].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Model : </b>" + DT.Rows[0][7].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Product ID : </b>" + DT.Rows[0][8].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Serial Number : </b>" + DT.Rows[0][9].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Build Series Number : </b>" + DT.Rows[0][10].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Version : </b>" + DT.Rows[0][11].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Situation : </b>" + DT.Rows[0][13].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Extra Unic ID 1 : </b>" + DT.Rows[0][15].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Extra Unic ID 2 : </b>" + DT.Rows[0][16].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Extra Unic ID 3 : </b>" + DT.Rows[0][17].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Extra Unic ID 4 : </b>" + DT.Rows[0][18].ToString().Trim() + "<br>";
                            ResSTR += "<b class=\"text-primary\">Description : </b>" + DT.Rows[0][14].ToString().Trim() + "<br>";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The server encountered an error while reloading software information";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading software information";
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
        public JsonResult Software_DeviceAdd_GetPropertiesData(string DID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 96;
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
                    int ObjectCount = 0;
                    DID = DID.Trim();
                    DataTable DT = new DataTable();
                    bool ExitNow = false;
                    List<string> GroupID = new List<string>();
                    DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Group_ID From Services_08_Softwares_List Where (ID = '" + DID + "') And (Removed = '0')");
                    string PDID = DT.Rows[0][0].ToString().Trim();
                    while (ExitNow == false)
                    {
                        DT = new DataTable();
                        DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Parent_ID,Name From Services_06_Softwares_Group Where (ID = '" + PDID + "') And (Removed = '0')");
                        if (DT.Rows.Count == 1)
                        {
                            if (DT.Rows[0][0].ToString().Trim() == "0") { ExitNow = true; }
                            GroupID.Add(PDID + "#" + DT.Rows[0][1].ToString().Trim());
                            PDID = DT.Rows[0][0].ToString().Trim();
                        }
                        else
                        {
                            ExitNow = true;
                        }
                    }
                    ResSTR = "";
                    if (GroupID.Count > 0)
                    {
                        DataTable DTVal = new DataTable();
                        ResSTR += "<div class=\"col-lg-12 form-inline\">";
                        for (int i = (GroupID.Count - 1); i >= 0; i--)
                        {
                            string[] GroupInfo = GroupID[i].Split('#');
                            DT = new DataTable();
                            DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Tag,Unit,Description,Width,ID From Services_07_Softwares_GroupProperties Where (Group_ID = '" + GroupInfo[0] + "') And (Status_Code = '1') And (Removed = '0') Order By RowIndex,Tag,ID");
                            if (DT.Rows.Count > 0)
                            {
                                ObjectCount++;
                                ResSTR += "<div class=\"divider divider-primary col-lg-12\">";
                                ResSTR += "<div class=\"divider-text\">" + GroupInfo[1].ToString().Trim() + "</div>";
                                ResSTR += "</div>";
                                foreach (DataRow RW in DT.Rows)
                                {
                                    string TxtValue = "";
                                    try
                                    {
                                        DTVal = new DataTable();
                                        DTVal = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Value From Services_09_Softwares_Properties Where (Software_ID = '" + DID + "') And (Properties_ID = '" + RW[4].ToString().Trim() + "')");
                                        if (DTVal.Rows.Count == 1) { TxtValue = DTVal.Rows[0][0].ToString().Trim(); }
                                    }
                                    catch (Exception) { }
                                    ResSTR += "<div class=\"col-lg-" + RW[3].ToString().Trim() + "\" style=\"opacity:1;pointer-events:auto\">";
                                    ResSTR += "<div class=\"text-bold-300 font-medium-1\" style=\"margin-top:10px;margin-left:5px\">";
                                    ResSTR += RW[0].ToString().Trim();
                                    if (RW[1].ToString().Trim() != "") { ResSTR += " [ " + RW[1].ToString().Trim() + " ]"; }
                                    ResSTR += "</div>";
                                    ResSTR += "<fieldset class=\"form-group position-relative\" style=\"width:100%\">";
                                    ResSTR += "<input type=\"text\" class=\"form-control GSelTXTAM\" id=\"DP_" + RW[4].ToString().Trim() + "\" placeholder=\"" + RW[2].ToString().Trim() + "\" style=\"width:100%\" maxlength=\"200\" value=\"" + TxtValue + "\">";
                                    ResSTR += "</fieldset>";
                                    ResSTR += "</div>";
                                }
                            }
                        }
                        ResSTR += "</div>";
                    }
                    if (ObjectCount == 0) { ResSTR = "<b>Dear admin ...</b><br>No properties have been defined for this software in the past"; }
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
        public void Software_DeviceAdd_PropertiesAdd(string DID, string PID, string PVal)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 97;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return; }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                DID = DID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                PID = PID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                PVal = PVal.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                Sq.Execute_TSql(DataBase_Selector.Administrator, "Delete From Services_09_Softwares_Properties Where (Software_ID = '" + DID + "') And (Properties_ID = '" + PID + "')");
                Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Services_09_Softwares_Properties Values ('" + DID + "','" + PID + "','" + PVal + "')");
            }
            catch (Exception) { }
        }

        [HttpPost]
        public JsonResult Software_DeviceAdd_GetCommentsData(string DID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 98;
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
                    DID = DID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    int ObjectCount = 0;
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Ins_Date,Ins_Time,Ins_ID,Ins_ID_Name,Degree_Code,Comment,Status_Code,ID From Services_10_Softwares_Comment Where (Software_ID = '" + DID + "') And (Removed = '0') Order By Ins_Date desc,Ins_Time desc");
                    if (DT.Rows.Count > 0)
                    {
                        ResSTR += "<div class=\"accordion\" id=\"EmasColApp\">";
                        foreach (DataRow RW in DT.Rows)
                        {
                            ObjectCount++;
                            ResSTR += "<div class=\"collapse-margin\">";
                            ResSTR += "<div class=\"card-header\" id=\"heading" + ObjectCount.ToString() + "\" data-toggle=\"collapse\" role=\"button\" data-target=\"#ColappN" + ObjectCount.ToString() + "\" aria-expanded=\"false\" aria-controls=\"ColappN" + ObjectCount.ToString() + "\">";
                            ResSTR += "<span class=\"lead collapse-title\">";
                            ResSTR += "<i class=\"fa fa-calendar\" style=\"margin-right:10px\"></i>" + RW[0].ToString().Substring(0, 10).Trim() + "<i class=\"fa fa-clock-o\" style=\"margin-right:10px;margin-left:30px\"></i>" + RW[1].ToString().Trim() + "<i class=\"fa fa-user\" style=\"margin-right:10px;margin-left:30px\"></i>" + RW[2].ToString().Trim() + " - " + RW[3].ToString().Trim();
                            ResSTR += "</span>";
                            switch (RW[4].ToString().Trim())
                            {
                                case "1": { ResSTR += "<div class=\"badge badge-pill badge-glow badge-primary\" style=\"width:95px;margin-left:30px\">Normal</div>"; break; }
                                case "2": { ResSTR += "<div class=\"badge badge-pill badge-glow badge-info\" style=\"width:95px;margin-left:30px\">information</div>"; break; }
                                case "3": { ResSTR += "<div class=\"badge badge-pill badge-glow badge-warning\" style=\"width:95px;margin-left:30px\">Warning</div>"; break; }
                                case "4": { ResSTR += "<div class=\"badge badge-pill badge-glow badge-danger\" style=\"width:95px;margin-left:30px\">Important</div>"; break; }
                                case "5": { ResSTR += "<div class=\"badge badge-pill badge-glow badge-success\" style=\"width:95px;margin-left:30px\">Successfull</div>"; break; }
                            }
                            if (RW[6].ToString().Trim() == "1")
                            {
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:70px;margin-left:30px\">Active</div>";
                            }
                            else
                            {
                                ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:70px;margin-left:30px\">Disabled</div>";
                            }
                            ResSTR += "</div>";
                            ResSTR += "<div id=\"ColappN" + ObjectCount.ToString() + "\" class=\"collapse\" aria-labelledby=\"heading" + ObjectCount.ToString() + "\" data-parent=\"#EmasColApp\">";
                            ResSTR += "<div class=\"card-body\">";
                            ResSTR += RW[5].ToString().Trim();
                            ResSTR += "</div>";
                            ResSTR += "<div class=\"card-footer col-lg-12\">";
                            ResSTR += "<a class=\"text-danger col-lg-2\" href=\"javascript:void(0)\" onclick=\"DeviceAdd_CommentRemove('" + DID + "','" + RW[7].ToString().Trim() + "')\"><i class=\"fa fa-trash\" style=\"margin-right:10px\"></i>Remove this comment</a>";
                            ResSTR += "<a class=\"text-info col-lg-2\" href=\"javascript:void(0)\" onclick=\"DeviceAdd_CommentChangeStatus('" + DID + "','" + RW[7].ToString().Trim() + "')\"><i class=\"fa fa-refresh\" style=\"margin-right:10px\"></i>Change status</a>";
                            ResSTR += "</div>";
                            ResSTR += "</div>";
                            ResSTR += "</div>";
                        }
                        ResSTR += "</div>";
                    }
                    if (ObjectCount == 0) { ResSTR = "<b>Dear admin ...</b><br>No comment have been added for this software in the past"; }
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
        public JsonResult Software_DeviceAdd_CommentADD(string DID, string CMT, string DVL, string DTX)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 99;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                string UNM = "";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                try { UNM = Session["Admin_UNM"].ToString().Trim(); } catch (Exception) { UNM = "0"; }
                UID = UID.Trim();
                UNM = UNM.Trim();
                DID = DID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                CMT = CMT.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DVL = DVL.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DTX = DTX.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_08_Softwares_List Where (ID = '" + DID + "') And (Removed = '0')");
                    if (DT.Rows.Count == 1)
                    {
                        string InsDate = Sq.Sql_Date();
                        string InsTime = Sq.Sql_Time();
                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Services_10_Softwares_Comment Values ('" + DID + "','" + CMT + "','" + DVL + "','" + DTX + "','1','Active','" + InsDate + "','" + InsTime + "','" + UID + "','" + UNM + "','" + InsDate + "','" + InsTime + "','" + UID + "','0','','','0','')");
                        ResVal = "0"; ResSTR = "Software's new comment was successfully added";
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The software could not be identified";
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
        public JsonResult Software_DeviceAdd_CommentChangeStatus(string DID, string CID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 100;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                DID = DID.Trim();
                CID = CID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Status_Code From Services_10_Softwares_Comment Where (ID = '" + CID + "') And (Software_ID = '" + DID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            if (DT.Rows[0][1].ToString().Trim() == "1")
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_10_Softwares_Comment Set [Status_Code] = '0',[Status_Text] = 'Disabled',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + CID + "') And (Software_ID = '" + DID + "')");
                                ResSTR = "The comment was successfully change status to disabled";
                            }
                            else
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_10_Softwares_Comment Set [Status_Code] = '1',[Status_Text] = 'Active',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + CID + "') And (Software_ID = '" + DID + "')");
                                ResSTR = "The comment was successfully change status to active";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified comment is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving comment information";
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
        public JsonResult Software_DeviceAdd_CommentRemove(string DID, string CID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 101;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                string UNM = "";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                try { UNM = Session["Admin_UNM"].ToString().Trim(); } catch (Exception) { UNM = "0"; }
                UID = UID.Trim();
                UNM = UNM.Trim();
                DID = DID.Trim();
                CID = CID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Services_10_Softwares_Comment Where (ID = '" + CID + "') And (Software_ID = '" + DID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Services_10_Softwares_Comment Set [Removed] = '1',[Removed_Date] = '" + InsDate + "',[Removed_Time] = '" + InsTime + "',[Removed_ID] = '" + UID + "',[Removed_ID_Name] = '" + UNM + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + CID + "') And (Software_ID = '" + DID + "')");
                            ResSTR = "The comment software was successfully deleted";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified comment is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving comment information";
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
        // Software List Search :
        public ActionResult Softwares_List()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 55;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" }); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------




                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" });
            }
        }

        //====================================================================================================================
        // Shopping Center :
        //====================================================================================================================
    }
}