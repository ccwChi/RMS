using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RecipeManageSystem.Models;
using RecipeManageSystem.Repository;
using RecipeManageSystem.Generic;
using System.Reflection;

namespace RecipeManageSystem.Services
{
    public class LogService
    {
        private readonly LogRepository _logRepository;

        public LogService()
        {
            _logRepository = new LogRepository();
        }

        /// <summary>
        /// 記錄新增操作
        /// </summary>
        public long LogCreate(string tableName, string entityId, string module, object newData, string description = null)
        {
            var userNo = GetCurrentUserNo();

            // 對於新增操作，只記錄關鍵欄位的新值
            var simplifiedNewData = ExtractKeyFields(newData, tableName);
            var summary = GenerateCreateSummary(tableName, simplifiedNewData);

            var dto = new CreateLogDto
            {
                TableName = tableName,
                EntityId = entityId,
                Operation = LogOperations.CREATE,
                Module = module,
                Description = description ?? summary,
                NewData = simplifiedNewData,
                ChangedFields = simplifiedNewData?.Keys.ToList() ?? new List<string>()
            };

            return _logRepository.CreateLog(dto, userNo, GetClientIP(), GetUserAgent());
        }

        /// <summary>
        /// 記錄更新操作 - 只記錄有異動的欄位
        /// </summary>
        public long LogUpdate(string tableName, string entityId, string module, object oldData, object newData, string description = null)
        {
            var changedFields = GetChangedFieldsWithValues(oldData, newData);
            if (!changedFields.Any()) return 0; // 沒有異動就不記錄

            var userNo = GetCurrentUserNo();
            var summary = GenerateUpdateSummary(tableName, changedFields);

            var dto = new CreateLogDto
            {
                TableName = tableName,
                EntityId = entityId,
                Operation = LogOperations.UPDATE,
                Module = module,
                Description = description ?? summary,
                OldData = changedFields.ToDictionary(kv => kv.Key, kv => kv.Value.OldValue),
                NewData = changedFields.ToDictionary(kv => kv.Key, kv => kv.Value.NewValue),
                ChangedFields = changedFields.Keys.ToList()
            };

            return _logRepository.CreateLog(dto, userNo, GetClientIP(), GetUserAgent());
        }

        /// <summary>
        /// 記錄刪除操作
        /// </summary>
        public long LogDelete(string tableName, string entityId, string module, object oldData, string description = null)
        {
            var userNo = GetCurrentUserNo();

            // 對於刪除操作，只記錄關鍵識別欄位
            var simplifiedOldData = ExtractKeyFields(oldData, tableName);
            var summary = GenerateDeleteSummary(tableName, simplifiedOldData);

            var dto = new CreateLogDto
            {
                TableName = tableName,
                EntityId = entityId,
                Operation = LogOperations.DELETE,
                Module = module,
                Description = description ?? summary,
                OldData = simplifiedOldData,
                ChangedFields = simplifiedOldData?.Keys.ToList() ?? new List<string>()
            };

            return _logRepository.CreateLog(dto, userNo, GetClientIP(), GetUserAgent());
        }

        /// <summary>
        /// 取得操作記錄列表
        /// </summary>
        public (List<LogListDto> logs, int total) GetLogs(LogQueryDto query)
        {
            return _logRepository.GetLogs(query);
        }

        /// <summary>
        /// 取得操作記錄詳細資訊
        /// </summary>
        public LogDetailDto GetLogDetail(long logId)
        {
            return _logRepository.GetLogDetail(logId);
        }

        /// <summary>
        /// 取得特定實體的操作記錄
        /// </summary>
        public List<LogListDto> GetEntityLogs(string tableName, string entityId)
        {
            return _logRepository.GetEntityLogs(tableName, entityId);
        }

        #region 私有方法

        /// <summary>
        /// 根據資料表類型提取關鍵欄位（用於新增和刪除操作）
        /// </summary>
        private Dictionary<string, object> ExtractKeyFields(object data, string tableName)
        {
            if (data == null) return new Dictionary<string, object>();

            var keyFields = GetKeyFieldsForTable(tableName);
            var result = new Dictionary<string, object>();
            var properties = data.GetType().GetProperties();

            foreach (var prop in properties)
            {
                if (keyFields.Contains(prop.Name))
                {
                    var value = prop.GetValue(data);
                    if (value != null)
                    {
                        result[prop.Name] = value;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 定義各資料表的關鍵欄位
        /// </summary>
        private HashSet<string> GetKeyFieldsForTable(string tableName)
        {
            switch (tableName?.ToLower())
            {
                case "users":
                    return new HashSet<string> { "UserNo", "UserName", "DepartmentName", "RoleId", "IsActive" };
                case "roles":
                    return new HashSet<string> { "RoleName", "Description", "Permissions", "IsActive" };
                case "parameter":
                    return new HashSet<string> { "ParamName", "Unit", "SectionCode", "IsActive" };
                case "recipeheader":
                    return new HashSet<string> { "ProdNo", "DeviceId", "MoldNo", "MaterialNo", "Version", "IsActive", "Remark" };
                case "machinegroup":
                    return new HashSet<string> { "GroupName", "Description" };
                case "alertgroup":
                    return new HashSet<string> { "GroupName", "Description", "IsActive" };
                default:
                    // 預設情況下，提取所有非 null 的字串和數值欄位
                    return new HashSet<string>();
            }
        }

        /// <summary>
        /// 比較兩個物件，取得有異動的欄位及其新舊值
        /// </summary>
        private Dictionary<string, (object OldValue, object NewValue)> GetChangedFieldsWithValues(object oldData, object newData)
        {
            var changedFields = new Dictionary<string, (object OldValue, object NewValue)>();

            if (oldData == null || newData == null) return changedFields;

            try
            {
                var oldProperties = oldData.GetType().GetProperties().ToDictionary(p => p.Name, p => p);
                var newProperties = newData.GetType().GetProperties().ToDictionary(p => p.Name, p => p);
                var commonPropertyNames = oldProperties.Keys.Intersect(newProperties.Keys);

                foreach (var propName in commonPropertyNames)
                {
                    try
                    {
                        var oldValue = oldProperties[propName].GetValue(oldData);
                        var newValue = newProperties[propName].GetValue(newData);

                        // 比較值是否不同
                        if (!object.Equals(oldValue, newValue))
                        {
                            changedFields[propName] = (oldValue, newValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"比較屬性 {propName} 時發生錯誤: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"比較物件時發生錯誤: {ex.Message}");
            }

            return changedFields;
        }

        /// <summary>
        /// 產生新增操作的摘要
        /// </summary>
        private string GenerateCreateSummary(string tableName, Dictionary<string, object> newData)
        {
            if (newData == null || !newData.Any()) return "新增記錄";

            switch (tableName?.ToLower())
            {
                case "users":
                    return $"新增使用者：{newData.GetValueOrDefault("UserName", "")} ({newData.GetValueOrDefault("UserNo", "")})";
                case "roles":
                    return $"新增角色：{newData.GetValueOrDefault("RoleName", "")}";
                case "parameter":
                    return $"新增參數：{newData.GetValueOrDefault("ParamName", "")}";
                case "recipeheader":
                    return $"新增配方：{newData.GetValueOrDefault("DeviceId", "")}-{newData.GetValueOrDefault("ProdNo", "")} v{newData.GetValueOrDefault("Version", "")}";
                default:
                    return $"新增{tableName}記錄";
            }
        }

        /// <summary>
        /// 產生更新操作的摘要
        /// </summary>
        private string GenerateUpdateSummary(string tableName, Dictionary<string, (object OldValue, object NewValue)> changedFields)
        {
            if (!changedFields.Any()) return "無異動";

            var fieldDisplayNames = GetFieldDisplayNames(tableName);
            var changes = changedFields.Take(3).Select(kv =>
            {
                var fieldName = fieldDisplayNames.ContainsKey(kv.Key) ? fieldDisplayNames[kv.Key] : kv.Key;
                var oldVal = FormatValue(kv.Value.OldValue);
                var newVal = FormatValue(kv.Value.NewValue);
                return $"{fieldName}: {oldVal} → {newVal}";
            });

            var summary = string.Join("; ", changes);
            if (changedFields.Count > 3)
            {
                summary += $" 等{changedFields.Count}項異動";
            }

            return summary;
        }

        /// <summary>
        /// 產生刪除操作的摘要
        /// </summary>
        private string GenerateDeleteSummary(string tableName, Dictionary<string, object> oldData)
        {
            if (oldData == null || !oldData.Any()) return "刪除記錄";

            switch (tableName?.ToLower())
            {
                case "users":
                    return $"刪除使用者：{oldData.GetValueOrDefault("UserName", "")} ({oldData.GetValueOrDefault("UserNo", "")})";
                case "roles":
                    return $"刪除角色：{oldData.GetValueOrDefault("RoleName", "")}";
                case "parameter":
                    return $"刪除參數：{oldData.GetValueOrDefault("ParamName", "")}";
                case "recipeheader":
                    return $"刪除配方：{oldData.GetValueOrDefault("DeviceId", "")}-{oldData.GetValueOrDefault("ProdNo", "")} v{oldData.GetValueOrDefault("Version", "")}";
                default:
                    return $"刪除{tableName}記錄";
            }
        }

        /// <summary>
        /// 根據資料表名稱取得欄位中文對照表
        /// </summary>
        private Dictionary<string, string> GetFieldDisplayNames(string tableName)
        {
            switch (tableName?.ToLower())
            {
                case "users":
                    return FieldDisplayNames.UserFields;
                case "roles":
                    return FieldDisplayNames.RoleFields;
                case "parameter":
                    return FieldDisplayNames.ParameterFields;
                case "recipeheader":
                    return FieldDisplayNames.RecipeFields;
                case "machinegroup":
                    return FieldDisplayNames.MachineGroupFields;
                case "alertgroup":
                    return FieldDisplayNames.AlertGroupFields;
                default:
                    return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// 格式化值用於顯示
        /// </summary>
        private string FormatValue(object value)
        {
            if (value == null) return "空值";
            if (value is bool boolVal) return boolVal ? "是" : "否";
            if (value is DateTime dateVal) return dateVal.ToString("yyyy-MM-dd HH:mm");

            var strVal = value.ToString();
            return string.IsNullOrEmpty(strVal) ? "空值" :
                   strVal.Length > 20 ? strVal.Substring(0, 20) + "..." : strVal;
        }

        /// <summary>
        /// 取得目前登入使用者工號
        /// </summary>
        private string GetCurrentUserNo()
        {
            try
            {
                if (HttpContext.Current?.User is CustomPrincipal user)
                {
                    return user.UserNo;
                }
                return "SYSTEM";
            }
            catch
            {
                return "SYSTEM";
            }
        }

        /// <summary>
        /// 取得客戶端 IP
        /// </summary>
        private string GetClientIP()
        {
            try
            {
                var request = HttpContext.Current?.Request;
                if (request == null) return null;

                var ip = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(ip))
                    ip = request.ServerVariables["REMOTE_ADDR"];

                return ip?.Split(',')[0]?.Trim();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 取得 User Agent
        /// </summary>
        private string GetUserAgent()
        {
            try
            {
                var userAgent = HttpContext.Current?.Request?.UserAgent;
                if (!string.IsNullOrEmpty(userAgent) && userAgent.Length > 200)
                {
                    userAgent = userAgent.Substring(0, 200);
                }
                return userAgent;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }

    /// <summary>
    /// 擴充方法
    /// </summary>
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            return dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;
        }
    }

    /// <summary>
    /// Log 靜態輔助類別 - 優化版本
    /// </summary>
    public static class LogHelper
    {
        private static readonly LogService _logService = new LogService();

        /// <summary>
        /// 記錄新增操作
        /// </summary>
        public static void LogCreate(string tableName, string entityId, string module, object newData, string description = null)
        {
            try
            {
                _logService.LogCreate(tableName, entityId, module, newData, description);
            }
            catch
            {
                // Log 失敗不影響主要業務邏輯
            }
        }

        /// <summary>
        /// 記錄更新操作
        /// </summary>
        public static void LogUpdate(string tableName, string entityId, string module, object oldData, object newData, string description = null)
        {
            try
            {
                _logService.LogUpdate(tableName, entityId, module, oldData, newData, description);
            }
            catch
            {
                // Log 失敗不影響主要業務邏輯
            }
        }

        /// <summary>
        /// 記錄刪除操作
        /// </summary>
        public static void LogDelete(string tableName, string entityId, string module, object oldData, string description = null)
        {
            try
            {
                _logService.LogDelete(tableName, entityId, module, oldData, description);
            }
            catch
            {
                // Log 失敗不影響主要業務邏輯
            }
        }

        /// <summary>
        /// 快速記錄配方操作 - 優化版本
        /// </summary>
        public static void LogRecipeOperation(string operation, int recipeId, object oldData = null, object newData = null)
        {
            try
            {
                // 針對配方操作產生更簡潔的描述
                string description = null;
                if (newData is RecipeTotalDto newRecipe)
                {
                    description = $"{newRecipe.DeviceId}-{newRecipe.ProdNo}";
                    if (!string.IsNullOrEmpty(newRecipe.MoldNo))
                        description += $"-{newRecipe.MoldNo}";
                    description += $" v{newRecipe.Version}";
                }

                switch (operation.ToUpper())
                {
                    case "CREATE":
                        LogCreate(LogTables.RECIPE_HEADER, recipeId.ToString(), LogModules.RECIPE, newData, description);
                        break;
                    case "UPDATE":
                        LogUpdate(LogTables.RECIPE_HEADER, recipeId.ToString(), LogModules.RECIPE, oldData, newData, description);
                        break;
                    case "DELETE":
                        LogDelete(LogTables.RECIPE_HEADER, recipeId.ToString(), LogModules.RECIPE, oldData, description);
                        break;
                }
            }
            catch
            {
                // Log 失敗不影響主要業務邏輯
            }
        }

        // 其他操作的快速記錄方法保持不變...
        public static void LogUserOperation(string operation, string userNo, object oldData = null, object newData = null)
        {
            switch (operation.ToUpper())
            {
                case "CREATE":
                    LogCreate(LogTables.USERS, userNo, LogModules.USER, newData);
                    break;
                case "UPDATE":
                    LogUpdate(LogTables.USERS, userNo, LogModules.USER, oldData, newData);
                    break;
                case "DELETE":
                    LogDelete(LogTables.USERS, userNo, LogModules.USER, oldData);
                    break;
            }
        }

        public static void LogRoleOperation(string operation, int roleId, object oldData = null, object newData = null)
        {
            switch (operation.ToUpper())
            {
                case "CREATE":
                    LogCreate(LogTables.ROLES, roleId.ToString(), LogModules.ROLE, newData);
                    break;
                case "UPDATE":
                    LogUpdate(LogTables.ROLES, roleId.ToString(), LogModules.ROLE, oldData, newData);
                    break;
                case "DELETE":
                    LogDelete(LogTables.ROLES, roleId.ToString(), LogModules.ROLE, oldData);
                    break;
            }
        }

        public static void LogParameterOperation(string operation, int paramId, object oldData = null, object newData = null)
        {
            switch (operation.ToUpper())
            {
                case "CREATE":
                    LogCreate(LogTables.PARAMETER, paramId.ToString(), LogModules.PARAMETER, newData);
                    break;
                case "UPDATE":
                    LogUpdate(LogTables.PARAMETER, paramId.ToString(), LogModules.PARAMETER, oldData, newData);
                    break;
                case "DELETE":
                    LogDelete(LogTables.PARAMETER, paramId.ToString(), LogModules.PARAMETER, oldData);
                    break;
            }
        }
    }
}