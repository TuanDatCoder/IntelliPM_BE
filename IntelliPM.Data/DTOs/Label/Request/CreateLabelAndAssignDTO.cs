using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Label.Request
{
    public class CreateLabelAndAssignDTO
    {
        public int ProjectId { get; set; }
        public string Name { get; set; } = null!;

        // WorkItem target
        public string? TaskId { get; set; }
        public string? EpicId { get; set; }
        public string? SubtaskId { get; set; }
    }

}
