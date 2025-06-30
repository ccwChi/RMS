using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using RecipeManageSystem.Models;
using System.Configuration;
using MeasrueVendor.Repository;
using System;

namespace RecipeManageSystem.Repository
{
    public class PermissionRepository : BaseRepository
    {
    

        public List<Permission> GetAllPermissions()
        {
            using (var conn = new SqlConnection(rmsString))
            {
                const string sql = "SELECT PermissionId, PermissionName FROM RMS.dbo.Permissions ORDER BY PermissionId";
                return conn.Query<Permission>(sql).ToList();
            }
        }

        public List<Role> GetRoles()
        {
            using (var conn = new SqlConnection(rmsString))
            {
                return conn.Query<Role>("SELECT RoleId, RoleName, Description, Permissions, IsActive FROM RMS.dbo.Roles").ToList();
            }
        }

        public Role GetRole(int id)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                return conn.QueryFirstOrDefault<Role>("SELECT RoleId, RoleName, Description, Permissions, IsActive FROM RMS.dbo.Roles WHERE RoleId = @id", new { id });
            }
        }


        public bool InsertRole(Role dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RoleName))
            {
                return false;
            }

            const string sql = @"INSERT INTO RMS.dbo.Roles (RoleName, Description, Permissions, IsActive) VALUES (@RoleName, @Description, @Permissions, @IsActive);";

            try
            {
                using (var conn = new SqlConnection(rmsString))
                {
                    conn.Open();
                    int rowsAffected = conn.Execute(sql, dto);
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public bool UpdateRole(Role dto)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    conn.Execute("UPDATE RMS.dbo.Roles SET RoleName = @RoleName, Description = @Description, Permissions = @Permissions, IsActive = @IsActive WHERE RoleId = @RoleId", dto, tran);

                    tran.Commit();
                    return true;
                }
            }
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        public List<User> GetAllEmployees()
        {
            using (var conn = new SqlConnection(mesString))
            {
                return conn.Query<User>("SELECT UserNo, UserName, Email, DepartmentName FROM dbo.MES_USERS WHERE ExpirationDate IS NULL").ToList();
            }
        }

        public List<User> GetUsers()
        {
            using (var conn = new SqlConnection(rmsString))
            {
                return conn.Query<User>("SELECT UserNo, UserName, DepartmentName,Email, RoleId, IsActive, CreateDate FROM RMS.dbo.Users").ToList();
            }
        }

        public List<Role> GetActiveRoles()
        {
            using (var conn = new SqlConnection(rmsString))
            {
                return conn.Query<Role>("SELECT RoleId, RoleName, Description, Permissions, IsActive FROM RMS.dbo.Roles WHERE IsActive = 1").ToList();
            }
        }

        public User GetUser(string userNo)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                return conn.QueryFirstOrDefault<User>("SELECT UserNo, UserName, DepartmentName, RoleId, IsActive FROM RMS.dbo.Users WHERE UserNo = @userNo", new { userNo });
            }
        }

        public bool ExistsUserByUserNo(string userNo)
        {
            const string sql = @"
                    SELECT CASE WHEN EXISTS (
                        SELECT 1 FROM RMS.dbo.Users WHERE UserNo = @userNo
                    ) THEN 1 ELSE 0 END
                ";

            try
            {
                using (var conn = new SqlConnection(rmsString))
                {
                    conn.Open();
                    // ExecuteScalar<int> 會回傳 1（存在）或 0（不存在）
                    int existsFlag = conn.ExecuteScalar<int>(sql, new { userNo });
                    return existsFlag == 1;
                }
            }
            catch
            {
                // 如果查詢發生例外，建議視為「不存在」或再拋例外都可以
                return false;
            }
        }


        public User GetUserByUserNo(string userNo)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                return conn.QueryFirstOrDefault<User>("SELECT UserNo, UserName, DepartmentName,Email, RoleId, IsActive FROM RMS.dbo.Users WHERE UserNo = @userNo", new { userNo });
            }
        }


        /// <summary>
        /// 新增一筆使用者到資料庫
        /// </summary>
        /// <param name="dto">包含 UserNo、UserName、DepartmentName、RoleId、IsActive、CreateDate、CreatedBy</param>
        /// <returns>成功回傳 true，失敗回傳 false</returns>
        public bool AddNewUser(User user)
        {
            // 基本必要欄位檢查
            if (string.IsNullOrWhiteSpace(user.UserNo) ||
                string.IsNullOrWhiteSpace(user.RoleId))
            {
                return false;
            }

            const string sql = @"
            INSERT INTO RMS.dbo.Users (UserNo, UserName, DepartmentName,Email, RoleId, IsActive, CreateDate, CreateBy)
                        VALUES (@UserNo, @UserName, @DepartmentName,@Email, @RoleId, @IsActive, @CreateDate, @CreateBy);";

            try
            {
                using (var conn = new SqlConnection(rmsString))
                {
                    conn.Open();
                    int rows = conn.Execute(sql, user);
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 更新既有使用者資料
        /// </summary>
        /// <param name="dto">包含 UserNo、UserName、DepartmentName、RoleId、IsActive、UpdateDate、UpdatedBy</param>
        /// <returns>成功回傳 true，失敗回傳 false</returns>
        public bool UpdateUser(User user)
        {
            // 基本檢查：UserNo、RoleId 不可空
            if (string.IsNullOrWhiteSpace(user.UserNo) || string.IsNullOrWhiteSpace(user.RoleId))
            {
                return false;
            }

            const string sql = @"
                    UPDATE RMS.dbo.Users
                    SET
                        UserName       = @UserName,
                        DepartmentName = @DepartmentName,
                        Email          = @Email,
                        RoleId         = @RoleId,
                        IsActive       = @IsActive,
                        UpdateDate     = @UpdateDate,
                        UpdateBy       = @UpdateBy
                    WHERE UserNo = @UserNo;";

            try
            {
                using (var conn = new SqlConnection(rmsString))
                {
                    conn.Open();
                    int rows = conn.Execute(sql, user);
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
