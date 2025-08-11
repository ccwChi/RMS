using System;
using System.Web.Mvc;
using RecipeManageSystem.Generic;
using RecipeManageSystem.Models;
using RecipeManageSystem.Services;

namespace RecipeManageSystem.Controllers
{
    public class LogController : Controller
    {
        private readonly LogService _logService;

        public LogController()
        {
            _logService = new LogService();
        }

        /// <summary>
        /// 操作記錄頁面
        /// </summary>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 取得操作記錄列表 (Ajax)
        /// </summary>
        [HttpGet]
        public JsonResult GetLogs(LogQueryDto query)
        {
            try
            {
                var (logs, total) = _logService.GetLogs(query);
                return Json(new
                {
                    success = true,
                    total = total,
                    rows = logs
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "查詢操作記錄失敗：" + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 取得操作記錄詳細資訊 (Ajax)
        /// </summary>
        [HttpGet]
        public JsonResult GetLogDetail(long logId)
        {
            try
            {
                var detail = _logService.GetLogDetail(logId);
                if (detail == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "找不到指定的操作記錄"
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        detail.Log.LogId,
                        detail.Log.TableName,
                        detail.Log.EntityId,
                        detail.Log.Operation,
                        detail.Log.OperateBy,
                        detail.OperatorName,
                        detail.DepartmentName,
                        OperateDate = detail.Log.OperateDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        detail.Log.Module,
                        detail.Log.Description,
                        detail.Log.IpAddress,
                        detail.Log.UserAgent,
                        FieldChanges = detail.FieldChanges
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "查詢操作記錄詳情失敗：" + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 取得特定實體的操作記錄 (Ajax)
        /// </summary>
        [HttpGet]
        public JsonResult GetEntityLogs(string tableName, string entityId)
        {
            try
            {
                var logs = _logService.GetEntityLogs(tableName, entityId);
                return Json(new
                {
                    success = true,
                    total = logs.Count,
                    rows = logs
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "查詢實體操作記錄失敗：" + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 操作記錄詳細頁面
        /// </summary>
        [PermissionAuthorize(7)]
        public ActionResult Detail(long id)
        {
            var detail = _logService.GetLogDetail(id);
            if (detail == null)
            {
                TempData["ErrorMessage"] = "找不到指定的操作記錄";
                return RedirectToAction("Index");
            }

            return View(detail);
        }

        /// <summary>
        /// 實體操作記錄頁面 (用於顯示某個使用者、角色等的所有操作記錄)
        /// </summary>
        public ActionResult EntityLogs(string tableName, string entityId)
        {
            ViewBag.TableName = tableName;
            ViewBag.EntityId = entityId;
            ViewBag.Title = GetEntityDisplayName(tableName, entityId);

            return View();
        }

        #region 私有方法

        /// <summary>
        /// 取得實體顯示名稱
        /// </summary>
        private string GetEntityDisplayName(string tableName, string entityId)
        {
            switch (tableName?.ToLower())
            {
                case "users":
                    return $"使用者 {entityId} 的操作記錄";
                case "roles":
                    return $"角色 {entityId} 的操作記錄";
                case "parameter":
                    return $"參數 {entityId} 的操作記錄";
                case "recipeheader":
                    return $"配方 {entityId} 的操作記錄";
                case "machineparameter":
                    return $"機台參數 {entityId} 的操作記錄";
                default:
                    return $"{tableName} {entityId} 的操作記錄";
            }
        }

        #endregion
    }
}