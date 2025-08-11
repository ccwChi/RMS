using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Principal;
using System.Web;
using Dapper;

namespace RecipeManageSystem.Generic
{
    public class CustomPrincipal : IPrincipal
    {
        private readonly IPrincipal _systemPrincipal;
        private readonly string _rmsConnectionString;
        private readonly string _envFlag;

        public CustomPrincipal(IPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException("principal");
            _systemPrincipal = principal;

            try
            {
                // 讀取環境設定，預設為開發環境
                _envFlag = ConfigurationManager.AppSettings["EnvFlag"] ?? "0";

                // 根據環境變數選擇對應的連線字串
                var connectionName = (_envFlag == "1") ? "RMSConnection" : "RMS_DEVConnection";
                var connectionConfig = ConfigurationManager.ConnectionStrings[connectionName];

                // 如果找不到指定的連線字串，嘗試其他選項
                if (connectionConfig == null)
                {
                    connectionConfig = ConfigurationManager.ConnectionStrings["RMSConnection"] ??
                                     ConfigurationManager.ConnectionStrings["RMS_DEVConnection"];
                }

                if (connectionConfig == null)
                {
                    // 記錄錯誤但不拋出異常，讓系統可以繼續運行
                    System.Diagnostics.Debug.WriteLine($"警告：找不到 RMS 連線字串設定 ({connectionName})");
                    _rmsConnectionString = string.Empty;
                }
                else
                {
                    _rmsConnectionString = connectionConfig.ConnectionString;
                    if (string.IsNullOrEmpty(_rmsConnectionString))
                    {
                        System.Diagnostics.Debug.WriteLine($"警告：RMS 連線字串為空 ({connectionName})");
                    }
                }
            }
            catch (Exception ex)
            {
                // 記錄錯誤但不讓整個系統崩潰
                System.Diagnostics.Debug.WriteLine($"CustomPrincipal 初始化錯誤: {ex.Message}");
                _envFlag = "0";
                _rmsConnectionString = string.Empty;
            }
        }

        public IIdentity Identity
        {
            get { return _systemPrincipal.Identity; }
        }

        public bool IsInRole(string role)
        {
            return _systemPrincipal.IsInRole(role);
        }

        public string UserNo { get; set; }
        public string UserName { get; set; }
        public string DeptNo { get; set; }
        public string DeptName { get; set; }
        public string Title { get; set; }
        public int RoleId { get; private set; }
        public List<int> PermissionIds { get; private set; } = new List<int>();

        /// <summary>
        /// 連到 RMS DB，一次拿到 RoleId + 在 Roles.Permissions 欄位的字串 (e.g. "1,3,5")
        /// 再拆成 int list
        /// </summary>
        //public void InitializePermissions()
        //{
        //    if (string.IsNullOrEmpty(UserNo) || string.IsNullOrEmpty(_rmsConnectionString))
        //    {
        //        System.Diagnostics.Debug.WriteLine($"跳過權限初始化 - UserNo: {UserNo}, 連線字串: {(!string.IsNullOrEmpty(_rmsConnectionString) ? "有" : "無")}");
        //        return;
        //    }

        //    try
        //    {
        //        using (var conn = new SqlConnection(_rmsConnectionString))
        //        {
        //            // 嘗試不同的 SQL 查詢（考慮 Schema 差異）
        //            var sqlQueries = new[]
        //            {
        //                // 有 RMS schema
        //                @"SELECT u.RoleId, r.Permissions
        //                  FROM RMS.dbo.Users u
        //                  JOIN RMS.dbo.Roles r ON u.RoleId = r.RoleId
        //                  WHERE u.UserNo = @userNo",

        //                // 有 dbo schema
        //                @"SELECT u.RoleId, r.Permissions
        //                  FROM dbo.Users u
        //                  JOIN dbo.Roles r ON u.RoleId = r.RoleId
        //                  WHERE u.UserNo = @userNo",

        //                // 沒有 schema
        //                @"SELECT u.RoleId, r.Permissions
        //                  FROM Users u
        //                  JOIN Roles r ON u.RoleId = r.RoleId
        //                  WHERE u.UserNo = @userNo"
        //            };

        //            object row = null;
        //            string usedSql = "";

        //            foreach (var sql in sqlQueries)
        //            {
        //                try
        //                {
        //                    row = conn.QuerySingleOrDefault(sql, new { userNo = UserNo });
        //                    if (row != null)
        //                    {
        //                        usedSql = sql;
        //                        break;
        //                    }
        //                }
        //                catch (SqlException sqlEx)
        //                {
        //                    // 如果是物件名稱無效，嘗試下一個查詢
        //                    if (sqlEx.Number == 208) // Invalid object name
        //                    {
        //                        continue;
        //                    }
        //                    throw; // 其他 SQL 錯誤就拋出
        //                }
        //            }

        //            if (row != null)
        //            {
        //                var roleIdValue = row.GetType().GetProperty("RoleId")?.GetValue(row);
        //                var permissionsValue = row.GetType().GetProperty("Permissions")?.GetValue(row);

        //                if (roleIdValue != null)
        //                {
        //                    RoleId = Convert.ToInt32(roleIdValue);
        //                }

        //                var raw = (permissionsValue as string) ?? "";
        //                PermissionIds = raw
        //                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
        //                    .Where(s => int.TryParse(s.Trim(), out _))
        //                    .Select(s => int.Parse(s.Trim()))
        //                    .ToList();

        //                System.Diagnostics.Debug.WriteLine($"成功載入權限 - 使用者: {UserNo}, 角色: {RoleId}, 權限數: {PermissionIds.Count}");
        //                System.Diagnostics.Debug.WriteLine($"使用的 SQL: {usedSql.Substring(0, Math.Min(50, usedSql.Length))}...");
        //            }
        //            else
        //            {
        //                System.Diagnostics.Debug.WriteLine($"找不到使用者權限資料: {UserNo}");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // 記錄錯誤但不拋出異常
        //        System.Diagnostics.Debug.WriteLine($"InitializePermissions 錯誤 (使用者: {UserNo}): {ex.Message}");
        //        RoleId = 0;
        //        PermissionIds = new List<int>();
        //    }
        //}
        public void InitializePermissions()
        {
            if (string.IsNullOrEmpty(UserNo))
                return;

            using (var conn = new SqlConnection(_rmsConnectionString))
            {
                const string sql = @"
                    SELECT u.RoleId, r.Permissions
                    FROM dbo.Users u
                    JOIN dbo.Roles r ON u.RoleId = r.RoleId
                    WHERE u.UserNo = @userNo";

                var row = conn.QuerySingleOrDefault(sql, new { userNo = UserNo });
                if (row != null)
                {
                    RoleId = (int)row.RoleId;
                    var raw = (row.Permissions as string) ?? "";
                    PermissionIds = raw
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();
                }
            }
        }



        /// <summary>
        /// 檢查是否有某個 PermissionId
        /// </summary>
        public bool HasPermission(int permissionId)
            => PermissionIds?.Contains(permissionId) ?? false;

        /// <summary>
        /// 取得目前使用的連線字串（除錯用）
        /// </summary>
        public string GetConnectionInfo()
        {
            return $"Environment: {(_envFlag == "1" ? "Production" : "Development")}, " +
                   $"Connection: {(_envFlag == "1" ? "RMSConnection" : "RMS_DEVConnection")}, " +
                   $"HasConnectionString: {(!string.IsNullOrEmpty(_rmsConnectionString) ? "是" : "否")}, " +
                   $"UserNo: {UserNo ?? "未設定"}";
        }

        /// <summary>
        /// 檢查系統是否正確初始化
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(_rmsConnectionString) && !string.IsNullOrEmpty(UserNo);
        }
    }
}