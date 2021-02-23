using iCore_Administrator.Modules;
using iCore_Administrator.Modules.SecurityAuthentication;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services.Description;
namespace iCore_Administrator.Controllers
{
    public class LoginController : Controller
    {
        //====================================================================================================================
        AuthenticationTester AAuth = new AuthenticationTester();
        SQL_Tranceiver Sq = new SQL_Tranceiver();
        PublicFunctions Pb = new PublicFunctions();
        EmailSender Email = new EmailSender();
        //====================================================================================================================
        public ActionResult Index() { FormsAuthentication.SignOut(); return View(); }

        [HttpPost]
        public JsonResult Admin_Request(string Username, string Password)
        {
            try
            {
                Session["Admin_UID"] = "0";
                Session["Admin_UNM"] = "";
                FormsAuthentication.SignOut();
                string ResVal = "0"; string ResSTR = "";
                Username = Username.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                Password = Password.Replace(",", " ").Replace("#", "").Replace("  ", " ").Trim();
                if ((Username.ToLower() == "tony@idvpacific.com.au") && (Password == "1234"))
                {
                    FormsAuthentication.SetAuthCookie("ICADLG", false);
                    Session["Admin_UID"] = "1";
                    Session["Admin_UNM"] = "Tony Merlo";
                    ResVal = "0"; ResSTR = "";
                }
                else
                {
                    ResVal = "1"; ResSTR = "Administrator username or password is incorrect, please try again later after check";
                }
                //DataTable DT = new DataTable();
                //DT = Sq.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select Element_ID,Element_Tag_Name,Element_Type_Text,Element_Row_Index,Element_Width,Status_Code From Admins_03_RegisterForms_Elements Where (Group_ID = '" + GID + "') And (Section_ID = '" + SID + "') And (Removed = '0') Order By Element_Row_Index,Element_Tag_Name");
                //if (DT.Rows != null)
                //{
                //    foreach (DataRow RW in DT.Rows)
                //    {

                //    }
                //}
                //else
                //{
                //    ResVal = "1"; ResSTR = "The server encountered an error while reloading information";
                //}
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
        public ActionResult AccessDenied()
        {
            return View();
        }
        //====================================================================================================================
    }
}