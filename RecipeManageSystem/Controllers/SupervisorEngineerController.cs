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
        [PermissionAuthorize(1, 2)]
        public JsonResult SaveRelation(SupervisorEngineerRelation relation)
        {
            try
            {
                var user = (User as CustomPrincipal)?.UserName ?? User.Identity.Name ?? "system";

                if (relation.Id == 0)
                {
                    relation.CreateTime = DateTime.Now;
                    relation.UpdateTime = DateTime.Now;
                    _repository.CreateRelation(relation);
                }
                else
                {
                    relation.UpdateTime = DateTime.Now;
                    _repository.UpdateRelation(relation);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [PermissionAuthorize(1, 2)]
        public JsonResult DeleteRelation(long id)
        {
            try
            {
                _repository.DeleteRelation(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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