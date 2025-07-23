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
        public JsonResult GetAlertGroups()
        {
            return null;
        }




        [HttpGet]
        public JsonResult GetRoles()
        {
            var roles = _permission.GetRoles(); // 取得 List<RoleDto> 
            return Json(roles, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAlertGroupRoles(int id)
        {
            var roleIds = _alarmManage.GetAlertGroupRoles(id); // 取得 List<int>
            return Json(new { success = true, data = roleIds }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveGroup(AlertGroupDto dto)
        {
            var user = (User as CustomPrincipal)?.UserName ?? "system";
            _alarmManage.SaveAlertGroup(dto, user);
            return Json(new { success = true });
        }
    }
}