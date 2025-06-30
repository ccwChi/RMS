using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RecipeManageSystem.Generic;
using RecipeManageSystem.Models;
using RecipeManageSystem.Repository;

namespace RecipeManageSystem.Controllers
{
    public class AlarmManageController : Controller
    {
        private readonly AlarmManageRepository _alarmManage = new AlarmManageRepository();
        // GET: AlarmManage

        [PermissionAuthorize]
        public ActionResult Index()
        {
            return View();
        }



        [HttpGet]
        public JsonResult GetAlertUsers()
        {
            List<AlertUser> rows = _alarmManage.GetAlertUsers();
            return Json(new { success = true, rows }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveAlertUser(string UserNo)
        {
            if (string.IsNullOrWhiteSpace(UserNo))
                return Json(new { success = false, message = "工號不可為空" });


            var cp = User as CustomPrincipal;
            string currentUser = cp?.UserName ?? User.Identity.Name ?? "system";

            bool ok = _alarmManage.InsertAlertUser(UserNo, currentUser);
            return Json(new { success = ok });
        }

        [HttpPost]
        public JsonResult DeleteAlertUser(int alertId)
        {
            bool ok = _alarmManage.DeleteAlertUser(alertId);
            return Json(new { success = ok });
        }


    }
}