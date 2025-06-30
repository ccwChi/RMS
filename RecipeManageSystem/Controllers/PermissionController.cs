using System.Web.Mvc;
using RecipeManageSystem.Models;
using RecipeManageSystem.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using System;
using RecipeManageSystem.Generic;

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
        [PermissionAuthorize(5)]
        public JsonResult SaveRole(Role dto)
        {
            bool success;
            if (dto.RoleId == 0)
                success = _permission.InsertRole(dto);
            else
                success = _permission.UpdateRole(dto);

            return Json(new { success });
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
                count = RoleList.Count,  // EasyUI 會去讀 total，知道總共有幾筆
                data = RoleList          // EasyUI 會去讀 rows，把它當作要在表格顯示的資料
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [PermissionAuthorize(6)]
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
                        UpdateDate = DateTime.Now,
                        UpdateBy = operatorUserNo
                    };

                    _permission.UpdateUser(updateUser);

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
                        CreateDate = DateTime.Now,  
                        CreateBy = operatorUserNo   
                    };
                    bool result =  _permission.AddNewUser(newUser);

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

    }
}
