using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using RecipeManageSystem.Models;
using System.Configuration;
using RecipeManageSystem.Repository;
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
                return conn.Query<Role>("SELECT RoleId, RoleName, Description, Permissions, IsActive FROM RMS.dbo.Roles order by RoleId").ToList();
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


        /// <summary>
        /// 刪除角色
        /// </summary>
        /// <param name="roleId">角色 ID</param>
        /// <param name="deletedBy">刪除者</param>
        /// <returns>是否成功</returns>
        public bool DeleteRole(int roleId, string deletedBy)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. 檢查是否有使用者正在使用此角色
                        const string checkUserSql = @"
                            SELECT COUNT(*) 
                            FROM RMS.dbo.Users 
                            WHERE RoleId = @RoleId AND IsActive = 1";

                        var userCount = conn.QuerySingle<int>(checkUserSql, new { RoleId = roleId }, transaction);

                        if (userCount > 0)
                        {
                            // 有使用者正在使用此角色，不允許刪除
                            transaction.Rollback();
                            return false;
                        }

                        // 2. 檢查是否有警報群組使用此角色
                        const string checkAlertSql = @"
                            SELECT COUNT(*) 
                            FROM RMS.dbo.AlertGroup
                            WHERE RoleId = @RoleId";

                        var alertCount = conn.QuerySingle<int>(checkAlertSql, new { RoleId = roleId }, transaction);

                        if (alertCount > 0)
                        {
                            // 先刪除警報群組中的角色關聯
                            conn.Execute("DELETE FROM RMS.dbo.AlertGroupRole WHERE RoleId = @RoleId",
                                       new { RoleId = roleId }, transaction);
                        }

                        // 3. 刪除角色
                        const string deleteRoleSql = @"
                            DELETE FROM RMS.dbo.Roles 
                            WHERE RoleId = @RoleId";

                        var affectedRows = conn.Execute(deleteRoleSql, new { RoleId = roleId }, transaction);

                        transaction.Commit();
                        return affectedRows > 0;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        // 可以記錄錯誤 Log
                        System.Diagnostics.Debug.WriteLine($"刪除角色失敗: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// 檢查角色是否可以被刪除
        /// </summary>
        /// <param name="roleId">角色 ID</param>
        /// <returns>檢查結果</returns>
        public (bool canDelete, string reason) CanDeleteRole(int roleId)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                // 檢查是否有使用者正在使用此角色
                const string checkUserSql = @"
                    SELECT COUNT(*) 
                    FROM RMS.dbo.Users 
                    WHERE RoleId = @RoleId AND IsActive = 1";

                var userCount = conn.QuerySingle<int>(checkUserSql, new { RoleId = roleId });

                if (userCount > 0)
                {
                    return (false, $"此角色目前有 {userCount} 個使用者正在使用，無法刪除");
                }

                // 檢查是否有警報群組使用此角色
                const string checkAlertSql = @"
                    SELECT COUNT(*) 
                    FROM RMS.dbo.AlertGroup
                    WHERE RoleId = @RoleId";

                var alertCount = conn.QuerySingle<int>(checkAlertSql, new { RoleId = roleId });

                if (alertCount > 0)
                {
                    return (true, $"此角色關聯到 {alertCount} 個警報群組，刪除時會一併移除這些關聯");
                }

                return (true, "可以安全刪除");
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
                return conn.Query<User>("SELECT UserNo, UserName, DepartmentName,Email, RoleId, IsActive, ReciveAlarmFlag , CreateDate FROM RMS.dbo.Users order by UserNo").ToList();
            }
        }

        public List<Role> GetActiveRoles()
        {
            using (var conn = new SqlConnection(rmsString))
            {
                return conn.Query<Role>("SELECT RoleId, RoleName, Description, Permissions, IsActive FROM RMS.dbo.Roles order by RoleId").ToList();
            }
        }

        public User GetUser(string userNo)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                return conn.QueryFirstOrDefault<User>("SELECT UserNo, UserName, DepartmentName, RoleId, IsActive, ReciveAlarmFlag FROM RMS.dbo.Users WHERE UserNo = @userNo", new { userNo });
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
                return conn.QueryFirstOrDefault<User>("SELECT UserNo, UserName, DepartmentName,Email, RoleId, IsActive, ReciveAlarmFlag FROM RMS.dbo.Users WHERE UserNo = @userNo", new { userNo });
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
            INSERT INTO RMS.dbo.Users (UserNo, UserName, DepartmentName,Email, RoleId, IsActive, ReciveAlarmFlag, CreateDate, CreateBy)
                        VALUES (@UserNo, @UserName, @DepartmentName,@Email, @RoleId, @IsActive,@ReciveAlarmFlag, @CreateDate, @CreateBy);";

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
                        ReciveAlarmFlag = @ReciveAlarmFlag,
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


        public bool DeleteUser(string userNo, string operatorUserNo)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. 記錄刪除日誌（可選）
                        const string logSql = @"
                    INSERT INTO RMS.dbo.UserOperationLog (OperationType, UserNo, OperatorUserNo, OperationDate, Remark)
                    VALUES ('DELETE', @userNo, @operatorUserNo, GETDATE(), '刪除使用者')";

                        // 如果沒有日誌表，可以註解掉這段
                        /*
                        conn.Execute(logSql, new 
                        { 
                            userNo = userNo,
                            operatorUserNo = operatorUserNo
                        }, tran);
                        */

                        // 2. 刪除使用者記錄
                        const string deleteSql = @"DELETE FROM RMS.dbo.Users WHERE UserNo = @userNo";

                        int rowsAffected = conn.Execute(deleteSql, new { userNo }, tran);

                        if (rowsAffected > 0)
                        {
                            tran.Commit();
                            return true;
                        }
                        else
                        {
                            tran.Rollback();
                            return false;
                        }
                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                        return false;
                    }
                }
            }
        }

    }
}
