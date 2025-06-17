using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RecipeManageSystem.Models
{
    public class RecipeTotalDto
    {
        public string Mode { get; set; }
        public int RecipeId { get; set; }
        public string ProdNo { get; set; }
        public string ProdName { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string MoldNo { get; set; }
        public string MaterialNo { get; set; }
        public int Version { get; set; }
        public bool IsActive { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public List<RecipeDetail> RecipeDetails { get; set; }
    }

    public class RecipeEditDto
    {
        public int RecipeId { get; set; }
        public string ProdNo { get; set; }
        public string ProdName { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string MoldNo { get; set; }
        public string MaterialNo { get; set; }
        public int Version { get; set; }

        public int ParamId { get; set; }
        public string ParamName { get; set; }
        public decimal? StdValue { get; set; }    
        public decimal? MaxValue { get; set; }
        public decimal? MinValue { get; set; }

        public string BiasMethod { get; set; } = "customize";
        public decimal? BiasValue { get; set; }

    }

    public class RecipeDetail
    {
        public int RecipeDetailId { get; set; }
        public int RecipeId { get; set; }
        public int ParamId { get; set; }
        public string ParamName { get; set; }
        public decimal? StdValue { get; set; }       
        public decimal? MaxValue { get; set; }
        public decimal? MinValue { get; set; }

        public string BiasMethod { get; set; } = "customize"; 
        public decimal? BiasValue { get; set; }
    }

    public class RecipeHeader
    {
        public int RecipeId { get; set; }
        public string PartNo { get; set; }
        public string DeviceId { get; set; }
        public string MoldNo { get; set; }
        public int Version { get; set; }
        public bool IsActive { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}