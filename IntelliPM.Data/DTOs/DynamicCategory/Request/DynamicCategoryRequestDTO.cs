using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.DynamicCategory.Request
{
    public class DynamicCategoryRequestDTO
    {
        public string CategoryGroup { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public int OrderIndex { get; set; }
    }
}
