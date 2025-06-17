using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RecipeManageSystem.Models
{
    public class User
    {
        public string UserNo { get; set; }
        public string LdapAccount { get; set; }
        public string UserName { get; set; }
        public string DepartmentNo { get; set; }
        public string DepartmentName { get; set; }
        public string TitleName { get; set; }

        public string RoleId { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public string CreateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string UpdateBy { get; set; }
    }

}