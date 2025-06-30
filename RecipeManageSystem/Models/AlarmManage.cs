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
}