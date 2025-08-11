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
using RecipeManageSystem.Services;

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
        public JsonResult GetRecipes(string prodNo, string deviceName, string moldNo, bool showAllVersions = false, int page = 1, int rows = 50)
        {
            // 1. 從 Repository 抓所有或條件後的列表
            var all = _recipeManage.GetRecipes(prodNo, deviceName, moldNo, showAllVersions);

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
        [PermissionAuthorize(1)]
        public JsonResult SaveRecipe()
        {
            try
            {
                string json;
                using (var reader = new StreamReader(Request.InputStream))
                {
                    Request.InputStream.Position = 0;
                    json = reader.ReadToEnd();
                }

                // 改善：加入反序列化錯誤處理
                RecipeTotalDto dto;
                try
                {
                    dto = JsonConvert.DeserializeObject<RecipeTotalDto>(json);
                }
                catch (JsonException ex)
                {
                    return Json(new { success = false, message = $"資料格式錯誤：{ex.Message}" });
                }

                // 改善：參數驗證
                if (string.IsNullOrWhiteSpace(dto.DeviceId) || string.IsNullOrWhiteSpace(dto.ProdNo))
                {
                    return Json(new { success = false, message = "機台代號和料號不能為空" });
                }

                var currentUser = User as CustomPrincipal;
                var userName = currentUser?.UserName ?? User.Identity.Name ?? "Unknown";

                // 取得舊資料用於 Log 記錄
                RecipeTotalDto oldRecipe = null;
                if (dto.RecipeId > 0 && (dto.Mode == "save" || dto.Mode == "newVersion"))
                {
                    oldRecipe = _recipeManage.GetRecipeById(dto.RecipeId);
                }

                // 儲存配方
                bool success = _recipeManage.SaveRecipe(dto, dto.Mode, userName);

                if (success)
                {
                    try
                    {
                        // 根據不同模式記錄 Log
                        if (dto.RecipeId == 0) // 全新建立
                        {
                            // 新建立的配方，RecipeId 在 SaveRecipe 後才會有值
                            // 這裡我們用其他識別資訊來記錄
                            var logEntityId = $"{dto.DeviceId}-{dto.ProdNo}-{dto.MoldNo ?? ""}";
                            LogHelper.LogCreate(
                                LogTables.RECIPE_HEADER,
                                logEntityId,
                                LogModules.RECIPE,
                                CreateLogObject(dto),
                                $"新增配方 {dto.DeviceId}-{dto.ProdNo} v{dto.Version}"
                            );
                        }
                        else if (dto.Mode == "save") // 直接修改
                        {
                            LogHelper.LogUpdate(
                                LogTables.RECIPE_HEADER,
                                dto.RecipeId.ToString(),
                                LogModules.RECIPE,
                                CreateLogObject(oldRecipe),
                                CreateLogObject(dto),
                                $"修改配方 {dto.DeviceId}-{dto.ProdNo} v{dto.Version}"
                            );
                        }
                        else if (dto.Mode == "newVersion") // 新版本
                        {
                            LogHelper.LogCreate(
                                LogTables.RECIPE_HEADER,
                                dto.RecipeId.ToString(),
                                LogModules.RECIPE,
                                CreateLogObject(dto),
                                $"新增配方版本 {dto.DeviceId}-{dto.ProdNo} v{dto.Version} (基於 v{oldRecipe?.Version})"
                            );
                        }
                    }
                    catch (Exception logEx)
                    {
                        // Log 記錄失敗不影響主要功能，但可以記錄到系統 Log
                        System.Diagnostics.Debug.WriteLine($"配方操作 Log 記錄失敗: {logEx.Message}");
                    }
                }

                return Json(new
                {
                    success,
                    message = success ? "配方儲存成功" : "配方儲存失敗"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"儲存時發生錯誤：{ex.Message}" });
            }
        }

        /// <summary>
        /// 刪除指定的Recipe版本
        /// </summary>
        [HttpPost]
        [PermissionAuthorize(1)] // 刪除權限
        public JsonResult DeleteRecipe(int recipeId)
        {
            try
            {
                // 改善：參數驗證
                if (recipeId <= 0)
                {
                    return Json(new { success = false, message = "無效的配方ID" });
                }

                // 先取得要刪除的配方資料
                var recipeToDelete = _recipeManage.GetRecipeById(recipeId);
                if (recipeToDelete == null)
                {
                    return Json(new { success = false, message = "找不到指定的配方" });
                }

                var currentUser = User as CustomPrincipal;
                var userName = currentUser?.UserName ?? User.Identity.Name ?? "Unknown";

                var success = _recipeManage.DeleteRecipe(recipeId, userName);

                if (success)
                {
                    try
                    {
                        // 記錄刪除 Log
                        LogHelper.LogDelete(
                            LogTables.RECIPE_HEADER,
                            recipeId.ToString(),
                            LogModules.RECIPE,
                            CreateLogObject(recipeToDelete),
                            $"刪除配方 {recipeToDelete.DeviceId}-{recipeToDelete.ProdNo} v{recipeToDelete.Version}"
                        );
                    }
                    catch (Exception logEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"配方刪除 Log 記錄失敗: {logEx.Message}");
                    }
                }

                return Json(new
                {
                    success,
                    message = success ? "配方已刪除" : "刪除失敗"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"刪除時發生錯誤：{ex.Message}" });
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
        [PermissionAuthorize(1)]
        public JsonResult ToggleRecipeStatus(int recipeId, bool isActive)
        {
            try
            {
                // 改善：參數驗證
                if (recipeId <= 0)
                {
                    return Json(new { success = false, message = "無效的配方ID" });
                }

                // 先取得舊資料
                var oldRecipe = _recipeManage.GetRecipeById(recipeId);
                if (oldRecipe == null)
                {
                    return Json(new { success = false, message = "找不到指定的配方" });
                }

                var currentUser = User as CustomPrincipal;
                var userName = currentUser?.UserName ?? User.Identity.Name ?? "Unknown";

                var success = _recipeManage.ToggleRecipeStatus(recipeId, isActive, userName);

                if (success)
                {
                    try
                    {
                        // 取得更新後的資料並記錄 Log
                        var newRecipe = _recipeManage.GetRecipeById(recipeId);
                        LogHelper.LogUpdate(
                            LogTables.RECIPE_HEADER,
                            recipeId.ToString(),
                            LogModules.RECIPE,
                            CreateLogObject(oldRecipe),
                            CreateLogObject(newRecipe),
                            $"{(isActive ? "啟用" : "停用")}配方 {oldRecipe.DeviceId}-{oldRecipe.ProdNo} v{oldRecipe.Version}"
                        );
                    }
                    catch (Exception logEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"配方狀態切換 Log 記錄失敗: {logEx.Message}");
                    }
                }

                return Json(new
                {
                    success,
                    message = success ? (isActive ? "版本已啟用" : "版本已停用") : "狀態切換失敗"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"狀態切換時發生錯誤：{ex.Message}" });
            }
        }

        private object CreateLogObject(RecipeTotalDto recipe)
        {
            if (recipe == null) return null;

            return new
            {
                recipe.RecipeId,
                recipe.DeviceId,
                recipe.ProdNo,
                recipe.MoldNo,
                recipe.MaterialNo,
                recipe.Version,
                recipe.IsActive,
                recipe.Remark,
                ParamCount = recipe.RecipeDetails?.Count ?? 0,
                // 只記錄有設定標準值的參數摘要
                ActiveParams = recipe.RecipeDetails?
                    .Where(d => !string.IsNullOrEmpty(d.StdValue?.ToString()))
                    .Select(d => new { d.ParamName, d.StdValue, d.AlarmFlag })
                    .Take(5) // 最多記錄5個參數
                    .ToList()
            };
        }
    }
}