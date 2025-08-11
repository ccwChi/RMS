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
    public class AlarmManageController : Controller
    {
        private readonly AlarmManageRepository _alarmManage = new AlarmManageRepository();
        private readonly PermissionRepository _permission = new PermissionRepository();
        // GET: AlarmManage

        [PermissionAuthorize]
        public ActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public JsonResult GetMachineGroups()
        {
            var list = _alarmManage.GetMachineGroups();
            return Json(new { total = list.Count, rows = list }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetMachineGroupsById(int id)
        {
            var deviceIds = _alarmManage.GetMachineGroupsById(id);
            return Json(new { success = true, data = deviceIds }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveMachineGroup(MachineGroup dto)
        {
            var user = (User as CustomPrincipal)?.UserName ?? User.Identity.Name ?? "system";
            var newId = _alarmManage.SaveMachineGroup(
                dto.MachineGroupId, dto.GroupName, dto.Description, dto.Devices, user
            );
            return Json(new { success = true, groupId = newId });
        }

        [HttpPost]
        public JsonResult DeleteMachineGroup(int id)
        {
            var ok = _alarmManage.DeleteMachineGroup(id);
            return Json(new { success = ok });
        }


        [HttpGet]
        public JsonResult GetRoles()
        {
            var roles = _permission.GetRoles(); // 取得 List<RoleDto> 
            return Json(roles, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAlertGroups()
        {
            var groups = _alarmManage.GetAlertGroups();
            return Json(new
            {
                success = true,
                total = groups.Count,
                rows = groups
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAlertGroupDetail(int alertGroupId)
        {
            var detail = _alarmManage.GetAlertGroupDetail(alertGroupId);
            return Json(new
            {
                success = true,
                data = detail
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetMachineGroupsForSelect()
        {
            var groups = _alarmManage.GetMachineGroupsWithDevices();
            return Json(new
            {
                success = true,
                data = groups.Select(g => new {
                    MachineGroupId = g.MachineGroupId,
                    GroupName = g.GroupName,
                    Description = $"{g.Description} ({g.DeviceCount}台機台: {g.DeviceList})"
                })
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ToggleGroupActive(int AlertGroupId, bool IsActive)
        {
            try
            {
                var user = (User as CustomPrincipal)?.UserName ?? "system";
                var success = _alarmManage.ToggleAlertGroupActive(AlertGroupId, IsActive, user);
                return Json(new { success });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        public JsonResult DeleteAlertGroup(int alertGroupId)
        {
            var ok = _alarmManage.DeleteAlertGroup(alertGroupId);
            return Json(new { success = ok });
        }

        [HttpPost]
        public JsonResult SaveGroup(AlertGroupDto dto)
        {
            try
            {
                var user = (User as CustomPrincipal)?.UserName ?? "system";
                _alarmManage.SaveAlertGroup(dto, user);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}