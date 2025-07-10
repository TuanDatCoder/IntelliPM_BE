using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.TaskDependency.Response
{
    public class TaskDependencyResponseDTO
    {
        public int Id { get; set; }
        public string TaskId { get; set; } = default!;
        public string LinkedFrom { get; set; } = default!;
        public string LinkedTo { get; set; } = default!;
        public string Type { get; set; } = default!;
    }

}
