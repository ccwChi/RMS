using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RecipeManageSystem.Models
{
    public class AlertUser
    {
        public int AlertId { get; set; }
        public string UserNo { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class AlertGroupDto
    {
        public int AlertGroupId { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public int RoleId { get; set; }  // 改為單一角色
        public List<int> MachineGroupIds { get; set; } = new List<int>(); // 新增機台群組清單
        public bool IsActive { get; set; } = true;  // 新增：是否啟用發報
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
    }

    // 新增用於編輯時取得詳細資料的 DTO
    public class AlertGroupDetailDto
    {
        public int AlertGroupId { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public int? RoleId { get; set; }
        public bool IsActive { get; set; } = true;  // 新增：是否啟用發報
        public List<int> MachineGroupIds { get; set; } = new List<int>();
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
        public DateTime UpdateDate { get; set; }

    }

    public class MachineGroupWithDevices
    {
        public int MachineGroupId { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public string DeviceList { get; set; }  // 機台清單 (逗號分隔)
        public int DeviceCount { get; set; }    // 機台數量
    }
}

