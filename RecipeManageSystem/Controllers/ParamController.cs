using System;
using System.Linq;
using System.Web.Mvc;
using RecipeManageSystem.Generic;
using RecipeManageSystem.Models;
using RecipeManageSystem.Repository;
using RecipeManageSystem.Services;

namespace RecipeManageSystem.Controllers
{
    public class ParamController : Controller
    {
        private readonly ParamRepository _param = new ParamRepository();

        [PermissionAuthorize]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetParameterList()
        {
            var list = _param.GetParameterList()
                            .Select((m, i) => new {
                                Index = i + 1,
                                m.ParamId,
                                m.ParamName,
                                m.SectionCode,
                                m.Unit,
                                m.SequenceNo,
                                m.CreateBy,
                                CreateDate = m.CreateDate.ToString("yyyy-MM-dd"),
                                m.IsActive
                            })
                            .ToList();
            return Json(new { total = list.Count, data = list, success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetParam(int paramId)
        {
            var m = _param.GetParameterById(paramId);
            if (m == null)
            {
                return Json(new { success = false, message = "找不到指定的參數定義" }, JsonRequestBehavior.AllowGet);
            }
            // 回傳時 data 裡面包原始欄位，加上 SuitableMachine 逗號字串
            return Json(new
            {
                success = true,
                data = new
                {
                    m.ParamId,
                    m.ParamName,
                    m.Unit,
                    m.SectionCode,
                    m.SequenceNo,
                    m.IsActive
                }
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [PermissionAuthorize(1, 2)]
        public JsonResult SaveParamDefinition(Parameter parameter)
        {
            try
            {
                // 改善：參數驗證
                if (string.IsNullOrWhiteSpace(parameter.ParamName))
                {
                    return Json(new { success = false, message = "參數名稱不能為空" });
                }

                // 改善：統一取得使用者方式
                var currentUser = User as CustomPrincipal;
                var userName = currentUser?.UserName ?? User.Identity.Name ?? "system";

                Parameter oldParameter = null;
                bool isUpdate = parameter.ParamId > 0;

                if (isUpdate)
                {
                    // 更新時先取得舊資料
                    oldParameter = _param.GetParameterById(parameter.ParamId);
                    if (oldParameter == null)
                    {
                        return Json(new { success = false, message = "找不到指定的參數" });
                    }

                    parameter.UpdateBy = userName;
                    _param.UpdateParameter(parameter);

                    // 記錄更新 Log
                    LogHelper.LogParameterOperation("UPDATE", parameter.ParamId, oldParameter, parameter);
                }
                else
                {
                    // 新增參數
                    parameter.CreateBy = userName;
                    _param.CreateParameter(parameter);

                    // 記錄新增 Log
                    LogHelper.LogParameterOperation("CREATE", parameter.ParamId, null, parameter);
                }

                return Json(new
                {
                    success = true,
                    message = isUpdate ? "參數已更新" : "參數已新增"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"儲存失敗：{ex.Message}" });
            }
        }
    }
}