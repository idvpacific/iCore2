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
    public class CustomersController : Controller
    {
        //====================================================================================================================
        ActionAuthentication AAuth = new ActionAuthentication();
        SQL_Tranceiver Sq = new SQL_Tranceiver();
        PublicFunctions Pb = new PublicFunctions();
        EmailSender Email = new EmailSender();
        MimeTypeMap MTM = new MimeTypeMap();
        Crypto Cry = new Crypto();
        //====================================================================================================================
        public ActionResult Index() { return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" }); }
        //====================================================================================================================
        // Type :
        // 1 : Group 
        // 2 : Customer
        // 3 : User
        // 4 : Device 
        //-------------------------------------
        // Status :
        // 0 : Disabled
        // 1 : Active
        //-------------------------------------
        // Verified :
        // 1 : Email send failed - !
        // 2 : Email sent but not verified - No
        // 3 : Email seen - Eye
        // 4 : Email not correct - X
        // 5 : Email verifed - Yes
        //-------------------------------------
        // Removed :
        // 0 : No and show to table
        // 1 : Yes and dont show
        //-------------------------------------
        public ActionResult Management()
        {
            try
            {


                Session["Admin_UID"] = "101";



                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 47;
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
                        DTConvert = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,ID_UnicCode,Name,Parent_ID From Customers_01_Group_User Where (ID_UnicCode = '" + Parent_IDUC + "') And (Removed = '0')");
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
                                        DT_Breadcrumb = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID_UnicCode,Name,Parent_ID,Removed From Customers_01_Group_User Where (ID = '" + PrntID + "')");
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
                                return RedirectToAction("Management", "Customers", new { id = "", area = "ManagementPortal" });
                            }
                        }
                        else
                        {
                            return RedirectToAction("Management", "Customers", new { id = "", area = "ManagementPortal" });
                        }
                    }
                    catch (Exception)
                    {
                        return RedirectToAction("Management", "Customers", new { id = "", area = "ManagementPortal" });
                    }
                }
                if (GroupTreeRemoved == true) { return RedirectToAction("Management", "Customers", new { id = "", area = "ManagementPortal" }); }
                DataTable DT = new DataTable();
                DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,ID_UnicCode,Type_Code,Name,Description_Email,Status_Code,Verify_Code,LName From Customers_01_Group_User Where (Parent_ID = '" + Parent_ID + "') And (Removed = '0') Order By Type_Code,Name,LName,Ins_Date,Ins_Time");
                ViewBag.DT = DT.Rows;
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
        public JsonResult Management_Grid(string PID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 12;
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
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,ID_UnicCode,Type_Code,Name,Description_Email,Status_Code,Verify_Code,LName From Customers_01_Group_User Where (Parent_ID = '" + PID + "') And (Removed = '0') Order By Type_Code,Name,LName,Ins_Date,Ins_Time");
                    if (DT.Rows != null)
                    {
                        foreach (DataRow RW in DT.Rows)
                        {
                            if (RW[2].ToString().Trim() == "1")
                            {
                                ResSTR += "<tr>";
                                ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\"><i class=\"fa fa-users text-primary\" style=\"font-size:25px\"></i></td>";
                                ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">Group</td>";
                                ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[3].ToString().Trim() + "</td>";
                                ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[4].ToString().Trim() + "</td>";
                                if (RW[5].ToString().Trim() == "0")
                                {
                                    ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                    ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:70px\">Disabled</div>";
                                    ResSTR += "</td>";
                                }
                                else
                                {
                                    ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                    ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:70px\">Active</div>";
                                    ResSTR += "</td>";
                                }
                                ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:35px\">Yes</div>";
                                ResSTR += "</td>";
                                ResSTR += "<td style=\"text-align:center\">";
                                ResSTR += "<div class=\"btn-group dropleft\">";
                                ResSTR += "<button type=\"button\" class=\"btn\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                                ResSTR += "<div class=\"MenuToolbox text-primary\"/>";
                                ResSTR += "</button>";
                                ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                                ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-search text-primary\" style=\"width:24px;font-size:14px\"></i>Show subgroup</a>";
                                ResSTR += "<div class=\"dropdown-divider\"></div>";
                                ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Group_Remove('" + RW[1].ToString().Trim() + "','" + RW[3].ToString().Trim() + "')\"><i class=\"fa fa-trash-o text-primary\" style=\"width:24px;font-size:14px\"></i>Remove group</a>";
                                ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Group_ChangeStatus('" + RW[1].ToString().Trim() + "','" + RW[3].ToString().Trim() + "')\"><i class=\"fa fa-refresh text-primary\" style=\"width:24px;font-size:14px\"></i>Change status</a>";
                                ResSTR += "<div class=\"dropdown-divider\"></div>";
                                ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Group_Edit('" + RW[1].ToString().Trim() + "','" + RW[3].ToString().Trim() + "','" + RW[4].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit group</a>";
                                ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Group_Permission('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-id-card-o text-primary\" style=\"width:24px;font-size:14px\"></i>Group permission</a>";
                                ResSTR += "</div>";
                                ResSTR += "</div>";
                                ResSTR += "</td>";
                                ResSTR += "</tr>";
                            }
                            else
                            {
                                ResSTR += "<tr>";
                                ResSTR += "<td onclick=\"Admin_ShowInformation('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                try
                                {
                                    string AvatarFileAddress = HttpContext.Server.MapPath("~/Image/AdminAvatar/" + RW[0].ToString().Trim() + ".png");
                                    if (System.IO.File.Exists(AvatarFileAddress) == true)
                                    {
                                        string AvatarFileFormat = RW[0].ToString().Trim() + ".png";
                                        ResSTR += "<img class=\"round\" src=\"~/Image/AdminAvatar/" + AvatarFileFormat + "\" alt=\"avatar\" height=\"35\" width=\"35\">";
                                    }
                                    else
                                    {
                                        ResSTR += "<i class=\"fa fa-user-circle-o text-primary\" style=\"font-size:30px\"></i>";
                                    }
                                }
                                catch (Exception) { ResSTR += "<i class=\"fa fa-user-circle-o text-primary\" style=\"font-size:30px\"></i>"; }
                                ResSTR += "</td>";
                                ResSTR += "<td onclick=\"Admin_ShowInformation('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">Admin</td>";
                                ResSTR += "<td onclick=\"Admin_ShowInformation('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[3].ToString().Trim() + " " + RW[7].ToString().Trim() + "</td>";
                                ResSTR += "<td onclick=\"Admin_ShowInformation('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[4].ToString().Trim() + "</td>";
                                if (RW[5].ToString().Trim() == "0")
                                {
                                    ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                    ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:70px\">Disabled</div>";
                                    ResSTR += "</td>";
                                }
                                else
                                {
                                    ResSTR += "<td onclick=\"Group_Show('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                    ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:70px\">Active</div>";
                                    ResSTR += "</td>";
                                }
                                switch (RW[6].ToString().Trim())
                                {
                                    case "1": //Email send failed - !
                                        {
                                            ResSTR += "<td onclick=\"Admin_ShowInformation('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                            ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:35px\"><i class=\"fa fa-exclamation\" style=\"font-size:14px\"></i></div>";
                                            ResSTR += "</td>";
                                            ResSTR += "<td style=\"text-align:center\">";
                                            ResSTR += "<div class=\"btn-group dropleft\">";
                                            ResSTR += "<button type=\"button\" class=\"btn\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                                            ResSTR += "<div class=\"MenuToolbox text-primary\"/>";
                                            ResSTR += "</button>";
                                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_ResendVerifyEmail('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-envelope text-danger\" style=\"width:24px;font-size:14px\"></i>Resend verify email</a>";
                                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_ShowInformation('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-search text-primary\" style=\"width:24px;font-size:14px\"></i>Show information</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_Edit('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit admin</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_Remove('" + RW[1].ToString().Trim() + "','" + RW[3].ToString().Trim() + " " + RW[7].ToString().Trim() + "')\"><i class=\"fa fa-trash-o text-primary\" style=\"width:24px;font-size:14px\"></i>Remove admin</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_ChangeStatus('" + RW[1].ToString().Trim() + "','" + RW[3].ToString().Trim() + " " + RW[7].ToString().Trim() + "')\"><i class=\"fa fa-refresh text-primary\" style=\"width:24px;font-size:14px\"></i>Change status</a>";
                                            ResSTR += "</div>";
                                            ResSTR += "</div>";
                                            ResSTR += "</td>";
                                            break;
                                        }
                                    case "2": //Email sent but not verified - No
                                        {
                                            ResSTR += "<td onclick=\"Admin_ShowInformation('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                            ResSTR += "<div class=\"badge badge-pill badge-light-warning\" style=\"width:35px\">No</div>";
                                            ResSTR += "</td>";
                                            ResSTR += "<td style=\"text-align:center\">";
                                            ResSTR += "<div class=\"btn-group dropleft\">";
                                            ResSTR += "<button type=\"button\" class=\"btn\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                                            ResSTR += "<div class=\"MenuToolbox text-primary\"/>";
                                            ResSTR += "</button>";
                                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_ShowInformation('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-search text-primary\" style=\"width:24px;font-size:14px\"></i>Show information</a>";
                                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_Edit('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit admin</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_Permission('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-address-card-o text-primary\" style=\"width:24px;font-size:14px\"></i>Manage permission</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_Remove('" + RW[1].ToString().Trim() + "','" + RW[3].ToString().Trim() + " " + RW[7].ToString().Trim() + "')\"><i class=\"fa fa-trash-o text-primary\" style=\"width:24px;font-size:14px\"></i>Remove admin</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_ChangeStatus('" + RW[1].ToString().Trim() + "','" + RW[3].ToString().Trim() + " " + RW[7].ToString().Trim() + "')\"><i class=\"fa fa-refresh text-primary\" style=\"width:24px;font-size:14px\"></i>Change status</a>";
                                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_PersonnelForm('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-file-text-o text-primary\" style=\"width:24px;font-size:14px\"></i>Send personnel form</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_NewNotification('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-bell-o text-primary\" style=\"width:24px;font-size:14px\"></i>Add new notification</a>";
                                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_NewTask('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-check-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Add new task</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_Chatbox('" + RW[1].ToString().Trim() + "')\"><i class=\"fa feather icon-message-square text-primary\" style=\"width:24px;font-size:14px\"></i>Send message</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_NewEmail('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-envelope-open-o text-primary\" style=\"width:24px;font-size:14px\"></i>Send new email</a>";
                                            ResSTR += "</div>";
                                            ResSTR += "</div>";
                                            ResSTR += "</div>";
                                            ResSTR += "</td>";
                                            break;
                                        }
                                    case "3": //Email seen - Eye
                                        {
                                            ResSTR += "<td onclick=\"Admin_ShowInformation('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                            ResSTR += "<div class=\"badge badge-pill badge-light-warning\" style=\"width:35px\"><i class=\"fa fa-eye\" style=\"font-size:14px\"></i></div>";
                                            ResSTR += "</td>";
                                            ResSTR += "<td style=\"text-align:center\">";
                                            ResSTR += "<div class=\"btn-group dropleft\">";
                                            ResSTR += "<button type=\"button\" class=\"btn\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                                            ResSTR += "<div class=\"MenuToolbox text-primary\"/>";
                                            ResSTR += "</button>";
                                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_ShowInformation('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-search text-primary\" style=\"width:24px;font-size:14px\"></i>Show information</a>";
                                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_Edit('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit admin</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_Permission('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-address-card-o text-primary\" style=\"width:24px;font-size:14px\"></i>Manage permission</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_Remove('" + RW[1].ToString().Trim() + "','" + RW[3].ToString().Trim() + " " + RW[7].ToString().Trim() + "')\"><i class=\"fa fa-trash-o text-primary\" style=\"width:24px;font-size:14px\"></i>Remove admin</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_ChangeStatus('" + RW[1].ToString().Trim() + "','" + RW[3].ToString().Trim() + " " + RW[7].ToString().Trim() + "')\"><i class=\"fa fa-refresh text-primary\" style=\"width:24px;font-size:14px\"></i>Change status</a>";
                                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_PersonnelForm('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-file-text-o text-primary\" style=\"width:24px;font-size:14px\"></i>Send personnel form</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_NewNotification('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-bell-o text-primary\" style=\"width:24px;font-size:14px\"></i>Add new notification</a>";
                                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_NewTask('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-check-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Add new task</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_Chatbox('" + RW[1].ToString().Trim() + "')\"><i class=\"fa feather icon-message-square text-primary\" style=\"width:24px;font-size:14px\"></i>Send message</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_NewEmail('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-envelope-open-o text-primary\" style=\"width:24px;font-size:14px\"></i>Send new email</a>";
                                            ResSTR += "</div>";
                                            ResSTR += "</div>";
                                            ResSTR += "</div>";
                                            ResSTR += "</td>";
                                            break;
                                        }
                                    case "4": //Email not correct - X
                                        {
                                            ResSTR += "<td onclick=\"Admin_ShowInformation('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                            ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:35px\"><i class=\"fa fa-close\" style=\"font-size:14px\"></i></div>";
                                            ResSTR += "</td>";
                                            ResSTR += "<td style=\"text-align:center\">";
                                            ResSTR += "<div class=\"btn-group dropleft\">";
                                            ResSTR += "<button type=\"button\" class=\"btn\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                                            ResSTR += "<div class=\"MenuToolbox text-primary\"/>";
                                            ResSTR += "</button>";
                                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_ResendVerifyEmail('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-envelope text-primary\" style=\"width:24px;font-size:14px\"></i>Resend verify email</a>";
                                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_ShowInformation('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-search text-primary\" style=\"width:24px;font-size:14px\"></i>Show information</a>";
                                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_Edit('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit admin</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_Permission('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-address-card-o text-primary\" style=\"width:24px;font-size:14px\"></i>Manage permission</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_Remove('" + RW[1].ToString().Trim() + "','" + RW[3].ToString().Trim() + " " + RW[7].ToString().Trim() + "')\"><i class=\"fa fa-trash-o text-primary\" style=\"width:24px;font-size:14px\"></i>Remove admin</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_ChangeStatus('" + RW[1].ToString().Trim() + "','" + RW[3].ToString().Trim() + " " + RW[7].ToString().Trim() + "')\"><i class=\"fa fa-refresh text-primary\" style=\"width:24px;font-size:14px\"></i>Change status</a>";
                                            ResSTR += "</div>";
                                            ResSTR += "</div>";
                                            ResSTR += "</td>";
                                            break;
                                        }
                                    case "5": //Email verifed - Yes
                                        {
                                            ResSTR += "<td onclick=\"Admin_ShowInformation('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                            ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:35px\">Yes</div>";
                                            ResSTR += "</td>";
                                            ResSTR += "<td style=\"text-align:center\">";
                                            ResSTR += "<div class=\"btn-group dropleft\">";
                                            ResSTR += "<button type=\"button\" class=\"btn\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                                            ResSTR += "<div class=\"MenuToolbox text-primary\"/>";
                                            ResSTR += "</button>";
                                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_ShowInformation('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-search text-primary\" style=\"width:24px;font-size:14px\"></i>Show information</a>";
                                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_Edit('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit admin</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_Permission('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-address-card-o text-primary\" style=\"width:24px;font-size:14px\"></i>Manage permission</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_Remove('" + RW[1].ToString().Trim() + "','" + RW[3].ToString().Trim() + " " + RW[7].ToString().Trim() + "')\"><i class=\"fa fa-trash-o text-primary\" style=\"width:24px;font-size:14px\"></i>Remove admin</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_ChangeStatus('" + RW[1].ToString().Trim() + "','" + RW[3].ToString().Trim() + " " + RW[7].ToString().Trim() + "')\"><i class=\"fa fa-refresh text-primary\" style=\"width:24px;font-size:14px\"></i>Change status</a>";
                                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_PersonnelForm('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-file-text-o text-primary\" style=\"width:24px;font-size:14px\"></i>Send personnel form</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_NewNotification('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-bell-o text-primary\" style=\"width:24px;font-size:14px\"></i>Add new notification</a>";
                                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_NewTask('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-check-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Add new task</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_Chatbox('" + RW[1].ToString().Trim() + "')\"><i class=\"fa feather icon-message-square text-primary\" style=\"width:24px;font-size:14px\"></i>Send message</a>";
                                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Admin_NewEmail('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-envelope-open-o text-primary\" style=\"width:24px;font-size:14px\"></i>Send new email</a>";
                                            ResSTR += "</div>";
                                            ResSTR += "</div>";
                                            ResSTR += "</div>";
                                            ResSTR += "</td>";
                                            break;
                                        }
                                }
                                ResSTR += "</tr>";
                            }
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloadind admin/group information";
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

        //[HttpPost]
        //public JsonResult Group_Addnew(string ParentID, string Name, string Desc)
        //{
        //    try
        //    {
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        // Test Menu Access :
        //        ViewBag.MenuCode = 12;
        //        if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        string ResVal = "0";
        //        string ResSTR = "";
        //        string UID = "0";
        //        string ParentID = "0";
        //        try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
        //        UID = UID.Trim();
        //        ParentIDUC = ParentIDUC.Trim();
        //        GroupName = GroupName.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
        //        GroupDescription = GroupDescription.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
        //        if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (ResVal == "0")
        //        {
        //            if (ParentIDUC != "0")
        //            {
        //                DataTable DT_PID = new DataTable();
        //                DT_PID = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Customers_01_Group_User Where (ID_UnicCode = '" + ParentIDUC + "')");
        //                if (DT_PID.Rows != null)
        //                {
        //                    if (DT_PID.Rows.Count == 1)
        //                    {
        //                        ParentID = DT_PID.Rows[0][0].ToString().Trim();
        //                    }
        //                    else
        //                    {
        //                        ResVal = "1"; ResSTR = "Parent group information not identified";
        //                    }
        //                }
        //                else
        //                {
        //                    ResVal = "1"; ResSTR = "Parent group information not identified";
        //                }
        //            }
        //            else
        //            {
        //                ParentID = "0";
        //            }
        //        }
        //        if (ResVal == "0")
        //        {
        //            if (GroupName == "")
        //            {
        //                ResVal = "1"; ResSTR = "It is necessary to enter the name to add a new group";
        //            }
        //            else
        //            {
        //                GroupName = Pb.Text_UpperCase_AfterSpase(GroupName);
        //            }
        //        }
        //        if (ResVal == "0")
        //        {
        //            DataTable DT = new DataTable();
        //            DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Customers_01_Group_User Where (Parent_ID = '" + ParentID + "') And (Name = '" + GroupName + "') And (Removed = '0')");
        //            if (DT.Rows.Count == 0)
        //            {
        //                string RndKey = Pb.Make_Security_CodeFake(15);
        //                long InsCode = Sq.Get_New_ID(DataBase_Selector.Administrator, "Customers_01_Group_User", "ID_Counter");
        //                string InsDate = Sq.Sql_Date();
        //                string InsTime = Sq.Sql_Time();
        //                Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Customers_01_Group_User Values ('" + InsCode.ToString() + "','" + InsCode.ToString() + RndKey + "','" + ParentID + "','1','Group','" + GroupName + "','','" + GroupDescription + "','1','Active','5','Verified','0','" + InsDate + "','" + InsTime + "','" + UID + "','" + InsDate + "','" + InsTime + "','" + UID + "','0')");
        //                ResVal = "0"; ResSTR = "Group named " + GroupName + " was successfully added";
        //            }
        //            else
        //            {
        //                ResVal = "1"; ResSTR = "The entered name of the group is duplicate, so it is not possible for you to add new group with this name";
        //            }
        //        }
        //        IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //        IList<SelectListItem> FeedBack = new List<SelectListItem>
        //        { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //}

        //[HttpPost]
        //public JsonResult Group_Remove(string ID, string Name)
        //{
        //    try
        //    {
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        // Test Menu Access :
        //        ViewBag.MenuCode = 13;
        //        if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        string ResVal = "0";
        //        string ResSTR = "";
        //        string UID = "0";
        //        try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
        //        UID = UID.Trim();
        //        GroupIDU = GroupIDU.Trim();
        //        GroupName = GroupName.Trim();
        //        if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (ResVal == "0")
        //        {
        //            DataTable DT = new DataTable();
        //            DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Customers_01_Group_User Where (ID_UnicCode = '" + GroupIDU + "') And (Type_Code = '1')");
        //            if (DT.Rows != null)
        //            {
        //                if (DT.Rows.Count == 1)
        //                {
        //                    string InsDate = Sq.Sql_Date();
        //                    string InsTime = Sq.Sql_Time();
        //                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Customers_01_Group_User Set [Removed] = '1',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '1')");
        //                    ResSTR = "The " + GroupName + " group was successfully deleted";
        //                }
        //                else
        //                {
        //                    ResVal = "1"; ResSTR = "The specified group is invalid";
        //                }
        //            }
        //            else
        //            {
        //                ResVal = "1"; ResSTR = "The server encountered an error while receiving group information";
        //            }
        //        }
        //        IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //        IList<SelectListItem> FeedBack = new List<SelectListItem>
        //        { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //}

        //[HttpPost]
        //public JsonResult Group_ChangeStatus(string ID, string Name)
        //{
        //    try
        //    {
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        // Test Menu Access :
        //        ViewBag.MenuCode = 14;
        //        if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        string ResVal = "0";
        //        string ResSTR = "";
        //        string UID = "0";
        //        try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
        //        UID = UID.Trim();
        //        GroupIDU = GroupIDU.Trim();
        //        GroupName = GroupName.Trim();
        //        if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (ResVal == "0")
        //        {
        //            DataTable DT = new DataTable();
        //            DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Status_Code From Customers_01_Group_User Where (ID_UnicCode = '" + GroupIDU + "') And (Type_Code = '1')");
        //            if (DT.Rows != null)
        //            {
        //                if (DT.Rows.Count == 1)
        //                {
        //                    string InsDate = Sq.Sql_Date();
        //                    string InsTime = Sq.Sql_Time();
        //                    if (DT.Rows[0][1].ToString().Trim() == "1")
        //                    {
        //                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Customers_01_Group_User Set [Status_Code] = '0',[Status_Text] = 'Disabled',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '1')");
        //                        ResSTR = "The " + GroupName + " group was successfully change status to disabled";
        //                    }
        //                    else
        //                    {
        //                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Customers_01_Group_User Set [Status_Code] = '1',[Status_Text] = 'Active',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '1')");
        //                        ResSTR = "The " + GroupName + " group was successfully change status to active";
        //                    }
        //                }
        //                else
        //                {
        //                    ResVal = "1"; ResSTR = "The specified group is invalid";
        //                }
        //            }
        //            else
        //            {
        //                ResVal = "1"; ResSTR = "The server encountered an error while receiving group information";
        //            }
        //        }
        //        IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //        IList<SelectListItem> FeedBack = new List<SelectListItem>
        //        { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //}

        //[HttpPost]
        //public JsonResult Group_SaveEdit(string ID, string Name, string Desc)
        //{
        //    try
        //    {
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        // Test Menu Access :
        //        ViewBag.MenuCode = 15;
        //        if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        string ResVal = "0";
        //        string ResSTR = "";
        //        string UID = "0";
        //        try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
        //        UID = UID.Trim();
        //        GroupIDUC = GroupIDUC.Trim();
        //        GroupName = GroupName.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
        //        GroupDescription = GroupDescription.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
        //        if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (ResVal == "0")
        //        {
        //            if (GroupName == "")
        //            {
        //                ResVal = "1"; ResSTR = "It is necessary to enter the name to edit group";
        //            }
        //            else
        //            {
        //                GroupName = Pb.Text_UpperCase_AfterSpase(GroupName);
        //            }
        //        }
        //        if (ResVal == "0")
        //        {
        //            DataTable DT = new DataTable();
        //            DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Parent_ID From Customers_01_Group_User Where (ID_UnicCode = '" + GroupIDUC + "') And (Removed = '0') And (Type_Code = '1')");
        //            if (DT.Rows != null)
        //            {
        //                if (DT.Rows.Count == 1)
        //                {
        //                    DataTable DT2 = new DataTable();
        //                    DT2 = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Customers_01_Group_User Where (ID <> '" + DT.Rows[0][0].ToString().Trim() + "') And (Parent_ID = '" + DT.Rows[0][1].ToString().Trim() + "') And (Name = '" + GroupName + "') And (Removed = '0')");
        //                    if (DT2.Rows != null)
        //                    {
        //                        if (DT2.Rows.Count == 0)
        //                        {
        //                            string InsDate = Sq.Sql_Date();
        //                            string InsTime = Sq.Sql_Time();
        //                            Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Customers_01_Group_User Set [Name] = '" + GroupName + "',[Description_Email] = '" + GroupDescription + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '1')");
        //                            ResVal = "0"; ResSTR = "Group named " + GroupName + " was successfully edited";
        //                        }
        //                        else
        //                        {
        //                            ResVal = "1"; ResSTR = "The entered name of the group is duplicate, so it is not possible for you to edit group with this name";
        //                        }
        //                    }
        //                    else
        //                    {
        //                        ResVal = "1"; ResSTR = "The server encountered an error while receiving group information";
        //                    }
        //                }
        //                else
        //                {
        //                    ResVal = "1"; ResSTR = "The specified group is invalid";
        //                }
        //            }
        //            else
        //            {
        //                ResVal = "1"; ResSTR = "The server encountered an error while receiving group information";
        //            }
        //        }
        //        IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //        IList<SelectListItem> FeedBack = new List<SelectListItem>
        //        { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //}



        //[HttpPost]
        //public JsonResult Customer_Addnew(string ParentID, string FirstName, string LastName, string Email)
        //{
        //    try
        //    {
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        // Test Menu Access :
        //        ViewBag.MenuCode = 12;
        //        if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        string ResVal = "0";
        //        string ResSTR = "";
        //        string UID = "0";
        //        string ParentID = "0";
        //        try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
        //        UID = UID.Trim();
        //        ParentIDUC = ParentIDUC.Trim();
        //        AdminFirstName = AdminFirstName.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
        //        AdminLastName = AdminLastName.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
        //        AdminEmail = AdminEmail.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
        //        if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (ResVal == "0")
        //        {
        //            if (ParentIDUC != "0")
        //            {
        //                DataTable DT_PID = new DataTable();
        //                DT_PID = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Customers_01_Group_User Where (ID_UnicCode = '" + ParentIDUC + "')");
        //                if (DT_PID.Rows != null)
        //                {
        //                    if (DT_PID.Rows.Count == 1)
        //                    {
        //                        ParentID = DT_PID.Rows[0][0].ToString().Trim();
        //                    }
        //                    else
        //                    {
        //                        ResVal = "1"; ResSTR = "Parent group information not identified";
        //                    }
        //                }
        //                else
        //                {
        //                    ResVal = "1"; ResSTR = "Parent group information not identified";
        //                }
        //            }
        //            else
        //            {
        //                ParentID = "0";
        //            }
        //        }
        //        if (ResVal == "0")
        //        {
        //            if (AdminFirstName == "")
        //            {
        //                ResVal = "1"; ResSTR = "It is necessary to enter the first name to add a new admin";
        //            }
        //            else
        //            {
        //                AdminFirstName = Pb.Text_UpperCase_AfterSpase(AdminFirstName);
        //            }
        //        }
        //        if (ResVal == "0")
        //        {
        //            if (AdminLastName == "")
        //            {
        //                ResVal = "1"; ResSTR = "It is necessary to enter the last name to add a new admin";
        //            }
        //            else
        //            {
        //                AdminLastName = Pb.Text_UpperCase_AfterSpase(AdminLastName);
        //            }
        //        }
        //        if (ResVal == "0")
        //        {
        //            if (AdminEmail == "")
        //            {
        //                ResVal = "1"; ResSTR = "It is necessary to enter the email address to add a new admin";
        //            }
        //            else
        //            {
        //                AdminEmail = Pb.Text_UpperCase_AfterSpase(AdminEmail);
        //            }
        //        }
        //        if (ResVal == "0")
        //        {
        //            DataTable DT = new DataTable();
        //            DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Customers_01_Group_User Where (Type_Code = '2') And (Description_Email = '" + AdminEmail + "') And (Removed = '0')");
        //            if (DT.Rows.Count == 0)
        //            {
        //                string RndKey = Pb.Make_Security_CodeFake(15);
        //                long InsCode = Sq.Get_New_ID(DataBase_Selector.Administrator, "Customers_01_Group_User", "ID_Counter");
        //                string InsDate = Sq.Sql_Date();
        //                string InsTime = Sq.Sql_Time();
        //                Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Customers_01_Group_User Values ('" + InsCode.ToString() + "','" + InsCode.ToString() + RndKey + "','" + ParentID + "','2','Admin','" + AdminFirstName + "','" + AdminLastName + "','" + AdminEmail + "','1','Active','1','Send Failed','0','" + InsDate + "','" + InsTime + "','" + UID + "','" + InsDate + "','" + InsTime + "','" + UID + "','0')");
        //                if (Email.Admin_Verify("", InsCode.ToString() + RndKey) == true)
        //                {
        //                    ResVal = "0"; ResSTR = "Admin named " + AdminFirstName + " " + AdminLastName + " was successfully added";
        //                }
        //                else
        //                {
        //                    ResVal = "3"; ResSTR = "Admin named " + AdminFirstName + " " + AdminLastName + " was successfully added, But no verification email was sent";
        //                }
        //            }
        //            else
        //            {
        //                ResVal = "1"; ResSTR = "The entered email address of the admin is duplicate, so it is not possible for you to add new admin with this email address";
        //            }
        //        }
        //        IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //        IList<SelectListItem> FeedBack = new List<SelectListItem>
        //        { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //}

        //[HttpPost]
        //public JsonResult Customer_Remove(string ID, string Name)
        //{
        //    try
        //    {
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        // Test Menu Access :
        //        ViewBag.MenuCode = 21;
        //        if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        string ResVal = "0";
        //        string ResSTR = "";
        //        string UID = "0";
        //        try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
        //        UID = UID.Trim();
        //        AdminIDU = AdminIDU.Trim();
        //        AdminName = AdminName.Trim();
        //        if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (ResVal == "0")
        //        {
        //            DataTable DT = new DataTable();
        //            DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Customers_01_Group_User Where (ID_UnicCode = '" + AdminIDU + "') And (Type_Code = '2')");
        //            if (DT.Rows != null)
        //            {
        //                if (DT.Rows.Count == 1)
        //                {
        //                    string InsDate = Sq.Sql_Date();
        //                    string InsTime = Sq.Sql_Time();
        //                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Customers_01_Group_User Set [Removed] = '1',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '2')");
        //                    ResSTR = "The " + AdminName + " admin was successfully deleted";
        //                }
        //                else
        //                {
        //                    ResVal = "1"; ResSTR = "The specified admin is invalid";
        //                }
        //            }
        //            else
        //            {
        //                ResVal = "1"; ResSTR = "The server encountered an error while receiving admin information";
        //            }
        //        }
        //        IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //        IList<SelectListItem> FeedBack = new List<SelectListItem>
        //        { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //}

        //[HttpPost]
        //public JsonResult Customer_ChangeStatus(string ID, string Name)
        //{
        //    try
        //    {
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        // Test Menu Access :
        //        ViewBag.MenuCode = 22;
        //        if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        string ResVal = "0";
        //        string ResSTR = "";
        //        string UID = "0";
        //        try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
        //        UID = UID.Trim();
        //        AdminIDU = AdminIDU.Trim();
        //        AdminName = AdminName.Trim();
        //        if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (ResVal == "0")
        //        {
        //            DataTable DT = new DataTable();
        //            DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Status_Code From Customers_01_Group_User Where (ID_UnicCode = '" + AdminIDU + "') And (Type_Code = '2')");
        //            if (DT.Rows != null)
        //            {
        //                if (DT.Rows.Count == 1)
        //                {
        //                    string InsDate = Sq.Sql_Date();
        //                    string InsTime = Sq.Sql_Time();
        //                    if (DT.Rows[0][1].ToString().Trim() == "1")
        //                    {
        //                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Customers_01_Group_User Set [Status_Code] = '0',[Status_Text] = 'Disabled',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '2')");
        //                        ResSTR = "The " + AdminName + " admin was successfully change status to disabled";
        //                    }
        //                    else
        //                    {
        //                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Customers_01_Group_User Set [Status_Code] = '1',[Status_Text] = 'Active',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '2')");
        //                        ResSTR = "The " + AdminName + " admin was successfully change status to active";
        //                    }
        //                }
        //                else
        //                {
        //                    ResVal = "1"; ResSTR = "The specified admin is invalid";
        //                }
        //            }
        //            else
        //            {
        //                ResVal = "1"; ResSTR = "The server encountered an error while receiving admin information";
        //            }
        //        }
        //        IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //        IList<SelectListItem> FeedBack = new List<SelectListItem>
        //        { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //}




        //[HttpPost]
        //public JsonResult User_Addnew(string ParentID, string FirstName, string LastName, string Email)
        //{
        //    try
        //    {
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        // Test Menu Access :
        //        ViewBag.MenuCode = 12;
        //        if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        string ResVal = "0";
        //        string ResSTR = "";
        //        string UID = "0";
        //        string ParentID = "0";
        //        try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
        //        UID = UID.Trim();
        //        ParentIDUC = ParentIDUC.Trim();
        //        AdminFirstName = AdminFirstName.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
        //        AdminLastName = AdminLastName.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
        //        AdminEmail = AdminEmail.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
        //        if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (ResVal == "0")
        //        {
        //            if (ParentIDUC != "0")
        //            {
        //                DataTable DT_PID = new DataTable();
        //                DT_PID = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Customers_01_Group_User Where (ID_UnicCode = '" + ParentIDUC + "')");
        //                if (DT_PID.Rows != null)
        //                {
        //                    if (DT_PID.Rows.Count == 1)
        //                    {
        //                        ParentID = DT_PID.Rows[0][0].ToString().Trim();
        //                    }
        //                    else
        //                    {
        //                        ResVal = "1"; ResSTR = "Parent group information not identified";
        //                    }
        //                }
        //                else
        //                {
        //                    ResVal = "1"; ResSTR = "Parent group information not identified";
        //                }
        //            }
        //            else
        //            {
        //                ParentID = "0";
        //            }
        //        }
        //        if (ResVal == "0")
        //        {
        //            if (AdminFirstName == "")
        //            {
        //                ResVal = "1"; ResSTR = "It is necessary to enter the first name to add a new admin";
        //            }
        //            else
        //            {
        //                AdminFirstName = Pb.Text_UpperCase_AfterSpase(AdminFirstName);
        //            }
        //        }
        //        if (ResVal == "0")
        //        {
        //            if (AdminLastName == "")
        //            {
        //                ResVal = "1"; ResSTR = "It is necessary to enter the last name to add a new admin";
        //            }
        //            else
        //            {
        //                AdminLastName = Pb.Text_UpperCase_AfterSpase(AdminLastName);
        //            }
        //        }
        //        if (ResVal == "0")
        //        {
        //            if (AdminEmail == "")
        //            {
        //                ResVal = "1"; ResSTR = "It is necessary to enter the email address to add a new admin";
        //            }
        //            else
        //            {
        //                AdminEmail = Pb.Text_UpperCase_AfterSpase(AdminEmail);
        //            }
        //        }
        //        if (ResVal == "0")
        //        {
        //            DataTable DT = new DataTable();
        //            DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Customers_01_Group_User Where (Type_Code = '2') And (Description_Email = '" + AdminEmail + "') And (Removed = '0')");
        //            if (DT.Rows.Count == 0)
        //            {
        //                string RndKey = Pb.Make_Security_CodeFake(15);
        //                long InsCode = Sq.Get_New_ID(DataBase_Selector.Administrator, "Customers_01_Group_User", "ID_Counter");
        //                string InsDate = Sq.Sql_Date();
        //                string InsTime = Sq.Sql_Time();
        //                Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Customers_01_Group_User Values ('" + InsCode.ToString() + "','" + InsCode.ToString() + RndKey + "','" + ParentID + "','2','Admin','" + AdminFirstName + "','" + AdminLastName + "','" + AdminEmail + "','1','Active','1','Send Failed','0','" + InsDate + "','" + InsTime + "','" + UID + "','" + InsDate + "','" + InsTime + "','" + UID + "','0')");
        //                if (Email.Admin_Verify("", InsCode.ToString() + RndKey) == true)
        //                {
        //                    ResVal = "0"; ResSTR = "Admin named " + AdminFirstName + " " + AdminLastName + " was successfully added";
        //                }
        //                else
        //                {
        //                    ResVal = "3"; ResSTR = "Admin named " + AdminFirstName + " " + AdminLastName + " was successfully added, But no verification email was sent";
        //                }
        //            }
        //            else
        //            {
        //                ResVal = "1"; ResSTR = "The entered email address of the admin is duplicate, so it is not possible for you to add new admin with this email address";
        //            }
        //        }
        //        IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //        IList<SelectListItem> FeedBack = new List<SelectListItem>
        //        { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //}

        //[HttpPost]
        //public JsonResult User_Remove(string ID, string Name)
        //{
        //    try
        //    {
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        // Test Menu Access :
        //        ViewBag.MenuCode = 21;
        //        if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        string ResVal = "0";
        //        string ResSTR = "";
        //        string UID = "0";
        //        try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
        //        UID = UID.Trim();
        //        AdminIDU = AdminIDU.Trim();
        //        AdminName = AdminName.Trim();
        //        if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (ResVal == "0")
        //        {
        //            DataTable DT = new DataTable();
        //            DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Customers_01_Group_User Where (ID_UnicCode = '" + AdminIDU + "') And (Type_Code = '2')");
        //            if (DT.Rows != null)
        //            {
        //                if (DT.Rows.Count == 1)
        //                {
        //                    string InsDate = Sq.Sql_Date();
        //                    string InsTime = Sq.Sql_Time();
        //                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Customers_01_Group_User Set [Removed] = '1',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '2')");
        //                    ResSTR = "The " + AdminName + " admin was successfully deleted";
        //                }
        //                else
        //                {
        //                    ResVal = "1"; ResSTR = "The specified admin is invalid";
        //                }
        //            }
        //            else
        //            {
        //                ResVal = "1"; ResSTR = "The server encountered an error while receiving admin information";
        //            }
        //        }
        //        IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //        IList<SelectListItem> FeedBack = new List<SelectListItem>
        //        { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //}

        //[HttpPost]
        //public JsonResult User_ChangeStatus(string ID, string Name)
        //{
        //    try
        //    {
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        // Test Menu Access :
        //        ViewBag.MenuCode = 22;
        //        if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        string ResVal = "0";
        //        string ResSTR = "";
        //        string UID = "0";
        //        try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
        //        UID = UID.Trim();
        //        AdminIDU = AdminIDU.Trim();
        //        AdminName = AdminName.Trim();
        //        if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (ResVal == "0")
        //        {
        //            DataTable DT = new DataTable();
        //            DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Status_Code From Customers_01_Group_User Where (ID_UnicCode = '" + AdminIDU + "') And (Type_Code = '2')");
        //            if (DT.Rows != null)
        //            {
        //                if (DT.Rows.Count == 1)
        //                {
        //                    string InsDate = Sq.Sql_Date();
        //                    string InsTime = Sq.Sql_Time();
        //                    if (DT.Rows[0][1].ToString().Trim() == "1")
        //                    {
        //                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Customers_01_Group_User Set [Status_Code] = '0',[Status_Text] = 'Disabled',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '2')");
        //                        ResSTR = "The " + AdminName + " admin was successfully change status to disabled";
        //                    }
        //                    else
        //                    {
        //                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Customers_01_Group_User Set [Status_Code] = '1',[Status_Text] = 'Active',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '2')");
        //                        ResSTR = "The " + AdminName + " admin was successfully change status to active";
        //                    }
        //                }
        //                else
        //                {
        //                    ResVal = "1"; ResSTR = "The specified admin is invalid";
        //                }
        //            }
        //            else
        //            {
        //                ResVal = "1"; ResSTR = "The server encountered an error while receiving admin information";
        //            }
        //        }
        //        IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //        IList<SelectListItem> FeedBack = new List<SelectListItem>
        //        { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //}



        //[HttpPost]
        //public JsonResult Device_Addnew(string ParentID, string DeviceID, string Desc)
        //{
        //    try
        //    {
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        // Test Menu Access :
        //        ViewBag.MenuCode = 12;
        //        if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        string ResVal = "0";
        //        string ResSTR = "";
        //        string UID = "0";
        //        string ParentID = "0";
        //        try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
        //        UID = UID.Trim();
        //        ParentIDUC = ParentIDUC.Trim();
        //        AdminFirstName = AdminFirstName.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
        //        AdminLastName = AdminLastName.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
        //        AdminEmail = AdminEmail.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
        //        if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (ResVal == "0")
        //        {
        //            if (ParentIDUC != "0")
        //            {
        //                DataTable DT_PID = new DataTable();
        //                DT_PID = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Customers_01_Group_User Where (ID_UnicCode = '" + ParentIDUC + "')");
        //                if (DT_PID.Rows != null)
        //                {
        //                    if (DT_PID.Rows.Count == 1)
        //                    {
        //                        ParentID = DT_PID.Rows[0][0].ToString().Trim();
        //                    }
        //                    else
        //                    {
        //                        ResVal = "1"; ResSTR = "Parent group information not identified";
        //                    }
        //                }
        //                else
        //                {
        //                    ResVal = "1"; ResSTR = "Parent group information not identified";
        //                }
        //            }
        //            else
        //            {
        //                ParentID = "0";
        //            }
        //        }
        //        if (ResVal == "0")
        //        {
        //            if (AdminFirstName == "")
        //            {
        //                ResVal = "1"; ResSTR = "It is necessary to enter the first name to add a new admin";
        //            }
        //            else
        //            {
        //                AdminFirstName = Pb.Text_UpperCase_AfterSpase(AdminFirstName);
        //            }
        //        }
        //        if (ResVal == "0")
        //        {
        //            if (AdminLastName == "")
        //            {
        //                ResVal = "1"; ResSTR = "It is necessary to enter the last name to add a new admin";
        //            }
        //            else
        //            {
        //                AdminLastName = Pb.Text_UpperCase_AfterSpase(AdminLastName);
        //            }
        //        }
        //        if (ResVal == "0")
        //        {
        //            if (AdminEmail == "")
        //            {
        //                ResVal = "1"; ResSTR = "It is necessary to enter the email address to add a new admin";
        //            }
        //            else
        //            {
        //                AdminEmail = Pb.Text_UpperCase_AfterSpase(AdminEmail);
        //            }
        //        }
        //        if (ResVal == "0")
        //        {
        //            DataTable DT = new DataTable();
        //            DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Customers_01_Group_User Where (Type_Code = '2') And (Description_Email = '" + AdminEmail + "') And (Removed = '0')");
        //            if (DT.Rows.Count == 0)
        //            {
        //                string RndKey = Pb.Make_Security_CodeFake(15);
        //                long InsCode = Sq.Get_New_ID(DataBase_Selector.Administrator, "Customers_01_Group_User", "ID_Counter");
        //                string InsDate = Sq.Sql_Date();
        //                string InsTime = Sq.Sql_Time();
        //                Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Customers_01_Group_User Values ('" + InsCode.ToString() + "','" + InsCode.ToString() + RndKey + "','" + ParentID + "','2','Admin','" + AdminFirstName + "','" + AdminLastName + "','" + AdminEmail + "','1','Active','1','Send Failed','0','" + InsDate + "','" + InsTime + "','" + UID + "','" + InsDate + "','" + InsTime + "','" + UID + "','0')");
        //                if (Email.Admin_Verify("", InsCode.ToString() + RndKey) == true)
        //                {
        //                    ResVal = "0"; ResSTR = "Admin named " + AdminFirstName + " " + AdminLastName + " was successfully added";
        //                }
        //                else
        //                {
        //                    ResVal = "3"; ResSTR = "Admin named " + AdminFirstName + " " + AdminLastName + " was successfully added, But no verification email was sent";
        //                }
        //            }
        //            else
        //            {
        //                ResVal = "1"; ResSTR = "The entered email address of the admin is duplicate, so it is not possible for you to add new admin with this email address";
        //            }
        //        }
        //        IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //        IList<SelectListItem> FeedBack = new List<SelectListItem>
        //        { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //}

        //[HttpPost]
        //public JsonResult Device_Remove(string ID, string Name)
        //{
        //    try
        //    {
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        // Test Menu Access :
        //        ViewBag.MenuCode = 21;
        //        if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        string ResVal = "0";
        //        string ResSTR = "";
        //        string UID = "0";
        //        try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
        //        UID = UID.Trim();
        //        AdminIDU = AdminIDU.Trim();
        //        AdminName = AdminName.Trim();
        //        if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (ResVal == "0")
        //        {
        //            DataTable DT = new DataTable();
        //            DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Customers_01_Group_User Where (ID_UnicCode = '" + AdminIDU + "') And (Type_Code = '2')");
        //            if (DT.Rows != null)
        //            {
        //                if (DT.Rows.Count == 1)
        //                {
        //                    string InsDate = Sq.Sql_Date();
        //                    string InsTime = Sq.Sql_Time();
        //                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Customers_01_Group_User Set [Removed] = '1',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '2')");
        //                    ResSTR = "The " + AdminName + " admin was successfully deleted";
        //                }
        //                else
        //                {
        //                    ResVal = "1"; ResSTR = "The specified admin is invalid";
        //                }
        //            }
        //            else
        //            {
        //                ResVal = "1"; ResSTR = "The server encountered an error while receiving admin information";
        //            }
        //        }
        //        IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //        IList<SelectListItem> FeedBack = new List<SelectListItem>
        //        { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //}

        //[HttpPost]
        //public JsonResult Device_ChangeStatus(string ID, string Name)
        //{
        //    try
        //    {
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        // Test Menu Access :
        //        ViewBag.MenuCode = 22;
        //        if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
        //        //--------------------------------------------------------------------------------------------------------------------------------------------------------------
        //        string ResVal = "0";
        //        string ResSTR = "";
        //        string UID = "0";
        //        try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
        //        UID = UID.Trim();
        //        AdminIDU = AdminIDU.Trim();
        //        AdminName = AdminName.Trim();
        //        if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
        //        if (ResVal == "0")
        //        {
        //            DataTable DT = new DataTable();
        //            DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Status_Code From Customers_01_Group_User Where (ID_UnicCode = '" + AdminIDU + "') And (Type_Code = '2')");
        //            if (DT.Rows != null)
        //            {
        //                if (DT.Rows.Count == 1)
        //                {
        //                    string InsDate = Sq.Sql_Date();
        //                    string InsTime = Sq.Sql_Time();
        //                    if (DT.Rows[0][1].ToString().Trim() == "1")
        //                    {
        //                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Customers_01_Group_User Set [Status_Code] = '0',[Status_Text] = 'Disabled',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '2')");
        //                        ResSTR = "The " + AdminName + " admin was successfully change status to disabled";
        //                    }
        //                    else
        //                    {
        //                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Customers_01_Group_User Set [Status_Code] = '1',[Status_Text] = 'Active',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Type_Code = '2')");
        //                        ResSTR = "The " + AdminName + " admin was successfully change status to active";
        //                    }
        //                }
        //                else
        //                {
        //                    ResVal = "1"; ResSTR = "The specified admin is invalid";
        //                }
        //            }
        //            else
        //            {
        //                ResVal = "1"; ResSTR = "The server encountered an error while receiving admin information";
        //            }
        //        }
        //        IList<SelectListItem> FeedBack = new List<SelectListItem> { new SelectListItem { Value = ResVal, Text = ResSTR.Trim() } };
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //        IList<SelectListItem> FeedBack = new List<SelectListItem>
        //        { new SelectListItem{Text = "The server encountered an error while executing your request" , Value = "1"}};
        //        return Json(FeedBack, JsonRequestBehavior.AllowGet);
        //    }
        //}
        //====================================================================================================================
    }
}