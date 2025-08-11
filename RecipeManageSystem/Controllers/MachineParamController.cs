using System;
using System.Collections.Generic;
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
        [PermissionAuthorize(1, 2)]
        public JsonResult SaveMapping(MachineParameterDto dto)
        {
            try
            {
                // 改善：參數驗證
                if (string.IsNullOrWhiteSpace(dto.DeviceId))
                {
                    return Json(new { success = false, message = "機台代號不能為空" });
                }

                if (dto.Params == null || !dto.Params.Any())
                {
                    return Json(new { success = false, message = "請至少選擇一個參數" });
                }

                // 先取得舊的機台參數設定
                var oldParams = _machineParam.GetParamIdsByMachine(dto.DeviceId);

                bool success = _machineParam.SaveMappings(dto);

                if (success)
                {
                    // 記錄 Log
                    LogHelper.LogUpdate(
                        LogTables.MACHINE_PARAMETER,
                        dto.DeviceId,
                        LogModules.MACHINE_PARAM,
                        new { DeviceId = dto.DeviceId, Params = oldParams },
                        dto,
                        $"更新機台 {dto.DeviceId} 的參數對照設定"
                    );
                }

                return Json(new
                {
                    success,
                    message = success ? "機台參數對照已更新" : "更新失敗"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"儲存失敗：{ex.Message}" });
            }
        }




    }
}