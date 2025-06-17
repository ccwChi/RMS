using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RecipeManageSystem.Models
{
    public class Parameter
    {
        public int ParamId { get; set; }
        public string ParamName { get; set; }
        public string Unit { get; set; }
        public bool? IsActive { get; set; }
        public string SectionCode { get; set; }
        public int SequenceNo { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
    }

    public class Machine
    {
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public string MpsSectionNo { get; set; }
        public string StateFlag { get; set; }
    }

    public class MachineParameter
    {
        public int MappingId { get; set; }
        public string DeviceId { get; set; }
        public int ParamId { get; set; }
        public string ParamName { get; set; }
        public string ParamUnit { get; set; }
    }

    public class MachineParameterView
    {
        public int MappingId { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public int ParamId { get; set; }
        public string ParamName { get; set; }
    }

    public class MachineParameterDto
    {
        public string DeviceId { get; set; }
        public List<int> Params { get; set; }
    }    
    
    public class MachineParameterDetail
    {
        public string DeviceId { get; set; }
        public string ParamId { get; set; }

    }


}