using System.Linq;
using System.Web.Mvc;
using RecipeManageSystem.Generic;
using RecipeManageSystem.Models;
using RecipeManageSystem.Repository;

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
        [PermissionAuthorize(1,2)]
        public JsonResult SaveParamDefinition(Parameter parameter)
        {
            if (parameter.ParamId == 0)
            {
                // 這邊可以從 Session 拿目前使用者當 CreatedBy
                parameter.CreateBy = User.Identity.Name;
                _param.CreateParameter(parameter);
            }
            else
            {
                parameter.UpdateBy = User.Identity.Name;
                _param.UpdateParameter(parameter);
            }
            return Json(new { success = true });
        }
    }
}