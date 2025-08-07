using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RecipeManageSystem.Models
{
    public class SupervisorEngineerRelation
    {
        public long Id { get; set; }
        public string SupervisorNo { get; set; }
        public string SupervisorName { get; set; }
        public string EngineerNo { get; set; }
        public string EngineerName { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }

    public class Engineer
    {
        public string EngineerNo { get; set; }
        public string EngineerName { get; set; }
        public string DepartmentNo { get; set; }
        public string DepartmentName { get; set; }
        public string Email { get; set; }
    }
}


