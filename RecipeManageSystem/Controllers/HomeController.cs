using System.Web.Mvc;
using RecipeManageSystem.Generic;

namespace RecipeManageSystem.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        
        {
            var user = User as CustomPrincipal;

            if (user?.UserNo == null)
            {
                TempData["LoginMessage"] = "尚未登入，請確認登入狀態。";
                return RedirectToAction("NoAuth");
            }

            // 有登入的情況
            ViewBag.UserName = user.UserName;
            ViewBag.UserNo = user.UserNo;

            return View();
        }

        public ActionResult NoAuth()
        {
            ViewBag.Message = TempData["LoginMessage"] ?? "無法存取此頁面。";
            return View();
        }

        public ActionResult GetRMSLogo()
        {
            string RMS_logo = Server.MapPath("~/App_Data/RMS_logo.png"); // 獲取圖片的物理路徑

            if (System.IO.File.Exists(RMS_logo))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(RMS_logo); // 讀取圖片為字節陣列
                return File(fileBytes, "image/png"); // 返回圖片
            }
            else
            {
                return HttpNotFound(); // 如果文件不存在，返回 404 錯誤
            }
        }
    }
}
