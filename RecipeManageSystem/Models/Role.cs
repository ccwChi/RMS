using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RecipeManageSystem.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public string Permissions { get; set; }
        public bool? IsActive { get; set; }
    }
    
    public class Permission
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; }
        public string ModuleGroup { get; set; }
    }


}