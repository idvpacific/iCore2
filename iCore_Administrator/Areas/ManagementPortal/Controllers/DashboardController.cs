using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using iCore_Administrator.Modules;

namespace iCore_Administrator.Areas.ManagementPortal.Controllers
{
    public class DashboardController : Controller
    {
        //====================================================================================================================
        AuthenticationTester AAuth = new AuthenticationTester();

        //====================================================================================================================
        public ActionResult Index()
        {
            ViewBag.MenuCode = 9;
            return View();
        }
        //====================================================================================================================
        public ActionResult Summary()
        {
            //--------------------------------------------------------------------------------------------------------------------------------------------------------------
            // Test Menu Access :
            ViewBag.MenuCode = 10;
            if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" }); }
            //--------------------------------------------------------------------------------------------------------------------------------------------------------------


            return View();
        }
        //====================================================================================================================
        public ActionResult Analytics()
        {
            //--------------------------------------------------------------------------------------------------------------------------------------------------------------
            // Test Menu Access :
            ViewBag.MenuCode = 11;
            if (AAuth.User_Authentication_Action(ViewBag.MenuCode) == false) { return RedirectToAction("Index", "Dashboard", new { id = "", area = "ManagementPortal" }); }
            //--------------------------------------------------------------------------------------------------------------------------------------------------------------


            return View();
        }
        //====================================================================================================================
        public ActionResult IconPack()
        {
            return View();
        }
        //====================================================================================================================
        public ActionResult Logout()
        {
            Session["Admin_UID"] = "0";
            Session["Admin_UNM"] = "";
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login", new { id = "", area = "" });
        }
        //====================================================================================================================
    }
}