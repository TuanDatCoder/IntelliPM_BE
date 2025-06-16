using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.DynamicCategory.Response
{
    public class DynamicCategoryResponseDTO
    {
        public int Id { get; set; }
        public string CategoryGroup { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int OrderIndex { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
