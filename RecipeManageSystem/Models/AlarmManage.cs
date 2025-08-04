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
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public List<int> Roles { get; internal set; }
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
}

