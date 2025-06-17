using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RecipeManageSystem.Models;
using RecipeManageSystem.Repository;

namespace RecipeManageSystem.Controllers
{
    public class MachineParamController : Controller
    {
        private readonly MachineParamRepository _machineParam = new MachineParamRepository();
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetMachineParamList(int page = 1, int rows = 20)
        {
            var data = _machineParam.GetAllMappings();
            return Json(new { success = true, total = data.Count, data = data }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetMappingByDevice(string deviceId)
        {
            // 只要取該機台目前所有 ParamId
            var paramIds = _machineParam.GetParamIdsByMachine(deviceId);
            return Json(new
            {
                success = true,
                data = new
                {
                    DeviceId = deviceId,
                    ParamIds = paramIds
                }
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveMapping(MachineParameterDto dto)
        {
            // 呼叫新的儲存邏輯：先刪再新增
            bool ok = _machineParam.SaveMappings(dto);
            return Json(new { success = ok });
        }

    }
}