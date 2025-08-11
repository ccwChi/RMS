using System;
using System.Collections.Generic;

namespace RecipeManageSystem.Models
{
    // 操作記錄主表
    public class OperationLog
    {
        public long LogId { get; set; }
        public string TableName { get; set; }
        public string EntityId { get; set; }
        public string Operation { get; set; }      // CREATE, UPDATE, DELETE
        public string OperateBy { get; set; }
        public DateTime OperateDate { get; set; }
        public string Module { get; set; }
        public string Description { get; set; }
        public string OldData { get; set; }        // JSON 格式的舊資料
        public string NewData { get; set; }        // JSON 格式的新資料
        public string ChangedFields { get; set; }  // 異動欄位名稱，逗號分隔
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }

    // Log 查詢 DTO
    public class LogQueryDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string UserNo { get; set; }
        public string Module { get; set; }
        public string Operation { get; set; }
        public string TableName { get; set; }
        public string EntityId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    // Log 列表顯示 DTO
    public class LogListDto
    {
        public long LogId { get; set; }
        public string TableName { get; set; }
        public string EntityId { get; set; }
        public string Operation { get; set; }
        public string OperateBy { get; set; }
        public string OperatorName { get; set; }     
        public DateTime OperateDate { get; set; }
        public string Module { get; set; }
        public string Description { get; set; }
        public string ChangedFields { get; set; }
        public string IpAddress { get; set; }

        // 格式化顯示用
        public string OperateDateString => OperateDate.ToString("yyyy-MM-dd HH:mm:ss");
        public string ChangedFieldsDisplay => string.IsNullOrEmpty(ChangedFields) ? "-" : ChangedFields;
    }

    // Log 詳細資訊 DTO (用於查看異動前後對比)
    public class LogDetailDto
    {
        public OperationLog Log { get; set; }
        public string OperatorName { get; set; }
        public string DepartmentName { get; set; }
        public Dictionary<string, object> OldDataObj { get; set; }  // 解析後的舊資料
        public Dictionary<string, object> NewDataObj { get; set; }  // 解析後的新資料
        public List<FieldChangeInfo> FieldChanges { get; set; }    // 欄位異動明細
    }

    // 欄位異動資訊 (用於詳細頁面顯示)
    public class FieldChangeInfo
    {
        public string FieldName { get; set; }
        public string FieldDisplayName { get; set; }  // 中文顯示名稱
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public bool HasChanged => OldValue != NewValue;
    }

    // Log 建立 DTO
    public class CreateLogDto
    {
        public string TableName { get; set; }
        public string EntityId { get; set; }
        public string Operation { get; set; }
        public string Module { get; set; }
        public string Description { get; set; }
        public object OldData { get; set; }           // 會轉成 JSON 儲存
        public object NewData { get; set; }           // 會轉成 JSON 儲存
        public List<string> ChangedFields { get; set; } = new List<string>();
    }

    // 常用的操作類型常數
    public static class LogOperations
    {
        public const string CREATE = "CREATE";
        public const string UPDATE = "UPDATE";
        public const string DELETE = "DELETE";
        public const string BATCH_CREATE = "BATCH_CREATE";
        public const string BATCH_UPDATE = "BATCH_UPDATE";
        public const string BATCH_DELETE = "BATCH_DELETE";
        public const string BATCH_TOGGLE = "BATCH_TOGGLE";
    }

    // 常用的模組名稱常數
    public static class LogModules
    {
        public const string PARAMETER = "Parameter";
        public const string RECIPE = "Recipe";
        public const string PERMISSION = "Permission";
        public const string USER = "User";
        public const string ROLE = "Role";
        public const string MACHINE_PARAM = "MachineParam";
        public const string ALARM = "Alarm";
    }

    // 常用的資料表名稱常數
    public static class LogTables
    {
        public const string USERS = "Users";
        public const string ROLES = "Roles";
        public const string PARAMETER = "Parameter";
        public const string MACHINE_PARAMETER = "MachineParameter";
        public const string RECIPE_HEADER = "RecipeHeader";
        public const string RECIPE_DETAIL = "RecipeDetail";
        public const string MACHINE_GROUP = "MachineGroup";
        public const string ALERT_GROUP = "AlertGroup";
    }

    // 欄位中文對照 (用於顯示)
    public static class FieldDisplayNames
    {
        public static readonly Dictionary<string, string> UserFields = new Dictionary<string, string>
        {
            {"UserNo", "使用者工號"},
            {"UserName", "使用者姓名"},
            {"DepartmentName", "部門名稱"},
            {"Email", "電子郵件"},
            {"RoleId", "權限角色"},
            {"IsActive", "是否啟用"}
        };

        public static readonly Dictionary<string, string> RoleFields = new Dictionary<string, string>
        {
            { "RoleName", "角色名稱" },
            { "Description", "描述" },
            { "Permissions", "權限功能" },
            { "IsActive", "是否啟用" }
        };

        public static readonly Dictionary<string, string> ParameterFields = new Dictionary<string, string>
        {
            { "ParamName", "參數名稱" },
            { "Unit", "單位" },
            { "SectionCode", "工段代碼" },
            { "SequenceNo", "排序序號" },
            { "IsActive", "是否啟用" }
        };

        public static readonly Dictionary<string, string> RecipeFields = new Dictionary<string, string>
        {
            { "ProdNo", "料號" },
            { "DeviceId", "機台代號" },
            { "MoldNo", "模具編號" },
            { "MaterialNo", "原料編號" },
            { "Version", "版本" },
            { "IsActive", "是否啟用" },
            { "Remark", "備註" }
        };

        public static readonly Dictionary<string, string> MachineGroupFields = new Dictionary<string, string>
        {
            {"GroupName", "群組名稱"},
            {"Description", "群組描述"},
            {"Devices", "包含機台"}
        };

        public static readonly Dictionary<string, string> AlertGroupFields = new Dictionary<string, string>
        {
            {"GroupName", "群組名稱"},
            {"Description", "群組描述"},
            {"RoleId", "權限角色"},
            {"MachineGroupId", "機台群組"},
            {"IsActive", "是否啟用"}
        };
    }
}