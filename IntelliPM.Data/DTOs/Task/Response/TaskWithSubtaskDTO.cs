using IntelliPM.Data.DTOs.Account.Response;
using IntelliPM.Data.DTOs.Subtask.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Task.Response
{
    public class TaskWithSubtaskDTO
    {
        public string Id { get; set; } = null!;
        public decimal? PlannedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public List<AccountBasicDTO> Accounts { get; set; } = new();
        public List<SubtaskBasicDTO> Subtasks { get; set; } = new();
    }
}
