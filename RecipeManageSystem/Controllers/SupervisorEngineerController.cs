using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RecipeManageSystem.Generic;
using RecipeManageSystem.Models;
using RecipeManageSystem.Repository;
using RecipeManageSystem.Services;

namespace RecipeManageSystem.Controllers
{
    public class SupervisorEngineerController : Controller
    {
        private readonly SupervisorEngineerRepository _repository = new SupervisorEngineerRepository();

        [PermissionAuthorize]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetSupervisorEngineers()
        {
            var list = _repository.GetAllRelations();
            return Json(new { success = true, total = list.Count, rows = list }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetRelation(long id)
        {
            var relation = _repository.GetRelationById(id);
            return Json(new { success = true, data = relation }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [PermissionAuthorize(5)]
        public JsonResult SaveRelation(SupervisorEngineerRelation relation)
        {
            try
            {
                // 改善：參數驗證
                if (string.IsNullOrWhiteSpace(relation.SupervisorNo) || string.IsNullOrWhiteSpace(relation.EngineerNo))
                {
                    return Json(new { success = false, message = "主管和工程師工號不能為空" });
                }

                var currentUser = User as CustomPrincipal;
                var userName = currentUser?.UserName ?? User.Identity.Name ?? "system";

                SupervisorEngineerRelation oldRelation = null;
                bool isUpdate = relation.Id > 0;

                if (isUpdate)
                {
                    // 更新時先取得舊資料
                    oldRelation = _repository.GetRelationById(relation.Id);
                    if (oldRelation == null)
                    {
                        return Json(new { success = false, message = "找不到指定的關聯記錄" });
                    }

                    relation.UpdateTime = DateTime.Now;
                    _repository.UpdateRelation(relation);

                    // 記錄更新 Log
                    LogHelper.LogUpdate(
                        "SupervisorEngineerRelation",
                        relation.Id.ToString(),
                        "SupervisorEngineer",
                        oldRelation,
                        relation,
                        $"修改主管工程師關聯：{relation.SupervisorNo} - {relation.EngineerNo}"
                    );
                }
                else
                {
                    // 新增
                    relation.CreateTime = DateTime.Now;
                    relation.UpdateTime = DateTime.Now;
                    _repository.CreateRelation(relation);

                    // 記錄新增 Log
                    LogHelper.LogCreate(
                        "SupervisorEngineerRelation",
                        relation.Id.ToString(),
                        "SupervisorEngineer",
                        relation,
                        $"新增主管工程師關聯：{relation.SupervisorNo} - {relation.EngineerNo}"
                    );
                }

                return Json(new
                {
                    success = true,
                    message = isUpdate ? "關聯記錄已更新" : "關聯記錄已新增"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"儲存失敗：{ex.Message}" });
            }
        }

        [HttpPost]
        [PermissionAuthorize(5)]
        public JsonResult DeleteRelation(long id)
        {
            try
            {
                // 改善：參數驗證
                if (id <= 0)
                {
                    return Json(new { success = false, message = "無效的關聯記錄ID" });
                }

                // 先取得要刪除的資料
                var relationToDelete = _repository.GetRelationById(id);
                if (relationToDelete == null)
                {
                    return Json(new { success = false, message = "找不到指定的關聯記錄" });
                }

                _repository.DeleteRelation(id);

                // 記錄刪除 Log
                LogHelper.LogDelete(
                    "SupervisorEngineerRelation",
                    id.ToString(),
                    "SupervisorEngineer",
                    relationToDelete,
                    $"刪除主管工程師關聯：{relationToDelete.SupervisorNo} - {relationToDelete.EngineerNo}"
                );

                return Json(new
                {
                    success = true,
                    message = "關聯記錄已刪除"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"刪除失敗：{ex.Message}" });
            }
        }

        [HttpGet]
        public JsonResult GetAllEngineers()
        {
            var engineers = _repository.GetAllEngineers();
            return Json(new { success = true, data = engineers }, JsonRequestBehavior.AllowGet);
        }
    }
}