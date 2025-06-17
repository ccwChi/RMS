using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RecipeManageSystem.Models;
using RecipeManageSystem.Repository;

namespace RecipeManageSystem.Controllers
{
    public class RecipeManageController : Controller
    {
        private readonly MachineParamRepository _machineParam = new MachineParamRepository();
        private readonly RecipeManageRepository _recipeManage = new RecipeManageRepository();
        public ActionResult Edit()
        {
            return View();
        }

        public ActionResult List()
        {
            return View();  
        }

        [HttpGet]
        public JsonResult GetParamDetailToEdit(string deviceId, string prodNo, string materialNo="", string moldNo = "")
        {
            // 只要取該機台目前所有 ParamId
            var paramDetails = _recipeManage.GetParamDetailToEdit(deviceId, prodNo, materialNo, moldNo);
            return Json(new
            {
                success = true,
                data = new
                {
                    DeviceID = deviceId,
                    Params = paramDetails
                }
            }, JsonRequestBehavior.AllowGet);
        }

        //[HttpGet]
        //public JsonResult GetRecipes(int page = 1, int rows = 10000)
        //{
        //    var all = _recipeManage.GetRecipes();
        //    // 簡易分頁（也可改成 SQL 分頁）
        //    var paged = all.Skip((page - 1) * rows).Take(rows).ToList();
        //    return Json(new
        //    {
        //        success = true,
        //        total = all.Count,
        //        rows = paged
        //    }, JsonRequestBehavior.AllowGet);
        //}
        [HttpGet]
        public JsonResult GetRecipes(string prodNo, string deviceName, int page = 1, int rows = 50)
        {
            // 1. 從 Repository 抓所有或條件後的列表
            var all = _recipeManage.GetRecipes(prodNo, deviceName);

            // 2. 分頁
            var paged = all.Skip((page - 1) * rows).Take(rows).ToList();
            return Json(new
            {
                success = true,
                total = all.Count,
                rows = paged
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetRecipeDetails(int recipeId)
        {
            var details = _recipeManage.GetRecipeDetails(recipeId);
            return Json(new
            {
                success = true,
                total = details.Count,
                rows = details
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveRecipe(RecipeTotalDto dto)
        {
            // 取當前登入者帳號
            var userName = User?.Identity?.Name ?? "Unknown";

            string mode = dto.Mode;

            var ok = _recipeManage.SaveRecipe(dto, mode, userName);
            return Json(new { success = ok });
        }

    }
}