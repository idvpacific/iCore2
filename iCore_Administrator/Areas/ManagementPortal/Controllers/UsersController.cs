using iCore_Administrator.Modules;
using iCore_Administrator.Modules.SecurityAuthentication;
using NPOI.POIFS.Crypt.Standard;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace iCore_Administrator.Areas.ManagementPortal.Controllers
{
    public class UsersController : Controller
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
        // Industry Type :
        // 1 : Hospitality 
        // 2 : Automative
        // 3 : API User
        // 101 : Sub Groups
        //-------------------------------------
        // Status :
        // 0 : Disabled
        // 1 : Active
        //-------------------------------------
        // Removed :
        // 0 : No and show to table
        // 1 : Yes and dont show
        //-------------------------------------
        [HttpGet]
        public ActionResult Management()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 47;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" }); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                ViewBag.DT_GroupType = null;
                ViewBag.DT_GroupUser = null;
                ViewBag.DT_ParentInfo = null;
                ViewBag.DT_Hospitality_SingleUser = null;
                ViewBag.DT_APIUser_SingleUser = null;
                ViewBag.PageID = "1";               //      1: Group Type | 2: Groups And Subgroups
                ViewBag.HeaderID = "0";
                ViewBag.HeaderType_Code = "0";      //      0: None | 1: Hospitality | 2: Automative | 3: API User
                ViewBag.HeaderType_Text = "";       //      0: None | 1: Hospitality | 2: Automative | 3: API User
                ViewBag.Parent_ID = "0";
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
                    ViewBag.PageID = "2";
                    DataTable DT_ParentInfo = new DataTable();
                    DT_ParentInfo = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Users_01_Type_Group Where (ID_UnicCode = '" + Parent_IDUC + "') And (Removed = '0')");
                    if (DT_ParentInfo.Rows != null)
                    {
                        if (DT_ParentInfo.Rows.Count == 1)
                        {
                            Parent_ID = DT_ParentInfo.Rows[0][0].ToString().Trim();
                            ViewBag.DT_ParentInfo = DT_ParentInfo.Rows[0];
                            DataTable DT_GroupTypeRoot = new DataTable();
                            DT_GroupTypeRoot = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Type_Code,Type_Text From Users_01_Type_Group Where (ID = '" + DT_ParentInfo.Rows[0][4].ToString().Trim() + "') And (Parent_ID = '0') And (Root_ID = '0') And (Removed = '0')");
                            if (DT_GroupTypeRoot.Rows != null)
                            {
                                if (DT_GroupTypeRoot.Rows.Count == 1)
                                {
                                    ViewBag.HeaderID = DT_GroupTypeRoot.Rows[0][0].ToString().Trim();
                                    ViewBag.HeaderType_Code = DT_GroupTypeRoot.Rows[0][1].ToString().Trim();
                                    ViewBag.HeaderType_Text = DT_GroupTypeRoot.Rows[0][2].ToString().Trim();
                                }
                                else
                                {
                                    DT_GroupTypeRoot = new DataTable();
                                    DT_GroupTypeRoot = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Type_Code,Type_Text From Users_01_Type_Group Where (ID = '" + DT_ParentInfo.Rows[0][0].ToString().Trim() + "') And (Parent_ID = '0') And (Root_ID = '0') And (Removed = '0')");
                                    if (DT_GroupTypeRoot.Rows != null)
                                    {
                                        if (DT_GroupTypeRoot.Rows.Count == 1)
                                        {
                                            ViewBag.HeaderID = DT_GroupTypeRoot.Rows[0][0].ToString().Trim();
                                            ViewBag.HeaderType_Code = DT_GroupTypeRoot.Rows[0][1].ToString().Trim();
                                            ViewBag.HeaderType_Text = DT_GroupTypeRoot.Rows[0][2].ToString().Trim();
                                        }
                                        else
                                        {
                                            return RedirectToAction("Management", "Users", new { id = "", area = "ManagementPortal" });
                                        }
                                    }
                                    else
                                    {
                                        return RedirectToAction("Management", "Users", new { id = "", area = "ManagementPortal" });
                                    }
                                }
                            }
                            else
                            {
                                return RedirectToAction("Management", "Users", new { id = "", area = "ManagementPortal" });
                            }
                        }
                        else
                        {
                            return RedirectToAction("Management", "Users", new { id = "", area = "ManagementPortal" });
                        }
                    }
                    else
                    {
                        return RedirectToAction("Management", "Users", new { id = "", area = "ManagementPortal" });
                    }
                }
                if (Parent_IDUC != "0")
                {
                    try
                    {
                        DataTable DTConvert = new DataTable();
                        DTConvert = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,ID_UnicCode,Name,Parent_ID From Users_01_Type_Group Where (ID_UnicCode = '" + Parent_IDUC + "') And (Removed = '0')");
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
                                        DT_Breadcrumb = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID_UnicCode,Name,Parent_ID,Removed From Users_01_Type_Group Where (ID = '" + PrntID + "') And (Removed = '0')");
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
                                return RedirectToAction("Management", "Users", new { id = "", area = "ManagementPortal" });
                            }
                        }
                        else
                        {
                            return RedirectToAction("Management", "Users", new { id = "", area = "ManagementPortal" });
                        }
                    }
                    catch (Exception)
                    {
                        return RedirectToAction("Management", "Users", new { id = "", area = "ManagementPortal" });
                    }
                }
                if (GroupTreeRemoved == true) { return RedirectToAction("Management", "Users", new { id = "", area = "ManagementPortal" }); }
                if (ViewBag.PageID == "1")
                {
                    DataTable DT_GroupType = new DataTable();
                    DT_GroupType = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,ID_UnicCode,Type_Code,Type_Text,Name,Description,Status_Code From Users_01_Type_Group Where (Parent_ID = '0') And (Type_Code <= '100') And (Removed = '0') Order By Name");
                    ViewBag.DT_GroupType = DT_GroupType.Rows;
                }
                else
                {
                    DataTable DT_GroupUser = new DataTable();
                    DT_GroupUser = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,ID_UnicCode,Name,Description,Status_Code From Users_01_Type_Group Where (Parent_ID = '" + Parent_ID + "') And (Type_Code = '101') And (Removed = '0') Order By Name");
                    ViewBag.DT_GroupUser = DT_GroupUser.Rows;
                    if (ViewBag.HeaderType_Code == "1")
                    {
                        DataTable DT_Hospitality_SingleUser = new DataTable();
                        DT_Hospitality_SingleUser = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,ID_UnicCode,Type_Code,Type_Text,Name,LName,Email,Description,Status_Code From Users_02_SingleUser Where (Parent_ID = '" + Parent_ID + "') And (Removed = '0') And (User_GroupType_Code = '1') Order By Name,LName");
                        ViewBag.DT_Hospitality_SingleUser = DT_Hospitality_SingleUser.Rows;
                    }
                    if (ViewBag.HeaderType_Code == "3")
                    {
                        DataTable DT_APIUser_SingleUser = new DataTable();
                        DT_APIUser_SingleUser = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,ID_UnicCode,Type_Code,Type_Text,Name,LName,Email,Description,Status_Code From Users_02_SingleUser Where (Parent_ID = '" + Parent_ID + "') And (Removed = '0') And (User_GroupType_Code = '3') Order By Name,LName");
                        ViewBag.DT_APIUser_SingleUser = DT_APIUser_SingleUser.Rows;
                    }

                }
                ViewBag.Parent_ID = Parent_ID;
                ViewBag.Parent_IDUC = Parent_IDUC;
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" });
            }
        }

        [HttpPost]
        public JsonResult GroupType_Grid()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 47;
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
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,ID_UnicCode,Type_Code,Type_Text,Name,Description,Status_Code From Users_01_Type_Group Where (Parent_ID = '0') And (Type_Code <= '100') And (Removed = '0') Order By Name");
                    if (DT.Rows != null)
                    {
                        string GroupTypeBadge = "";
                        foreach (DataRow RW in DT.Rows)
                        {
                            GroupTypeBadge = "";
                            switch (RW[2].ToString().Trim())
                            {
                                case "1": { GroupTypeBadge = "badge-light-primary"; break; }
                                case "2": { GroupTypeBadge = "badge-light-danger"; break; }
                                case "3": { GroupTypeBadge = "badge-light-info"; break; }
                            }
                            ResSTR += "<tr>";
                            ResSTR += "<td onclick=\"GroupType_GroupShow('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\"><i class=\"fa fa-tag text-primary\" style=\"font-size:25px\"></i></td>";
                            ResSTR += "<td onclick=\"GroupType_GroupShow('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[4].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"GroupType_GroupShow('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[5].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"GroupType_GroupShow('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                            ResSTR += "<div class=\"badge badge-pill " + GroupTypeBadge + "\" style=\"width:95px\">" + RW[3].ToString().Trim() + "</div>";
                            ResSTR += "</td>";
                            if (RW[6].ToString().Trim() == "0")
                            {
                                ResSTR += "<td onclick=\"GroupType_GroupShow('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:70px\">Disabled</div>";
                                ResSTR += "</td>";
                            }
                            else
                            {
                                ResSTR += "<td onclick=\"GroupType_GroupShow('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:70px\">Active</div>";
                                ResSTR += "</td>";
                            }
                            ResSTR += "<td style=\"text-align:center\">";
                            ResSTR += "<div class=\"btn-group dropleft\">";
                            ResSTR += "<button type=\"button\" class=\"btn\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                            ResSTR += "<div class=\"MenuToolbox text-primary\"/>";
                            ResSTR += "</button>";
                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"GroupType_GroupShow('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-search text-primary\" style=\"width:24px;font-size:14px\"></i>Show subgroup</a>";
                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"GroupType_Remove('" + RW[0].ToString().Trim() + "','" + RW[4].ToString().Trim() + "')\"><i class=\"fa fa-trash-o text-primary\" style=\"width:24px;font-size:14px\"></i>Remove type</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"GroupType_ChangeStatus('" + RW[0].ToString().Trim() + "','" + RW[4].ToString().Trim() + "')\"><i class=\"fa fa-refresh text-primary\" style=\"width:24px;font-size:14px\"></i>Change status</a>";
                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"GroupType_Edit('" + RW[0].ToString().Trim() + "','" + RW[4].ToString().Trim() + "','" + RW[5].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit type</a>";
                            ResSTR += "</div>";
                            ResSTR += "</div>";
                            ResSTR += "</td>";
                            ResSTR += "</tr>";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading information list";
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
        public JsonResult GroupType_AddNew(string NM, string DC, string IC, string IN)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 102;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                NM = NM.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DC = DC.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                IC = IC.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                IN = IN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    if (NM == "")
                    {
                        ResVal = "1"; ResSTR = "It is necessary to enter the name to add a new type of group";
                    }
                    else
                    {
                        NM = Pb.Text_UpperCase_AfterSpase(NM);
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_01_Type_Group Where (Name = '" + NM + "') And (Removed = '0')");
                    if (DT.Rows.Count == 0)
                    {
                        string RndKey = Pb.Make_Security_CodeFake(40);
                        long InsCode = Sq.Get_New_ID(DataBase_Selector.Administrator, "Users_01_Type_Group", "ID_Counter");
                        string InsDate = Sq.Sql_Date();
                        string InsTime = Sq.Sql_Time();
                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_01_Type_Group Values ('" + InsCode.ToString() + "','" + InsCode.ToString() + RndKey + "','0','0','" + IC + "','" + IN + "','" + NM + "','" + DC + "','1','Active','" + InsDate + "','" + InsTime + "','" + UID + "','" + InsDate + "','" + InsTime + "','" + UID + "','0')");
                        ResVal = "0"; ResSTR = "Type named " + NM + " was successfully added";
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The entered name of the type is duplicate, so it is not possible for you to add new type with this name";
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
        public JsonResult GroupType_Remove(string ID, string NM)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 103;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                ID = ID.Trim();
                NM = NM.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_01_Type_Group Where (ID = '" + ID + "') And (Parent_ID = '0') And (Type_Code <= '100') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_01_Type_Group Set [Removed] = '1',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Parent_ID = '0') And (Type_Code <= '100') And (Removed = '0')");
                            ResSTR = "The " + NM + " type was successfully deleted";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified type is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving type of group information";
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
        public JsonResult GroupType_ChangeStatus(string ID, string NM)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 104;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                ID = ID.Trim();
                NM = NM.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Status_Code From Users_01_Type_Group Where (ID = '" + ID + "') And (Parent_ID = '0') And (Type_Code <= '100') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            if (DT.Rows[0][1].ToString().Trim() == "1")
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_01_Type_Group Set [Status_Code] = '0',[Status_Text] = 'Disabled',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Parent_ID = '0') And (Type_Code <= '100') And (Removed = '0')");
                                ResSTR = "The " + NM + " type was successfully change status to disabled";
                            }
                            else
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_01_Type_Group Set [Status_Code] = '1',[Status_Text] = 'Active',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Parent_ID = '0') And (Type_Code <= '100') And (Removed = '0')");
                                ResSTR = "The " + NM + " type was successfully change status to active";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified type is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving type information";
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
        public JsonResult GroupType_SaveEdit(string ID, string NM, string DC)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 105;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                ID = ID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                NM = NM.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DC = DC.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    if (NM == "")
                    {
                        ResVal = "1"; ResSTR = "It is necessary to enter the name to edit type";
                    }
                    else
                    {
                        NM = Pb.Text_UpperCase_AfterSpase(NM);
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_01_Type_Group Where (ID = '" + ID + "') And (Parent_ID = '0') And (Type_Code <= '100') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            DataTable DT2 = new DataTable();
                            DT2 = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_01_Type_Group Where (ID <> '" + DT.Rows[0][0].ToString().Trim() + "') And (Name = '" + NM + "') And (Parent_ID = '0') And (Type_Code <= '100') And (Removed = '0')");
                            if (DT2.Rows != null)
                            {
                                if (DT2.Rows.Count == 0)
                                {
                                    string InsDate = Sq.Sql_Date();
                                    string InsTime = Sq.Sql_Time();
                                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_01_Type_Group Set [Name] = '" + NM + "',[Description] = '" + DC + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Parent_ID = '0') And (Type_Code <= '100') And (Removed = '0')");
                                    ResVal = "0"; ResSTR = "Type named " + NM + " was successfully edited";
                                }
                                else
                                {
                                    ResVal = "1"; ResSTR = "The entered name of the type is duplicate, so it is not possible for you to edit type with this name";
                                }
                            }
                            else
                            {
                                ResVal = "1"; ResSTR = "The server encountered an error while receiving type information";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified type is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving type information";
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
        public JsonResult GroupUser_Grid(string PID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 47;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                PID = PID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,ID_UnicCode,Name,Description,Status_Code From Users_01_Type_Group Where (Parent_ID = '" + PID + "') And (Type_Code = '101') And (Removed = '0') Order By Name");
                    if (DT.Rows != null)
                    {
                        foreach (DataRow RW in DT.Rows)
                        {
                            ResSTR += "<tr>";
                            ResSTR += "<td onclick=\"GroupUser_GroupShow('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\"><i class=\"fa fa-id-badge text-primary\" style=\"font-size:25px\"></i></td>";
                            ResSTR += "<td onclick=\"GroupUser_GroupShow('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[2].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"GroupUser_GroupShow('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[3].ToString().Trim() + "</td>";
                            if (RW[4].ToString().Trim() == "0")
                            {
                                ResSTR += "<td onclick=\"GroupUser_GroupShow('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:70px\">Disabled</div>";
                                ResSTR += "</td>";
                            }
                            else
                            {
                                ResSTR += "<td onclick=\"GroupUser_GroupShow('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:70px\">Active</div>";
                                ResSTR += "</td>";
                            }
                            ResSTR += "<td style=\"text-align:center\">";
                            ResSTR += "<div class=\"btn-group dropleft\">";
                            ResSTR += "<button type=\"button\" class=\"btn\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                            ResSTR += "<div class=\"MenuToolbox text-primary\"/>";
                            ResSTR += "</button>";
                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"GroupUser_GroupShow('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-search text-primary\" style=\"width:24px;font-size:14px\"></i>Show subgroup</a>";
                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"GroupUser_Remove('" + RW[0].ToString().Trim() + "','" + RW[2].ToString().Trim() + "')\"><i class=\"fa fa-trash-o text-primary\" style=\"width:24px;font-size:14px\"></i>Remove group</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"GroupUser_ChangeStatus('" + RW[0].ToString().Trim() + "','" + RW[2].ToString().Trim() + "')\"><i class=\"fa fa-refresh text-primary\" style=\"width:24px;font-size:14px\"></i>Change status</a>";
                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"GroupUser_Edit('" + RW[0].ToString().Trim() + "','" + RW[2].ToString().Trim() + "','" + RW[3].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit group</a>";
                            ResSTR += "</div>";
                            ResSTR += "</div>";
                            ResSTR += "</td>";
                            ResSTR += "</tr>";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading group information list";
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
        public JsonResult GroupUser_AddNew(string PID, string NM, string DC, string RID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 106;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                PID = PID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                NM = NM.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DC = DC.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                RID = RID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    if (NM == "")
                    {
                        ResVal = "1"; ResSTR = "It is necessary to enter the name to add a new group";
                    }
                    else
                    {
                        NM = Pb.Text_UpperCase_AfterSpase(NM);
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_01_Type_Group Where (Parent_ID = '" + PID + "') And (Name = '" + NM + "') And (Removed = '0')");
                    if (DT.Rows.Count == 0)
                    {
                        string RndKey = Pb.Make_Security_CodeFake(40);
                        long InsCode = Sq.Get_New_ID(DataBase_Selector.Administrator, "Users_01_Type_Group", "ID_Counter");
                        string InsDate = Sq.Sql_Date();
                        string InsTime = Sq.Sql_Time();
                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_01_Type_Group Values ('" + InsCode.ToString() + "','" + InsCode.ToString() + RndKey + "','" + PID + "','" + RID + "','101','Group','" + NM + "','" + DC + "','1','Active','" + InsDate + "','" + InsTime + "','" + UID + "','" + InsDate + "','" + InsTime + "','" + UID + "','0')");
                        ResVal = "0"; ResSTR = "Group named " + NM + " was successfully added";
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
        public JsonResult GroupUser_Remove(string PID, string ID, string NM)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 107;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                PID = PID.Trim();
                ID = ID.Trim();
                NM = NM.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_01_Type_Group Where (ID = '" + ID + "') And (Parent_ID = '" + PID + "') And (Type_Code = '101') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_01_Type_Group Set [Removed] = '1',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Parent_ID = '" + PID + "') And (Type_Code = '101') And (Removed = '0')");
                            ResSTR = "The " + NM + " group was successfully deleted";
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
        public JsonResult GroupUser_ChangeStatus(string PID, string ID, string NM)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 108;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                PID = PID.Trim();
                ID = ID.Trim();
                NM = NM.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Status_Code From Users_01_Type_Group Where (ID = '" + ID + "') And (Parent_ID = '" + PID + "') And (Type_Code = '101') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            if (DT.Rows[0][1].ToString().Trim() == "1")
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_01_Type_Group Set [Status_Code] = '0',[Status_Text] = 'Disabled',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Parent_ID = '" + PID + "') And (Type_Code = '101') And (Removed = '0')");
                                ResSTR = "The " + NM + " group was successfully change status to disabled";
                            }
                            else
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_01_Type_Group Set [Status_Code] = '1',[Status_Text] = 'Active',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Parent_ID = '" + PID + "') And (Type_Code = '101') And (Removed = '0')");
                                ResSTR = "The " + NM + " group was successfully change status to active";
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
        public JsonResult GroupUser_SaveEdit(string PID, string ID, string NM, string DC)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 109;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                PID = PID.Trim();
                ID = ID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                NM = NM.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DC = DC.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    if (NM == "")
                    {
                        ResVal = "1"; ResSTR = "It is necessary to enter the name to edit group";
                    }
                    else
                    {
                        NM = Pb.Text_UpperCase_AfterSpase(NM);
                    }
                }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_01_Type_Group Where (ID = '" + ID + "') And (Parent_ID = '" + PID + "') And (Type_Code = '101') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            DataTable DT2 = new DataTable();
                            DT2 = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_01_Type_Group Where (ID <> '" + DT.Rows[0][0].ToString().Trim() + "') And (Name = '" + NM + "') And (Parent_ID = '" + PID + "') And (Type_Code = '101') And (Removed = '0')");
                            if (DT2.Rows != null)
                            {
                                if (DT2.Rows.Count == 0)
                                {
                                    string InsDate = Sq.Sql_Date();
                                    string InsTime = Sq.Sql_Time();
                                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_01_Type_Group Set [Name] = '" + NM + "',[Description] = '" + DC + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Parent_ID = '" + PID + "') And (Type_Code = '101') And (Removed = '0')");
                                    ResVal = "0"; ResSTR = "Group named " + NM + " was successfully edited";
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
        public JsonResult Hospitality_SingleUser_Grid(string PID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 47;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                PID = PID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,ID_UnicCode,Type_Code,Type_Text,Name,LName,Email,Description,Status_Code From Users_02_SingleUser Where (Parent_ID = '" + PID + "') And (Removed = '0') Order By Name,LName");
                    if (DT.Rows != null)
                    {
                        string Hospitality_SingleUserBadge = "";
                        string LstName = "";
                        foreach (DataRow RW in DT.Rows)
                        {
                            Hospitality_SingleUserBadge = "";
                            switch (RW[2].ToString().Trim())
                            {
                                case "1": { LstName = RW[4].ToString().Trim(); Hospitality_SingleUserBadge = "badge-light-primary"; break; }
                                case "2": { LstName = RW[4].ToString().Trim() + " " + RW[5].ToString().Trim(); Hospitality_SingleUserBadge = "badge-light-danger"; break; }
                            }
                            ResSTR += "<tr>";
                            if (RW[2].ToString().Trim() == "1")
                            {
                                ResSTR += "<td onclick=\"Hospitality_SingleUser_UserDetails('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\"><i class=\"fa fa-building-o text-primary\" style=\"font-size:25px\"></i></td>";
                            }
                            else
                            {
                                ResSTR += "<td onclick=\"Hospitality_SingleUser_UserDetails('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\"><i class=\"fa fa-user-o text-primary\" style=\"font-size:25px\"></i></td>";
                            }
                            ResSTR += "<td onclick=\"Hospitality_SingleUser_UserDetails('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + LstName + "</td>";
                            ResSTR += "<td onclick=\"Hospitality_SingleUser_UserDetails('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[6].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"Hospitality_SingleUser_UserDetails('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[7].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"Hospitality_SingleUser_UserDetails('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                            ResSTR += "<div class=\"badge badge-pill " + Hospitality_SingleUserBadge + "\" style=\"width:95px\">" + RW[3].ToString().Trim() + "</div>";
                            ResSTR += "</td>";
                            if (RW[8].ToString().Trim() == "0")
                            {
                                ResSTR += "<td onclick=\"Hospitality_SingleUser_UserDetails('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:70px\">Disabled</div>";
                                ResSTR += "</td>";
                            }
                            else
                            {
                                ResSTR += "<td onclick=\"Hospitality_SingleUser_UserDetails('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:70px\">Active</div>";
                                ResSTR += "</td>";
                            }
                            ResSTR += "<td style=\"text-align:center\">";
                            ResSTR += "<div class=\"btn-group dropleft\">";
                            ResSTR += "<button type=\"button\" class=\"btn\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                            ResSTR += "<div class=\"MenuToolbox text-primary\"/>";
                            ResSTR += "</button>";
                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Hospitality_SingleUser_UserDetails('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-search text-primary\" style=\"width:24px;font-size:14px\"></i>Show user account details</a>";
                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Hospitality_SingleUser_Remove('" + RW[0].ToString().Trim() + "','" + LstName + "')\"><i class=\"fa fa-trash-o text-primary\" style=\"width:24px;font-size:14px\"></i>Remove user</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Hospitality_SingleUser_ChangeStatus('" + RW[0].ToString().Trim() + "','" + LstName + "')\"><i class=\"fa fa-refresh text-primary\" style=\"width:24px;font-size:14px\"></i>Change status</a>";
                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"Hospitality_SingleUser_Edit('" + RW[0].ToString().Trim() + "','" + RW[2].ToString().Trim() + "','" + RW[4].ToString().Trim() + "','" + RW[5].ToString().Trim() + "','" + RW[6].ToString().Trim() + "','" + RW[7].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit user</a>";
                            ResSTR += "</div>";
                            ResSTR += "</div>";
                            ResSTR += "</td>";
                            ResSTR += "</tr>";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading hospitality single user information list";
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
        public JsonResult Hospitality_SingleUser_AddNew(string PID, string RID, string TC, string TN, string CN, string FN, string LN, string EM, string DC)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 110;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                PID = PID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                RID = RID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                TC = TC.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                TN = TN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                CN = CN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                FN = FN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                LN = LN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                EM = EM.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DC = DC.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                CN = Pb.Text_UpperCase_AfterSpase(CN);
                FN = Pb.Text_UpperCase_AfterSpase(FN);
                LN = Pb.Text_UpperCase_AfterSpase(LN);
                if (TC == "1")
                {
                    if (ResVal == "0") { if (CN == "") { ResVal = "1"; ResSTR = "It is necessary to enter the name to add a new user as organization"; } }
                }
                else
                {
                    if (ResVal == "0") { if (FN == "") { ResVal = "1"; ResSTR = "It is necessary to enter the first name to add a new user"; } }
                    if (ResVal == "0") { if (LN == "") { ResVal = "1"; ResSTR = "It is necessary to enter the last name to add a new user"; } }
                }
                if (ResVal == "0") { if (EM == "") { ResVal = "1"; ResSTR = "It is necessary to enter the email address to add a user"; } }
                ////////////////////////////////////////////////////////////////////////////
                // Test Duplicate email :
                if (AAuth.EmailTester(1, "0", EM) == false) { ResVal = "1"; ResSTR = "The entered email of the user is duplicate, so it is not possible for you to add new user with this email"; }
                ////////////////////////////////////////////////////////////////////////////
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    if (TC == "1")
                    {
                        DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (Name = '" + CN + "') And (Removed = '0')");
                        FN = "";
                        LN = "";
                    }
                    else
                    {
                        DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (Name = '" + FN + "') And (LName = '" + LN + "') And (Removed = '0')");
                        CN = "";
                    }
                    if (DT.Rows.Count == 0)
                    {
                        string RndKey = Pb.Make_Security_CodeFake(40);
                        long InsCode = Sq.Get_New_ID(DataBase_Selector.Administrator, "Users_02_SingleUser", "ID_Counter");
                        string InsDate = Sq.Sql_Date();
                        string InsTime = Sq.Sql_Time();
                        string NMlast = CN.Trim() + FN.Trim();
                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_02_SingleUser Values ('" + InsCode.ToString() + "','" + InsCode.ToString() + RndKey + "','" + PID + "','" + RID + "','" + TC + "','" + TN + "','" + NMlast + "','" + LN + "','" + EM + "','" + DC + "','1','Active','" + InsDate + "','" + InsTime + "','" + UID + "','" + InsDate + "','" + InsTime + "','" + UID + "','0','','','','','','','','','1')");
                        if (TC == "1")
                        {
                            ResVal = "0"; ResSTR = "Organization named " + CN + " was successfully added";
                        }
                        else
                        {
                            ResVal = "0"; ResSTR = "User named " + FN + " " + LN + " was successfully added";
                        }
                    }
                    else
                    {
                        if (TC == "1")
                        {
                            ResVal = "1"; ResSTR = "The entered organization name of the user is duplicate, so it is not possible for you to add new organization with this name";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The entered first\\last name of the user is duplicate, so it is not possible for you to add new user with this first\\last name";
                        }
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
        public JsonResult Hospitality_SingleUser_Remove(string ID, string NM)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 111;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                ID = ID.Trim();
                NM = NM.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (ID = '" + ID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_02_SingleUser Set [Removed] = '1',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Removed = '0')");
                            ResSTR = "The " + NM + " user was successfully deleted";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified user is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving user information";
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
        public JsonResult Hospitality_SingleUser_ChangeStatus(string ID, string NM)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 112;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                ID = ID.Trim();
                NM = NM.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Status_Code From Users_02_SingleUser Where (ID = '" + ID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            if (DT.Rows[0][1].ToString().Trim() == "1")
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_02_SingleUser Set [Status_Code] = '0',[Status_Text] = 'Disabled',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Removed = '0')");
                                ResSTR = "The " + NM + " user was successfully change status to disabled";
                            }
                            else
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_02_SingleUser Set [Status_Code] = '1',[Status_Text] = 'Active',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Removed = '0')");
                                ResSTR = "The " + NM + " user was successfully change status to active";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified user is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving user information";
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
        public JsonResult Hospitality_SingleUser_SaveEdit(string PID, string ID, string TC, string TN, string CN, string FN, string LN, string EM, string DC)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 113;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                PID = PID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                ID = ID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                TC = TC.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                TN = TN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                CN = CN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                FN = FN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                LN = LN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                EM = EM.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DC = DC.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                CN = Pb.Text_UpperCase_AfterSpase(CN);
                FN = Pb.Text_UpperCase_AfterSpase(FN);
                LN = Pb.Text_UpperCase_AfterSpase(LN);
                if (TC == "1")
                {
                    if (ResVal == "0") { if (CN == "") { ResVal = "1"; ResSTR = "It is necessary to enter the name to save user as organization"; } }
                }
                else
                {
                    if (ResVal == "0") { if (FN == "") { ResVal = "1"; ResSTR = "It is necessary to enter the first name to save user"; } }
                    if (ResVal == "0") { if (LN == "") { ResVal = "1"; ResSTR = "It is necessary to enter the last name to save user"; } }
                }
                if (ResVal == "0") { if (EM == "") { ResVal = "1"; ResSTR = "It is necessary to enter the email address save user"; } }
                ////////////////////////////////////////////////////////////////////////////
                // Test Duplicate email :
                DataTable DT_EmailTest = new DataTable();
                DT_EmailTest = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (ID <> '" + ID + "') And (Email = '" + EM + "') And (Removed = '0')");
                if (DT_EmailTest.Rows.Count != 0) { ResVal = "1"; ResSTR = "The entered email of the user is duplicate, so it is not possible for you to save user information with this email"; }
                ////////////////////////////////////////////////////////////////////////////
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (ID = '" + ID + "') And (Parent_ID = '" + PID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            DataTable DT2 = new DataTable();

                            if (TC == "1")
                            {
                                DT2 = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (ID <> '" + DT.Rows[0][0].ToString().Trim() + "') And (Name = '" + CN + "') And (Removed = '0')");
                                FN = "";
                                LN = "";
                            }
                            else
                            {
                                DT2 = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (ID <> '" + DT.Rows[0][0].ToString().Trim() + "') And (Name = '" + FN + "') And (LName = '" + LN + "') And (Removed = '0')");
                                CN = "";
                            }
                            if (DT2.Rows != null)
                            {
                                if (DT2.Rows.Count == 0)
                                {
                                    string InsDate = Sq.Sql_Date();
                                    string InsTime = Sq.Sql_Time();
                                    string NMlast = CN.Trim() + FN.Trim();
                                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_02_SingleUser Set [Type_Code] = '" + TC + "',[Type_Text] = '" + TN + "',[Name] = '" + NMlast + "',[LName] = '" + LN + "',[Email] = '" + EM + "',[Description] = '" + DC + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Parent_ID = '" + PID + "') And (Removed = '0')");
                                    if (TC == "1")
                                    {
                                        ResVal = "0"; ResSTR = "Organization named " + CN + " was successfully edited";
                                    }
                                    else
                                    {
                                        ResVal = "0"; ResSTR = "User named " + FN + " " + LN + " was successfully edited";
                                    }
                                }
                                else
                                {
                                    if (TC == "1")
                                    {
                                        ResVal = "1"; ResSTR = "The entered organization name of the user is duplicate, so it is not possible for you to save organization with this name";
                                    }
                                    else
                                    {
                                        ResVal = "1"; ResSTR = "The entered first\\last name of the user is duplicate, so it is not possible for you to save user with this first\\last name";
                                    }
                                }
                            }
                            else
                            {
                                ResVal = "1"; ResSTR = "The server encountered an error while receiving user information";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified user is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving user information";
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
        public JsonResult APIUser_SingleUser_Grid(string PID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 47;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                PID = PID.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,ID_UnicCode,Type_Code,Type_Text,Name,LName,Email,Description,Status_Code From Users_02_SingleUser Where (Parent_ID = '" + PID + "') And (Removed = '0') Order By Name,LName");
                    if (DT.Rows != null)
                    {
                        string APIUser_SingleUserBadge = "";
                        string LstName = "";
                        foreach (DataRow RW in DT.Rows)
                        {
                            APIUser_SingleUserBadge = "";
                            switch (RW[2].ToString().Trim())
                            {
                                case "1": { LstName = RW[4].ToString().Trim(); APIUser_SingleUserBadge = "badge-light-primary"; break; }
                                case "2": { LstName = RW[4].ToString().Trim() + " " + RW[5].ToString().Trim(); APIUser_SingleUserBadge = "badge-light-danger"; break; }
                            }
                            ResSTR += "<tr>";
                            if (RW[2].ToString().Trim() == "1")
                            {
                                ResSTR += "<td onclick=\"APIUser_SingleUser_UserDetails('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\"><i class=\"fa fa-building-o text-primary\" style=\"font-size:25px\"></i></td>";
                            }
                            else
                            {
                                ResSTR += "<td onclick=\"APIUser_SingleUser_UserDetails('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\"><i class=\"fa fa-user-o text-primary\" style=\"font-size:25px\"></i></td>";
                            }
                            ResSTR += "<td onclick=\"APIUser_SingleUser_UserDetails('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + LstName + "</td>";
                            ResSTR += "<td onclick=\"APIUser_SingleUser_UserDetails('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[6].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIUser_SingleUser_UserDetails('" + RW[1].ToString().Trim() + "')\" style=\"cursor:pointer\">" + RW[7].ToString().Trim() + "</td>";
                            ResSTR += "<td onclick=\"APIUser_SingleUser_UserDetails('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                            ResSTR += "<div class=\"badge badge-pill " + APIUser_SingleUserBadge + "\" style=\"width:95px\">" + RW[3].ToString().Trim() + "</div>";
                            ResSTR += "</td>";
                            if (RW[8].ToString().Trim() == "0")
                            {
                                ResSTR += "<td onclick=\"APIUser_SingleUser_UserDetails('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-danger\" style=\"width:70px\">Disabled</div>";
                                ResSTR += "</td>";
                            }
                            else
                            {
                                ResSTR += "<td onclick=\"APIUser_SingleUser_UserDetails('" + RW[1].ToString().Trim() + "')\" style=\"text-align:center;cursor:pointer\">";
                                ResSTR += "<div class=\"badge badge-pill badge-light-success\" style=\"width:70px\">Active</div>";
                                ResSTR += "</td>";
                            }
                            ResSTR += "<td style=\"text-align:center\">";
                            ResSTR += "<div class=\"btn-group dropleft\">";
                            ResSTR += "<button type=\"button\" class=\"btn\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">";
                            ResSTR += "<div class=\"MenuToolbox text-primary\"/>";
                            ResSTR += "</button>";
                            ResSTR += "<div class=\"dropdown-menu\" style=\"font-size:12px\">";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"APIUser_SingleUser_UserDetails('" + RW[1].ToString().Trim() + "')\"><i class=\"fa fa-search text-primary\" style=\"width:24px;font-size:14px\"></i>Show user account details</a>";
                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"APIUser_SingleUser_Remove('" + RW[0].ToString().Trim() + "','" + LstName + "')\"><i class=\"fa fa-trash-o text-primary\" style=\"width:24px;font-size:14px\"></i>Remove user</a>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"APIUser_SingleUser_ChangeStatus('" + RW[0].ToString().Trim() + "','" + LstName + "')\"><i class=\"fa fa-refresh text-primary\" style=\"width:24px;font-size:14px\"></i>Change status</a>";
                            ResSTR += "<div class=\"dropdown-divider\"></div>";
                            ResSTR += "<a class=\"dropdown-item\" href=\"javascript:void(0)\" onclick=\"APIUser_SingleUser_Edit('" + RW[0].ToString().Trim() + "','" + RW[2].ToString().Trim() + "','" + RW[4].ToString().Trim() + "','" + RW[5].ToString().Trim() + "','" + RW[6].ToString().Trim() + "','" + RW[7].ToString().Trim() + "')\"><i class=\"fa fa-pencil-square-o text-primary\" style=\"width:24px;font-size:14px\"></i>Edit user</a>";
                            ResSTR += "</div>";
                            ResSTR += "</div>";
                            ResSTR += "</td>";
                            ResSTR += "</tr>";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while reloading APIUser single user information list";
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
        public JsonResult APIUser_SingleUser_AddNew(string PID, string RID, string TC, string TN, string CN, string FN, string LN, string EM, string DC)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 120;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                PID = PID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                RID = RID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                TC = TC.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                TN = TN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                CN = CN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                FN = FN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                LN = LN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                EM = EM.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DC = DC.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                CN = Pb.Text_UpperCase_AfterSpase(CN);
                FN = Pb.Text_UpperCase_AfterSpase(FN);
                LN = Pb.Text_UpperCase_AfterSpase(LN);
                if (TC == "1")
                {
                    if (ResVal == "0") { if (CN == "") { ResVal = "1"; ResSTR = "It is necessary to enter the name to add a new user as organization"; } }
                }
                else
                {
                    if (ResVal == "0") { if (FN == "") { ResVal = "1"; ResSTR = "It is necessary to enter the first name to add a new user"; } }
                    if (ResVal == "0") { if (LN == "") { ResVal = "1"; ResSTR = "It is necessary to enter the last name to add a new user"; } }
                }
                if (ResVal == "0") { if (EM == "") { ResVal = "1"; ResSTR = "It is necessary to enter the email address to add a user"; } }
                ////////////////////////////////////////////////////////////////////////////
                // Test Duplicate email :
                if (AAuth.EmailTester(1, "0", EM) == false) { ResVal = "1"; ResSTR = "The entered email of the user is duplicate, so it is not possible for you to add new user with this email"; }
                ////////////////////////////////////////////////////////////////////////////
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    if (TC == "1")
                    {
                        DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (Name = '" + CN + "') And (Removed = '0')");
                        FN = "";
                        LN = "";
                    }
                    else
                    {
                        DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (Name = '" + FN + "') And (LName = '" + LN + "') And (Removed = '0')");
                        CN = "";
                    }
                    if (DT.Rows.Count == 0)
                    {
                        string RndKey = Pb.Make_Security_CodeFake(40);
                        long InsCode = Sq.Get_New_ID(DataBase_Selector.Administrator, "Users_02_SingleUser", "ID_Counter");
                        string InsDate = Sq.Sql_Date();
                        string InsTime = Sq.Sql_Time();
                        string NMlast = CN.Trim() + FN.Trim();
                        Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_02_SingleUser Values ('" + InsCode.ToString() + "','" + InsCode.ToString() + RndKey + "','" + PID + "','" + RID + "','" + TC + "','" + TN + "','" + NMlast + "','" + LN + "','" + EM + "','" + DC + "','1','Active','" + InsDate + "','" + InsTime + "','" + UID + "','" + InsDate + "','" + InsTime + "','" + UID + "','0','','','','','','','','','3')");
                        if (TC == "1")
                        {
                            ResVal = "0"; ResSTR = "Organization named " + CN + " was successfully added";
                        }
                        else
                        {
                            ResVal = "0"; ResSTR = "User named " + FN + " " + LN + " was successfully added";
                        }
                    }
                    else
                    {
                        if (TC == "1")
                        {
                            ResVal = "1"; ResSTR = "The entered organization name of the user is duplicate, so it is not possible for you to add new organization with this name";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The entered first\\last name of the user is duplicate, so it is not possible for you to add new user with this first\\last name";
                        }
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
        public JsonResult APIUser_SingleUser_Remove(string ID, string NM)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 121;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                ID = ID.Trim();
                NM = NM.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (ID = '" + ID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_02_SingleUser Set [Removed] = '1',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Removed = '0')");
                            ResSTR = "The " + NM + " user was successfully deleted";
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified user is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving user information";
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
        public JsonResult APIUser_SingleUser_ChangeStatus(string ID, string NM)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 122;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                ID = ID.Trim();
                NM = NM.Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Status_Code From Users_02_SingleUser Where (ID = '" + ID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string InsDate = Sq.Sql_Date();
                            string InsTime = Sq.Sql_Time();
                            if (DT.Rows[0][1].ToString().Trim() == "1")
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_02_SingleUser Set [Status_Code] = '0',[Status_Text] = 'Disabled',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Removed = '0')");
                                ResSTR = "The " + NM + " user was successfully change status to disabled";
                            }
                            else
                            {
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_02_SingleUser Set [Status_Code] = '1',[Status_Text] = 'Active',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Removed = '0')");
                                ResSTR = "The " + NM + " user was successfully change status to active";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified user is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving user information";
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
        public JsonResult APIUser_SingleUser_SaveEdit(string PID, string ID, string TC, string TN, string CN, string FN, string LN, string EM, string DC)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 123;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                PID = PID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                ID = ID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                TC = TC.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                TN = TN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                CN = CN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                FN = FN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                LN = LN.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                EM = EM.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                DC = DC.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                CN = Pb.Text_UpperCase_AfterSpase(CN);
                FN = Pb.Text_UpperCase_AfterSpase(FN);
                LN = Pb.Text_UpperCase_AfterSpase(LN);
                if (TC == "1")
                {
                    if (ResVal == "0") { if (CN == "") { ResVal = "1"; ResSTR = "It is necessary to enter the name to save user as organization"; } }
                }
                else
                {
                    if (ResVal == "0") { if (FN == "") { ResVal = "1"; ResSTR = "It is necessary to enter the first name to save user"; } }
                    if (ResVal == "0") { if (LN == "") { ResVal = "1"; ResSTR = "It is necessary to enter the last name to save user"; } }
                }
                if (ResVal == "0") { if (EM == "") { ResVal = "1"; ResSTR = "It is necessary to enter the email address save user"; } }
                ////////////////////////////////////////////////////////////////////////////
                // Test Duplicate email :
                DataTable DT_EmailTest = new DataTable();
                DT_EmailTest = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (ID <> '" + ID + "') And (Email = '" + EM + "') And (Removed = '0')");
                if (DT_EmailTest.Rows.Count != 0) { ResVal = "1"; ResSTR = "The entered email of the user is duplicate, so it is not possible for you to save user information with this email"; }
                ////////////////////////////////////////////////////////////////////////////
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (ID = '" + ID + "') And (Parent_ID = '" + PID + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            DataTable DT2 = new DataTable();

                            if (TC == "1")
                            {
                                DT2 = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (ID <> '" + DT.Rows[0][0].ToString().Trim() + "') And (Name = '" + CN + "') And (Removed = '0')");
                                FN = "";
                                LN = "";
                            }
                            else
                            {
                                DT2 = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (ID <> '" + DT.Rows[0][0].ToString().Trim() + "') And (Name = '" + FN + "') And (LName = '" + LN + "') And (Removed = '0')");
                                CN = "";
                            }
                            if (DT2.Rows != null)
                            {
                                if (DT2.Rows.Count == 0)
                                {
                                    string InsDate = Sq.Sql_Date();
                                    string InsTime = Sq.Sql_Time();
                                    string NMlast = CN.Trim() + FN.Trim();
                                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_02_SingleUser Set [Type_Code] = '" + TC + "',[Type_Text] = '" + TN + "',[Name] = '" + NMlast + "',[LName] = '" + LN + "',[Email] = '" + EM + "',[Description] = '" + DC + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (Parent_ID = '" + PID + "') And (Removed = '0')");
                                    if (TC == "1")
                                    {
                                        ResVal = "0"; ResSTR = "Organization named " + CN + " was successfully edited";
                                    }
                                    else
                                    {
                                        ResVal = "0"; ResSTR = "User named " + FN + " " + LN + " was successfully edited";
                                    }
                                }
                                else
                                {
                                    if (TC == "1")
                                    {
                                        ResVal = "1"; ResSTR = "The entered organization name of the user is duplicate, so it is not possible for you to save organization with this name";
                                    }
                                    else
                                    {
                                        ResVal = "1"; ResSTR = "The entered first\\last name of the user is duplicate, so it is not possible for you to save user with this first\\last name";
                                    }
                                }
                            }
                            else
                            {
                                ResVal = "1"; ResSTR = "The server encountered an error while receiving user information";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified user is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving user information";
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
        public ActionResult HospitalitySingleUser()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 114;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" }); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string User_UID = "0";
                try { User_UID = Url.RequestContext.RouteData.Values["id"].ToString().Trim(); } catch (Exception) { }
                User_UID = User_UID.Trim();
                DataTable DT_UserCheck = new DataTable();
                DT_UserCheck = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Users_02_SingleUser Where (ID_UnicCode = '" + User_UID + "') And (Removed = '0') And (User_GroupType_Code = '1')");
                if (DT_UserCheck.Rows == null) { return RedirectToAction("Management", "Users", new { id = "", area = "ManagementPortal" }); }
                if (DT_UserCheck.Rows.Count != 1) { return RedirectToAction("Management", "Users", new { id = "", area = "ManagementPortal" }); }
                ViewBag.UserFullname = "Unknow";
                ViewBag.DT_UserInfo = null;
                ViewBag.UserFullname = (DT_UserCheck.Rows[0][7].ToString().Trim() + " " + DT_UserCheck.Rows[0][8].ToString().Trim()).Trim();
                ViewBag.DT_UserInfo = DT_UserCheck.Rows[0];
                ViewBag.UserID = DT_UserCheck.Rows[0][0].ToString().Trim();





                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" });
            }
        }

        [HttpPost]
        public JsonResult HSU_Account_Login_Reload(string UI)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 114;
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
                    UI = UI.Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Email,Account_Login_Username,Account_Login_Password From Users_02_SingleUser Where (ID = '" + UI + "') And (Removed = '0') And (User_GroupType_Code = '1')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            ResSTR = DT.Rows[0][0].ToString().Trim() + "#" + DT.Rows[0][1].ToString().Trim() + "#" + DT.Rows[0][2].ToString().Trim();
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified user is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving user account information";
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
        public JsonResult HSU_Account_Login_Save(string UI, string R1, string R2, string R3)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 115;
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
                    UI = UI.Trim();
                    R1 = R1.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    R2 = R2.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    R3 = R3.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Name,LName From Users_02_SingleUser Where (ID = '" + UI + "') And (Removed = '0') And (User_GroupType_Code = '1')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string Fieldname = "";
                            if (ResVal == "0") { if (AAuth.EmailTester(1, UI, R1) == false) { Fieldname = "login Email Address"; ResVal = "1"; } }
                            if (ResVal == "0") { if (AAuth.UsernameTester(1, UI, R2) == false) { Fieldname = "login Username"; ResVal = "1"; } }
                            if (ResVal == "0")
                            {
                                string InsDate = Sq.Sql_Date();
                                string InsTime = Sq.Sql_Time();
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_02_SingleUser Set [Email] = '" + R1 + "',[Account_Login_Username] = '" + R2 + "',[Account_Login_Password] = '" + R3 + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (User_GroupType_Code = '1')");
                                ResSTR = "User " + (DT.Rows[0][1].ToString().Trim() + " " + DT.Rows[0][2].ToString().Trim()).Trim() + " account information was successfully saved";
                            }
                            else
                            {
                                ResVal = "1"; ResSTR = "User account filed " + Fieldname + " is duplicated, Please retry after check";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified user is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving user account information";
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
        public JsonResult HSU_Account_API_Reload(string UI)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 114;
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
                    UI = UI.Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select API_Authentication_Username,API_Authentication_Key,API_GetRequest_Username,API_GetRequest_Key,API_PostRequest_Username,API_PostRequest_Key From Users_02_SingleUser Where (ID = '" + UI + "') And (Removed = '0') And (User_GroupType_Code = '1')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            ResSTR = DT.Rows[0][0].ToString().Trim() + "#" + DT.Rows[0][1].ToString().Trim() + "#" + DT.Rows[0][2].ToString().Trim() + "#" + DT.Rows[0][3].ToString().Trim() + "#" + DT.Rows[0][4].ToString().Trim() + "#" + DT.Rows[0][5].ToString().Trim();
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified user is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving user API information";
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
        public JsonResult HSU_Account_API_Save(string UI, string R1, string R2, string R3, string R4, string R5, string R6)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 116;
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
                    UI = UI.Trim();
                    R1 = R1.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    R2 = R2.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    R3 = R3.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    R4 = R4.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    R5 = R5.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    R6 = R6.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Name,LName From Users_02_SingleUser Where (ID = '" + UI + "') And (Removed = '0') And (User_GroupType_Code = '1')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string Fieldname = "";
                            if (ResVal == "0") { if (AAuth.APIUsernameTester(1, UI, R1) == false) { Fieldname = "Authentication request API username"; ResVal = "1"; } }
                            if (ResVal == "0") { if (AAuth.APIUsernameTester(1, UI, R3) == false) { Fieldname = "Get request API username"; ResVal = "1"; } }
                            if (ResVal == "0") { if (AAuth.APIUsernameTester(1, UI, R5) == false) { Fieldname = "Post request API username"; ResVal = "1"; } }
                            if (ResVal == "0")
                            {
                                string InsDate = Sq.Sql_Date();
                                string InsTime = Sq.Sql_Time();
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_02_SingleUser Set [API_Authentication_Username] = '" + R1 + "',[API_Authentication_Key] = '" + R2 + "',[API_GetRequest_Username] = '" + R3 + "',[API_GetRequest_Key] = '" + R4 + "',[API_PostRequest_Username] = '" + R5 + "',[API_PostRequest_Key] = '" + R6 + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (User_GroupType_Code = '1')");
                                ResSTR = "User " + (DT.Rows[0][1].ToString().Trim() + " " + DT.Rows[0][2].ToString().Trim()).Trim() + " API information was successfully saved";
                            }
                            else
                            {
                                ResVal = "1"; ResSTR = "User account filed " + Fieldname + " is duplicated, Please retry after check";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified user is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving user API information";
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
        public JsonResult HSU_Account_KeyGenerator(string UI, string KID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 114;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                UI = UI.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                KID = KID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (ID = '" + UI + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            bool ExitLoop = false;
                            string KeyGen = "";
                            switch (KID)
                            {
                                case "1":
                                    {
                                        while (ExitLoop == false)
                                        {
                                            KeyGen = "";
                                            Thread.Sleep(5);
                                            KeyGen = Pb.Make_Security_Code(15);
                                            if (AAuth.UsernameTester(0, "0", KeyGen) == true)
                                            {
                                                ExitLoop = true;
                                                ResSTR = KeyGen;
                                            }
                                        }
                                        break;
                                    }
                                case "2":
                                    {
                                        ResSTR = Pb.Make_Security_Code(15);
                                        break;
                                    }
                                case "3":
                                    {
                                        while (ExitLoop == false)
                                        {
                                            KeyGen = "";
                                            Thread.Sleep(5);
                                            KeyGen = Pb.Make_Security_Code(40);
                                            if (AAuth.APIUsernameTester(0, "0", KeyGen) == true)
                                            {
                                                ExitLoop = true;
                                                ResSTR = KeyGen;
                                            }
                                        }
                                        break;
                                    }
                                case "4":
                                    {
                                        ResSTR = Pb.Make_Security_Code(40);
                                        break;
                                    }
                                case "5":
                                    {
                                        while (ExitLoop == false)
                                        {
                                            KeyGen = "";
                                            Thread.Sleep(5);
                                            KeyGen = Pb.Make_Security_Code(40);
                                            if (AAuth.APIUsernameTester(0, "0", KeyGen) == true)
                                            {
                                                ExitLoop = true;
                                                ResSTR = KeyGen;
                                            }
                                        }
                                        break;
                                    }
                                case "6":
                                    {
                                        ResSTR = Pb.Make_Security_Code(40);
                                        break;
                                    }
                                case "7":
                                    {
                                        while (ExitLoop == false)
                                        {
                                            KeyGen = "";
                                            Thread.Sleep(5);
                                            KeyGen = Pb.Make_Security_Code(40);
                                            if (AAuth.APIUsernameTester(0, "0", KeyGen) == true)
                                            {
                                                ExitLoop = true;
                                                ResSTR = KeyGen;
                                            }
                                        }
                                        break;
                                    }
                                case "8":
                                    {
                                        ResSTR = Pb.Make_Security_Code(40);
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified user is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving user information";
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
        public ActionResult APISingleUser()
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 124;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" }); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string User_UID = "0";
                try { User_UID = Url.RequestContext.RouteData.Values["id"].ToString().Trim(); } catch (Exception) { }
                User_UID = User_UID.Trim();
                DataTable DT_UserCheck = new DataTable();
                DT_UserCheck = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Users_02_SingleUser Where (ID_UnicCode = '" + User_UID + "') And (Removed = '0') And (User_GroupType_Code = '3')");
                if (DT_UserCheck.Rows == null) { return RedirectToAction("Management", "Users", new { id = "", area = "ManagementPortal" }); }
                if (DT_UserCheck.Rows.Count != 1) { return RedirectToAction("Management", "Users", new { id = "", area = "ManagementPortal" }); }
                ViewBag.UserFullname = "Unknow";
                ViewBag.DT_UserInfo = null;
                ViewBag.UserFullname = (DT_UserCheck.Rows[0][7].ToString().Trim() + " " + DT_UserCheck.Rows[0][8].ToString().Trim()).Trim();
                ViewBag.DT_UserInfo = DT_UserCheck.Rows[0];
                ViewBag.UserID = DT_UserCheck.Rows[0][0].ToString().Trim();





                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" });
            }
        }

        [HttpPost]
        public JsonResult APIUser_Account_Login_Reload(string UI)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 124;
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
                    UI = UI.Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Email,Account_Login_Username,Account_Login_Password From Users_02_SingleUser Where (ID = '" + UI + "') And (Removed = '0') And (User_GroupType_Code = '3')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            ResSTR = DT.Rows[0][0].ToString().Trim() + "#" + DT.Rows[0][1].ToString().Trim() + "#" + DT.Rows[0][2].ToString().Trim();
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified user is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving user account information";
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
        public JsonResult APIUser_Account_Login_Save(string UI, string R1, string R2, string R3)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 125;
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
                    UI = UI.Trim();
                    R1 = R1.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    R2 = R2.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    R3 = R3.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Name,LName From Users_02_SingleUser Where (ID = '" + UI + "') And (Removed = '0') And (User_GroupType_Code = '3')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string Fieldname = "";
                            if (ResVal == "0") { if (AAuth.EmailTester(1, UI, R1) == false) { Fieldname = "login Email Address"; ResVal = "1"; } }
                            if (ResVal == "0") { if (AAuth.UsernameTester(1, UI, R2) == false) { Fieldname = "login Username"; ResVal = "1"; } }
                            if (ResVal == "0")
                            {
                                string InsDate = Sq.Sql_Date();
                                string InsTime = Sq.Sql_Time();
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_02_SingleUser Set [Email] = '" + R1 + "',[Account_Login_Username] = '" + R2 + "',[Account_Login_Password] = '" + R3 + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (User_GroupType_Code = '3')");
                                ResSTR = "User " + (DT.Rows[0][1].ToString().Trim() + " " + DT.Rows[0][2].ToString().Trim()).Trim() + " account information was successfully saved";
                            }
                            else
                            {
                                ResVal = "1"; ResSTR = "User account filed " + Fieldname + " is duplicated, Please retry after check";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified user is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving user account information";
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
        public JsonResult APIUser_Account_API_Reload(string UI)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 124;
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
                    UI = UI.Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select API_Authentication_Username,API_Authentication_Key,API_GetRequest_Username,API_GetRequest_Key,API_PostRequest_Username,API_PostRequest_Key From Users_02_SingleUser Where (ID = '" + UI + "') And (Removed = '0') And (User_GroupType_Code = '3')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            ResSTR = DT.Rows[0][0].ToString().Trim() + "#" + DT.Rows[0][1].ToString().Trim() + "#" + DT.Rows[0][2].ToString().Trim() + "#" + DT.Rows[0][3].ToString().Trim() + "#" + DT.Rows[0][4].ToString().Trim() + "#" + DT.Rows[0][5].ToString().Trim();
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified user is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving user API information";
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
        public JsonResult APIUser_Account_API_Save(string UI, string R1, string R2, string R3, string R4, string R5, string R6)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 126;
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
                    UI = UI.Trim();
                    R1 = R1.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    R2 = R2.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    R3 = R3.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    R4 = R4.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    R5 = R5.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    R6 = R6.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID,Name,LName From Users_02_SingleUser Where (ID = '" + UI + "') And (Removed = '0') And (User_GroupType_Code = '3')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            string Fieldname = "";
                            if (ResVal == "0") { if (AAuth.APIUsernameTester(1, UI, R1) == false) { Fieldname = "Authentication request API username"; ResVal = "1"; } }
                            if (ResVal == "0") { if (AAuth.APIUsernameTester(1, UI, R3) == false) { Fieldname = "Get request API username"; ResVal = "1"; } }
                            if (ResVal == "0") { if (AAuth.APIUsernameTester(1, UI, R5) == false) { Fieldname = "Post request API username"; ResVal = "1"; } }
                            if (ResVal == "0")
                            {
                                string InsDate = Sq.Sql_Date();
                                string InsTime = Sq.Sql_Time();
                                Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_02_SingleUser Set [API_Authentication_Username] = '" + R1 + "',[API_Authentication_Key] = '" + R2 + "',[API_GetRequest_Username] = '" + R3 + "',[API_GetRequest_Key] = '" + R4 + "',[API_PostRequest_Username] = '" + R5 + "',[API_PostRequest_Key] = '" + R6 + "',[Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + DT.Rows[0][0].ToString().Trim() + "') And (User_GroupType_Code = '3')");
                                ResSTR = "User " + (DT.Rows[0][1].ToString().Trim() + " " + DT.Rows[0][2].ToString().Trim()).Trim() + " API information was successfully saved";
                            }
                            else
                            {
                                ResVal = "1"; ResSTR = "User account filed " + Fieldname + " is duplicated, Please retry after check";
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified user is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving user API information";
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
        public JsonResult APIUser_Account_KeyGenerator(string UI, string KID)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 124;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                UI = UI.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                KID = KID.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    DataTable DT = new DataTable();
                    DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select ID From Users_02_SingleUser Where (ID = '" + UI + "') And (Removed = '0')");
                    if (DT.Rows != null)
                    {
                        if (DT.Rows.Count == 1)
                        {
                            bool ExitLoop = false;
                            string KeyGen = "";
                            switch (KID)
                            {
                                case "1":
                                    {
                                        while (ExitLoop == false)
                                        {
                                            KeyGen = "";
                                            Thread.Sleep(5);
                                            KeyGen = Pb.Make_Security_Code(15);
                                            if (AAuth.UsernameTester(0, "0", KeyGen) == true)
                                            {
                                                ExitLoop = true;
                                                ResSTR = KeyGen;
                                            }
                                        }
                                        break;
                                    }
                                case "2":
                                    {
                                        ResSTR = Pb.Make_Security_Code(15);
                                        break;
                                    }
                                case "3":
                                    {
                                        while (ExitLoop == false)
                                        {
                                            KeyGen = "";
                                            Thread.Sleep(5);
                                            KeyGen = Pb.Make_Security_Code(40);
                                            if (AAuth.APIUsernameTester(0, "0", KeyGen) == true)
                                            {
                                                ExitLoop = true;
                                                ResSTR = KeyGen;
                                            }
                                        }
                                        break;
                                    }
                                case "4":
                                    {
                                        ResSTR = Pb.Make_Security_Code(40);
                                        break;
                                    }
                                case "5":
                                    {
                                        while (ExitLoop == false)
                                        {
                                            KeyGen = "";
                                            Thread.Sleep(5);
                                            KeyGen = Pb.Make_Security_Code(40);
                                            if (AAuth.APIUsernameTester(0, "0", KeyGen) == true)
                                            {
                                                ExitLoop = true;
                                                ResSTR = KeyGen;
                                            }
                                        }
                                        break;
                                    }
                                case "6":
                                    {
                                        ResSTR = Pb.Make_Security_Code(40);
                                        break;
                                    }
                                case "7":
                                    {
                                        while (ExitLoop == false)
                                        {
                                            KeyGen = "";
                                            Thread.Sleep(5);
                                            KeyGen = Pb.Make_Security_Code(40);
                                            if (AAuth.APIUsernameTester(0, "0", KeyGen) == true)
                                            {
                                                ExitLoop = true;
                                                ResSTR = KeyGen;
                                            }
                                        }
                                        break;
                                    }
                                case "8":
                                    {
                                        ResSTR = Pb.Make_Security_Code(40);
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            ResVal = "1"; ResSTR = "The specified user is invalid";
                        }
                    }
                    else
                    {
                        ResVal = "1"; ResSTR = "The server encountered an error while receiving user information";
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
        public JsonResult APIUser_AccessP_Reload(string UI)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 124;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                UI = UI.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    string LastRes = "0-0-0-0-0-0-0-0-0-0-0-0-0-3-3";
                    try
                    {
                        DataTable DT = new DataTable();
                        DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Users_14_APIUser_AccessPolicy Where (User_ID = '" + UI + "')");
                        if (DT.Rows != null)
                        {
                            if (DT.Rows.Count == 1)
                            {
                                LastRes = DT.Rows[0][1].ToString().Trim() + "-" + DT.Rows[0][2].ToString().Trim() + "-" + DT.Rows[0][3].ToString().Trim() + "-" + DT.Rows[0][4].ToString().Trim() + "-" + DT.Rows[0][5].ToString().Trim() + "-" + DT.Rows[0][6].ToString().Trim() + "-" + DT.Rows[0][7].ToString().Trim() + "-" + DT.Rows[0][8].ToString().Trim() + "-" + DT.Rows[0][9].ToString().Trim() + "-" + DT.Rows[0][10].ToString().Trim() + "-" + DT.Rows[0][11].ToString().Trim() + "-" + DT.Rows[0][12].ToString().Trim() + "-" + DT.Rows[0][13].ToString().Trim() + "-" + DT.Rows[0][14].ToString().Trim() + "-" + DT.Rows[0][15].ToString().Trim();
                            }
                        }
                    }
                    catch (Exception)
                    { LastRes = "0-0-0-0-0-0-0-0-0-0-0-0-0-0-3-3"; }
                    ResSTR = LastRes;
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
        public JsonResult APIUser_AccessP_Save(string UI, string C1, string C2, string C3, string C4, string C5, string C6, string C7, string C8, string C9, string C10, string C11, string C12, string C13, string C14, string C15, string C16)
        {
            try
            {
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                // Test Menu Access :
                ViewBag.MenuCode = 127;
                if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { IList<SelectListItem> FB = new List<SelectListItem> { new SelectListItem { Text = "You do not have permission to access this section", Value = "1" } }; return Json(FB, JsonRequestBehavior.AllowGet); }
                //--------------------------------------------------------------------------------------------------------------------------------------------------------------
                string ResVal = "0";
                string ResSTR = "";
                string UID = "0";
                try { UID = Session["Admin_UID"].ToString().Trim(); } catch (Exception) { UID = "0"; }
                UID = UID.Trim();
                UI = UI.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if (UID == "0") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (UID == "") { ResVal = "2"; ResSTR = "Your license has expired, Please login again"; }
                if (ResVal == "0")
                {
                    C1 = C1.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    C2 = C2.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    C3 = C3.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    C4 = C4.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    C5 = C5.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    C6 = C6.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    C7 = C7.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    C8 = C8.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    C9 = C9.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    C10 = C10.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    C11 = C11.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    C12 = C12.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    C13 = C13.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    C14 = C14.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    C15 = C15.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    C16 = C16.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Delete From Users_14_APIUser_AccessPolicy Where (User_ID = '" + UI + "')");
                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_14_APIUser_AccessPolicy Values ('" + UI + "','" + C1 + "','" + C2 + "','" + C3 + "','" + C4 + "','" + C5 + "','" + C6 + "','" + C7 + "','" + C8 + "','" + C9 + "','" + C10 + "','" + C11 + "','" + C12 + "','" + C13 + "','" + C14 + "','" + C15 + "','" + C16 + "')");
                    string InsDate = Sq.Sql_Date();
                    string InsTime = Sq.Sql_Time();
                    Sq.Execute_TSql(DataBase_Selector.Administrator, "Update Users_02_SingleUser Set [Last_Update_Date] = '" + InsDate + "',[Last_Update_Time] = '" + InsTime + "',[Last_Update_ID] = '" + UID + "' Where (ID = '" + UI + "') And (User_GroupType_Code = '3')");
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

        //====================================================================================================================
    }
}