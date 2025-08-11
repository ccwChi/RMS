using System.Web.Mvc;
using RecipeManageSystem.Models;
using RecipeManageSystem.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using System;
using RecipeManageSystem.Generic;
using RecipeManageSystem.Services;

namespace RecipeManageSystem.Controllers
{
    public class PermissionController : Controller
    {
        private readonly PermissionRepository _permission = new PermissionRepository();

        //[PermissionAuthorize(PermissionId = 6)]
        [PermissionAuthorize]
        public ActionResult RoleList()
        {
            return View("~/Views/Permission/RoleList.cshtml");
        }

        [PermissionAuthorize]
        // 使用者帳號與權限管理頁
        public ActionResult UserList()
        {
            return View("~/Views/Permission/UserList.cshtml");
        }


        [HttpGet]
        public JsonResult GetRoles()
        {
            var roles = _permission.GetRoles();

            return Json(new
            {
                success =true,
                count = roles.Count,  // EasyUI 會去讀 total，知道總共有幾筆
                data = roles          // EasyUI 會去讀 rows，把它當作要在表格顯示的資料
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAllPermissions()
        {
            List<Permission> list = _permission.GetAllPermissions();

            return Json(new
            {
                data = list,
                success = true
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetRole(int id)
        {
            var role = _permission.GetRole(id);
            return Json(new
            {
                data = role,
                success = true
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [PermissionAuthorize(4)]
        public JsonResult SaveRole(Role dto)
        {
            try
            {
                bool success;
                Role oldRole = null;

                if (dto.RoleId != 0)
                {
                    // 更新時先取得舊資料
                    oldRole = _permission.GetRole(dto.RoleId);
                    success = _permission.UpdateRole(dto);

                    if (success)
                    {
                        LogHelper.LogRoleOperation("UPDATE", dto.RoleId, oldRole, dto);
                    }
                }
                else
                {
                    success = _permission.InsertRole(dto);

                    if (success)
                    {
                        LogHelper.LogRoleOperation("CREATE", dto.RoleId, null, dto);
                    }
                }

                return Json(new { success });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [PermissionAuthorize(4)] // 使用與編輯相同的權限，或者設定更嚴格的權限
        public JsonResult DeleteRole(int roleId)
        {
            try
            {
                // 參數驗證
                if (roleId <= 0)
                {
                    return Json(new { success = false, message = "無效的角色ID" });
                }

                // 先取得要刪除的角色資料
                var roleToDelete = _permission.GetRole(roleId);
                if (roleToDelete == null)
                {
                    return Json(new { success = false, message = "找不到指定的角色" });
                }

                // 檢查是否可以刪除
                var (canDelete, reason) = _permission.CanDeleteRole(roleId);
                if (!canDelete)
                {
                    return Json(new { success = false, message = reason });
                }

                // 取得當前操作者資訊
                var currentUser = User as CustomPrincipal;
                string operatorUserNo = currentUser?.UserNo ?? "system";

                // 執行刪除（這裡提供兩種選擇）
                bool result;

                // 選擇1：硬刪除（完全移除）
                result = _permission.DeleteRole(roleId, operatorUserNo);

                // 選擇2：軟刪除（只是設為非啟用，保留歷史記錄）
                // result = _permission.SoftDeleteRole(roleId, operatorUserNo);

                if (result)
                {
                    // 記錄刪除 Log
                    LogHelper.LogRoleOperation("DELETE", roleId, roleToDelete, null);
                }

                return Json(new
                {
                    success = result,
                    message = result ? "角色已成功刪除" : "刪除失敗，請稍後再試"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"刪除時發生錯誤：{ex.Message}" });
            }
        }


        [HttpGet]
        public JsonResult CheckCanDeleteRole(int roleId)
        {
            try
            {
                var (canDelete, reason) = _permission.CanDeleteRole(roleId);
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
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////
        ///
        ///
        ///

        [HttpGet]
        public JsonResult GetAllEmployees()
        {
            // 取得 Employee 資料，範例只回傳 EmployeeId, EmployeeNo, EmployeeName
            List<User> employeeList = _permission.GetAllEmployees();

            return Json(new
            {
                success = true,
                data = employeeList          // EasyUI 會去讀 rows，把它當作要在表格顯示的資料
            }, JsonRequestBehavior.AllowGet);
        }
                
        
        [HttpGet]
        public JsonResult GetActiveRoles()
        {
            List<Role> RoleList = _permission.GetActiveRoles();

            return Json(new
            {
                success = true,
                count = RoleList.Count,  
                data = RoleList         
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [PermissionAuthorize(3)]
        public JsonResult SaveUser(User user)
        {
            // 1. 檢查必填
            if (string.IsNullOrEmpty(user.UserName))
            {
                return Json(new { success = false, message = "請先選擇使用者。" });
            }
            if (string.IsNullOrEmpty(user.RoleId))
            {
                return Json(new { success = false, message = "請先選擇角色。" });
            }

            try
            {
                // 2. 先取得當前登入者的 UserNo（即要寫進 CreatedBy 或 UpdatedBy）

                var current = HttpContext.User as CustomPrincipal;
                string operatorUserNo = current?.UserNo;

                // 3. 嘗試從資料庫取得這個 UserNo 對應的使用者
                var oldUser = _permission.GetUser(user.UserNo);
                bool exists = _permission.ExistsUserByUserNo(user.UserNo);

                if (exists)
                {
                    var updateUser = new User
                    {
                        UserNo = user.UserNo,
                        UserName = user.UserName,
                        DepartmentName = user.DepartmentName,
                        RoleId = user.RoleId,
                        Email = user.Email,
                        IsActive = user.IsActive,
                        ReciveAlarmFlag = user.ReciveAlarmFlag,
                        UpdateDate = DateTime.Now,
                        UpdateBy = operatorUserNo
                    };

                    _permission.UpdateUser(updateUser);

                    LogHelper.LogUserOperation("UPDATE", user.UserNo, oldUser, updateUser);

                    return Json(new { success = true, message = "已成功更新使用者。" });
                }
                else
                {
                    // 5. 如果不存在，就做「新增」動作
                    var newUser = new User
                    {
                        UserNo = user.UserNo,
                        UserName = user.UserName,
                        DepartmentName = user.DepartmentName,
                        Email = user.Email,
                        RoleId = user.RoleId,
                        IsActive = user.IsActive,
                        ReciveAlarmFlag = user.ReciveAlarmFlag,
                        CreateDate = DateTime.Now,  
                        CreateBy = operatorUserNo   
                    };
                    bool result =  _permission.AddNewUser(newUser);

                    LogHelper.LogUserOperation("CREATE", user.UserNo, null, newUser);

                    return Json(new { success = true, message = "已成功新增使用者。" });
                }
            }
            catch (Exception ex)
            {
                // 6. 捕捉例外並回傳錯誤訊息
                return Json(new { success = false, message = ex.Message });
            }
        }



        /// <summary>
        /// 不再回傳使用者列表給 Datagrid，因為一開始要讓 Datagrid 保持空白
        /// </summary>
        [HttpGet]
        public JsonResult GetUsers()
        {
            var userList = _permission.GetUsers();
            return Json(new { success = true, total = 0, rows = userList }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetUser(string userNo)
        {
            var user = _permission.GetUser(userNo);
            return Json(new { success = true,  data = user }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [PermissionAuthorize(3)] // 刪除權限應該更嚴格
        public JsonResult DeleteUser(string userNo)
        {
            try
            {
                // 改善：參數驗證
                if (string.IsNullOrWhiteSpace(userNo))
                {
                    return Json(new { success = false, message = "使用者工號不能為空" });
                }

                // 檢查使用者是否存在，同時取得要刪除的使用者資料
                var userToDelete = _permission.GetUser(userNo);
                if (userToDelete == null)
                {
                    return Json(new { success = false, message = "找不到指定的使用者" });
                }

                // 取得當前操作者資訊
                var currentUser = User as CustomPrincipal;
                string operatorUserNo = currentUser?.UserNo;

                // 防止自己刪除自己
                if (userNo == operatorUserNo)
                {
                    return Json(new { success = false, message = "不能刪除自己的帳號" });
                }

                // 執行刪除
                bool result = _permission.DeleteUser(userNo, operatorUserNo);

                if (result)
                {
                    // 記錄刪除 Log
                    LogHelper.LogUserOperation("DELETE", userNo, userToDelete, null);
                }

                return Json(new
                {
                    success = result,
                    message = result ? "使用者已成功刪除" : "刪除失敗，請稍後再試"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"刪除時發生錯誤：{ex.Message}" });
            }
        }

    }
}
