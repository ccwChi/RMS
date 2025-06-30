using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RecipeManageSystem.Generic;
using RecipeManageSystem.Repository;

namespace RecipeManageSystem.Controllers
{
    public class MachineManageController : Controller
    {
        private readonly MachineParamRepository _machineParam = new MachineParamRepository();

        [PermissionAuthorize]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetMachineList()
        {
            var data = _machineParam.GetMachineList()
                            .Select(m => new {
                                m.DeviceID,
                                m.DeviceName
                            });
            return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
        }
    }
}