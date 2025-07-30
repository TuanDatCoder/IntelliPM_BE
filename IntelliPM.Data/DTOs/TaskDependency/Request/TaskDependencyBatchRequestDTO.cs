using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.TaskDependency.Request
{
    public class TaskDependencyBatchRequestDTO
    {
        public List<TaskDependencyIdRequestDTO> Dependencies { get; set; } = new();
    }
}
