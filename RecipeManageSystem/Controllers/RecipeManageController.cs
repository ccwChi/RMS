using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using RecipeManageSystem.Generic;
using RecipeManageSystem.Models;
using RecipeManageSystem.Repository;

namespace RecipeManageSystem.Controllers
{
    public class RecipeManageController : Controller
    {
        private readonly MachineParamRepository _machineParam = new MachineParamRepository();
        private readonly RecipeManageRepository _recipeManage = new RecipeManageRepository();

        [PermissionAuthorize]
        public ActionResult Edit()
        {
            return View();
        }

        [PermissionAuthorize]
        public ActionResult List()
        {
            return View();  
        }

        [HttpGet]
        public JsonResult GetDetailToEdit(string deviceId, string prodNo, string materialNo = "", string moldNo = "")
        {
            // 1. 先撈出這組鍵值所有 RecipeHeader / RecipeDetail
            var versions = _recipeManage.GetRecipeVersions(deviceId, prodNo, materialNo, moldNo);

            return Json(new
            {
                success = true,
                data = versions.Select(v => new {
                    v.RecipeId,
                    v.DeviceId,
                    v.MoldNo,
                    v.MaterialNo,
                    v.ProdNo,
                    v.Version,
                    v.Remark,        
                    v.CreateBy,
                    v.CreateDate,
                    v.UpdateBy,
                    v.UpdateDate,
                    Params = v.Params // 這版本的參數明細
                })
            }, JsonRequestBehavior.AllowGet);
        }


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
        [ValidateInput(false)] // 避免 MVC 自動擋掉包含 HTML 的內容
        [PermissionAuthorize(3)]
        public JsonResult SaveRecipe()
        {
            string json;
            using (var reader = new StreamReader(Request.InputStream))
            {
                Request.InputStream.Position = 0; // 確保從開頭讀
                json = reader.ReadToEnd();
            }

            // 這裡使用 Json.NET（Newtonsoft.Json）反序列化
            var dto = JsonConvert.DeserializeObject<RecipeTotalDto>(json);

            var userName = User?.Identity?.Name ?? "Unknown";
            var ok = _recipeManage.SaveRecipe(dto, dto.Mode, userName);

            return Json(new { success = ok });
        }

    }
}