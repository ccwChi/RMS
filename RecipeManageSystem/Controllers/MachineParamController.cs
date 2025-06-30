using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using RecipeManageSystem.Generic;
using RecipeManageSystem.Models;
using RecipeManageSystem.Repository;

namespace RecipeManageSystem.Controllers
{
    public class MachineParamController : Controller
    {
        private readonly MachineParamRepository _machineParam = new MachineParamRepository();
        
        [PermissionAuthorize]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetMachineParamList(int page = 1, int rows = 20)
        {
            var data = _machineParam.GetAllMappings();

            return Content(JsonConvert.SerializeObject(new
            {
                success = true,
                data = data,
                total = data.Count
            }, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }), "application/json");
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
        [PermissionAuthorize(1,2)]
        public JsonResult SaveMapping(MachineParameterDto dto)
        {
            // 呼叫新的儲存邏輯：先刪再新增
            bool ok = _machineParam.SaveMappings(dto);
            return Json(new { success = ok });
        }

    }
}