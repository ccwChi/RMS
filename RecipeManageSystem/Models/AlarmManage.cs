using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RecipeManageSystem.Models
{
    //public class AlertUser
    //{
    //    public int AlertId { get; set; }
    //    public string UserNo { get; set; }
    //    public string CreateBy { get; set; }
    //    public DateTime CreateDate { get; set; }
    //}

    public class AlertGroupDto
    {
        public int AlertGroupId { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public int MachineGroupId { get; set; }  // 保留向後相容
        public bool IsActive { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public List<int> RoleIds { get; set; } = new List<int>();
        public List<int> MachineGroupIds { get; set; } = new List<int>();
    }


    public class AlertGroupSummaryDto
    {
        public int AlertGroupId { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public int MachineGroupId { get; set; }
        public bool IsActive { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string RoleIds { get; set; } // 逗號分隔的角色ID字串

        // 輔助屬性：轉換成角色ID列表
        public List<int> RoleIdList
        {
            get
            {
                if (string.IsNullOrEmpty(RoleIds)) return new List<int>();
                return RoleIds.Split(',').Select(int.Parse).ToList();
            }
        }
    }

    public class AlertGroupDetailDto
    {
        public int AlertGroupId { get; set; }  // 新增
        public string GroupName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int MachineGroupId { get; set; }
        public List<int> RoleIds { get; set; } = new List<int>();
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
        public string DeviceList { get; set; }
        public int DeviceCount { get; set; }
    }
}

