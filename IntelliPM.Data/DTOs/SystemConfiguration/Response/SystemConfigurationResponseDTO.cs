using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.SystemConfiguration.Response
{
    public class SystemConfigurationResponseDTO
    {
        public int Id { get; set; }
        public string ConfigKey { get; set; }
        public string? ValueConfig { get; set; }
        public string? MinValue { get; set; }
        public string? MaxValue { get; set; }
        public string? EstimateValue { get; set; }
        public string? Description { get; set; }
        public string? Note { get; set; }
        public DateTime? EffectedFrom { get; set; }
        public DateTime? EffectedTo { get; set; }
    }
}
