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
    public class AlarmManageController : Controller
    {
        private readonly AlarmManageRepository _alarmManage = new AlarmManageRepository();
        private readonly PermissionRepository _permission = new PermissionRepository();

        [PermissionAuthorize]
        public ActionResult Index()
        {
            return View();
        }

        // ===== 機台群組相關 API =====
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
        [PermissionAuthorize(6)]
        public JsonResult SaveMachineGroup(MachineGroup dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.GroupName))
                {
                    return Json(new { success = false, message = "群組名稱不能為空" });
                }

                if (dto.Devices == null || !dto.Devices.Any())
                {
                    return Json(new { success = false, message = "請至少選擇一台機台" });
                }

                var currentUser = User as CustomPrincipal;
                var userName = currentUser?.UserName ?? User.Identity.Name ?? "system";

                MachineGroup oldGroup = null;
                bool isUpdate = dto.MachineGroupId > 0;

                if (isUpdate)
                {
                    oldGroup = _alarmManage.GetMachineGroupById(dto.MachineGroupId);
                    if (oldGroup == null)
                    {
                        return Json(new { success = false, message = "找不到指定的機台群組" });
                    }
                }

                var newId = _alarmManage.SaveMachineGroup(
                    dto.MachineGroupId,
                    dto.GroupName,
                    dto.Description,
                    dto.Devices,
                    userName
                );

                var newGroup = _alarmManage.CreateMachineGroupDto(
                    newId,
                    dto.GroupName,
                    dto.Description,
                    dto.Devices,
                    userName
                );

                // 記錄 Log
                if (isUpdate)
                {
                    LogHelper.LogUpdate(LogTables.MACHINE_GROUP, dto.MachineGroupId.ToString(), LogModules.ALARM, oldGroup, newGroup);
                }
                else
                {
                    LogHelper.LogCreate(LogTables.MACHINE_GROUP, newId.ToString(), LogModules.ALARM, newGroup);
                }

                return Json(new
                {
                    success = true,
                    groupId = newId,
                    message = isUpdate ? "機台群組已更新" : "機台群組已新增"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"儲存失敗：{ex.Message}"
                });
            }
        }

        [HttpPost]
        [PermissionAuthorize(6)]
        public JsonResult DeleteMachineGroup(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Json(new { success = false, message = "無效的機台群組ID" });
                }

                var groupToDelete = _alarmManage.GetMachineGroupById(id);
                if (groupToDelete == null)
                {
                    return Json(new { success = false, message = "找不到指定的機台群組" });
                }

                // 檢查是否被警報群組使用
                var (canDelete, reason) = _alarmManage.CanDeleteMachineGroup(id);
                if (!canDelete)
                {
                    return Json(new { success = false, message = reason });
                }

                var success = _alarmManage.DeleteMachineGroup(id);

                if (success)
                {
                    LogHelper.LogDelete(LogTables.MACHINE_GROUP, id.ToString(), LogModules.ALARM, groupToDelete);
                }

                return Json(new
                {
                    success = success,
                    message = success ? $"機台群組「{groupToDelete.GroupName}」已刪除" : "刪除失敗"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"刪除失敗：{ex.Message}"
                });
            }
        }

        [HttpGet]
        public JsonResult CheckMachineGroupCanDelete(int machineGroupId)
        {
            try
            {
                var (canDelete, reason) = _alarmManage.CanDeleteMachineGroup(machineGroupId);
                return Json(new
                {
                    success = true,
                    canDelete = canDelete,
                    reason = reason
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"檢查失敗：{ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // ===== 警報群組相關 API =====
        [HttpGet]
        public JsonResult GetAlertGroups()
        {
            try
            {
                var groups = _alarmManage.GetAlertGroupsSimplified();
                return Json(new
                {
                    success = true,
                    total = groups.Count,
                    rows = groups
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"取得警報群組失敗：{ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [PermissionAuthorize(6)]
        public JsonResult SaveAlertGroups(AlertGroupBatchDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.GroupName))
                {
                    return Json(new { success = false, message = "群組名稱不能為空" });
                }

                if (!dto.RoleId.HasValue)
                {
                    return Json(new { success = false, message = "請選擇一個角色" });
                }

                if (dto.MachineGroupIds == null || !dto.MachineGroupIds.Any())
                {
                    return Json(new { success = false, message = "請至少選擇一個機台群組" });
                }

                var currentUser = User as CustomPrincipal;
                var userName = currentUser?.UserName ?? User.Identity.Name ?? "system";

                // 檢查是否為更新（根據群組名稱和角色判斷）
                var existingGroups = _alarmManage.GetAlertGroupsByNameAndRole(dto.GroupName, dto.RoleId.Value);
                bool isUpdate = existingGroups.Any();

                List<AlertGroupDto> oldGroups = null;
                if (isUpdate)
                {
                    oldGroups = existingGroups;
                }

                // 執行批量儲存
                var savedGroups = _alarmManage.SaveAlertGroupBatch(dto, userName);

                // 記錄 Log
                foreach (var savedGroup in savedGroups)
                {
                    if (isUpdate)
                    {
                        var oldGroup = oldGroups?.FirstOrDefault(g => g.MachineGroupId == savedGroup.MachineGroupId);
                        LogHelper.LogUpdate(LogTables.ALERT_GROUP, savedGroup.AlertGroupId.ToString(), LogModules.ALARM, oldGroup, savedGroup);
                    }
                    else
                    {
                        LogHelper.LogCreate(LogTables.ALERT_GROUP, savedGroup.AlertGroupId.ToString(), LogModules.ALARM, savedGroup);
                    }
                }

                return Json(new
                {
                    success = true,
                    message = isUpdate ? "警報群組已更新" : "警報群組已新增",
                    count = savedGroups.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"儲存失敗：{ex.Message}"
                });
            }
        }

        [HttpGet]
        public JsonResult GetAlertGroupDetail(string groupName, int roleId)
        {
            try
            {
                var detail = _alarmManage.GetAlertGroupDetailByNameAndRole(groupName, roleId);
                return Json(new
                {
                    success = true,
                    data = detail
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"取得詳細資料失敗：{ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [PermissionAuthorize(6)]
        public JsonResult DeleteAlertGroups(AlertGroupDeleteDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.GroupName) || !dto.RoleId.HasValue)
                {
                    return Json(new { success = false, message = "參數不完整" });
                }

                // 先取得要刪除的群組資料
                var groupsToDelete = _alarmManage.GetAlertGroupsByNameAndRole(dto.GroupName, dto.RoleId.Value);
                if (!groupsToDelete.Any())
                {
                    return Json(new { success = false, message = "找不到指定的警報群組" });
                }

                var success = _alarmManage.DeleteAlertGroupsByNameAndRole(dto.GroupName, dto.RoleId.Value);

                if (success)
                {
                    // 記錄每個被刪除的群組
                    foreach (var group in groupsToDelete)
                    {
                        LogHelper.LogDelete(LogTables.ALERT_GROUP, group.AlertGroupId.ToString(), LogModules.ALARM, group);
                    }
                }

                return Json(new
                {
                    success = success,
                    message = success ? $"警報群組「{dto.GroupName}」已刪除" : "刪除失敗",
                    deletedCount = groupsToDelete.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"刪除失敗：{ex.Message}"
                });
            }
        }

        [HttpPost]
        [PermissionAuthorize(6)]
        public JsonResult ToggleAlertGroupsActive(AlertGroupToggleDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.GroupName) || !dto.RoleId.HasValue)
                {
                    return Json(new { success = false, message = "參數不完整" });
                }

                var currentUser = User as CustomPrincipal;
                var userName = currentUser?.UserName ?? User.Identity.Name ?? "system";

                // 先取得舊資料
                var oldGroups = _alarmManage.GetAlertGroupsByNameAndRole(dto.GroupName, dto.RoleId.Value);
                if (!oldGroups.Any())
                {
                    return Json(new { success = false, message = "找不到指定的警報群組" });
                }

                var success = _alarmManage.ToggleAlertGroupsActive(dto.GroupName, dto.RoleId.Value, dto.IsActive, userName);

                if (success)
                {
                    // 取得新資料並記錄 Log
                    var newGroups = _alarmManage.GetAlertGroupsByNameAndRole(dto.GroupName, dto.RoleId.Value);
                    for (int i = 0; i < oldGroups.Count && i < newGroups.Count; i++)
                    {
                        LogHelper.LogUpdate(LogTables.ALERT_GROUP, oldGroups[i].AlertGroupId.ToString(), LogModules.ALARM, oldGroups[i], newGroups[i]);
                    }
                }

                return Json(new
                {
                    success = success,
                    message = success ? $"警報群組已{(dto.IsActive ? "啟用" : "停用")}" : "操作失敗"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"操作失敗：{ex.Message}"
                });
            }
        }

        [HttpGet]
        public JsonResult GetRoles()
        {
            var roles = _permission.GetRoles();
            return Json(roles, JsonRequestBehavior.AllowGet);
        }
    }
}