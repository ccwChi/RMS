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

        // 採用與 BaseRepository 相同的配置模式
        private readonly string _rmsConnectionString;
        private readonly string _envFlag;

        public CustomPrincipal(IPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException("principal");
            _systemPrincipal = principal;

            // 讀取環境設定
            _envFlag = ConfigurationManager.AppSettings["EnvFlag"];

            // 根據環境變數選擇對應的連線字串
            _rmsConnectionString = ConfigurationManager.ConnectionStrings[
                (_envFlag == "1") ? "RMSConnection" : "RMS_DEVConnection"
            ].ConnectionString;
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
            => PermissionIds.Contains(permissionId);

        /// <summary>
        /// 取得目前使用的連線字串（除錯用）
        /// </summary>
        public string GetConnectionInfo()
        {
            return $"Environment: {(_envFlag == "1" ? "Production" : "Development")}, " +
                   $"Connection: {(_envFlag == "1" ? "RMSConnection" : "RMS_DEVConnection")}";
        }
    }
}