using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.TaskDependency.Request
{
    public class TaskDependencyRequestDTO
    {
        public string TaskId { get; set; } 
        public string LinkedFrom { get; set; }
        public string LinkedTo { get; set; }
        public string Type { get; set; }
    }
}
