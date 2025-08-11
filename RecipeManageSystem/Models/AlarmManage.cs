using System;
using System.Collections.Generic;

namespace RecipeManageSystem.Models
{
    // 原有的 AlertGroupDto 保持不變
    public class AlertGroupDto
    {
        public int AlertGroupId { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public int MachineGroupId { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
    }

    // 新增：警報群組批量處理 DTO (一個角色對應多個機台群組)
    public class AlertGroupBatchDto
    {
        public string GroupName { get; set; }
        public string Description { get; set; }
        public int? RoleId { get; set; }
        public List<int> MachineGroupIds { get; set; } = new List<int>();
        public bool IsActive { get; set; } = true;
    }

    // 新增：警報群組刪除 DTO
    public class AlertGroupDeleteDto
    {
        public string GroupName { get; set; }
        public int? RoleId { get; set; }
    }

    // 新增：警報群組啟用/停用 DTO
    public class AlertGroupToggleDto
    {
        public string GroupName { get; set; }
        public int? RoleId { get; set; }
        public bool IsActive { get; set; }
    }

    // 新增：警報群組摘要 DTO (用於列表顯示)
    public class AlertGroupSummaryDto
    {
        public int RowNumber { get; set; }  // 序號
        public string GroupName { get; set; }
        public string Description { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        public int MachineGroupCount { get; set; }
        public string MachineGroupNames { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }

        // 格式化顯示用
        public string IsActiveDisplay => IsActive ? "啟用" : "停用";
        public string CreateDateDisplay => CreateDate.ToString("yyyy-MM-dd");
        public string MachineGroupDisplay => $"{MachineGroupNames} ({MachineGroupCount}個群組)";
    }

    // 新增：警報群組詳細資料 DTO (用於編輯時載入)
    public class AlertGroupDetailDto
    {
        public string GroupName { get; set; }
        public string Description { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
        public List<int> RoleIds { get; set; } = new List<int>();
        public List<int> MachineGroupIds { get; set; } = new List<int>();
    }

    // 原有的 MachineGroup 保持不變
    public class MachineGroup
    {
        public int IdentityId { get; set; }
        public int MachineGroupId { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public string DeviceId { get; set; }
        public List<string> Devices { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
    }

    // 其他原有的類別保持不變
    public class AlertUser
    {
        public int AlertId { get; set; }
        public string UserNo { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class AlertGroupDeviceDto
    {
        public int Id { get; set; }
        public int AlertGroupId { get; set; }
        public string DeviceId { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
    }

    public class AlertGroupUserDto
    {
        public int Id { get; set; }
        public int AlertGroupId { get; set; }
        public string UserNo { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}