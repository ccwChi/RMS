using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Dapper;
using RecipeManageSystem.Repository;
using RecipeManageSystem.Models;
using Newtonsoft.Json;

namespace RecipeManageSystem.Repository
{
    public class LogRepository : BaseRepository
    {
        /// <summary>
        /// 寫入操作記錄
        /// </summary>
        public long CreateLog(CreateLogDto dto, string operateBy, string ipAddress = null, string userAgent = null)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                const string sql = @"
                    INSERT INTO RMS.dbo.OperationLogs 
                    (TableName, EntityId, Operation, OperateBy, Module, Description, 
                     OldData, NewData, ChangedFields, IpAddress, UserAgent)
                    VALUES 
                    (@TableName, @EntityId, @Operation, @OperateBy, @Module, @Description,
                     @OldData, @NewData, @ChangedFields, @IpAddress, @UserAgent);
                    SELECT CAST(SCOPE_IDENTITY() AS bigint);";

                var logId = conn.QuerySingle<long>(sql, new
                {
                    dto.TableName,
                    dto.EntityId,
                    dto.Operation,
                    OperateBy = operateBy,
                    dto.Module,
                    dto.Description,
                    OldData = dto.OldData != null ? JsonConvert.SerializeObject(dto.OldData) : null,
                    NewData = dto.NewData != null ? JsonConvert.SerializeObject(dto.NewData) : null,
                    ChangedFields = dto.ChangedFields?.Any() == true ? string.Join(",", dto.ChangedFields) : null,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                });

                return logId;
            }
        }

        /// <summary>
        /// 取得操作記錄列表 (分頁)
        /// </summary>
        public (List<LogListDto> logs, int total) GetLogs(LogQueryDto query)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                var whereConditions = new List<string>();
                var parameters = new DynamicParameters();

                // 建立查詢條件
                if (query.StartDate.HasValue)
                {
                    whereConditions.Add("ol.OperateDate >= @StartDate");
                    parameters.Add("StartDate", query.StartDate.Value);
                }

                if (query.EndDate.HasValue)
                {
                    whereConditions.Add("ol.OperateDate <= @EndDate");
                    parameters.Add("EndDate", query.EndDate.Value.AddDays(1).AddSeconds(-1));
                }

                if (!string.IsNullOrEmpty(query.UserNo))
                {
                    whereConditions.Add("ol.OperateBy LIKE @UserNo");
                    parameters.Add("UserNo", $"%{query.UserNo}%");
                }

                if (!string.IsNullOrEmpty(query.Module))
                {
                    whereConditions.Add("ol.Module = @Module");
                    parameters.Add("Module", query.Module);
                }

                if (!string.IsNullOrEmpty(query.Operation))
                {
                    whereConditions.Add("ol.Operation = @Operation");
                    parameters.Add("Operation", query.Operation);
                }

                if (!string.IsNullOrEmpty(query.TableName))
                {
                    whereConditions.Add("ol.TableName = @TableName");
                    parameters.Add("TableName", query.TableName);
                }

                if (!string.IsNullOrEmpty(query.EntityId))
                {
                    whereConditions.Add("ol.EntityId LIKE @EntityId");
                    parameters.Add("EntityId", $"%{query.EntityId}%");
                }

                var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";

                // 計算總筆數
                var countSql = $@"
                    SELECT COUNT(*)
                    FROM RMS.dbo.OperationLogs ol
                    {whereClause}";

                var total = conn.QuerySingle<int>(countSql, parameters);

                // 查詢資料 (分頁)
                var offset = (query.Page - 1) * query.PageSize;
                parameters.Add("Offset", offset);
                parameters.Add("PageSize", query.PageSize);

                var dataSql = $@"
                    SELECT 
                        ol.LogId,
                        ol.TableName,
                        ol.EntityId,
                        ol.Operation,
                        ol.OperateBy,
                        ISNULL(u.UserName, ol.OperateBy) as OperatorName,
                        ol.OperateDate,
                        ol.Module,
                        ol.Description,
                        ol.ChangedFields,
                        ol.IpAddress
                    FROM RMS.dbo.OperationLogs ol
                    LEFT JOIN RMS.dbo.Users u ON ol.OperateBy = u.UserNo
                    {whereClause}
                    ORDER BY ol.OperateDate DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                var logs = conn.Query<LogListDto>(dataSql, parameters).ToList();

                return (logs, total);
            }
        }

        /// <summary>
        /// 取得單筆操作記錄詳細資訊
        /// </summary>
        public LogDetailDto GetLogDetail(long logId)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                const string sql = @"
                    SELECT 
                        ol.*,
                        u.UserName as OperatorName,
                        u.DepartmentName
                    FROM RMS.dbo.OperationLogs ol
                    LEFT JOIN RMS.dbo.Users u ON ol.OperateBy = u.UserNo
                    WHERE ol.LogId = @LogId";

                var log = conn.QueryFirstOrDefault<OperationLog>(sql, new { LogId = logId });
                if (log == null) return null;

                var result = new LogDetailDto
                {
                    Log = log,
                    OperatorName = conn.QueryFirstOrDefault<string>(
                        "SELECT UserName FROM RMS.dbo.Users WHERE UserNo = @UserNo",
                        new { UserNo = log.OperateBy }) ?? log.OperateBy,
                    DepartmentName = conn.QueryFirstOrDefault<string>(
                        "SELECT DepartmentName FROM RMS.dbo.Users WHERE UserNo = @UserNo",
                        new { UserNo = log.OperateBy })
                };

                // 解析 JSON 資料
                try
                {
                    result.OldDataObj = !string.IsNullOrEmpty(log.OldData)
                        ? JsonConvert.DeserializeObject<Dictionary<string, object>>(log.OldData)
                        : new Dictionary<string, object>();

                    result.NewDataObj = !string.IsNullOrEmpty(log.NewData)
                        ? JsonConvert.DeserializeObject<Dictionary<string, object>>(log.NewData)
                        : new Dictionary<string, object>();

                    // 產生欄位異動明細
                    result.FieldChanges = GenerateFieldChanges(log.TableName, result.OldDataObj, result.NewDataObj);
                }
                catch (Exception ex)
                {
                    // JSON 解析失敗時的處理
                    result.OldDataObj = new Dictionary<string, object>();
                    result.NewDataObj = new Dictionary<string, object>();
                    result.FieldChanges = new List<FieldChangeInfo>();
                }

                return result;
            }
        }

        /// <summary>
        /// 產生欄位異動明細
        /// </summary>
        private List<FieldChangeInfo> GenerateFieldChanges(string tableName,
            Dictionary<string, object> oldData, Dictionary<string, object> newData)
        {
            var changes = new List<FieldChangeInfo>();
            var fieldNames = new HashSet<string>();

            if (oldData != null) fieldNames.UnionWith(oldData.Keys);
            if (newData != null) fieldNames.UnionWith(newData.Keys);

            // 取得欄位中文對照表
            var displayNames = GetFieldDisplayNames(tableName);

            foreach (var fieldName in fieldNames)
            {
                var oldValue = oldData?.ContainsKey(fieldName) == true ? oldData[fieldName]?.ToString() : "";
                var newValue = newData?.ContainsKey(fieldName) == true ? newData[fieldName]?.ToString() : "";

                changes.Add(new FieldChangeInfo
                {
                    FieldName = fieldName,
                    FieldDisplayName = displayNames.ContainsKey(fieldName) ? displayNames[fieldName] : fieldName,
                    OldValue = oldValue ?? "",
                    NewValue = newValue ?? ""
                });
            }

            return changes.Where(c => c.HasChanged).ToList();
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
                default:
                    return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// 取得特定實體的操作記錄
        /// </summary>
        public List<LogListDto> GetEntityLogs(string tableName, string entityId)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                const string sql = @"
                    SELECT 
                        ol.LogId,
                        ol.TableName,
                        ol.EntityId,
                        ol.Operation,
                        ol.OperateBy,
                        ISNULL(u.UserName, ol.OperateBy) as OperatorName,
                        ol.OperateDate,
                        ol.Module,
                        ol.Description,
                        ol.ChangedFields,
                        ol.IpAddress
                    FROM RMS.dbo.OperationLogs ol
                    LEFT JOIN RMS.dbo.Users u ON ol.OperateBy = u.UserNo
                    WHERE ol.TableName = @TableName AND ol.EntityId = @EntityId
                    ORDER BY ol.OperateDate DESC";

                return conn.Query<LogListDto>(sql, new { TableName = tableName, EntityId = entityId }).ToList();
            }
        }

        /// <summary>
        /// 刪除過期的 Log 記錄 (例如保留 3 個月)
        /// </summary>
        public int CleanupOldLogs(int keepMonths = 3)
        {
            using (var conn = new SqlConnection(rmsString))
            {
                var cutoffDate = DateTime.Now.AddMonths(-keepMonths);
                const string sql = "DELETE FROM RMS.dbo.OperationLogs WHERE OperateDate < @CutoffDate";
                return conn.Execute(sql, new { CutoffDate = cutoffDate });
            }
        }


    }
}