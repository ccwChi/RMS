using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        public ActionResult Index()
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
                    v.IsActive,
                    v.CreateBy,
                    v.CreateDate,
                    v.UpdateBy,
                    v.UpdateDate,
                    v.Params
                })
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetRecipes(string prodNo, string deviceName, string moldNo, int page = 1, int rows = 50)
        {
            // 1. 從 Repository 抓所有或條件後的列表
            var all = _recipeManage.GetRecipes(prodNo, deviceName, moldNo);

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


        /// <summary>
        /// 根據機台ID取得該機台所有的料號清單
        /// </summary>
        [HttpGet]
        public JsonResult GetProdNosByDevice(string deviceId)
        {
            try
            {
                var prodNos = _recipeManage.GetProdNosByDevice(deviceId);
                return Json(new
                {
                    success = true,
                    data = prodNos
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "取得料號清單失敗：" + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 根據機台ID和料號取得模具清單
        /// </summary>
        [HttpGet]
        public JsonResult GetMoldNosByDeviceAndProd(string deviceId, string prodNo)
        {
            try
            {
                var moldNos = _recipeManage.GetMoldNosByDeviceAndProd(deviceId, prodNo);
                return Json(new
                {
                    success = true,
                    data = moldNos
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "取得模具清單失敗：" + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 根據機台ID和料號取得原料清單
        /// </summary>
        [HttpGet]
        public JsonResult GetMaterialNosByDeviceAndProd(string deviceId, string prodNo)
        {
            try
            {
                var materialNos = _recipeManage.GetMaterialNosByDeviceAndProd(deviceId, prodNo);
                return Json(new
                {
                    success = true,
                    data = materialNos
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "取得原料清單失敗：" + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
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

        /// <summary>
        /// 刪除指定的Recipe版本
        /// </summary>
        [HttpPost]
        [PermissionAuthorize(4)] // 假設權限ID 4 是刪除權限
        public JsonResult DeleteRecipe(int recipeId)
        {
            try
            {
                var userName = User?.Identity?.Name ?? "Unknown";
                var success = _recipeManage.DeleteRecipe(recipeId, userName);

                return Json(new
                {
                    success = success,
                    message = success ? "刪除成功" : "刪除失敗"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "刪除時發生錯誤：" + ex.Message
                });
            }
        }

        // 在 RecipeManageController.cs 中新增此方法
        /// <summary>
        /// 取得指定機台的參數定義（用於建立新Recipe）
        /// </summary>
        [HttpGet]
        public JsonResult GetMachineParameterDefinitions(string deviceId)
        {
            try
            {
                var paramDefinitions = _recipeManage.GetMachineParameterDefinitions(deviceId);
                return Json(new
                {
                    success = true,
                    data = paramDefinitions
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "取得機台參數定義失敗：" + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // 在 RecipeManageController.cs 中新增此方法
        /// <summary>
        /// 切換Recipe版本的啟用/停用狀態
        /// </summary>
        [HttpPost]
        [PermissionAuthorize(3)] // 使用與儲存相同的權限
        public JsonResult ToggleRecipeStatus(int recipeId, bool isActive)
        {
            try
            {
                var userName = User?.Identity?.Name ?? "Unknown";
                var success = _recipeManage.ToggleRecipeStatus(recipeId, isActive, userName);

                return Json(new
                {
                    success = success,
                    message = success ? (isActive ? "版本已啟用" : "版本已停用") : "狀態切換失敗"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "狀態切換時發生錯誤：" + ex.Message
                });
            }
        }

    }
}