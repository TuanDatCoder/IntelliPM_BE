using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.TaskDependency.Request
{
    public class TaskDependencyIdRequestDTO
    {
        public int Id { get; set; }
        public string FromType { get; set; } = null!;
        public string LinkedFrom { get; set; } = null!;
        public string ToType { get; set; } = null!;
        public string LinkedTo { get; set; } = null!;
        public string Type { get; set; } = null!;
    }
}
