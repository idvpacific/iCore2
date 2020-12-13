using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iCore_Administrator.Modules.SecurityAuthentication;

namespace iCore_Administrator.Areas.ManagementPortal.Controllers
{
    public class DashboardController : Controller
    {
        //====================================================================================================================
        ActionAuthentication AAuth = new ActionAuthentication();

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
            return RedirectToAction("Index", "Login", new { id = "", area = "" });
        }
        //====================================================================================================================
    }
}