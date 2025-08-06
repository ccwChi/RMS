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
        public string Remark { get; set; }
        public string AlarmFlag { get; set; }
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

        public string Remark { get; set; }
        public string AlarmFlag { get; set; }

    }

    public class RecipeDetail
    {
        public int RecipeDetailId { get; set; }
        public int RecipeId { get; set; }
        public int ParamId { get; set; }
        public string ParamName { get; set; }
        public string AlarmFlag { get; set; }
        public string Unit { get; set; }
        public decimal? StdValue { get; set; }       
        public decimal? MaxValue { get; set; }
        public decimal? MinValue { get; set; }

        public string BiasMethod { get; set; } = "customize"; 
        public decimal? BiasValue { get; set; }
    }

    public class RecipeHeader
    {
        public int RecipeId { get; set; }
        public string ProdNo { get; set; }
        public string DeviceId { get; set; }
        public string MoldNo { get; set; }
        public string MaterialNo { get; set; }
        public string Remark { get; set; }
        public int Version { get; set; }
        public bool IsActive { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
    }


    public class RecipeVersionDto
    {
        public int RecipeId { get; set; }
        public string ProdNo { get; set; }
        public string DeviceId { get; set; }
        public string MoldNo { get; set; }
        public string MaterialNo { get; set; }
        public string Remark { get; set; }
        public int Version { get; set; }
        public bool IsActive { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public List<RecipeDetailDto> Params { get; set; } = new List<RecipeDetailDto>();
    }

    // Models/RecipeDetailDto.cs
    public class RecipeDetailDto
    {
        public int ParamId { get; set; }
        public string ParamName { get; set; }
        public string StdValue { get; set; }
        public string MaxValue { get; set; }
        public string MinValue { get; set; }
        public string BiasMethod { get; set; }
        public string BiasValue { get; set; }
        public string AlarmFlag { get; set; }
    }
}